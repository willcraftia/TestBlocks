#region Using

using System;
using Willcraftia.Xna.Framework.Content;
using Willcraftia.Xna.Framework.Component;
using Willcraftia.Xna.Framework.IO;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class SingleBiomeManagerCore : IBiomeManagerCore
    {
        public string BiomeUri { get; set; }

        public Biome Biome { get; private set; }

        // I/F
        public Biome GetBiome(Chunk chunk)
        {
            return Biome;
        }
    }
}
