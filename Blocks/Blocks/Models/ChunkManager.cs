#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Collections;
using Willcraftia.Xna.Framework.Diagnostics;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Framework.Threading;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class ChunkManager
    {
        // TODO
        //
        // 実行で最適と思われる値を調べて決定するが、
        // 最終的には定義ファイルのようなもので定義を変更できるようにする。
        //

        public const ushort VertexCapacity = 8000;

        public const ushort IndexCapacity = 11000;

        // 更新の最大試行数。
        public const int UpdateCapacity = 1000;

        public const int InitialActiveChunkCapacity = 3000;

        public const int InterChunkCapacity = 100;

        static readonly VectorI3 chunkSize = Chunk.Size;

        static readonly Vector3 chunkMeshOffset = Chunk.HalfSize.ToVector3();

        Region region;

        Vector3 inverseChunkSize;

        ChunkCollection activeChunks = new ChunkCollection(InitialActiveChunkCapacity);

        Queue<Chunk> updatingChunks = new Queue<Chunk>(UpdateCapacity);

        // 更新の開始インデックス。
        int updateOffset = 0;

        // チャンク数はパーティション数に等しい。
        // このため、ここでは最大チャンク数を決定できない。

        ConcurrentPool<Chunk> chunkPool;

        // 最大チャンク数をここで決定できないことから、
        // 同様に最大メッシュ数もここで決定できない。

        ConcurrentPool<ChunkMesh> chunkMeshPool;

        Pool<InterChunk> interChunkPool;

        Pool<VertexBuffer> vertexBufferPool;

        Pool<IndexBuffer> indexBufferPool;

        ChunkMeshUpdateManager chunkMeshUpdateManager;

        bool closing;

        bool closed;

        public ChunkManager(Region region)
        {
            if (region == null) throw new ArgumentNullException("region");

            this.region = region;

            inverseChunkSize.X = 1 / (float) chunkSize.X;
            inverseChunkSize.Y = 1 / (float) chunkSize.Y;
            inverseChunkSize.Z = 1 / (float) chunkSize.Z;

            chunkPool = new ConcurrentPool<Chunk>(CreateChunk);
            chunkMeshPool = new ConcurrentPool<ChunkMesh>(CreateChunkMesh);
            interChunkPool = new Pool<InterChunk>(CreateInterChunk)
            {
                MaxCapacity = InterChunkCapacity
            };
            vertexBufferPool = new Pool<VertexBuffer>(CreateVertexBuffer);
            indexBufferPool = new Pool<IndexBuffer>(CreateIndexBuffer);
            chunkMeshUpdateManager = new ChunkMeshUpdateManager(region, this);
        }

        public void Update()
        {
            if (closed) return;

            if (closing)
            {
                lock (activeChunks)
                {
                    if (activeChunks.Count == 0)
                    {
                        closing = false;
                        closed = true;
                        return;
                    }
                }
            }

            region.Monitor.TotalChunkCount = chunkPool.TotalObjectCount;
            region.Monitor.ActiveChunkCount = activeChunks.Count;
            region.Monitor.TotalChunkMeshCount = chunkMeshPool.TotalObjectCount;
            region.Monitor.PassiveChunkMeshCount = chunkMeshPool.Count;
            region.Monitor.TotalInterChunkMeshCount = interChunkPool.TotalObjectCount;
            region.Monitor.PassiveInterChunkMeshCount = interChunkPool.Count;
            region.Monitor.TotalVertexBufferCount = vertexBufferPool.TotalObjectCount;
            region.Monitor.PassiveVertexBufferCount = vertexBufferPool.Count;

            // 長時間のロックを避けるために、一時的に作業リストへコピー。
            lock (activeChunks)
            {
                // アクティブ チャンクが無いならば更新処理終了。
                if (activeChunks.Count == 0) return;

                int index = updateOffset;
                bool cycled = false;
                while (updatingChunks.Count < UpdateCapacity)
                {
                    if (activeChunks.Count <= index)
                    {
                        index = 0;
                        cycled = true;
                    }

                    if (cycled && updateOffset <= index) break;

                    var chunk = activeChunks[index++];
                    updatingChunks.Enqueue(chunk);
                }

                updateOffset = index;
            }

            Debug.Assert(updatingChunks.Count <= UpdateCapacity);

            int count = updatingChunks.Count;
            for (int i = 0; i < count; i++)
            {
                var chunk = updatingChunks.Dequeue();

                if (!chunk.EnterUpdate()) continue;

                // 現在の隣接チャンクのアクティブ状態が前回のメッシュ更新時のアクティブ状態と異なるならば、
                // 新たにアクティブ化された隣接チャンクを考慮してメッシュを更新するために、
                // 強制的にチャンクを Dirty とする。
                if (chunk.ActiveNeighbors != chunk.NeighborsReferencedOnUpdate)
                    chunk.MeshDirty = true;

                if (!chunk.MeshDirty)
                {
                    chunk.ExitUpdate();
                    continue;
                }

                if (chunk.InterChunk == null)
                {
                    // 中間チャンク未設定ならば設定を試行。

                    if (closing)
                    {
                        // クローズが開始したならば新規の更新要求は破棄。
                        chunk.ExitUpdate();
                    }
                    else if (BorrowInterChunk(chunk))
                    {
                        chunk.InterChunk.Completed = false;
                        chunkMeshUpdateManager.EnqueueChunk(chunk);
                    }
                    else
                    {
                        // 中間チャンクが枯渇しているため更新を次回の試行に委ねる。
                        chunk.ExitUpdate();
                    }
                }
                else if (chunk.InterChunk.Completed)
                {
                    // 中間チャンクが更新完了ならば Mesh を更新。
                    UpdateChunkMesh(chunk);

                    // 中間チャンクは不要となるためプールへ返却。
                    ReturnInterChunk(chunk.InterChunk);
                    chunk.InterChunk = null;

                    chunk.MeshDirty = false;
                    chunk.ExitUpdate();
                }
            }

            Debug.Assert(updatingChunks.Count == 0);

            chunkMeshUpdateManager.Update();
        }

        // 非同期呼び出し。
        public Chunk ActivateChunk(ref VectorI3 position)
        {
            var chunk = chunkPool.Borrow();
            if (chunk == null) return null;

            Debug.Assert(!chunk.Active);

            if (!region.ChunkStore.GetChunk(ref position, chunk))
            {
                chunk.Position = position;

                foreach (var procedure in region.ChunkProcesures)
                    procedure.Generate(chunk);
            }

            chunk.OnActivated();

            lock (activeChunks) activeChunks.Add(chunk);

            return chunk;
        }

        // 非同期呼び出し。
        public bool PassivateChunk(Chunk chunk)
        {
            if (chunk == null) throw new ArgumentNullException("chunk");

            Debug.Assert(chunk.Active);

            if (!chunk.EnterPassivate()) return false;

            lock (activeChunks) activeChunks.Remove(chunk);

            if (chunk.OpaqueMesh != null)
            {
                PassivateChunkMesh(chunk.OpaqueMesh);
                chunk.OpaqueMesh = null;
            }
            if (chunk.TranslucentMesh != null)
            {
                PassivateChunkMesh(chunk.TranslucentMesh);
                chunk.TranslucentMesh = null;
            }
            if (chunk.InterChunk != null)
            {
                ReturnInterChunk(chunk.InterChunk);
                chunk.InterChunk = null;
            }

            // 定義に変更があるならば永続化領域を更新。
            if (chunk.DefinitionDirty) region.ChunkStore.AddChunk(chunk);

            chunk.OnPassivated();
            chunk.ExitPassivate();

            chunkPool.Return(chunk);

            return true;
        }

        public void Close()
        {
            if (closing || closed) return;

            closing = true;
        }

        public void GetNearbyActiveChunks(ref VectorI3 position, out NearbyChunks nearbyChunks)
        {
            nearbyChunks = new NearbyChunks();

            foreach (var side in CubicSide.Items)
            {
                var nearbyPosition = position + side.Direction;

                Chunk nearbyChunk;
                lock (activeChunks)
                    activeChunks.TryGetItem(ref nearbyPosition, out nearbyChunk);

                nearbyChunks[side] = nearbyChunk;
            }
        }

        Chunk CreateChunk()
        {
            return new Chunk(chunkSize);
        }

        ChunkMesh CreateChunkMesh()
        {
            return new ChunkMesh(region);
        }

        InterChunk CreateInterChunk()
        {
            return new InterChunk();
        }

        VertexBuffer CreateVertexBuffer()
        {
            return new VertexBuffer(region.GraphicsDevice, typeof(VertexPositionNormalTexture), VertexCapacity, BufferUsage.WriteOnly);
        }

        IndexBuffer CreateIndexBuffer()
        {
            return new IndexBuffer(region.GraphicsDevice, IndexElementSize.SixteenBits, IndexCapacity, BufferUsage.WriteOnly);
        }

        bool BorrowInterChunk(Chunk chunk)
        {
            if (chunk.InterChunk == null)
                chunk.InterChunk = interChunkPool.Borrow();

            return chunk.InterChunk != null;
        }

        void ReturnInterChunk(InterChunk interChunk)
        {
            interChunk.Opaque.Clear();
            interChunk.Translucent.Clear();
            interChunkPool.Return(interChunk);
        }

        ChunkMesh ActivateChunkMesh(bool translucent)
        {
            var chunkMesh = chunkMeshPool.Borrow();
            chunkMesh.Translucent = translucent;

            chunkMesh.VertexBuffer = vertexBufferPool.Borrow();
            chunkMesh.IndexBuffer = indexBufferPool.Borrow();
            
            region.SceneManager.AddSceneObject(chunkMesh);
            
            return chunkMesh;
        }

        void PassivateChunkMesh(ChunkMesh chunkMesh)
        {
            region.Monitor.TotalChunkVertexCount -= chunkMesh.VertexCount;
            region.Monitor.TotalChunkIndexCount -= chunkMesh.IndexCount;

            region.SceneManager.RemoveSceneObject(chunkMesh);

            if (chunkMesh.VertexBuffer != null)
            {
                vertexBufferPool.Return(chunkMesh.VertexBuffer);
                chunkMesh.VertexBuffer = null;
                chunkMesh.VertexCount = 0;
            }
            if (chunkMesh.IndexBuffer != null)
            {
                indexBufferPool.Return(chunkMesh.IndexBuffer);
                chunkMesh.IndexBuffer = null;
                chunkMesh.IndexCount = 0;
            }

            chunkMeshPool.Return(chunkMesh);
        }

        void UpdateChunkMesh(Chunk chunk)
        {
            var interMesh = chunk.InterChunk;

            // メッシュに設定するワールド座標。
            // チャンクの中心をメッシュの位置とする。
            var position = chunk.WorldPosition + chunkMeshOffset;

            // メッシュに設定するワールド行列。
            Matrix world;
            Matrix.CreateTranslation(ref position, out world);

            //----------------------------------------------------------------
            // 不透明メッシュ

            if (interMesh.Opaque.VertexCount == 0 || interMesh.Opaque.IndexCount == 0)
            {
                if (chunk.OpaqueMesh != null)
                {
                    PassivateChunkMesh(chunk.OpaqueMesh);
                    chunk.OpaqueMesh = null;
                }
            }
            else
            {
                if (chunk.OpaqueMesh == null)
                {
                    chunk.OpaqueMesh = ActivateChunkMesh(false);
                }
                else
                {
                    region.Monitor.TotalChunkVertexCount -= chunk.OpaqueMesh.VertexCount;
                    region.Monitor.TotalChunkIndexCount -= chunk.OpaqueMesh.IndexCount;
                }

                chunk.OpaqueMesh.Position = position;
                chunk.OpaqueMesh.World = world;
                interMesh.Opaque.Populate(chunk.OpaqueMesh);

                region.Monitor.TotalChunkVertexCount += chunk.OpaqueMesh.VertexCount;
                region.Monitor.TotalChunkIndexCount += chunk.OpaqueMesh.IndexCount;
                region.Monitor.MaxChunkVertexCount = Math.Max(region.Monitor.MaxChunkVertexCount, chunk.OpaqueMesh.VertexCount);
                region.Monitor.MaxChunkIndexCount = Math.Max(region.Monitor.MaxChunkIndexCount, chunk.OpaqueMesh.IndexCount);
            }

            //----------------------------------------------------------------
            // 半透明メッシュ

            if (interMesh.Translucent.VertexCount == 0 || interMesh.Translucent.IndexCount == 0)
            {
                if (chunk.TranslucentMesh != null)
                {
                    PassivateChunkMesh(chunk.TranslucentMesh);
                    chunk.TranslucentMesh = null;
                }
            }
            else
            {
                if (chunk.TranslucentMesh == null)
                {
                    chunk.TranslucentMesh = ActivateChunkMesh(true);
                }
                else
                {
                    region.Monitor.TotalChunkVertexCount -= chunk.TranslucentMesh.VertexCount;
                    region.Monitor.TotalChunkIndexCount -= chunk.TranslucentMesh.IndexCount;
                }

                chunk.TranslucentMesh.Position = position;
                chunk.TranslucentMesh.World = world;
                interMesh.Translucent.Populate(chunk.TranslucentMesh);

                region.Monitor.TotalChunkVertexCount += chunk.TranslucentMesh.VertexCount;
                region.Monitor.TotalChunkIndexCount += chunk.TranslucentMesh.IndexCount;
                region.Monitor.MaxChunkVertexCount = Math.Max(region.Monitor.MaxChunkVertexCount, chunk.TranslucentMesh.VertexCount);
                region.Monitor.MaxChunkIndexCount = Math.Max(region.Monitor.MaxChunkIndexCount, chunk.TranslucentMesh.IndexCount);
            }
        }
    }
}
