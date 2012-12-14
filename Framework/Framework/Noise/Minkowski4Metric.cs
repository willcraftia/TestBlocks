#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Noise
{
    /// <summary>
    /// Calculate Minkowski distance with p = 4.
    /// </summary>
    public sealed class Minkowski4Metric : IMetric
    {
        public static float Function(float x, float y, float z)
        {
            x *= x;
            y *= y;
            z *= z;
            return (float) Math.Sqrt(Math.Sqrt(x * x + y * y + z * z));
        }

        // I/F
        public float Calculate(float x, float y, float z)
        {
            return Function(x, y, z);
        }
    }
}
