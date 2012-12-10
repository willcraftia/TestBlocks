#region Using

using System;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Blocks.Models;
using Willcraftia.Xna.Blocks.Serialization;

#endregion

namespace Willcraftia.Xna.Blocks.Assets
{
    public sealed class TileCatalogLoader : AssetLoaderBase
    {
        DefinitionSerializer serializer = new DefinitionSerializer(typeof(TileCatalogDefinition));

        GraphicsDevice graphicsDevice;

        public TileCatalogLoader(ResourceManager resourceManager, GraphicsDevice graphicsDevice)
            : base(resourceManager)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");

            this.graphicsDevice = graphicsDevice;
        }

        public override object Load(IResource resource)
        {
            var definition = (TileCatalogDefinition) serializer.Deserialize(resource);

            var tileCatalog = new TileCatalog(graphicsDevice, definition.Entries.Length);

            tileCatalog.Resource = resource;
            tileCatalog.Name = definition.Name;

            foreach (var entry in definition.Entries)
            {
                var tile = LoadTile(resource, entry.Tile);
                tile.Index = entry.Index;
                tileCatalog.Tiles.Add(tile);
            }

            return tileCatalog;
        }

        Tile LoadTile(IResource baseResource, string tileUri)
        {
            var resource = ResourceManager.Load(baseResource, tileUri);
            return AssetManager.Load<Tile>(resource);
        }

        public override void Save(IResource resource, object asset)
        {
            var tileCatalog = asset as TileCatalog;

            var definition = new TileCatalogDefinition();

            definition.Name = tileCatalog.Name;
            definition.Entries = new TileIndexDefinition[tileCatalog.Tiles.Count];
            for (int i = 0; i < tileCatalog.Tiles.Count; i++)
            {
                var tile = tileCatalog.Tiles[i];
                definition.Entries[i] = new TileIndexDefinition
                {
                    Index = tile.Index,
                    Tile = ResourceManager.CreateRelativeUri(resource, tile.Resource)
                };
            }

            serializer.Serialize(resource, definition);

            tileCatalog.Resource = resource;
        }
    }
}
