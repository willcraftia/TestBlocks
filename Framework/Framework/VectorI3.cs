#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework
{
    public struct VectorI3 : IEquatable<VectorI3>
    {
        public static VectorI3 Zero
        {
            get { return new VectorI3(); }
        }

        public static VectorI3 One
        {
            get { return new VectorI3(1, 1, 1); }
        }

        public int X;

        public int Y;

        public int Z;

        public VectorI3(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(X, Y, Z);
        }

        #region Equatable

        public static bool operator ==(VectorI3 p1, VectorI3 p2)
        {
            return p1.Equals(p2);
        }

        public static bool operator !=(VectorI3 p1, VectorI3 p2)
        {
            return !p1.Equals(p2);
        }

        // I/F
        public bool Equals(VectorI3 other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;

            return Equals((VectorI3) obj);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return "[" + X + ", " + Y + ", " + Z + "]";
        }

        #endregion
    }
}
