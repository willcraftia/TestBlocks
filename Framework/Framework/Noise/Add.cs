#region Using

using System;
using Willcraftia.Xna.Framework.Component;

#endregion

namespace Willcraftia.Xna.Framework.Noise
{
    [Component]
    public sealed class Add : INoiseSource
    {
        INoiseSource source0;

        INoiseSource source1;

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
            return source0.Sample(x, y, z) + source1.Sample(x, y, z);
        }
    }
}
