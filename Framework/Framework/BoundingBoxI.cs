#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework
{
    /// <summary>
    /// 整数による境界ボックスです。
    /// </summary>
    public struct BoundingBoxI
    {
        /// <summary>
        /// 最小点。
        /// </summary>
        public VectorI3 Min;

        /// <summary>
        /// サイズ。
        /// </summary>
        public VectorI3 Size;

        /// <summary>
        /// 境界ボックスに含まれる最大点を取得します。
        /// この最大点は (Min + Size - VectorI3(1)) です。
        /// </summary>
        public VectorI3 Max
        {
            get
            {
                VectorI3 result;
                VectorI3.Add(ref Min, ref Size, out result);
                result.X -= 1;
                result.Y -= 1;
                result.Z -= 1;
                return result;
            }
        }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        /// <param name="min">最小点。</param>
        /// <param name="size">サイズ。</param>
        public BoundingBoxI(VectorI3 min, VectorI3 size)
        {
            Min = min;
            Size = size;
        }

        /// <summary>
        /// 指定の点が境界ボックスに含まれるか否かを検査します。
        /// </summary>
        /// <param name="point">点。</param>
        /// <param name="result">
        /// true (点が境界ボックスに含まれる場合)、false (それ以外の場合)。
        /// </param>
        public void Contains(ref VectorI3 point, out bool result)
        {
            if (point.X < Min.X || point.Y < Min.Y || point.Z < Min.Z ||
                (point.X + Size.X) <= point.X || (point.Y + Size.Y) <= point.Y || (point.Z + Size.Z) <= point.Z)
            {
                result = false;
            }
            else
            {
                result = true;
            }
        }

        /// <summary>
        /// 指定の点が境界ボックスに含まれるか否かを検査します。
        /// </summary>
        /// <param name="point">点。</param>
        /// <returns>
        /// true (点が境界ボックスに含まれる場合)、false (それ以外の場合)。
        /// </returns>
        public bool Contains(VectorI3 point)
        {
            bool result;
            Contains(ref point, out result);
            return result;
        }
    }
}
