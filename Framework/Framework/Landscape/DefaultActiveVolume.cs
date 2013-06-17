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
    public sealed class DefaultActiveVolume : IActiveVolume
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
        public DefaultActiveVolume(int radius)
        {
            if (radius < 0) throw new ArgumentOutOfRangeException("radius");

            this.radius = radius;

            radiusSquared = radius * radius;
        }

        // I/F
        public bool Contains(IntVector3 eyePosition, IntVector3 point)
        {
            int distanceSquared;
            IntVector3.DistanceSquared(ref eyePosition, ref point, out distanceSquared);

            return distanceSquared <= radiusSquared;
        }

        // I/F
        public void ForEach(Action<IntVector3> action)
        {
            for (int z = -radius; z < radius; z++)
            {
                for (int y = -radius; y < radius; y++)
                {
                    for (int x = -radius; x < radius; x++)
                    {
                        var point = new IntVector3(x, y, z);
                        var lengthSquared = point.LengthSquared();
                        if (lengthSquared <= radiusSquared)
                            action(point);
                    }
                }
            }
        }
    }
}
