#region Using

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

#endregion

namespace Willcraftia.Xna.Framework.Landscape
{
    internal sealed class DefaultLandscapeVolume : ILandscapeVolume
    {
        VectorI3 center;

        int radius;

        int radiusSquared;

        // I/F
        public VectorI3 Center
        {
            get { return center; }
            set { center = value; }
        }

        public int Radius
        {
            get { return radius; }
        }

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
        public void Contains(ref VectorI3 point, out bool result)
        {
            int distanceSquared;
            VectorI3.DistanceSquared(ref center, ref point, out distanceSquared);

            result = distanceSquared <= radiusSquared;
        }
    }
}
