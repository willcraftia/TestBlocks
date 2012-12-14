#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework.Noise
{
    public sealed class SimpleVoronoi : Voronoi
    {
        int positionIndex;

        public int PositionIndex
        {
            get { return positionIndex; }
            set
            {
                if (value < 0 || 3 < value) throw new ArgumentOutOfRangeException("value");
                positionIndex = value;
            }
        }

        protected override float Calculate(float x, float y, float z)
        {
            VoronoiDistance distance;
            CalculateDistance(x, y, z, out distance);

            Vector3 position;
            distance.GetPosition(positionIndex, out position);

            var xci = MathExtension.Floor(position.X);
            var yci = MathExtension.Floor(position.Y);
            var zci = MathExtension.Floor(position.Z);

            float value = !DistanceEnabled ? 0 : distance.GetDistance(positionIndex);
            return value + Displacement * GetPosition(xci, yci, zci, 0);
        }
    }
}
