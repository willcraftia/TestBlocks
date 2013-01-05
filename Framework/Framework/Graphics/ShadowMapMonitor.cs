#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class ShadowMapMonitor
    {
        #region Split

        public sealed class Split
        {
            public int ShadowCasterCount { get; set; }

            internal Split() { }
        }

        #endregion

        Split[] split;

        public int SplitCount { get; private set; }

        public Split this[int index]
        {
            get
            {
                if (index < 0 || split.Length <= index) throw new ArgumentOutOfRangeException("index");
                return split[index];
            }
        }

        public int TotalShadowCasterCount { get; set; }

        public ShadowMapMonitor(int splitCount)
        {
            SplitCount = splitCount;

            split = new Split[SplitCount];
            for (int i = 0; i < split.Length; i++) split[i] = new Split();
        }
    }
}
