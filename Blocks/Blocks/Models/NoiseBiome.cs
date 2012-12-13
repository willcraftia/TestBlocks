#region Using

using System;
using Willcraftia.Xna.Framework.Noise;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class NoiseBiome : Biome
    {
        public override BiomeElement this[int x, int z]
        {
            get { throw new NotImplementedException(); }
        }

        public override void Initialize()
        {
            
        }
    }
}
