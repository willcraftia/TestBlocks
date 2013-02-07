#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework
{
    public static class MathExtension
    {
        /// <summary>
        /// floor を計算します。
        /// </summary>
        /// <remarks>
        /// 概算であるため Math.Floor より高速ですが、それゆえに Math.Floor とは挙動が異なります。
        /// 例えば、Math.Floor(-1) == -1 であるに対し、MathExtension.Floor(-1) == -2 となります。
        /// </remarks>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int Floor(float value)
        {
            return 0 <= value ? (int) value : (int) (value - 1);
        }

        /// <summary>
        /// 絶対値を計算します。
        /// </summary>
        /// <remarks>
        /// Math.Abs よりわずかに高速ですが、
        /// int.MaxValue を指定した場合に例外を発生させずに不正な値を返します。
        /// </remarks>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int Abs(int value)
        {
            return 0 <= value ? value : -value;
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
