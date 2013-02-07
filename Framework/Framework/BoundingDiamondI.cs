#region Using

using System;
using System.Collections.Generic;

#endregion

namespace Willcraftia.Xna.Framework
{
    public struct BoundingDiamondI : IEquatable<BoundingDiamondI>
    {
        public VectorI3 Center;

        /// <summary>
        /// 中心位置からのダイアモンド領域の広がりの度合い。
        /// </summary>
        /// <remarks>
        /// 0 は中心のみ、1 は中心とその上下前後左右を含むというように、
        /// 中心からの広がりの度合いを示します。
        /// </remarks>
        public int Level;

        public BoundingDiamondI(VectorI3 center, int level)
        {
            Center = center;
            Level = level;
        }

        public bool Contains(ref VectorI3 point)
        {
            int x = MathExtension.Abs(point.X);
            int y = MathExtension.Abs(point.Y);
            int z = MathExtension.Abs(point.Z);

            return (x + y + z) <= Level;
        }

        public void ForEach(RefAction<VectorI3> action)
        {
            ForEach(action, 0);
        }

        public void ForEach(RefAction<VectorI3> action, int startLevel)
        {
            if (action == null) throw new ArgumentNullException("action");
            if (startLevel < 0 || Level < startLevel) throw new ArgumentOutOfRangeException("startLevel");
            if (Level < 0) throw new InvalidOperationException("Invalid value: Level");

            var point = new VectorI3();

            int x = 0;
            int y = 0;
            int z = 0;

            int currentLevel = startLevel;
            x = -currentLevel;

            while (currentLevel <= Level)
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
                    z = -(currentLevel - MathExtension.Abs(x) - MathExtension.Abs(y));
                }
                else
                {
                    y = -y + 1;
                    if (0 < y)
                    {
                        if (++x <= currentLevel)
                        {
                            y = MathExtension.Abs(x) - currentLevel;
                            z = 0;
                        }
                        else
                        {
                            currentLevel++;
                            x = -currentLevel;
                            y = 0;
                            z = 0;
                        }
                    }
                    else
                    {
                        z = -(currentLevel - MathExtension.Abs(x) - MathExtension.Abs(y));
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
            return Center == other.Center && Level == other.Level;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;

            return Equals((BoundingDiamondI) obj);
        }

        public override int GetHashCode()
        {
            return Center.GetHashCode() ^ Level.GetHashCode();
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return "Center = " + Center + ", Size = " + Level;
        }

        #endregion
    }
}
