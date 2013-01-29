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
            get { return new VectorI3(1); }
        }

        public static VectorI3 Top
        {
            get { return new VectorI3(0, 1, 0); }
        }

        public static VectorI3 Bottom
        {
            get { return new VectorI3(0, -1, 0); }
        }

        public static VectorI3 Front
        {
            get { return new VectorI3(0, 0, 1); }
        }

        public static VectorI3 Back
        {
            get { return new VectorI3(0, 0, -1); }
        }

        public static VectorI3 Left
        {
            get { return new VectorI3(-1, 0, 0); }
        }

        public static VectorI3 Right
        {
            get { return new VectorI3(1, 0, 0); }
        }

        public int X;

        public int Y;

        public int Z;

        public VectorI3(int value)
        {
            X = value;
            Y = value;
            Z = value;
        }

        public VectorI3(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static VectorI3 Negate(VectorI3 value)
        {
            VectorI3 result;
            Negate(ref value, out result);
            return result;
        }

        public static void Negate(ref VectorI3 value, out VectorI3 result)
        {
            result = new VectorI3
            {
                X = -value.X,
                Y = -value.Y,
                Z = -value.Z
            };
        }

        public static VectorI3 Add(VectorI3 value1, VectorI3 value2)
        {
            VectorI3 result;
            Add(ref value1, ref value2, out result);
            return result;
        }

        public static void Add(ref VectorI3 value1, ref VectorI3 value2, out VectorI3 result)
        {
            result = new VectorI3
            {
                X = value1.X + value2.X,
                Y = value1.Y + value2.Y,
                Z = value1.Z + value2.Z
            };
        }

        public static VectorI3 Subtract(VectorI3 value1, VectorI3 value2)
        {
            VectorI3 result;
            Subtract(ref value1, ref value2, out result);
            return result;
        }

        public static void Subtract(ref VectorI3 value1, ref VectorI3 value2, out VectorI3 result)
        {
            result = new VectorI3
            {
                X = value1.X - value2.X,
                Y = value1.Y - value2.Y,
                Z = value1.Z - value2.Z
            };
        }

        public static VectorI3 Multiply(VectorI3 value1, VectorI3 value2)
        {
            VectorI3 result;
            Multiply(ref value1, ref value1, out result);
            return result;
        }

        public static void Multiply(ref VectorI3 value1, ref VectorI3 value2, out VectorI3 result)
        {
            result = new VectorI3
            {
                X = value1.X * value2.X,
                Y = value1.Y * value2.Y,
                Z = value1.Z * value2.Z
            };
        }

        public static VectorI3 Multiply(VectorI3 value, int scaleFactor)
        {
            VectorI3 result;
            Multiply(ref value, scaleFactor, out result);
            return result;
        }

        public static void Multiply(ref VectorI3 value, int scaleFactor, out VectorI3 result)
        {
            result = new VectorI3
            {
                X = value.X * scaleFactor,
                Y = value.Y * scaleFactor,
                Z = value.Z * scaleFactor
            };
        }

        public static VectorI3 Divide(VectorI3 value1, VectorI3 value2)
        {
            VectorI3 result;
            Divide(ref value1, ref value2, out result);
            return result;
        }

        public static void Divide(ref VectorI3 value1, ref VectorI3 value2, out VectorI3 result)
        {
            result = new VectorI3
            {
                X = value1.X / value2.X,
                Y = value1.Y / value2.Y,
                Z = value1.Z / value2.Z
            };
        }

        public static VectorI3 Divide(VectorI3 value, int divider)
        {
            VectorI3 result;
            Divide(ref value, divider, out result);
            return result;
        }

        public static void Divide(ref VectorI3 value, int divider, out VectorI3 result)
        {
            result = new VectorI3
            {
                X = value.X / divider,
                Y = value.Y / divider,
                Z = value.Z / divider
            };
        }

        public static void Distance(ref VectorI3 v1, ref VectorI3 v2, out float result)
        {
            int distanceSquared;
            DistanceSquared(ref v1, ref v2, out distanceSquared);
            result = (float) Math.Sqrt(distanceSquared);
        }

        public static void DistanceSquared(ref VectorI3 v1, ref VectorI3 v2, out int result)
        {
            var x = v2.X - v1.X;
            var y = v2.Y - v1.Y;
            var z = v2.Z - v1.Z;
            result = x * x + y * y + z * z;
        }

        public float Length()
        {
            return (float) Math.Sqrt(LengthSquared());
        }

        public int LengthSquared()
        {
            return X * X + Y * Y + Z * Z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(X, Y, Z);
        }

        #region Operators

        public static VectorI3 operator +(VectorI3 value1, VectorI3 value2)
        {
            VectorI3 result;
            Add(ref value1, ref value2, out result);
            return result;
        }

        public static VectorI3 operator -(VectorI3 value)
        {
            VectorI3 result;
            Negate(ref value, out result);
            return result;
        }

        public static VectorI3 operator -(VectorI3 value1, VectorI3 value2)
        {
            VectorI3 result;
            Subtract(ref value1, ref value2, out result);
            return result;
        }

        public static VectorI3 operator *(VectorI3 value1, VectorI3 value2)
        {
            VectorI3 result;
            Multiply(ref value1, ref value2, out result);
            return result;
        }

        public static VectorI3 operator *(int scaleFactor, VectorI3 value)
        {
            VectorI3 result;
            Multiply(ref value, scaleFactor, out result);
            return result;
        }

        public static VectorI3 operator *(VectorI3 value, int scaleFactor)
        {
            VectorI3 result;
            Multiply(ref value, scaleFactor, out result);
            return result;
        }

        public static VectorI3 operator /(VectorI3 value1, VectorI3 value2)
        {
            VectorI3 result;
            Divide(ref value1, ref value2, out result);
            return result;
        }

        public static VectorI3 operator /(VectorI3 value, int divider)
        {
            VectorI3 result;
            Divide(ref value, divider, out result);
            return result;
        }

        #endregion

        #region Equatable

        public static bool operator ==(VectorI3 value1, VectorI3 value2)
        {
            return value1.Equals(value2);
        }

        public static bool operator !=(VectorI3 value1, VectorI3 value2)
        {
            return !value1.Equals(value2);
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
