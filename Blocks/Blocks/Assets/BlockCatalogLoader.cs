﻿#region Using

using System;
using Willcraftia.Xna.Framework.Assets;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Blocks.Models;
using Willcraftia.Xna.Blocks.Serialization;

#endregion

namespace Willcraftia.Xna.Blocks.Assets
{
    public sealed class BlockCatalogLoader : IAssetLoader, IAssetManagerAware, IResourceManagerAware
    {
        DefinitionSerializer serializer = new DefinitionSerializer(typeof(BlockCatalogDefinition));

        // I/F
        public AssetManager AssetManager { private get; set; }

        // I/F
        public ResourceManager ResourceManager { private get; set; }

        // I/F
        public object Load(IResource resource)
        {
            var definition = (BlockCatalogDefinition) serializer.Deserialize(resource);

            var blockCatalog = new BlockCatalog(definition.Entries.Length)
            {
                Resource = resource,
                Name = definition.Name
            };

            foreach (var entry in definition.Entries)
            {
                var block = Load<Block>(resource, entry.Uri);
                if (block != null)
                {
                    block.Index = entry.Index;
                    blockCatalog.Add(block);
                }
            }

            return blockCatalog;
        }

        // I/F
        public void Save(IResource resource, object asset)
        {
            var blockCatalog = asset as BlockCatalog;

            var definition = new BlockCatalogDefinition
            {
                Name = blockCatalog.Name,
                Entries = new IndexedUriDefinition[blockCatalog.Count]
            };

            for (int i = 0; i < blockCatalog.Count; i++)
            {
                var block = blockCatalog[i];
                definition.Entries[i] = new IndexedUriDefinition
                {
                    Index = block.Index,
                    Uri = ToUri(resource, block)
                };
            }

            serializer.Serialize(resource, definition);

            blockCatalog.Resource = resource;
        }

        T Load<T>(IResource baseResource, string uri) where T : class
        {
            if (string.IsNullOrEmpty(uri)) return null;

            var resource = ResourceManager.Load(baseResource, uri);
            return AssetManager.Load<T>(resource);
        }

        string ToUri(IResource baseResource, IAsset asset)
        {
            if (asset == null || asset.Resource == null) return null;

            return ResourceManager.CreateRelativeUri(baseResource, asset.Resource);
        }
    }
}
