#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Noise
{
    /// <summary>
    /// Calculate Manhattan/Cityblock distance.
    /// </summary>
    public sealed class ManhattanMetric : IMetric
    {
        public static float Function(float x, float y, float z)
        {
            return Math.Abs(x) + Math.Abs(y) + Math.Abs(z);
        }

        // I/F
        public float Calculate(float x, float y, float z)
        {
            return Function(x, y, z);
        }
    }
}
