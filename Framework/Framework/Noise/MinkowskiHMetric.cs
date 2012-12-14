#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Noise
{
    /// <summary>
    /// Calculate Minkowski distance with p = 0.5.
    /// </summary>
    public sealed class MinkowskiHMetric : IMetric
    {
        public static float Function(float x, float y, float z)
        {
            float d = (float) (Math.Sqrt(Math.Abs(x)) + Math.Sqrt(Math.Abs(y)) + Math.Sqrt(Math.Abs(z)));
            return d * d;
        }

        // I/F
        public float Calculate(float x, float y, float z)
        {
            return Function(x, y, z);
        }
    }
}
