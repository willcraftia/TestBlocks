#region Using

using System;
using Willcraftia.Xna.Framework.Assets;
using Willcraftia.Xna.Framework.Serialization;
using Willcraftia.Xna.Blocks.Models;
using Willcraftia.Xna.Blocks.Serialization;

#endregion

namespace Willcraftia.Xna.Blocks.Assets
{
    public sealed class BlockCatalogLoader : IAssetLoader
    {
        public object Load(AssetManager assetManager, Uri uri)
        {
            var resource = ResourceManager.Instance.Load<BlockCatalogDefinition>(uri);

            var blockCatalog = new BlockCatalog(resource.Entries.Length);

            blockCatalog.Uri = uri;
            blockCatalog.Name = resource.Name;

            foreach (var entry in resource.Entries)
            {
                var block = assetManager.Load<Block>(entry.Block);
                block.Index = entry.Index;
                blockCatalog.Blocks.Add(block);
            }

            return blockCatalog;
        }

        public void Unload(AssetManager assetManager, Uri uri, object asset)
        {
            var blockCatalog = asset as BlockCatalog;

            blockCatalog.Uri = null;
            blockCatalog.Name = null;
            blockCatalog.Blocks.Clear();
        }

        public void Save(AssetManager assetManager, Uri uri, object asset)
        {
            var blockCatalog = asset as BlockCatalog;

            var resource = new BlockCatalogDefinition();

            resource.Name = blockCatalog.Name;

            resource.Entries = new BlockIndexDefinition[blockCatalog.Blocks.Count];
            for (int i = 0; i < blockCatalog.Blocks.Count; i++)
            {
                var block = blockCatalog.Blocks[i];
                resource.Entries[i] = new BlockIndexDefinition
                {
                    Index = block.Index,
                    Block = block.Uri.OriginalString
                };
            }

            ResourceManager.Instance.Save(uri, resource);
            blockCatalog.Uri = uri;
        }
    }
}
