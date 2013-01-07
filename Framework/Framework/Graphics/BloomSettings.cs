#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class BloomSettings
    {
        public const float DefaultMapScale = 0.25f;

        float mapScale = DefaultMapScale;

        public float MapScale
        {
            get { return mapScale; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");

                mapScale = value;
            }
        }

        public BlurSettings Blur { get; private set; }

        public BloomSettings()
        {
            Blur = new BlurSettings
            {
                Radius = 1,
                Amount = 4
            };
        }
    }
}
