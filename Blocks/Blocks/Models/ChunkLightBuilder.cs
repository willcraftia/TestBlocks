#region Using

using System;
using System.Diagnostics;
using Willcraftia.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class ChunkLightBuilder
    {
        ChunkManager manager;

        volatile bool completed;

        public Chunk Chunk { get; internal set; }

        public bool Completed
        {
            get { return completed; }
            set { completed = value; }
        }

        public Action ExecuteAction { get; private set; }

        public ChunkLightBuilder(ChunkManager manager)
        {
            if (manager == null) throw new ArgumentNullException("manager");

            this.manager = manager;
            ExecuteAction = new Action(Execute);
        }

        public void Execute()
        {
            Debug.Assert(!completed);
            Debug.Assert(Chunk != null);

            // 空チャンクならば頂点が存在しないため、天空光の処理そのものが不要。
            if (Chunk.SolidCount == 0) return;

            var topNeighborPosition = Chunk.Position + CubicSide.Top.Direction;
            var topNeighbor = manager.GetChunk(ref topNeighborPosition);

            FallSkylight(topNeighbor);
            PropagateSkylight();

            completed = true;
        }

        void FallSkylight(Chunk topNeighbor)
        {
            for (int z = 0; z < manager.ChunkSize.Z; z++)
            {
                for (int x = 0; x < manager.ChunkSize.X; x++)
                {
                    Block topBlock = null;

                    if (topNeighbor != null)
                    {
                        // 上にチャンクがあるならば、その情報を参照。
                        if (topNeighbor.GetSkylightLevel(x, 0, z) < 15)
                        {
                            // 既に減衰した光量の場合、上のチャンク内のどこかで遮蔽されているため、
                            // 直接的な天空光の到達はこれ以上ない。
                            continue;
                        }

                        topBlock = topNeighbor.GetBlock(x, 0, z);
                    }

                    // 上から順に天空光の到達を試行。
                    for (int y = manager.ChunkSize.Y - 1; 0 <= y; y--)
                    {
                        if (topBlock == null || topBlock.Translucent)
                        {
                            // 上が空ブロック、あるいは、半透明ブロックならば、光は減衰せずに伝播。
                            Chunk.SetSkylightLevel(x, y, z, 15);

                            // 次のループのために今のブロックを上のブロックとして設定。
                            topBlock = Chunk.GetBlock(x, y, z);
                        }
                        else
                        {
                            // 上が不透明ブロックならば、それより下にある全部ブロック位置で光の伝播をスキップ。
                            break;
                        }
                    }
                }
            }
        }

        void PropagateSkylight()
        {
            PropagateSkylightFromTopNeighbor();
            PropagateSkylightFromBottomNeighbor();
            PropagateSkylightFromFrontNeighbor();
            PropagateSkylightFromBackNeighbor();
            PropagateSkylightFromLeftNeighbor();
            PropagateSkylightFromRightNeighbor();

            for (int z = 0; z < manager.ChunkSize.Z; z++)
            {
                for (int x = 0; x < manager.ChunkSize.X; x++)
                {
                    for (int y = manager.ChunkSize.Y - 1; 0 <= y; y--)
                    {
                        var blockPosition = new VectorI3(x, y, z);
                        PropagateSkylight(ref blockPosition);
                    }
                }
            }
        }

        void PropagateSkylightFromTopNeighbor()
        {
            var neighborChunkPosition = Chunk.Position + CubicSide.Top.Direction;
            var neighborChunk = manager.GetChunk(ref neighborChunkPosition);
            if (neighborChunk == null) return;

            for (int z = 0; z < manager.ChunkSize.Z; z++)
            {
                for (int x = 0; x < manager.ChunkSize.X; x++)
                {
                    var skyLight = neighborChunk.GetSkylightLevel(x, 0, z);
                    if (skyLight <= 1) return;

                    if (skyLight <= Chunk.GetSkylightLevel(x, manager.ChunkSize.Y - 1, z)) continue;

                    var block = Chunk.GetBlock(x, manager.ChunkSize.Y - 1, z);
                    if (block == null || block.Translucent)
                    {
                        Chunk.SetSkylightLevel(x, manager.ChunkSize.Y - 1, z, (byte) (skyLight - 1));
                    }
                }
            }
        }

        void PropagateSkylightFromBottomNeighbor()
        {
            var neighborChunkPosition = Chunk.Position + CubicSide.Bottom.Direction;
            var neighborChunk = manager.GetChunk(ref neighborChunkPosition);
            if (neighborChunk == null) return;

            for (int z = 0; z < manager.ChunkSize.Z; z++)
            {
                for (int x = 0; x < manager.ChunkSize.X; x++)
                {
                    var skyLight = neighborChunk.GetSkylightLevel(x, manager.ChunkSize.Y - 1, z);
                    if (skyLight <= 1) return;

                    if (skyLight <= Chunk.GetSkylightLevel(x, 0, z)) continue;

                    var block = Chunk.GetBlock(x, 0, z);
                    if (block == null || block.Translucent)
                    {
                        Chunk.SetSkylightLevel(x, 0, z, (byte) (skyLight - 1));
                    }
                }
            }
        }

        void PropagateSkylightFromFrontNeighbor()
        {
            var neighborChunkPosition = Chunk.Position + CubicSide.Front.Direction;
            var neighborChunk = manager.GetChunk(ref neighborChunkPosition);
            if (neighborChunk == null) return;

            for (int y = 0; y < manager.ChunkSize.Y; y++)
            {
                for (int x = 0; x < manager.ChunkSize.X; x++)
                {
                    var skyLight = neighborChunk.GetSkylightLevel(x, y, 0);
                    if (skyLight <= 1) return;

                    if (skyLight <= Chunk.GetSkylightLevel(x, y, manager.ChunkSize.Z - 1)) continue;

                    var block = Chunk.GetBlock(x, y, manager.ChunkSize.Z - 1);
                    if (block == null || block.Translucent)
                    {
                        Chunk.SetSkylightLevel(x, y, manager.ChunkSize.Z - 1, (byte) (skyLight - 1));
                    }
                }
            }
        }

        void PropagateSkylightFromBackNeighbor()
        {
            var neighborChunkPosition = Chunk.Position + CubicSide.Back.Direction;
            var neighborChunk = manager.GetChunk(ref neighborChunkPosition);
            if (neighborChunk == null) return;

            for (int y = 0; y < manager.ChunkSize.Y; y++)
            {
                for (int x = 0; x < manager.ChunkSize.X; x++)
                {
                    var skyLight = neighborChunk.GetSkylightLevel(x, y, manager.ChunkSize.Z - 1);
                    if (skyLight <= 1) return;

                    if (skyLight <= Chunk.GetSkylightLevel(x, y, 0)) continue;

                    var block = Chunk.GetBlock(x, y, 0);
                    if (block == null || block.Translucent)
                    {
                        Chunk.SetSkylightLevel(x, y, 0, (byte) (skyLight - 1));
                    }
                }
            }
        }

        void PropagateSkylightFromLeftNeighbor()
        {
            var neighborChunkPosition = Chunk.Position + CubicSide.Left.Direction;
            var neighborChunk = manager.GetChunk(ref neighborChunkPosition);
            if (neighborChunk == null) return;

            for (int z = 0; z < manager.ChunkSize.Z; z++)
            {
                for (int y = 0; y < manager.ChunkSize.Y; y++)
                {
                    var skyLight = neighborChunk.GetSkylightLevel(manager.ChunkSize.X - 1, y, z);
                    if (skyLight <= 1) return;

                    if (skyLight <= Chunk.GetSkylightLevel(0, y, z)) continue;

                    var block = Chunk.GetBlock(0, y, z);
                    if (block == null || block.Translucent)
                    {
                        Chunk.SetSkylightLevel(0, y, z, (byte) (skyLight - 1));
                    }
                }
            }
        }

        void PropagateSkylightFromRightNeighbor()
        {
            var neighborChunkPosition = Chunk.Position + CubicSide.Right.Direction;
            var neighborChunk = manager.GetChunk(ref neighborChunkPosition);
            if (neighborChunk == null) return;

            for (int z = 0; z < manager.ChunkSize.Z; z++)
            {
                for (int y = 0; y < manager.ChunkSize.Y; y++)
                {
                    var skyLight = neighborChunk.GetSkylightLevel(0, y, z);
                    if (skyLight <= 1) return;

                    if (skyLight <= Chunk.GetSkylightLevel(manager.ChunkSize.X - 1, y, z)) continue;

                    var block = Chunk.GetBlock(manager.ChunkSize.X - 1, y, z);
                    if (block == null || block.Translucent)
                    {
                        Chunk.SetSkylightLevel(manager.ChunkSize.X - 1, y, z, (byte) (skyLight - 1));
                    }
                }
            }
        }

        void PropagateSkylight(ref VectorI3 blockPosition)
        {
            var skyLight = Chunk.GetSkylightLevel(ref blockPosition);

            // 1 以下はこれ以上伝搬できない。
            if (skyLight <= 1) return;

            var block = Chunk.GetBlock(ref blockPosition);

            foreach (var side in CubicSide.Items)
            {
                var neighborBlockPosition = blockPosition + side.Direction;

                // チャンクの外になる場合はスキップ。
                if (neighborBlockPosition.X < 0 || manager.ChunkSize.X <= neighborBlockPosition.X ||
                    neighborBlockPosition.Y < 0 || manager.ChunkSize.Y <= neighborBlockPosition.Y ||
                    neighborBlockPosition.Z < 0 || manager.ChunkSize.Z <= neighborBlockPosition.Z)
                    continue;

                // より光量の多い位置へは伝播する必要がない。
                if (skyLight <= Chunk.GetSkylightLevel(ref neighborBlockPosition)) continue;

                var neighborBlock = Chunk.GetBlock(ref neighborBlockPosition);
                if (neighborBlock == null || neighborBlock.Translucent)
                {
                    Chunk.SetSkylightLevel(ref neighborBlockPosition, (byte) (skyLight - 1));

                    PropagateSkylight(ref neighborBlockPosition);
                }
            }
        }
    }
}
