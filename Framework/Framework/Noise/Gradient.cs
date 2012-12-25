#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Noise
{
    public sealed class Gradient : INoiseSource
    {
        float x1;

        float x2;

        float y1;

        float y2;

        float z1;

        float z2;

        float lengthX;

        float lengthY;

        float lengthZ;

        float lengthSquared;

        public float X1
        {
            get { return x1; }
            set
            {
                x1 = value;
                UpdateLengthX();
            }
        }

        public float X2
        {
            get { return x2; }
            set
            {
                x2 = value;
                UpdateLengthX();
            }
        }

        public float Y1
        {
            get { return y1; }
            set
            {
                y1 = value;
                UpdateLengthY();
            }
        }

        public float Y2
        {
            get { return y2; }
            set
            {
                y2 = value;
                UpdateLengthY();
            }
        }

        public float Z1
        {
            get { return z1; }
            set
            {
                z1 = value;
                UpdateLengthZ();
            }
        }

        public float Z2
        {
            get { return z2; }
            set
            {
                z2 = value;
                UpdateLengthZ();
            }
        }

        // I/F
        public float Sample(float x, float y, float z)
        {
            var dx = x - x1;
            var dy = y - y1;
            var dz = z - z1;
            var dot = dx * lengthX + dy * lengthY + dz * lengthZ;
            dot /= lengthSquared;
            return dot;
        }

        void UpdateLengthX()
        {
            lengthX = x2 - x1;
            UpdateLength();
        }

        void UpdateLengthY()
        {
            lengthY = y2 - y1;
            UpdateLength();
        }

        void UpdateLengthZ()
        {
            lengthZ = z2 - z1;
            UpdateLength();
        }

        void UpdateLength()
        {
            lengthSquared = lengthX * lengthX + lengthY * lengthY + lengthZ * lengthZ;
        }
    }
}
