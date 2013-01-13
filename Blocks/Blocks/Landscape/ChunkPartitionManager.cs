#region Using

using System;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Landscape;
using Willcraftia.Xna.Blocks.Models;

#endregion

namespace Willcraftia.Xna.Blocks.Landscape
{
    public sealed class ChunkPartitionManager : PartitionManager
    {
        ChunkManager chunkManager;

        RegionManager regionManager;

        public ChunkPartitionManager(Settings settings, ChunkManager chunkManager, RegionManager regionManager)
            : base(settings)
        {
            if (chunkManager == null) throw new ArgumentNullException("chunkManager");
            if (regionManager == null) throw new ArgumentNullException("regionManager");

            this.chunkManager = chunkManager;
            this.regionManager = regionManager;
        }

        protected override Partition CreatePartition()
        {
            return new ChunkPartition(chunkManager, regionManager);
        }

        protected override bool CanActivatePartition(ref VectorI3 position)
        {
            if (!regionManager.RegionExists(ref position)) return false;

            return base.CanActivatePartition(ref position);
        }

        protected override void OnClosing()
        {
            // チャンク マネージャにクローズ処理を要求。
            // チャンク マネージャは即座に更新を終えるのではなく、
            // 更新のために占有しているチャンクの解放を全て待ってから更新を終える。
            chunkManager.Close();

            base.OnClosing();
        }
    }
}
