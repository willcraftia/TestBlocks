#region Using

using System;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Content;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Blocks.Models;
using Willcraftia.Xna.Blocks.Serialization;

#endregion

namespace Willcraftia.Xna.Blocks.Content
{
    public sealed class TileCatalogLoader : IAssetLoader, IAssetManagerAware
    {
        ResourceManager resourceManager;

        GraphicsDevice graphicsDevice;

        DefinitionSerializer serializer = new DefinitionSerializer(typeof(TileCatalogDefinition));

        // I/F
        public AssetManager AssetManager { private get; set; }

        public TileCatalogLoader(ResourceManager resourceManager, GraphicsDevice graphicsDevice)
        {
            if (resourceManager == null) throw new ArgumentNullException("resourceManager");
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");

            this.resourceManager = resourceManager;
            this.graphicsDevice = graphicsDevice;
        }

        // I/F
        public object Load(IResource resource)
        {
            var definition = (TileCatalogDefinition) serializer.Deserialize(resource);

            var tileCatalog = new TileCatalog(graphicsDevice, definition.Entries.Length)
            {
                Resource = resource,
                Name = definition.Name
            };

            foreach (var entry in definition.Entries)
            {
                var tile = Load<Tile>(resource, entry.Uri);
                if (tile != null)
                {
                    tile.Index = entry.Index;
                    tileCatalog.Add(tile);
                }
            }

            return tileCatalog;
        }

        // I/F
        public void Save(IResource resource, object asset)
        {
            var tileCatalog = asset as TileCatalog;

            var definition = new TileCatalogDefinition
            {
                Name = tileCatalog.Name,
                Entries = new IndexedUriDefinition[tileCatalog.Count]
            };

            for (int i = 0; i < tileCatalog.Count; i++)
            {
                var tile = tileCatalog[i];
                definition.Entries[i] = new IndexedUriDefinition
                {
                    Index = tile.Index,
                    Uri = ToUri(resource, tile)
                };
            }

            serializer.Serialize(resource, definition);

            tileCatalog.Resource = resource;
        }

        T Load<T>(IResource baseResource, string uri) where T : class
        {
            if (string.IsNullOrEmpty(uri)) return null;

            var resource = resourceManager.Load(baseResource, uri);
            return AssetManager.Load<T>(resource);
        }

        string ToUri(IResource baseResource, IAsset asset)
        {
            if (asset == null || asset.Resource == null) return null;

            return resourceManager.CreateRelativeUri(baseResource, asset.Resource);
        }
    }
}
