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

        public const int InterChunkMeshCapacity = 100;

        Region region;

        VectorI3 chunkSize;

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

        Pool<InterChunkMesh> interChunkMeshPool;

        Pool<VertexBuffer> vertexBufferPool;

        Pool<IndexBuffer> indexBufferPool;

        ChunkMeshUpdateManager chunkMeshUpdateManager;

        bool closing;

        bool closed;

        public VectorI3 ChunkSize
        {
            get { return chunkSize; }
        }

        public ChunkManager(Region region, VectorI3 chunkSize)
        {
            if (region == null) throw new ArgumentNullException("region");

            this.region = region;
            this.chunkSize = chunkSize;

            inverseChunkSize.X = 1 / (float) chunkSize.X;
            inverseChunkSize.Y = 1 / (float) chunkSize.Y;
            inverseChunkSize.Z = 1 / (float) chunkSize.Z;

            chunkPool = new ConcurrentPool<Chunk>(CreateChunk);
            chunkMeshPool = new ConcurrentPool<ChunkMesh>(CreateChunkMesh);
            interChunkMeshPool = new Pool<InterChunkMesh>(CreateInterChunkMesh)
            {
                MaxCapacity = InterChunkMeshCapacity
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

#if DEBUG
            region.Monitor.TotalChunkCount = chunkPool.TotalObjectCount;
            region.Monitor.ActiveChunkCount = activeChunks.Count;
            region.Monitor.TotalChunkMeshCount = chunkMeshPool.TotalObjectCount;
            region.Monitor.PassiveChunkMeshCount = chunkMeshPool.Count;
            region.Monitor.TotalInterChunkMeshCount = interChunkMeshPool.TotalObjectCount;
            region.Monitor.PassiveInterChunkMeshCount = interChunkMeshPool.Count;
            region.Monitor.TotalVertexBufferCount = vertexBufferPool.TotalObjectCount;
            region.Monitor.PassiveVertexBufferCount = vertexBufferPool.Count;
#endif

            // 長時間のロックを避けるために、一時的に作業リストへコピー。
            lock (activeChunks)
            {
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

                if (chunk.InterMesh == null)
                {
                    // 中間メッシュ未設定ならば設定を試行。

                    if (closing)
                    {
                        // クローズが開始したならば新規の更新要求は破棄。
                        chunk.ExitUpdate();
                    }
                    else if (BorrowInterChunkMesh(chunk))
                    {
                        chunk.InterMesh.Completed = false;
                        chunkMeshUpdateManager.EnqueueChunk(chunk);
                    }
                    else
                    {
                        // 中間メッシュが枯渇しているため更新を次回の試行に委ねる。
                        chunk.ExitUpdate();
                    }
                }
                else if (chunk.InterMesh.Completed)
                {
                    // 中間メッシュが更新完了ならば Mesh を更新。
                    UpdateChunkMesh(chunk);

                    // 中間メッシュは不要となるためプールへ返却。
                    ReturnInterChunkMesh(chunk.InterMesh);
                    chunk.InterMesh = null;

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
            if (chunk.InterMesh != null)
            {
                ReturnInterChunkMesh(chunk.InterMesh);
                chunk.InterMesh = null;
            }

            // 定義に変更があるならば永続化領域を更新。
            if (chunk.DefinitionDirty) region.ChunkStore.AddChunk(chunk);

            chunk.OnPassivated();
            chunk.ExitPassivate();

            chunk.Clear();
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

        InterChunkMesh CreateInterChunkMesh()
        {
            return new InterChunkMesh();
        }

        VertexBuffer CreateVertexBuffer()
        {
            return new VertexBuffer(region.GraphicsDevice, typeof(VertexPositionNormalTexture), VertexCapacity, BufferUsage.WriteOnly);
        }

        IndexBuffer CreateIndexBuffer()
        {
            return new IndexBuffer(region.GraphicsDevice, IndexElementSize.SixteenBits, IndexCapacity, BufferUsage.WriteOnly);
        }

        bool BorrowInterChunkMesh(Chunk chunk)
        {
            if (chunk.InterMesh == null)
                chunk.InterMesh = interChunkMeshPool.Borrow();

            return chunk.InterMesh != null;
        }

        void ReturnInterChunkMesh(InterChunkMesh interChunkMesh)
        {
            interChunkMesh.Opaque.Clear();
            interChunkMesh.Translucent.Clear();
            interChunkMeshPool.Return(interChunkMesh);
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
#if DEBUG
            region.Monitor.TotalChunkVertexCount -= chunkMesh.VertexCount;
            region.Monitor.TotalChunkIndexCount -= chunkMesh.IndexCount;
#endif

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
            var interMesh = chunk.InterMesh;

            // メッシュに設定するワールド座標。
            var position = chunk.WorldPosition;

            //----------------------------------------------------------------
            // Opaque

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
#if DEBUG
                    region.Monitor.TotalChunkVertexCount -= chunk.OpaqueMesh.VertexCount;
                    region.Monitor.TotalChunkIndexCount -= chunk.OpaqueMesh.IndexCount;
#endif
                }

                chunk.OpaqueMesh.Position = position;
                interMesh.Opaque.Populate(chunk.OpaqueMesh);

#if DEBUG
                region.Monitor.TotalChunkVertexCount += chunk.OpaqueMesh.VertexCount;
                region.Monitor.TotalChunkIndexCount += chunk.OpaqueMesh.IndexCount;
                region.Monitor.MaxChunkVertexCount = Math.Max(region.Monitor.MaxChunkVertexCount, chunk.OpaqueMesh.VertexCount);
                region.Monitor.MaxChunkIndexCount = Math.Max(region.Monitor.MaxChunkIndexCount, chunk.OpaqueMesh.IndexCount);
#endif
            }

            //----------------------------------------------------------------
            // Translucent

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
#if DEBUG
                    region.Monitor.TotalChunkVertexCount -= chunk.TranslucentMesh.VertexCount;
                    region.Monitor.TotalChunkIndexCount -= chunk.TranslucentMesh.IndexCount;
#endif
                }

                chunk.TranslucentMesh.Position = position;
                interMesh.Translucent.Populate(chunk.TranslucentMesh);

#if DEBUG
                region.Monitor.TotalChunkVertexCount += chunk.TranslucentMesh.VertexCount;
                region.Monitor.TotalChunkIndexCount += chunk.TranslucentMesh.IndexCount;
                region.Monitor.MaxChunkVertexCount = Math.Max(region.Monitor.MaxChunkVertexCount, chunk.TranslucentMesh.VertexCount);
                region.Monitor.MaxChunkIndexCount = Math.Max(region.Monitor.MaxChunkIndexCount, chunk.TranslucentMesh.IndexCount);
#endif
            }
        }
    }
}
