#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Collections;
using Willcraftia.Xna.Framework.Graphics;
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

        #region CloseChunks

        struct CloseChunks
        {
            public Chunk Top;

            public Chunk Bottom;

            public Chunk Front;

            public Chunk Back;

            public Chunk Left;

            public Chunk Right;

            public Chunk this[CubicSide side]
            {
                get
                {
                    switch (side.Index)
                    {
                        case CubicSide.TopIndex: return Top;
                        case CubicSide.BottomIndex: return Bottom;
                        case CubicSide.FrontIndex: return Front;
                        case CubicSide.BackIndex: return Back;
                        case CubicSide.LeftIndex: return Left;
                        case CubicSide.RightIndex: return Right;
                    }
                    throw new InvalidOperationException();
                }
                set
                {
                    switch (side.Index)
                    {
                        case CubicSide.TopIndex: Top = value; return;
                        case CubicSide.BottomIndex: Bottom = value; return;
                        case CubicSide.FrontIndex: Front = value; return;
                        case CubicSide.BackIndex: Back = value; return;
                        case CubicSide.LeftIndex: Left = value; return;
                        case CubicSide.RightIndex: Right = value; return;
                    }
                    throw new InvalidOperationException();
                }
            }
        }

        #endregion

        // TODO
        public const int TaskQueueSlotCount = 20;

        static readonly VectorI3 chunkSize = Chunk.Size;

        static readonly Vector3 chunkMeshOffset = Chunk.HalfSize.ToVector3();

        static readonly Vector3 blockMeshOffset = new Vector3(0.5f);

        ChunkManager chunkManager;

        Pool<Task> taskPool;

        // 中間チャンク容量を超えないので、これを初期容量とする。
        List<Chunk> updatingChunks = new List<Chunk>(ChunkManager.InterChunkCapacity);

        // 全件を対象に削除判定を行うため、リストではなくキューで管理。
        Queue<Task> activeTasks = new Queue<Task>();

        TaskQueue taskQueue = new TaskQueue();

        public ChunkMeshUpdateManager(ChunkManager chunkManager)
        {
            if (chunkManager == null) throw new ArgumentNullException("chunkManager");

            this.chunkManager = chunkManager;

            taskPool = new Pool<Task>(() => { return new Task(this); })
            {
                // 同時更新許容量を超えることはない。
                MaxCapacity = ChunkManager.UpdateCapacity
            };

            taskQueue.SlotCount = TaskQueueSlotCount;
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
            CloseChunks closeChunks;
            GetCloseChunks(ref position, out closeChunks);

            // この更新で利用する隣接チャンクを記録。
            var flags = CubicSide.Flags.None;
            foreach (var side in CubicSide.Items)
            {
                if (closeChunks[side] != null)
                    flags |= side.ToFlags();
            }
            chunk.NeighborsReferencedOnUpdate = flags;

            // メッシュを更新。
            VectorI3 blockPosition = new VectorI3();
            for (blockPosition.Z = 0; blockPosition.Z < chunkSize.Z; blockPosition.Z++)
                for (blockPosition.Y = 0; blockPosition.Y < chunkSize.Y; blockPosition.Y++)
                    for (blockPosition.X = 0; blockPosition.X < chunkSize.X; blockPosition.X++)
                        UpdateChunk(chunk, ref closeChunks, ref blockPosition);
        }

        void GetCloseChunks(ref VectorI3 position, out CloseChunks closeChunks)
        {
            closeChunks = new CloseChunks();

            foreach (var side in CubicSide.Items)
            {
                var closePosition = position + side.Direction;

                Chunk chunk;
                chunkManager.TryGetChunk(ref closePosition, out chunk);

                closeChunks[side] = chunk;
            }
        }

        // blockPosition はチャンク内の相対ブロック座標。
        void UpdateChunk(Chunk chunk, ref CloseChunks closeChunks, ref VectorI3 blockPosition)
        {
            var blockIndex = chunk[blockPosition.X, blockPosition.Y, blockPosition.Z];

            // 空ならば頂点は存在しない。
            if (Block.EmptyIndex == blockIndex) return;

            var block = chunk.Region.BlockCatalog[blockIndex];

            // MeshPart が必ずしも平面であるとは限らないが、
            // ここでは平面を仮定して隣接状態を考える。

            foreach (var side in CubicSide.Items)
            {
                var meshPart = block.Mesh.MeshParts[side];

                // 対象面が存在しない場合はスキップ。
                if (meshPart == null) continue;

                // 対象面に隣接するブロックの座標 (現在のチャンク内での相対ブロック座標)
                var closeBlockPosition = blockPosition + side.Direction;

                // 対象面に隣接するブロックを探索。
                var closeBlockIndex = GetBlockIndex(chunk, ref closeChunks, ref closeBlockPosition, side);

                // 未定の場合は面なしとする。
                // デバッグ上は未定の場合に面を描画したいが、
                // 未定の場合に面を無視することで相当数の頂点を節約できる。
                if (closeBlockIndex == null) continue;

                if (closeBlockIndex != Block.EmptyIndex)
                {
                    // 隣接ブロックとの関係から対象面の要否を判定。
                    var closeBlock = chunk.Region.BlockCatalog[closeBlockIndex.Value];

                    // 半透明な連続した流体ブロックを並べる際、流体ブロック間の面は不要。
                    // ※流体ブロックは常に半透明を仮定して処理。
                    if (closeBlock.Fluid && block.Fluid) continue;

                    // 対象面が半透明ではないならば、隣接ブロックにより不可視面となるため不要。
                    if (!block.IsTranslucentTile(side)) continue;
                }

                // 環境光遮蔽を計算。
                var ambientOcclusion = CalculateAmbientOcclusion(chunk, ref closeChunks, ref closeBlockPosition, side);

                // 環境光遮蔽に基づいた頂点色を計算。
                // 一切の遮蔽が無い場合は Color.White、
                // 現在対象とする面を遮蔽するブロックが存在する程に Color.Block へ近づく。
                var vertexColor = new Color(ambientOcclusion, ambientOcclusion, ambientOcclusion);

                // メッシュを追加。
                if (block.Fluid || block.IsTranslucentTile(side))
                {
                    AddMesh(ref blockPosition, ref vertexColor, meshPart, chunk.InterChunk.Translucent);
                }
                else
                {
                    AddMesh(ref blockPosition, ref vertexColor, meshPart, chunk.InterChunk.Opaque);
                }
            }
        }

        float CalculateAmbientOcclusion(Chunk chunk, ref CloseChunks closeChunks, ref VectorI3 closeBlockPosition, CubicSide side)
        {
            const float occlusionPerFace = 1 / 5f;

            // 1 は一切遮蔽されていない状態を表す。
            float occlustion = 1;

            var mySide = side.Reverse();

            // 隣接ブロック位置の各方向に隣接ブロックが存在する場合、遮蔽有りと判定。
            foreach (var s in CubicSide.Items)
            {
                // 自身に対する方向はスキップ。
                if (mySide == s) continue;

                // 遮蔽対象のブロック位置を算出。
                var occluderBlockPosition = closeBlockPosition + s.Direction;

                // 遮蔽対象のブロックのインデックスを取得。
                var occluderBlockIndex = GetBlockIndex(chunk, ref closeChunks, ref occluderBlockPosition, s);

                // 未定と空の場合は遮蔽無し。
                if (occluderBlockIndex == null || occluderBlockIndex == Block.EmptyIndex) continue;

                // ブロック情報を取得。
                var occluderBlock = chunk.Region.BlockCatalog[occluderBlockIndex.Value];

                // 対象とする面が存在しない場合は光を遮らないものとする。
                if (occluderBlock.Mesh.MeshParts[s.Reverse()] == null) continue;

                // 流体ブロックは光を遮らないものとする。
                if (occluderBlock.Fluid) continue;

                // 遮蔽面が半透明の場合は光を遮らないものとする。
                if (occluderBlock.IsTranslucentTile(s.Reverse())) continue;

                // 遮蔽度で減算。
                occlustion -= occlusionPerFace;
            }

            return occlustion;
        }

        byte? GetBlockIndex(Chunk chunk, ref CloseChunks closeChunks, ref VectorI3 blockPosition, CubicSide side)
        {
            if (chunk.Contains(ref blockPosition))
            {
                // ブロックが対象チャンクに含まれている場合。
                return chunk[blockPosition.X, blockPosition.Y, blockPosition.Z];
            }
            else
            {
                // ブロックが隣接チャンクに含まれている場合。
                var closeChunk = closeChunks[side];
                
                // 隣接チャンクがないならば未定として null。
                if (closeChunk == null) return null;

                // 隣接チャンクにおける相対ブロック座標を算出。
                var relativeX = blockPosition.X % chunkSize.X;
                var relativeY = blockPosition.Y % chunkSize.Y;
                var relativeZ = blockPosition.Z % chunkSize.Z;
                if (relativeX < 0) relativeX += chunkSize.X;
                if (relativeY < 0) relativeY += chunkSize.Y;
                if (relativeZ < 0) relativeZ += chunkSize.Z;

                return closeChunk[relativeX, relativeY, relativeZ];
            }
        }

        void AddMesh(ref VectorI3 blockPosition, ref Color color, MeshPart source, InterChunkMesh destination)
        {
            foreach (var index in source.Indices)
                destination.AddIndex(index);

            var vertices = source.Vertices;
            for (int i = 0; i < vertices.Length; i++)
            {
                var sourceVertex = vertices[i];

                var vertex = new VertexPositionNormalColorTexture
                {
                    Position = sourceVertex.Position,
                    Normal = sourceVertex.Normal,
                    Color = color,
                    TextureCoordinate = sourceVertex.TextureCoordinate
                };

                // チャンク座標内での位置へ移動。
                vertex.Position.X += blockPosition.X;
                vertex.Position.Y += blockPosition.Y;
                vertex.Position.Z += blockPosition.Z;

                // ブロックの MeshPart はその中心に原点があるため、半ブロック移動。
                vertex.Position += blockMeshOffset;
                
                // チャンク メッシュはチャンクの中心位置を原点とするため、半チャンク移動。
                vertex.Position -= chunkMeshOffset;

                destination.AddVertex(ref vertex);
            }
        }
    }
}
