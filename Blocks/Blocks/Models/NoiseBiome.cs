#region Using

using System;
using Willcraftia.Xna.Framework.Noise;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class NoiseBiome : Biome
    {
        INoiseSource noise;

        SumFractal sumFractal = new SumFractal();

        public string NoiseName { get; set; }

        public override BiomeElement this[int x, int z]
        {
            get { throw new NotImplementedException(); }
        }

        public override void Initialize()
        {
            
        }
    }
}
