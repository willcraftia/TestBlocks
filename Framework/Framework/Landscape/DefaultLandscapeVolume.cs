#region Using

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

#endregion

namespace Willcraftia.Xna.Framework.Landscape
{
    /// <summary>
    /// アクティブ パーティション領域のデフォルト実装です。
    /// この実装では、円形のような形状で領域を管理します。
    /// </summary>
    public sealed class DefaultLandscapeVolume : ILandscapeVolume
    {
        /// <summary>
        /// 領域の中心位置。
        /// </summary>
        VectorI3 center;

        /// <summary>
        /// 領域の半径。
        /// </summary>
        int radius;

        /// <summary>
        /// radius * radius。
        /// </summary>
        int radiusSquared;

        // I/F
        public VectorI3 Center
        {
            get { return center; }
            set { center = value; }
        }

        /// <summary>
        /// 領域の半径を取得します。
        /// </summary>
        public int Radius
        {
            get { return radius; }
        }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        /// <param name="center">領域の中心位置。</param>
        /// <param name="radius">領域の半径。</param>
        public DefaultLandscapeVolume(VectorI3 center, int radius)
        {
            if (radius < 0) throw new ArgumentOutOfRangeException("radius");

            this.center = center;
            this.radius = radius;

            radiusSquared = radius * radius;
        }

        // I/F
        public VectorI3[] GetPoints()
        {
            int size = 0;
            for (int z = -radius; z < radius; z++)
            {
                for (int y = -radius; y < radius; y++)
                {
                    for (int x = -radius; x < radius; x++)
                    {
                        var lengthSquared = new VectorI3(x, y, z).LengthSquared();
                        if (lengthSquared <= radiusSquared)
                            size++;
                    }
                }
            }

            var points = new VectorI3[size];

            int index = 0;
            for (int z = -radius; z < radius; z++)
            {
                for (int y = -radius; y < radius; y++)
                {
                    for (int x = -radius; x < radius; x++)
                    {
                        var point = new VectorI3(x, y, z);
                        var lengthSquared = point.LengthSquared();
                        if (lengthSquared <= radiusSquared)
                            points[index++] = point + Center;
                    }
                }
            }

            return points;
        }

        // I/F
        public bool Contains(VectorI3 point)
        {
            int distanceSquared;
            VectorI3.DistanceSquared(ref center, ref point, out distanceSquared);

            return distanceSquared <= radiusSquared;
        }
    }
}
