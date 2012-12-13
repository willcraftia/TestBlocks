#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Noise
{
    public sealed class ScaleBias : INoiseSource
    {
        public const float DefaultBias = 0;

        public const float DefaultScale = 1;

        INoiseSource source;

        float bias = DefaultBias;

        float scale = DefaultScale;

        public INoiseSource Source
        {
            get { return source; }
            set { source = value; }
        }

        public float Bias
        {
            get { return bias; }
            set { bias = value; }
        }

        public float Scale
        {
            get { return scale; }
            set { scale = value; }
        }

        // I/F
        public float Sample(float x, float y, float z)
        {
            return source.Sample(x, y, z) * scale + bias;
        }
    }
}
