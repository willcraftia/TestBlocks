#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework.Noise
{
    public class Difference10Voronoi : Voronoi
    {
        protected override float Calculate(float x, float y, float z)
        {
            VoronoiDistance distance;
            CalculateDistance(x, y, z, out distance);

            Vector3 position0;
            Vector3 position1;
            distance.GetPosition(0, out position0);
            distance.GetPosition(1, out position1);

            var position = position1 - position0;
            position *= 0.5f;

            var xci = MathExtension.Floor(position.X);
            var yci = MathExtension.Floor(position.Y);
            var zci = MathExtension.Floor(position.Z);

            float value = !DistanceEnabled ? 0 : distance.GetDistance(1) - distance.GetDistance(0);
            return value + Displacement * GetPosition(xci, yci, zci, 0);
        }
    }
}
