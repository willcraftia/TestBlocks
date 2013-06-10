#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework
{
    /// <summary>
    /// 整数による境界ボックスです。
    /// </summary>
    public struct IntBoundingBox
    {
        /// <summary>
        /// 最小点。
        /// </summary>
        public IntVector3 Min;

        /// <summary>
        /// サイズ。
        /// </summary>
        public IntVector3 Size;

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        /// <param name="min">最小点。</param>
        /// <param name="size">サイズ。</param>
        public IntBoundingBox(IntVector3 min, IntVector3 size)
        {
            Min = min;
            Size = size;
        }

        public static IntBoundingBox CreateFromCenterExtents(IntVector3 center, IntVector3 extents)
        {
            IntBoundingBox result;
            CreateFromCenterExtents(ref center, ref extents, out result);
            return result;
        }

        public static void CreateFromCenterExtents(ref IntVector3 center, ref IntVector3 extents, out IntBoundingBox result)
        {
            result.Min = new IntVector3
            {
                X = center.X - extents.X,
                Y = center.Y - extents.Y,
                Z = center.Z - extents.Z,
            };
            result.Size = new IntVector3
            {
                X = extents.X * 2 + 1,
                Y = extents.Y * 2 + 1,
                Z = extents.Z * 2 + 1
            };
        }

        /// <summary>
        /// 指定の点が境界ボックスに含まれるか否かを検査します。
        /// </summary>
        /// <param name="point">点。</param>
        /// <param name="result">
        /// true (点が境界ボックスに含まれる場合)、false (それ以外の場合)。
        /// </param>
        public void Contains(ref IntVector3 point, out bool result)
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
        public bool Contains(ref IntVector3 point)
        {
            bool result;
            Contains(ref point, out result);
            return result;
        }

        /// <summary>
        /// 指定の点が境界ボックスに含まれるか否かを検査します。
        /// </summary>
        /// <param name="point">点。</param>
        /// <returns>
        /// true (点が境界ボックスに含まれる場合)、false (それ以外の場合)。
        /// </returns>
        public bool Contains(IntVector3 point)
        {
            return Contains(ref point);
        }
    }
}
