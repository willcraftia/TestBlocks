#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Noise
{
    /// <summary>
    /// Calculate Euclidean distance squared.
    /// </summary>
    public sealed class SquaredMetric : IMetric
    {
        public static float Function(float x, float y, float z)
        {
            return x * x + y * y + z * z;
        }

        // I/F
        public float Calculate(float x, float y, float z)
        {
            return Function(x, y, z);
        }
    }
}
