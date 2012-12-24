#region Using

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

#endregion

namespace Willcraftia.Xna.Framework.Landscape
{
    public struct PartitionSpaceBounds
    {
        public VectorI3 Center;

        public int Radius;

        public PartitionSpaceBounds(VectorI3 center, int radius)
        {
            if (radius < 0) throw new ArgumentOutOfRangeException("radius");

            Center = center;
            Radius = radius;
        }

        public VectorI3[] GetPoints()
        {
            var radiusSquared = Radius * Radius;

            int size = 0;
            for (int z = -Radius; z < Radius; z++)
            {
                for (int y = -Radius; y < Radius; y++)
                {
                    for (int x = -Radius; x < Radius; x++)
                    {
                        var lengthSquared = new VectorI3(x, y, z).LengthSquared();
                        if (lengthSquared <= radiusSquared)
                            size++;
                    }
                }
            }

            var points = new VectorI3[size];

            int index = 0;
            for (int z = -Radius; z < Radius; z++)
            {
                for (int y = -Radius; y < Radius; y++)
                {
                    for (int x = -Radius; x < Radius; x++)
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

        public bool Contains(VectorI3 point)
        {
            bool result;
            Contains(ref point, out result);
            return result;
        }

        public void Contains(ref VectorI3 point, out bool result)
        {
            int distanceSquared;
            VectorI3.DistanceSquared(ref Center, ref point, out distanceSquared);

            result = distanceSquared <= (Radius * Radius);
        }
    }
}
