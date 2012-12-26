#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Noise
{
    public sealed class Displace : INoiseSource
    {
        public string Name { get; set; }

        public INoiseSource Source { get; set; }

        public INoiseSource DisplaceX { get; set; }

        public INoiseSource DisplaceY { get; set; }
        
        public INoiseSource DisplaceZ { get; set; }

        // I/F
        public float Sample(float x, float y, float z)
        {
            var nx = x + DisplaceX.Sample(x, y, z);
            var ny = y + DisplaceY.Sample(x, y, z);
            var nz = z + DisplaceZ.Sample(x, y, z);

            return Source.Sample(nx, ny, nz);
        }

        #region ToString

        public override string ToString()
        {
            return "[Name:" + (Name ?? string.Empty) + "]";
        }

        #endregion
    }
}
