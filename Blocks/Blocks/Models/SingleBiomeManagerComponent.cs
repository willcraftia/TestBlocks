#region Using

using System;
using Willcraftia.Xna.Framework.Assets;
using Willcraftia.Xna.Framework.Component;
using Willcraftia.Xna.Framework.IO;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class SingleBiomeManagerComponent : IBiomeManagerComponent, IAssetReferencingComponent, IInitializingComponent
    {
        public string BiomeUri { get; set; }

        public Biome Biome { get; private set; }

        // I/F
        public void Initialize()
        {
            throw new NotImplementedException();
        }

        // I/F
        public void BindAssets(AssetManager assetManager, ResourceManager resourceManager, IResource componentResource)
        {
            var resource = resourceManager.Load(componentResource, BiomeUri);
            Biome = assetManager.Load<Biome>(resource);
        }

        // I/F
        public Biome GetBiome(Chunk chunk)
        {
            return Biome;
        }
    }
}
