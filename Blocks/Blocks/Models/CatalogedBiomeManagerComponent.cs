#region Using

using System;
using Willcraftia.Xna.Framework.Content;
using Willcraftia.Xna.Framework.IO;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public abstract class CatalogedBiomeManagerComponent : IBiomeManagerComponent
    {
        public string BiomeCatalogUri { get; set; }

        public BiomeCatalog BiomeCatalog { get; private set; }

        // I/F
        public Biome GetBiome(Chunk chunk)
        {
            return BiomeCatalog[GetBiomeIndex(chunk)];
        }

        protected abstract byte GetBiomeIndex(Chunk chunk);
    }
}
