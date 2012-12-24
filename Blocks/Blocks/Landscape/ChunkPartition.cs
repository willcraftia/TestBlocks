#region Using

using System;
using System.Diagnostics;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Landscape;
using Willcraftia.Xna.Blocks.Models;

#endregion

namespace Willcraftia.Xna.Blocks.Landscape
{
    //
    // このクラスは、Partition から Chunk へのブリッジとして機能する。
    // Partition は複数ある Region のうちの一つに含まれる Chunk を一つ管理する。
    //
    // 実際の Chunk の管理は、一元管理のために、対象とする Region の ChunkManager で行う。
    // ChunkManager で Chunk を一元管理する理由は、非同期操作に対する同期処理を、
    // Region および ChunkManager で集約するためである。
    //
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

        protected override void InitializeOverride()
        {
            base.InitializeOverride();
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
            chunk.OnNeighborActivated(side);

            base.OnNeighborActivated(neighbor, side);
        }

        public override void OnNeighborPassivated(Partition neighbor, CubicSide side)
        {
            chunk.OnNeighborPassivated(side);

            base.OnNeighborPassivated(neighbor, side);
        }
    }
}
