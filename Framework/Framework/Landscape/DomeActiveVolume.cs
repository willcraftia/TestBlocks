#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Landscape
{
    public sealed class DomeActiveVolume : IActiveVolume
    {
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
        /// <param name="radius">領域の半径。</param>
        /// <param name="minY">Y 軸方向の下限。</param>
        public DomeActiveVolume(int radius, int minY)
        {
            if (radius < 0) throw new ArgumentOutOfRangeException("radius");
            if (minY < -radius) throw new ArgumentOutOfRangeException("minY");

            this.radius = radius;
            this.minY = minY;

            radiusSquared = radius * radius;
        }

        // I/F
        public bool Contains(ref VectorI3 eyePosition, ref VectorI3 point)
        {
            if (point.Y < minY + eyePosition.Y)
            {
                return false;
            }

            int distanceSquared;
            VectorI3.DistanceSquared(ref eyePosition, ref point, out distanceSquared);

            return distanceSquared <= radiusSquared;
        }

        // I/F
        public void ForEach(RefAction<VectorI3> action)
        {
            throw new NotImplementedException();
        }
    }
}
