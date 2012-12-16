#region Using

using System;
using Willcraftia.Xna.Framework.Content;
using Willcraftia.Xna.Framework.Component;
using Willcraftia.Xna.Framework.IO;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class SingleBiomeManager : IBiomeManager
    {
        public string Name { get; set; }

        public IBiome Biome { get; set; }

        // I/F
        [PropertyIgnored]
        public IResource Resource { get; set; }

        // I/F
        public IBiome GetBiome(Chunk chunk)
        {
            return Biome;
        }
    }
}
