#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Noise
{
    public sealed class Const : INoiseSource
    {
        public string Name { get; set; }

        public float Value { get; set; }

        // I/F
        public float Sample(float x, float y, float z)
        {
            return Value;
        }

        #region ToString

        public override string ToString()
        {
            return "[Name:" + (Name ?? string.Empty) + "]";
        }

        #endregion
    }
}
