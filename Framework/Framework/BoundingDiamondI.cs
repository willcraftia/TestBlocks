#region Using

using System;
using System.Collections.Generic;

#endregion

namespace Willcraftia.Xna.Framework
{
    public struct BoundingDiamondI : IEquatable<BoundingDiamondI>
    {
        public VectorI3 Center;

        public int Size;

        public BoundingDiamondI(VectorI3 center, int size)
        {
            Center = center;
            Size = size;
        }

        public bool Contains(ref VectorI3 point)
        {
            int x = MathExtension.Abs(point.X);
            int y = MathExtension.Abs(point.Y);
            int z = MathExtension.Abs(point.Z);

            return (x + y + z) < Size;
        }

        public void ForEach(RefAction<VectorI3> action)
        {
            ForEach(action, 0);
        }

        public void ForEach(RefAction<VectorI3> action, int startSize)
        {
            if (action == null) throw new ArgumentNullException("action");
            if (startSize < 0 || Size <= startSize) throw new ArgumentOutOfRangeException("startSize");

            var point = new VectorI3();

            int x = 0;
            int y = 0;
            int z = 0;

            int level = startSize;
            while (level < Size)
            {
                point.X = Center.X + x;
                point.Y = Center.Y + y;
                point.Z = Center.Z + z;

                action(ref point);

                if (z < 0)
                {
                    z *= -1;
                }
                else if (y < 0)
                {
                    y *= -1;
                    z = -(level - MathExtension.Abs(x) - MathExtension.Abs(y));
                }
                else
                {
                    y = -y + 1;
                    if (0 < y)
                    {
                        if (++x <= level)
                        {
                            y = MathExtension.Abs(x) - level;
                            z = 0;
                        }
                        else
                        {
                            level++;
                            x = -level;
                            y = 0;
                            z = 0;
                        }
                    }
                    else
                    {
                        z = -(level - MathExtension.Abs(x) - MathExtension.Abs(y));
                    }
                }
            }
        }

        #region Equatable

        public static bool operator ==(BoundingDiamondI value1, BoundingDiamondI value2)
        {
            return value1.Equals(value2);
        }

        public static bool operator !=(BoundingDiamondI value1, BoundingDiamondI value2)
        {
            return !value1.Equals(value2);
        }

        // I/F
        public bool Equals(BoundingDiamondI other)
        {
            return Center == other.Center && Size == other.Size;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;

            return Equals((BoundingDiamondI) obj);
        }

        public override int GetHashCode()
        {
            return Center.GetHashCode() ^ Size.GetHashCode();
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return "Center = " + Center + ", Size = " + Size;
        }

        #endregion
    }
}
