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
        /// インスタンスを生成します。
        /// </summary>
        /// <param name="min">最小点。</param>
        /// <param name="size">サイズ。</param>
        public BoundingBoxI(VectorI3 min, VectorI3 size)
        {
            Min = min;
            Size = size;
        }

        public static BoundingBoxI CreateFromCenterExtents(VectorI3 center, VectorI3 extents)
        {
            BoundingBoxI result;
            CreateFromCenterExtents(ref center, ref extents, out result);
            return result;
        }

        public static void CreateFromCenterExtents(ref VectorI3 center, ref VectorI3 extents, out BoundingBoxI result)
        {
            result.Min = new VectorI3
            {
                X = center.X - extents.X,
                Y = center.Y - extents.Y,
                Z = center.Z - extents.Z,
            };
            result.Size = new VectorI3
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
