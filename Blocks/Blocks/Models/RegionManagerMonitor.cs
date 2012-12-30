#region Using

using System;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class RegionManagerMonitor
    {
        public event EventHandler BeginUpdate = delegate { };

        public event EventHandler EndUpdate = delegate { };

        RegionManager regionManager;

        public RegionManagerMonitor(RegionManager regionManager)
        {
            if (regionManager == null) throw new ArgumentNullException("regionManager");

            this.regionManager = regionManager;
        }

        internal void OnBeginUpdate()
        {
            BeginUpdate(regionManager, EventArgs.Empty);
        }

        internal void OnEndUpdate()
        {
            EndUpdate(regionManager, EventArgs.Empty);
        }
    }
}
