#region Using

using System;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Blocks.Models;
using Willcraftia.Xna.Blocks.Serialization;

#endregion

namespace Willcraftia.Xna.Blocks.Assets
{
    public sealed class BlockCatalogLoader : AssetLoaderBase
    {
        public BlockCatalogLoader(UriManager uriManager)
            : base(uriManager)
        {
        }

        public override object Load(IUri uri)
        {
            var resource = ResourceSerializer.Deserialize<BlockCatalogDefinition>(uri);

            var blockCatalog = new BlockCatalog(resource.Entries.Length);

            blockCatalog.Uri = uri;
            blockCatalog.Name = resource.Name;

            foreach (var entry in resource.Entries)
            {
                var blockUri = UriManager.Create(uri.BaseUri, entry.Block);
                var block = AssetManager.Load<Block>(blockUri);
                block.Index = entry.Index;
                blockCatalog.Blocks.Add(block);
            }

            return blockCatalog;
        }

        public override void Save(IUri uri, object asset)
        {
            var blockCatalog = asset as BlockCatalog;

            var resource = new BlockCatalogDefinition();

            resource.Name = blockCatalog.Name;

            resource.Entries = new BlockIndexDefinition[blockCatalog.Blocks.Count];
            for (int i = 0; i < blockCatalog.Blocks.Count; i++)
            {
                var block = blockCatalog.Blocks[i];
                var blockUri = UriManager.CreateRelativeUri(uri.BaseUri, block.Uri);
                resource.Entries[i] = new BlockIndexDefinition
                {
                    Index = block.Index,
                    Block = blockUri
                };
            }

            ResourceSerializer.Serialize<BlockCatalogDefinition>(uri, resource);

            blockCatalog.Uri = uri;
        }
    }
}
