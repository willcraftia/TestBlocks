#region Using

using System;
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

        public ChunkPartition(RegionManager regionManager)
        {
            if (regionManager == null) throw new ArgumentNullException("regionManager");

            this.regionManager = regionManager;
        }

        protected override void InitializeOverride()
        {
            base.InitializeOverride();
        }

        protected override void ActivateOverride()
        {
            if (!regionManager.TryGetRegion(ref GridPosition, out region))
                throw new InvalidOperationException("The specified region can not be found.");

            region.ActivateChunk(ref GridPosition);

            base.ActivateOverride();
        }

        protected override void PassivateOverride()
        {
            region.PassivateChunk(ref GridPosition);

            region = null;

            base.PassivateOverride();
        }
    }
}
