#region Using

using System;
using Willcraftia.Xna.Framework.Noise;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class GradientDensity : INoiseSource
    {
        float minY;

        float maxY = 1;

        float range = 1;

        float inverseRange = 1;

        public float MinY
        {
            get { return minY; }
            set
            {
                minY = value;
                UpdateRange();
            }
        }

        public float MaxY
        {
            get { return maxY; }
            set
            {
                maxY = value;
                UpdateRange();
            }
        }

        public float Sample(float x, float y, float z)
        {
            if (y <= minY) return 1;
            if (maxY <= y) return 0;

            return 1 - (y - minY) * inverseRange;
        }

        void UpdateRange()
        {
            range = maxY - minY;
            inverseRange = 1 / range;
        }
    }
}
