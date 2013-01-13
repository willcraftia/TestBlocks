#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Noise
{
    public sealed class Multiply : INoiseSource
    {
        INoiseSource source0;

        INoiseSource source1;

        public string Name { get; set; }

        public INoiseSource Source0
        {
            get { return source0; }
            set { source0 = value; }
        }

        public INoiseSource Source1
        {
            get { return source1; }
            set { source1 = value; }
        }

        // I/F
        public float Sample(float x, float y, float z)
        {
            return source0.Sample(x, y, z) * source1.Sample(x, y, z);
        }

        #region ToString

        public override string ToString()
        {
            return "[Name:" + (Name ?? string.Empty) + "]";
        }

        #endregion
    }
}
