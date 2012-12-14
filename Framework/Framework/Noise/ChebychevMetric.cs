#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Noise
{
    /// <summary>
    /// Calculate Chebychev distance.
    /// </summary>
    public sealed class ChebychevMetric : IMetric
    {
        public static float Function(float x, float y, float z)
        {
            x = Math.Abs(x);
            y = Math.Abs(y);
            z = Math.Abs(z);
            return Math.Max(Math.Max(x, y), z);
        }

        // I/F
        public float Calculate(float x, float y, float z)
        {
            return Function(x, y, z);
        }
    }
}
