#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Landscape
{
    public sealed class FlatActiveVolume : IActiveVolume
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
        /// 領域の半径を取得します。
        /// </summary>
        public int Radius
        {
            get { return radius; }
        }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        /// <param name="radius">領域の半径。</param>
        public FlatActiveVolume(int radius)
        {
            if (radius < 0) throw new ArgumentOutOfRangeException("radius");

            this.radius = radius;

            radiusSquared = radius * radius;
        }

        // I/F
        public bool Contains(VectorI3 eyePosition, VectorI3 point)
        {
            eyePosition.Y = 0;
            point.Y = 0;

            int distanceSquared;
            VectorI3.DistanceSquared(ref eyePosition, ref point, out distanceSquared);

            return distanceSquared <= radiusSquared;
        }

        // I/F
        public void ForEach(Action<VectorI3> action)
        {
            throw new NotImplementedException();
        }
    }
}
