#region Using

using System;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Assets;
using Willcraftia.Xna.Framework.Serialization;
using Willcraftia.Xna.Blocks.Models;
using Willcraftia.Xna.Blocks.Serialization;

#endregion

namespace Willcraftia.Xna.Blocks.Assets
{
    public sealed class TileCatalogLoader : IAssetLoader
    {
        GraphicsDevice graphicsDevice;

        public TileCatalogLoader(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");

            this.graphicsDevice = graphicsDevice;
        }

        public object Load(AssetManager assetManager, Uri uri)
        {
            var resource = ResourceManager.Instance.Load<TileCatalogDefinition>(uri);

            var tileCatalog = new TileCatalog(graphicsDevice, resource.Entries.Length);

            tileCatalog.Uri = uri;
            tileCatalog.Name = resource.Name;

            foreach (var entry in resource.Entries)
            {
                var tile = assetManager.Load<Tile>(entry.Tile);
                tile.Index = entry.Index;
                tileCatalog.Tiles.Add(tile);
            }

            return tileCatalog;
        }

        public void Unload(AssetManager assetManager, Uri uri, object asset)
        {
            var tileCatalog = asset as TileCatalog;

            tileCatalog.Uri = null;
            tileCatalog.Name = null;
            tileCatalog.Tiles.Clear();
            tileCatalog.TileMap.Dispose();
            tileCatalog.DiffuseColorMap.Dispose();
            tileCatalog.EmissiveColorMap.Dispose();
            tileCatalog.SpecularColorMap.Dispose();
        }

        public void Save(AssetManager assetManager, Uri uri, object asset)
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
                    Tile = tile.Uri.OriginalString
                };
            }

            ResourceManager.Instance.Save(uri, resource);
            tileCatalog.Uri = uri;
        }
    }
}
