#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Landscape
{
    public sealed class FlatLandscapeVolume : ILandscapeVolume
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
            set
            {
                center = value;
                center.Y = 0;
            }
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
        public FlatLandscapeVolume(VectorI3 center, int radius)
        {
            if (radius < 0) throw new ArgumentOutOfRangeException("radius");

            this.center = center;
            this.center.Y = 0;
            this.radius = radius;

            radiusSquared = radius * radius;
        }

        // I/F
        public VectorI3[] GetPoints()
        {
            int size = 0;
            for (int z = -radius; z < radius; z++)
            {
                for (int x = -radius; x < radius; x++)
                {
                    var lengthSquared = new VectorI3(x, 0, z).LengthSquared();
                    if (lengthSquared <= radiusSquared)
                        size++;
                }
            }

            var points = new VectorI3[size];

            int index = 0;
            for (int z = -radius; z < radius; z++)
            {
                for (int x = -radius; x < radius; x++)
                {
                    var point = new VectorI3(x, 0, z);
                    var lengthSquared = point.LengthSquared();
                    if (lengthSquared <= radiusSquared)
                        points[index++] = point + Center;
                }
            }

            return points;
        }

        // I/F
        public void Contains(ref VectorI3 point, out bool result)
        {
            var flatPoint = point;
            flatPoint.Y = 0;

            int distanceSquared;
            VectorI3.DistanceSquared(ref center, ref point, out distanceSquared);

            result = distanceSquared <= radiusSquared;
        }
    }
}
