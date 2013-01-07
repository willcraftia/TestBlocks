#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class BloomSettings
    {
        public const float DefaultMapScale = 0.25f;

        public const float DefaultThreshold = 0.25f;

        public const float DefaultBloomIntensity = 1.25f;

        public const float DefaultBaseIntensity = 1;

        public const float DefaultBloomSaturation = 1;

        public const float DefaultBaseSaturation = 1;

        float mapScale = DefaultMapScale;

        float threshold = DefaultThreshold;

        float bloomIntensity = DefaultBloomIntensity;

        float baseIntensity = DefaultBaseIntensity;

        float bloomSaturation = DefaultBloomSaturation;

        float baseSaturation = DefaultBaseSaturation;

        public float MapScale
        {
            get { return mapScale; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");

                mapScale = value;
            }
        }

        public float Threshold
        {
            get { return threshold; }
            set
            {
                if (value < 0 || 1 < value) throw new ArgumentOutOfRangeException("value");

                threshold = value;
            }
        }

        public float BloomIntensity
        {
            get { return bloomIntensity; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");

                bloomIntensity = value;
            }
        }

        public float BaseIntensity
        {
            get { return baseIntensity; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");

                baseIntensity = value;
            }
        }

        public float BloomSaturation
        {
            get { return bloomSaturation; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");

                bloomSaturation = value;
            }
        }

        public float BaseSaturation
        {
            get { return baseSaturation; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");

                baseSaturation = value;
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
