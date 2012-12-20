#region Using

using System;
using System.Collections.Generic;
using System.Threading;
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

            public bool IsCompleted { get; set; }

            public Task(ChunkMeshUpdateManager chunkMeshUpdateManager)
            {
                this.chunkMeshUpdateManager = chunkMeshUpdateManager;
            }

            public void Execute()
            {
                chunkMeshUpdateManager.UpdateChunk(Chunk);

                IsCompleted = true;
            }
        }

        #endregion

        static readonly VectorI3[] nearbyBlockOffsets =
        {
            // Top
            new VectorI3(0, 1, 0),
            // Bottom
            new VectorI3(0, -1, 0),
            // Front
            new VectorI3(0, 0, 1),
            // Back
            new VectorI3(0, 0, -1),
            // Left
            new VectorI3(-1, 0, 0),
            // Right
            new VectorI3(1, 0, 0)
        };

        Region region;

        ChunkManager chunkManager;

        VectorI3 chunkSize;

        // TODO
        // プール サイズは、メモリ占有量の観点で決定する。
        Pool<Task> taskPool;

        // TODO 初期容量

        List<Chunk> updatingChunks = new List<Chunk>();

        List<Task> activeTasks = new List<Task>();

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
            task.IsCompleted = false;
            taskQueue.Enqueue(task.Execute);
            activeTasks.Add(task);
        }

        public void Update()
        {
            // Update the task queue.
            taskQueue.Update();

            CheckCompletedTasks();
        }

        void CheckCompletedTasks()
        {
            int index = 0;
            while (index < activeTasks.Count)
            {
                var task = activeTasks[index];
                if (!task.IsCompleted)
                {
                    index++;
                    continue;
                }

                // Task の登録を解除
                activeTasks.RemoveAt(index);

                var chunk = task.Chunk;

                // Task に Chunk の参照が残らないようにリセットしてからプールへ戻す。
                task.Chunk = null;
                taskPool.Return(task);

                // 更新の完了した Chunk を記録から消し、完了マークを付ける。
                updatingChunks.Remove(chunk);
                chunk.PendingMesh.IsLoaded = true;
            }
        }

        void UpdateChunk(Chunk chunk)
        {
            if (!chunk.Dirty) return;

            //
            // パッシベーションとの同期のためのロック。
            // ロックを取れない場合は、パッシベーション中とみなし、処理を中断する。
            // 仮に、パッシベーションによるものでなければ、次のゲーム更新で Chunk が再登録されるであろう。
            //
            if (Monitor.TryEnter(chunk))
            {
                try
                {
                    BuildChunkMesh(chunk);
                }
                finally
                {
                    Monitor.Exit(chunk);
                }
            }
        }

        void BuildChunkMesh(Chunk chunk)
        {
            for (int z = 0; z < chunkSize.Z; z++)
            {
                for (int y = 0; y < chunkSize.Y; y++)
                {
                    for (int x = 0; x < chunkSize.X; x++)
                    {
                        var blockIndex = chunk[x, y, z];
                        if (Block.EmptyIndex == blockIndex)
                            continue;

                        //
                        // TODO
                        //
                        // エディタにおいて、このスレッドで処理している間に、
                        // BlockCatalog の編集が行われた場合はどうするのか？
                        // また、Block が編集されるのもまずい。
                        //
                        // エディタの場合は、このようなスレッドが全て終わるまで、
                        // 編集できないようにブロックすることが妥当と思われる。
                        // 少なくとも、個々の処理について同期を考えることは非現実的である。
                        //
                        var block = region.BlockCatalog[blockIndex];

                        BuildChunkMesh(chunk, x, y, z, block);
                    }
                }
            }
        }

        void BuildChunkMesh(Chunk chunk, int x, int y, int z, Block block)
        {
            var chunkMesh = chunk.PendingMesh;

            for (int i = 0; i < 6; i++)
            {
                var side = (Side) i;
                if (!ShouldDrawSurface(chunk, x, y, z, block, side)) continue;

                var meshPart = block.Mesh[side];

                if (block.Fluid || IsTranslucentSurface(block, side))
                {
                    AddMesh(chunkMesh.Translucent, x, y, z, meshPart);
                }
                else
                {
                    AddMesh(chunkMesh.Opaque, x, y, z, meshPart);
                }
            }
        }

        void AddMesh(ChunkMeshPart chunkMeshPart, int x, int y, int z, MeshPart blockMeshPart)
        {
            var startIndex = chunkMeshPart.VertexCount;

            var vertices = blockMeshPart.Vertices;
            for (int i = 0; i < vertices.Length; i++)
            {
                var adjustedVertex = vertices[i];
                adjustedVertex.Position.X += x;
                adjustedVertex.Position.Y += y;
                adjustedVertex.Position.Z += z;
                chunkMeshPart.AddVertex(ref adjustedVertex);
            }

            var indices = blockMeshPart.Indices;
            for (int i = 0; i < vertices.Length; i++)
                chunkMeshPart.AddIndex((ushort) (indices[i] + startIndex));
        }

        bool ShouldDrawSurface(Chunk chunk, int x, int y, int z, Block block, Side side)
        {
            if (block.Mesh[side] == null) return false;

            var nearbyBlockPosition = nearbyBlockOffsets[(byte) side];
            nearbyBlockPosition.X += x;
            nearbyBlockPosition.Y += y;
            nearbyBlockPosition.Z += z;

            var nearbyBlockIndex = chunkManager.FindActiveBlockIndex(chunk, ref nearbyBlockPosition);
            if (Block.EmptyIndex == nearbyBlockIndex)
                return true;

            var nearbyBlock = region.BlockCatalog[nearbyBlockIndex];
            if (nearbyBlock.Fluid && block.Fluid)
                return false;

            if (IsTranslucentSurface(block, side))
                return true;

            return false;
        }

        bool IsTranslucentSurface(Block block, Side side)
        {
            var tile = block.GetTile(side);
            return tile != null && tile.Translucent;
        }
    }
}
