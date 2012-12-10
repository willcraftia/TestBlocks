#region Using

using System;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Blocks.Models;
using Willcraftia.Xna.Blocks.Serialization;

#endregion

namespace Willcraftia.Xna.Blocks.Assets
{
    public sealed class TileCatalogLoader : AssetLoaderBase
    {
        DefinitionSerializer serializer = new DefinitionSerializer(typeof(TileCatalogDefinition));

        GraphicsDevice graphicsDevice;

        public TileCatalogLoader(UriManager uriManager, GraphicsDevice graphicsDevice)
            : base(uriManager)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");

            this.graphicsDevice = graphicsDevice;
        }

        public override object Load(IUri uri)
        {
            var resource = (TileCatalogDefinition) serializer.Deserialize(uri);

            var tileCatalog = new TileCatalog(graphicsDevice, resource.Entries.Length);

            tileCatalog.Uri = uri;
            tileCatalog.Name = resource.Name;

            foreach (var entry in resource.Entries)
            {
                var tile = LoadTile(uri.BaseUri, entry.Tile);
                tile.Index = entry.Index;
                tileCatalog.Tiles.Add(tile);
            }

            return tileCatalog;
        }

        Tile LoadTile(string baseUri, string tileUri)
        {
            var uri = UriManager.Create(baseUri, tileUri);
            return AssetManager.Load<Tile>(uri);
        }

        public override void Save(IUri uri, object asset)
        {
            var tileCatalog = asset as TileCatalog;

            var resource = new TileCatalogDefinition();

            resource.Name = tileCatalog.Name;
            resource.Entries = new TileIndexDefinition[tileCatalog.Tiles.Count];
            for (int i = 0; i < tileCatalog.Tiles.Count; i++)
            {
                var tile = tileCatalog.Tiles[i];
                resource.Entries[i] = new TileIndexDefinition
                {
                    Index = tile.Index,
                    Tile = UriManager.CreateRelativeUri(uri.BaseUri, tile.Uri)
                };
            }

            serializer.Serialize(uri, resource);

            tileCatalog.Uri = uri;
        }
    }
}
