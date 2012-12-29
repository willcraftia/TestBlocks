#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Collections;
using Willcraftia.Xna.Framework.Threading;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class ChunkMeshUpdateManager
    {
        #region Task

        class Task
        {
            ChunkMeshUpdateManager chunkMeshUpdateManager;

            public Chunk Chunk { get; set; }

            public bool Completed { get; set; }

            public Action ExecuteAction { get; private set; }

            public Task(ChunkMeshUpdateManager chunkMeshUpdateManager)
            {
                this.chunkMeshUpdateManager = chunkMeshUpdateManager;

                ExecuteAction = new Action(Execute);
            }

            public void Execute()
            {
                chunkMeshUpdateManager.UpdateChunk(Chunk);

                Completed = true;
            }
        }

        #endregion

        Region region;

        ChunkManager chunkManager;

        VectorI3 chunkSize;

        // TODO
        // プール サイズは、メモリ占有量の観点で決定する。
        Pool<Task> taskPool;

        // TODO 初期容量

        // TODO: HashSet にしたい。
        List<Chunk> updatingChunks = new List<Chunk>();

        // 全件を対象に削除判定を行うため、リストではなくキューで管理。
        Queue<Task> activeTasks = new Queue<Task>();

        TaskQueue taskQueue = new TaskQueue();

        public ChunkMeshUpdateManager(Region region, ChunkManager chunkManager)
        {
            if (region == null) throw new ArgumentNullException("region");
            if (chunkManager == null) throw new ArgumentNullException("chunkManager");

            this.region = region;
            this.chunkManager = chunkManager;

            chunkSize = chunkManager.ChunkSize;

            taskPool = new Pool<Task>(() => { return new Task(this); });
        }

        public void EnqueueChunk(Chunk chunk)
        {
            // 重複登録を避けるための処置。
            if (updatingChunks.Contains(chunk)) return;

            // プールからタスクを得られない時には、更新処理の登録をスキップ。
            // ここでスキップしても、Chunk.Dirty = true ならば、次のゲーム更新でまたここに来る。
            var task = taskPool.Borrow();
            if (task == null) return;

            // Chunk を記録。
            updatingChunks.Add(chunk);

            // Task を準備して登録。
            task.Chunk = chunk;
            task.Completed = false;
            taskQueue.Enqueue(task.ExecuteAction);
            activeTasks.Enqueue(task);
        }

        public void Update()
        {
#if DEBUG
            region.Monitor.UpdatingChunkCount = updatingChunks.Count;
#endif

            // Update the task queue.
            taskQueue.Update();

            CheckCompletedTasks();
        }

        void CheckCompletedTasks()
        {
            int activeTaskCount = activeTasks.Count;
            for (int i = 0; i < activeTaskCount; i++)
            {
                var task = activeTasks.Dequeue();
                if (!task.Completed)
                {
                    // リストへ戻す。
                    activeTasks.Enqueue(task);
                    continue;
                }

                var chunk = task.Chunk;

                // Task に Chunk の参照が残らないようにリセットしてからプールへ戻す。
                task.Chunk = null;
                taskPool.Return(task);

                // 更新の完了した Chunk を記録から消し、完了マークを付ける。
                updatingChunks.Remove(chunk);
                chunk.InterChunk.Completed = true;
            }
        }

        void UpdateChunk(Chunk chunk)
        {
            Debug.Assert(chunk.Active);
            Debug.Assert(chunk.Updating);
            Debug.Assert(chunk.MeshDirty);

            var position = chunk.Position;

            // この更新で利用する隣接チャンクを探索。
            NearbyChunks nearbyChunks;
            chunkManager.GetNearbyActiveChunks(ref position, out nearbyChunks);

            // この更新で利用する隣接チャンクを記録。
            var flags = CubicSide.Flags.None;
            foreach (var side in CubicSide.Items)
            {
                if (nearbyChunks[side] != null)
                    flags |= side.ToFlags();
            }
            chunk.NeighborsReferencedOnUpdate = flags;

            // メッシュを更新。
            for (int z = 0; z < chunkSize.Z; z++)
                for (int y = 0; y < chunkSize.Y; y++)
                    for (int x = 0; x < chunkSize.X; x++)
                        UpdateChunk(chunk, x, y, z, ref nearbyChunks);
        }

        void UpdateChunk(Chunk chunk, int x, int y, int z, ref NearbyChunks nearbyChunks)
        {
            var blockIndex = chunk[x, y, z];

            // 空ならば頂点は存在しない。
            if (Block.EmptyIndex == blockIndex) return;

            var block = region.BlockCatalog[blockIndex];

            // MeshPart が必ずしも平面であるとは限らないが、
            // ここでは平面を仮定して隣接状態を考える。

            foreach (var side in CubicSide.Items)
            {
                var meshPart = block.Mesh.MeshParts[side];

                // 対象面が存在しない場合はスキップ。
                if (meshPart == null) continue;

                // 対象面に隣接する Block を探索。
                var nearbyBlockIndex = GetNearbyBlockIndex(chunk, x, y, z, ref nearbyChunks, side);
                if (nearbyBlockIndex != Block.EmptyIndex)
                {
                    // 隣接 Block との関係から対象面の要否を判定。
                    var nearbyBlock = region.BlockCatalog[nearbyBlockIndex];

                    // 半透明な連続した流体 Block を並べる際、流体 Block 間の面は不要。
                    // ※流体 Block は常に半透明を仮定して処理。
                    if (nearbyBlock.Fluid && block.Fluid) continue;

                    // 対象面が半透明ではないならば、隣接 Block により不可視面となるため不要。
                    if (!block.IsTranslucentTile(side)) continue;
                }

                if (block.Fluid || block.IsTranslucentTile(side))
                {
                    AddMesh(x, y, z, meshPart, chunk.InterChunk.Translucent);
                }
                else
                {
                    AddMesh(x, y, z, meshPart, chunk.InterChunk.Opaque);
                }
            }
        }

        byte GetNearbyBlockIndex(Chunk chunk, int x, int y, int z, ref NearbyChunks nearbyChunks, CubicSide side)
        {
            var nearbyBlockPosition = side.Direction;
            nearbyBlockPosition.X += x;
            nearbyBlockPosition.Y += y;
            nearbyBlockPosition.Z += z;

            // 対象面に隣接する Block を探索。
            if (chunk.Contains(ref nearbyBlockPosition))
            {
                // 隣接 Block が対象 Chunk に含まれている場合。
                return chunk[nearbyBlockPosition.X, nearbyBlockPosition.Y, nearbyBlockPosition.Z];
            }
            else
            {
                // 隣接 Block が隣接 Chunk に含まれている場合。
                var nearbyChunk = nearbyChunks[side];
                
                // 隣接 Chunk がないならば空。
                if (nearbyChunk == null) return Block.EmptyIndex;

                // 隣接 Chunk での相対座標を算出。
                var relativeX = nearbyBlockPosition.X % chunkSize.X;
                var relativeY = nearbyBlockPosition.Y % chunkSize.Y;
                var relativeZ = nearbyBlockPosition.Z % chunkSize.Z;
                if (relativeX < 0) relativeX += chunkSize.X;
                if (relativeY < 0) relativeY += chunkSize.Y;
                if (relativeZ < 0) relativeZ += chunkSize.Z;

                return nearbyChunk[relativeX, relativeY, relativeZ];
            }
        }

        void AddMesh(int x, int y, int z, MeshPart source, InterChunkMesh destination)
        {
            foreach (var index in source.Indices)
                destination.AddIndex(index);

            var vertices = source.Vertices;
            for (int i = 0; i < vertices.Length; i++)
            {
                var vertex = vertices[i];
                vertex.Position.X += x;
                vertex.Position.Y += y;
                vertex.Position.Z += z;
                // Block の MeshPart はその中心に原点があるため、半 Block サイズだけずらす必要がある。
                vertex.Position.X += 0.5f;
                vertex.Position.Y += 0.5f;
                vertex.Position.Z += 0.5f;
                destination.AddVertex(ref vertex);
            }
        }
    }
}
