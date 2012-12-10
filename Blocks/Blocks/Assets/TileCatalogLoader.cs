#region Using

using System;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Assets;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Blocks.Models;
using Willcraftia.Xna.Blocks.Serialization;

#endregion

namespace Willcraftia.Xna.Blocks.Assets
{
    public sealed class TileCatalogLoader : IAssetLoader, IAssetManagerAware, IResourceManagerAware
    {
        DefinitionSerializer serializer = new DefinitionSerializer(typeof(TileCatalogDefinition));

        GraphicsDevice graphicsDevice;

        // I/F
        public AssetManager AssetManager { private get; set; }

        // I/F
        public ResourceManager ResourceManager { private get; set; }

        public TileCatalogLoader(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");

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
                var tile = Load<Tile>(resource, entry.Tile);
                if (tile != null)
                {
                    tile.Index = entry.Index;
                    tileCatalog.Tiles.Add(tile);
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
                Entries = new TileIndexDefinition[tileCatalog.Tiles.Count]
            };

            for (int i = 0; i < tileCatalog.Tiles.Count; i++)
            {
                var tile = tileCatalog.Tiles[i];
                definition.Entries[i] = new TileIndexDefinition
                {
                    Index = tile.Index,
                    Tile = ToUri(resource, tile)
                };
            }

            serializer.Serialize(resource, definition);

            tileCatalog.Resource = resource;
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
