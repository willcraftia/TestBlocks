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

                    // メッシュ枯渇ならば次回の試行とする。
                    if (!BorrowChunkMesh(chunk)) continue;

                    // メッシュが変わるためシーン マネージャから削除。
                    region.SceneManager.RemoveSceneObject(chunk.Mesh.Opaque);
                    region.SceneManager.RemoveSceneObject(chunk.Mesh.Translucent);

#if DEBUG
                    region.Monitor.TotalChunkVertexCount -= chunk.Mesh.Opaque.VertexCount;
                    region.Monitor.TotalChunkVertexCount -= chunk.Mesh.Translucent.VertexCount;
                    region.Monitor.TotalChunkIndexCount -= chunk.Mesh.Opaque.IndexCount;
                    region.Monitor.TotalChunkIndexCount -= chunk.Mesh.Translucent.IndexCount;
#endif

                    // メッシュを更新。
                    UpdateChunkMesh(chunk);

                    ReturnInterChunkMesh(chunk.InterMesh);
                    chunk.InterMesh = null;

                    chunk.MeshDirty = false;
                    chunk.ExitUpdate();

                    // 頂点を持つメッシュをシーン マネージャへ登録。
                    if (chunk.Mesh.Opaque.VertexCount != 0 && chunk.Mesh.Opaque.IndexCount != 0)
                        region.SceneManager.AddSceneObject(chunk.Mesh.Opaque);

                    if (chunk.Mesh.Translucent.VertexCount != 0 && chunk.Mesh.Translucent.IndexCount != 0)
                        region.SceneManager.AddSceneObject(chunk.Mesh.Translucent);

#if DEBUG
                    region.Monitor.TotalChunkVertexCount += chunk.Mesh.Opaque.VertexCount;
                    region.Monitor.TotalChunkVertexCount += chunk.Mesh.Translucent.VertexCount;
                    region.Monitor.TotalChunkIndexCount += chunk.Mesh.Opaque.IndexCount;
                    region.Monitor.TotalChunkIndexCount += chunk.Mesh.Translucent.IndexCount;
                    region.Monitor.MaxChunkVertexCount = Math.Max(region.Monitor.MaxChunkVertexCount, chunk.Mesh.Opaque.VertexCount);
                    region.Monitor.MaxChunkVertexCount = Math.Max(region.Monitor.MaxChunkVertexCount, chunk.Mesh.Translucent.VertexCount);
                    region.Monitor.MaxChunkIndexCount = Math.Max(region.Monitor.MaxChunkIndexCount, chunk.Mesh.Opaque.IndexCount);
                    region.Monitor.MaxChunkIndexCount = Math.Max(region.Monitor.MaxChunkIndexCount, chunk.Mesh.Translucent.IndexCount);
#endif
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

            // シーン マネージャから削除。
            if (chunk.Mesh != null)
            {
                region.SceneManager.RemoveSceneObject(chunk.Mesh.Opaque);
                region.SceneManager.RemoveSceneObject(chunk.Mesh.Translucent);

#if DEBUG
                region.Monitor.TotalChunkVertexCount -= chunk.Mesh.Opaque.VertexCount;
                region.Monitor.TotalChunkVertexCount -= chunk.Mesh.Translucent.VertexCount;
                region.Monitor.TotalChunkIndexCount -= chunk.Mesh.Opaque.IndexCount;
                region.Monitor.TotalChunkIndexCount -= chunk.Mesh.Translucent.IndexCount;
#endif
            }

            // 定義に変更があるならば永続化領域を更新。
            if (chunk.DefinitionDirty) region.ChunkStore.AddChunk(chunk);

            chunk.OnPassivated();
            chunk.ExitPassivate();

            ReturnChunk(chunk);

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

        void ReturnChunk(Chunk chunk)
        {
            chunk.Clear();

            ReturnChunkMesh(chunk);
            ReturnInterChunkMesh(chunk);

            chunkPool.Return(chunk);
        }

        void BorrowBuffer(ChunkMeshPart meshPart)
        {
            if (meshPart.VertexBuffer == null)
                meshPart.VertexBuffer = vertexBufferPool.Borrow();
            if (meshPart.IndexBuffer == null)
                meshPart.IndexBuffer = indexBufferPool.Borrow();
        }

        void ReturnBuffer(ChunkMeshPart meshPart)
        {
            if (meshPart.VertexBuffer != null)
            {
                vertexBufferPool.Return(meshPart.VertexBuffer);
                meshPart.VertexBuffer = null;
                meshPart.VertexCount = 0;
            }
            if (meshPart.IndexBuffer != null)
            {
                indexBufferPool.Return(meshPart.IndexBuffer);
                meshPart.IndexBuffer = null;
                meshPart.IndexCount = 0;
            }
        }

        bool BorrowChunkMesh(Chunk chunk)
        {
            if (chunk.Mesh == null)
                chunk.Mesh = chunkMeshPool.Borrow();

            return chunk.Mesh != null;
        }

        void ReturnChunkMesh(Chunk chunk)
        {
            if (chunk.Mesh != null)
            {
                ReturnChunkMesh(chunk.Mesh);
                chunk.Mesh = null;
            }
        }

        void ReturnChunkMesh(ChunkMesh chunkMesh)
        {
            ReturnBuffer(chunkMesh.Opaque);
            ReturnBuffer(chunkMesh.Translucent);

            chunkMeshPool.Return(chunkMesh);
        }

        bool BorrowInterChunkMesh(Chunk chunk)
        {
            if (chunk.InterMesh == null)
                chunk.InterMesh = interChunkMeshPool.Borrow();

            return chunk.InterMesh != null;
        }

        void ReturnInterChunkMesh(Chunk chunk)
        {
            if (chunk.InterMesh != null)
            {
                ReturnInterChunkMesh(chunk.InterMesh);
                chunk.InterMesh = null;
            }
        }

        void ReturnInterChunkMesh(InterChunkMesh interChunkMesh)
        {
            interChunkMesh.Opaque.Clear();
            interChunkMesh.Translucent.Clear();
            interChunkMeshPool.Return(interChunkMesh);
        }

        void UpdateChunkMesh(Chunk chunk)
        {
            var mesh = chunk.Mesh;
            var interMesh = chunk.InterMesh;

            // メッシュ パートに設定するワールド座標。
            var position = chunk.WorldPosition;

            UpdateChunkMeshPart(interMesh.Opaque, mesh.Opaque, ref position);
            UpdateChunkMeshPart(interMesh.Translucent, mesh.Translucent, ref position);
        }

        void UpdateChunkMeshPart(InterChunkMeshPart interMeshPart, ChunkMeshPart meshPart, ref Vector3 position)
        {
            // Populate 呼び出しの前に設定。
            meshPart.Position = position;

            if (0 < interMeshPart.VertexCount && 0 < interMeshPart.IndexCount)
            {
                BorrowBuffer(meshPart);
                interMeshPart.Populate(meshPart);
            }
            else
            {
                ReturnBuffer(meshPart);
            }
        }
    }
}
