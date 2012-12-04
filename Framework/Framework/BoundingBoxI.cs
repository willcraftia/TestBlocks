#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework
{
    public struct BoundingBoxI
    {
        public VectorI3 Min;

        public VectorI3 Max;

        public BoundingBoxI(VectorI3 min, VectorI3 max)
        {
            Min = min;
            Max = max;
        }

        public void Contains(ref VectorI3 point, out ContainmentType result)
        {
            if (point.X < Min.X || point.Y < Min.Y || point.Z < Min.Z ||
                Max.X < point.X || Max.Y < point.Y || Max.Z < point.Z)
            {
                result = ContainmentType.Disjoint;
            }
            else
            {
                result = ContainmentType.Contains;
            }
        }

        public ContainmentType Contains(VectorI3 point)
        {
            ContainmentType result;
            Contains(ref point, out result);
            return result;
        }
    }
}
