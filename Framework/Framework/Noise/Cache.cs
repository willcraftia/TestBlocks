#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Noise
{
    public sealed class Cache : INoiseSource
    {
        bool cached;

        float cacheX;

        float cacheY;

        float cacheZ;

        float cacheValue;

        public string Name { get; set; }

        public INoiseSource Source { get; set; }

        // I/F
        public float Sample(float x, float y, float z)
        {
            if (!cached || cacheX != x || cacheY != y || cacheZ != z)
            {
                cacheX = x;
                cacheY = y;
                cacheZ = z;
                cached = true;
                cacheValue = Source.Sample(x, y, z);
            }

            return cacheValue;
        }

        #region ToString

        public override string ToString()
        {
            return "[Name:" + (Name ?? string.Empty) + "]";
        }

        #endregion
    }
}
