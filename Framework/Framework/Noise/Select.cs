#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework.Noise
{
    public sealed class Select : INoiseSource
    {
        public const float DefaultEdgeFalloff = 0;

        public const float DefaultLowerBound = -1;

        public const float DefaultUpperBound = 1;

        INoiseSource controller;

        INoiseSource source0;

        INoiseSource source1;

        float edgeFalloff = DefaultEdgeFalloff;

        float lowerBound = DefaultLowerBound;

        float upperBound = DefaultUpperBound;

        [NoiseReference]
        public INoiseSource Controller
        {
            get { return controller; }
            set { controller = value; }
        }

        [NoiseReference]
        public INoiseSource Source0
        {
            get { return source0; }
            set { source0 = value; }
        }

        [NoiseReference]
        public INoiseSource Source1
        {
            get { return source1; }
            set { source1 = value; }
        }

        [NoiseParameter]
        public float EdgeFalloff
        {
            get { return edgeFalloff; }
            set { edgeFalloff = value; }
        }

        [NoiseParameter]
        public float LowerBound
        {
            get { return lowerBound; }
            set { lowerBound = value; }
        }

        [NoiseParameter]
        public float UpperBound
        {
            get { return upperBound; }
            set { upperBound = value; }
        }

        // I/F
        public float Sample(float x, float y, float z)
        {
            var control = controller.Sample(x, y, z);
            var halfSize = (upperBound - lowerBound) * 0.5f;
            var ef = (halfSize < edgeFalloff) ? halfSize : edgeFalloff;

            if (0 < ef)
            {
                if (control < lowerBound - edgeFalloff)
                    return source0.Sample(x, y, z);
                
                if (control < lowerBound + edgeFalloff)
                {
                    var lowerCurve = lowerBound - edgeFalloff;
                    var upperCurve = lowerBound + edgeFalloff;
                    var amount = FadeCurves.SCurve3((control - lowerCurve) / (upperCurve - lowerCurve));
                    return MathHelper.Lerp(source0.Sample(x, y, z), source1.Sample(x, y, z), amount);
                }

                if (control < upperBound - edgeFalloff)
                    return source1.Sample(x, y, z);

                if (control < upperBound + edgeFalloff)
                {
                    var lowerCurve = upperBound - edgeFalloff;
                    var upperCurve = upperBound + edgeFalloff;
                    var amount = FadeCurves.SCurve3((control - lowerCurve) / (upperCurve - lowerCurve));
                    return MathHelper.Lerp(source1.Sample(x, y, z), source0.Sample(x, y, z), amount);
                }

                return source0.Sample(x, y, z);
            }

            if (control < lowerBound || upperBound < control)
                return source0.Sample(x, y, z);

            return source1.Sample(x, y, z);
        }
    }
}
