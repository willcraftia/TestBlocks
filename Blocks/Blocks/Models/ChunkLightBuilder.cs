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
            Debug.Assert(0 < Chunk.SolidCount);

            FallSkylight();
            DiffuseSkylight();

            completed = true;
        }

        void FallSkylight()
        {
            var topNeighborChunkPosition = Chunk.Position + CubicSide.Top.Direction;
            var topNeighborChunk = manager.GetChunk(ref topNeighborChunkPosition);
            if (topNeighborChunk != null && !topNeighborChunk.LocalLightingCompleted)
                topNeighborChunk = null;

            for (int z = 0; z < manager.ChunkSize.Z; z++)
            {
                for (int x = 0; x < manager.ChunkSize.X; x++)
                {
                    Block topBlock = null;

                    // 上隣接チャンクが存在し、かつ、ローカル光レベル更新済みであり、
                    // 対象とする位置に直射日光が到達していないならば、
                    // 上隣接チャンク内で既に遮蔽状態となっている。
                    if (topNeighborChunk != null && topNeighborChunk.GetSkylightLevel(x, 0, z) < 15)
                        continue;

                    // 上から順に擬似直射日光の到達を試行。
                    for (int y = manager.ChunkSize.Y - 1; 0 <= y; y--)
                    {
                        if (topBlock == null || topBlock.Translucent)
                        {
                            // 上が空ブロック、あるいは、半透明ブロックならば、直射日光が到達。
                            Chunk.SetSkylightLevel(x, y, z, 15);

                            // 次のループのために今のブロックを上のブロックとして設定。
                            topBlock = Chunk.GetBlock(x, y, z);
                        }
                        else
                        {
                            // 上が不透明ブロックならば、以下全ての位置は遮蔽状態。
                            break;
                        }
                    }
                }
            }
        }

        void DiffuseSkylight()
        {
            for (int z = 0; z < manager.ChunkSize.Z; z++)
            {
                for (int x = 0; x < manager.ChunkSize.X; x++)
                {
                    for (int y = manager.ChunkSize.Y - 1; 0 <= y; y--)
                    {
                        var blockPosition = new VectorI3(x, y, z);
                        DiffuseSkylight(ref blockPosition);
                    }
                }
            }
        }

        void DiffuseSkylight(ref VectorI3 blockPosition)
        {
            var level = Chunk.GetSkylightLevel(ref blockPosition);

            // 1 以下はこれ以上拡散できない。
            if (level <= 1) return;

            var block = Chunk.GetBlock(ref blockPosition);

            foreach (var side in CubicSide.Items)
            {
                var neighborBlockPosition = blockPosition + side.Direction;

                // チャンク外はスキップ。
                if (neighborBlockPosition.X < 0 || manager.ChunkSize.X <= neighborBlockPosition.X ||
                    neighborBlockPosition.Y < 0 || manager.ChunkSize.Y <= neighborBlockPosition.Y ||
                    neighborBlockPosition.Z < 0 || manager.ChunkSize.Z <= neighborBlockPosition.Z)
                    continue;

                var diffuseLevel = (byte) (level - 1);

                // 光レベルの高い位置へは拡散しない。
                if (diffuseLevel <= Chunk.GetSkylightLevel(ref neighborBlockPosition)) continue;

                var neighborBlock = Chunk.GetBlock(ref neighborBlockPosition);
                if (neighborBlock == null || neighborBlock.Translucent)
                {
                    Chunk.SetSkylightLevel(ref neighborBlockPosition, diffuseLevel);

                    DiffuseSkylight(ref neighborBlockPosition);
                }
            }
        }
    }
}
