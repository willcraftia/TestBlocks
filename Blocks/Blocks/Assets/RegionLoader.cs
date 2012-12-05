#region Using

using System;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Assets;
using Willcraftia.Xna.Blocks.Models;
using Willcraftia.Xna.Blocks.Serialization;

#endregion

namespace Willcraftia.Xna.Blocks.Assets
{
    public sealed class RegionLoader : IAssetLoader
    {
        public object Load(AssetManager assetManager, Uri uri)
        {
            var resource = ResourceSerializer.Deserialize<RegionDefinition>(uri);

            var region = new Region();

            region.Uri = uri;
            region.Name = resource.Name;
            region.Bounds = resource.Bounds;
            region.TileCatalog = assetManager.Load<TileCatalog>(resource.TileCatalog);
            region.BlockCatalog = assetManager.Load<BlockCatalog>(resource.BlockCatalog);
            if (resource.ChunkBundle != null) region.ChunkBundleUri = new Uri(resource.ChunkBundle);

            return region;
        }

        public void Unload(AssetManager assetManager, Uri uri, object asset)
        {
            // Region と AssetManager は同じ寿命を持つため、GC に委ねるのみである。
        }

        public void Save(AssetManager assetManager, Uri uri, object asset)
        {
            var region = asset as Region;

            var resource = new RegionDefinition();

            resource.Name = region.Name;
            resource.Bounds = region.Bounds;
            if (region.TileCatalog != null && region.TileCatalog.Uri != null)
                resource.TileCatalog = region.TileCatalog.Uri.OriginalString;
            if (region.BlockCatalog != null && region.BlockCatalog.Uri != null)
                resource.BlockCatalog = region.BlockCatalog.Uri.OriginalString;
            resource.ChunkBundle = region.ChunkBundleUri.OriginalString;

            ResourceSerializer.Serialize<RegionDefinition>(uri, resource);
        }
    }
}
