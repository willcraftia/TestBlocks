#region Using

using System;
using Willcraftia.Xna.Framework.Assets;
using Willcraftia.Xna.Framework.IO;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public abstract class CatalogedBiomeManagerComponent : IBiomeManagerComponent, IAssetReferencingComponent
    {
        public string BiomeCatalogUri { get; set; }

        public BiomeCatalog BiomeCatalog { get; private set; }

        // I/F
        public void BindAssets(AssetManager assetManager, ResourceManager resourceManager, IResource componentResource)
        {
            var resource = resourceManager.Load(componentResource, BiomeCatalogUri);
            BiomeCatalog = assetManager.Load<BiomeCatalog>(resource);

            BindAssetsOverride(assetManager, resourceManager, componentResource);
        }

        // I/F
        public Biome GetBiome(Chunk chunk)
        {
            return BiomeCatalog[GetBiomeIndex(chunk)];
        }

        protected abstract byte GetBiomeIndex(Chunk chunk);

        protected virtual void BindAssetsOverride(AssetManager assetManager, ResourceManager resourceManager, IResource componentResource)
        {
        }
    }
}
