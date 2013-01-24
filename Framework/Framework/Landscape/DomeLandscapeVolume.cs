#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Landscape
{
    public sealed class DomeLandscapeVolume : ILandscapeVolume
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

        /// <summary>
        /// Y 軸方向の下限。
        /// </summary>
        int minY;

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
        /// Y 軸方向の下限を取得します。
        /// </summary>
        public int MinY
        {
            get { return minY; }
        }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        /// <param name="center">領域の中心位置。</param>
        /// <param name="radius">領域の半径。</param>
        /// <param name="minY">Y 軸方向の下限。</param>
        public DomeLandscapeVolume(VectorI3 center, int radius, int minY)
        {
            if (radius < 0) throw new ArgumentOutOfRangeException("radius");
            if (minY < -radius) throw new ArgumentOutOfRangeException("minY");

            this.center = center;
            this.radius = radius;
            this.minY = minY;

            radiusSquared = radius * radius;
        }

        // I/F
        public VectorI3[] GetPoints()
        {
            int size = 0;
            for (int z = -radius; z < radius; z++)
            {
                for (int y = minY; y < radius; y++)
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
                for (int y = minY; y < radius; y++)
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
            if (point.Y < minY + center.Y)
            {
                return false;
            }

            int distanceSquared;
            VectorI3.DistanceSquared(ref center, ref point, out distanceSquared);

            return distanceSquared <= radiusSquared;
        }

        // I/F
        public void ForEach(Action<VectorI3> action)
        {
            throw new NotImplementedException();
        }

        // I/F
        public void ForEach(Func<VectorI3, bool> function)
        {
            throw new NotImplementedException();
        }
    }
}
