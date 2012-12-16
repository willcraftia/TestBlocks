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
        // TODO
        public const int InitialPoolCapacity = 0;

        RegionManager regionManager;

        public ChunkPartitionManager(RegionManager regionManager)
            : base(RegionManager.ChunkSize.ToVector3(), InitialPoolCapacity)
        {
            if (regionManager == null) throw new ArgumentNullException("regionManager");
            this.regionManager = regionManager;
        }

        protected override Partition CreatePartition()
        {
            return new ChunkPartition(regionManager);
        }

        protected override bool CanActivatePartition(ref VectorI3 gridPosition)
        {
            if (!regionManager.RegionExists(ref gridPosition)) return false;

            return base.CanActivatePartition(ref gridPosition);
        }
    }
}
