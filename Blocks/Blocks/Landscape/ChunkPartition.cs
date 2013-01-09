#region Using

using System;
using System.Diagnostics;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Landscape;
using Willcraftia.Xna.Blocks.Models;

#endregion

namespace Willcraftia.Xna.Blocks.Landscape
{
    /// <summary>
    /// パーティションにチャンクを対応させるクラスです。
    /// </summary>
    public sealed class ChunkPartition : Partition
    {
        RegionManager regionManager;

        Region region;

        Chunk chunk;

        public ChunkPartition(RegionManager regionManager)
        {
            if (regionManager == null) throw new ArgumentNullException("regionManager");

            this.regionManager = regionManager;
        }

        protected override bool ActivateOverride()
        {
            var position = Position;

            if (!regionManager.TryGetRegion(ref position, out region))
                throw new InvalidOperationException("Region not found: " + position);

            chunk = region.ActivateChunk(ref position);
            if (chunk == null) return false;

            return base.ActivateOverride();
        }

        protected override bool PassivateOverride()
        {
            Debug.Assert(chunk != null);

            if (!region.PassivateChunk(chunk)) return false;

            chunk = null;
            region = null;

            return base.PassivateOverride();
        }

        public override void OnNeighborActivated(Partition neighbor, CubicSide side)
        {
            // 非アクティブ化によりチャンクが null の場合あり。
            if (chunk != null) chunk.OnNeighborActivated(side);

            base.OnNeighborActivated(neighbor, side);
        }

        public override void OnNeighborPassivated(Partition neighbor, CubicSide side)
        {
            // 非アクティブ化によりチャンクが null の場合あり。
            if (chunk != null) chunk.OnNeighborPassivated(side);

            base.OnNeighborPassivated(neighbor, side);
        }
    }
}
