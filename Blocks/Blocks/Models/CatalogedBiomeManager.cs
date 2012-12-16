#region Using

using System;
using Willcraftia.Xna.Framework.Component;
using Willcraftia.Xna.Framework.Content;
using Willcraftia.Xna.Framework.IO;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public abstract class CatalogedBiomeManager : IBiomeManager
    {
        public BiomeCatalog BiomeCatalog { get; private set; }

        // I/F
        [PropertyIgnored]
        public IResource Resource { get; set; }

        // I/F
        public IBiome GetBiome(Chunk chunk)
        {
            return BiomeCatalog[GetBiomeIndex(chunk)];
        }

        protected abstract byte GetBiomeIndex(Chunk chunk);
    }
}
