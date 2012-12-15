#region Using

using System;
using Willcraftia.Xna.Framework.Component;
using Willcraftia.Xna.Framework.IO;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class BiomeManager : IAsset
    {
        // I/F
        public IResource Resource { get; set; }
        
        public ComponentFactory ComponentFactory { get; set; }

        public IBiomeComponent Component { get; set; }

        public Biome GetBiome(Chunk chunk)
        {
            throw new NotImplementedException();
        }
    }
}
