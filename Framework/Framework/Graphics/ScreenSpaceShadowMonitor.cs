#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class ScreenSpaceShadowMonitor
    {
        public event EventHandler BeginFilter = delegate { };

        public event EventHandler EndFilter = delegate { };

        ScreenSpaceShadow screenSpaceShadow;

        public ScreenSpaceShadowMonitor(ScreenSpaceShadow screenSpaceShadow)
        {
            if (screenSpaceShadow == null) throw new ArgumentNullException("screenSpaceShadow");

            this.screenSpaceShadow = screenSpaceShadow;
        }

        internal void OnBeginFilter()
        {
            BeginFilter(screenSpaceShadow, EventArgs.Empty);
        }

        internal void OnEndFilter()
        {
            EndFilter(screenSpaceShadow, EventArgs.Empty);
        }
    }
}
