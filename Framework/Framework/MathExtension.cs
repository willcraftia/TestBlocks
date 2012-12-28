#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework
{
    public static class MathExtension
    {
        public static int Floor(float value)
        {
            // Faster than using (int) Math.Floor(x).
            return 0 <= value ? (int) value : (int) value - 1;
        }

        public static float Saturate(float value)
        {
            if (1 < value) return 1;
            if (value < 0) return 0;
            return value;
        }

        public static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (max < value) return max;
            return value;
        }

        public static float CalculateGaussian(float sigma, float n)
        {
            //
            // REFERENCE: sigmaRoot = (float) Math.Sqrt(2.0f * Math.PI * sigma * sigma)
            //
            var twoSigmaSquare = 2.0f * sigma * sigma;
            var sigmaRoot = (float) Math.Sqrt(Math.PI * twoSigmaSquare);
            return (float) Math.Exp(-(n * n) / twoSigmaSquare) / sigmaRoot;
        }
    }
}
