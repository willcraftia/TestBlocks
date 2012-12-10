#region Using

using System;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Blocks.Models;
using Willcraftia.Xna.Blocks.Serialization;

#endregion

namespace Willcraftia.Xna.Blocks.Assets
{
    public sealed class BlockCatalogLoader : AssetLoaderBase
    {
        DefinitionSerializer serializer = new DefinitionSerializer(typeof(BlockCatalogDefinition));

        public BlockCatalogLoader(ResourceManager resourceManager)
            : base(resourceManager)
        {
        }

        public override object Load(IResource resource)
        {
            var definition = (BlockCatalogDefinition) serializer.Deserialize(resource);

            var blockCatalog = new BlockCatalog(definition.Entries.Length);

            blockCatalog.Resource = resource;
            blockCatalog.Name = definition.Name;

            foreach (var entry in definition.Entries)
            {
                var blockResource = ResourceManager.Load(resource, entry.Block);
                var block = AssetManager.Load<Block>(blockResource);
                block.Index = entry.Index;
                blockCatalog.Blocks.Add(block);
            }

            return blockCatalog;
        }

        public override void Save(IResource resource, object asset)
        {
            var blockCatalog = asset as BlockCatalog;

            var definition = new BlockCatalogDefinition();

            definition.Name = blockCatalog.Name;

            definition.Entries = new BlockIndexDefinition[blockCatalog.Blocks.Count];
            for (int i = 0; i < blockCatalog.Blocks.Count; i++)
            {
                var block = blockCatalog.Blocks[i];
                var blockUri = ResourceManager.CreateRelativeUri(resource, block.Resource);
                definition.Entries[i] = new BlockIndexDefinition
                {
                    Index = block.Index,
                    Block = blockUri
                };
            }

            serializer.Serialize(resource, definition);

            blockCatalog.Resource = resource;
        }
    }
}
