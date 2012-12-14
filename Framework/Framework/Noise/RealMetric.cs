#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Noise
{
    /// <summary>
    /// Calculate Euclidean distance.
    /// </summary>
    public sealed class RealMetric : IMetric
    {
        public static float Function(float x, float y, float z)
        {
            return (float) Math.Sqrt(SquaredMetric.Function(x, y, z));
        }

        // I/F
        public float Calculate(float x, float y, float z)
        {
            return Function(x, y, z);
        }
    }
}
