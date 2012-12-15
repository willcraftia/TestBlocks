#region Using

using System;
using Willcraftia.Xna.Framework.Content;
using Willcraftia.Xna.Framework.Component;
using Willcraftia.Xna.Framework.IO;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class SingleBiomeManagerComponent : IBiomeManagerComponent
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
