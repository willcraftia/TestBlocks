#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Noise
{
    public sealed class TranslateDomain : INoiseSource
    {
        public INoiseSource Source { get; set; }

        public INoiseSource SourceX { get; set; }

        public INoiseSource SourceY { get; set; }
        
        public INoiseSource SourceZ { get; set; }

        // I/F
        public float Sample(float x, float y, float z)
        {
            var nx = x + SourceX.Sample(x, y, z);
            var ny = y + SourceY.Sample(x, y, z);
            var nz = z + SourceZ.Sample(x, y, z);

            return Source.Sample(nx, ny, nz);
        }
    }
}
