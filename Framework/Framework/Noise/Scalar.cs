#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Noise
{
    public sealed class Scalar : INoiseSource
    {
        public float Value { get; set; }

        // I/F
        public float Sample(float x, float y, float z)
        {
            return Value;
        }
    }
}
