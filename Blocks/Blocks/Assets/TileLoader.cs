#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Assets;
using Willcraftia.Xna.Blocks.Models;
using Willcraftia.Xna.Blocks.Serialization;

#endregion

namespace Willcraftia.Xna.Blocks.Assets
{
    public sealed class TileLoader : IAssetLoader
    {
        public object Load(AssetManager assetManager, Uri uri)
        {
            var resource = ResourceSerializer.Deserialize<TileDefinition>(uri);

            var tile = new Tile();

            tile.Uri = uri;
            tile.Name = resource.Name;
            tile.TextureUri = assetManager.CreateUri(resource.Texture);
            tile.Texture = assetManager.Load<Texture2D>(tile.TextureUri);
            tile.Translucent = resource.Translucent;

            Color color;

            UnpackColor(resource.DiffuseColor, out color);
            tile.DiffuseColor = color;

            UnpackColor(resource.EmissiveColor, out color);
            tile.EmissiveColor = color;

            UnpackColor(resource.SpecularColor, out color);
            tile.SpecularColor = color;

            tile.SpecularPower = resource.SpecularPower;

            return tile;
        }

        public void Unload(AssetManager assetManager, Uri uri, object asset)
        {
            var tile = asset as Tile;

            tile.Uri = null;
            tile.Name = null;
            tile.TextureUri = null;
            tile.Texture = null;
            tile.Translucent = false;
            tile.DiffuseColor = default(Color);
            tile.EmissiveColor = default(Color);
            tile.SpecularColor = default(Color);
            tile.SpecularPower = 0;
        }

        public void Save(AssetManager assetManager, Uri uri, object asset)
        {
            var tile = asset as Tile;

            var resource = new TileDefinition();

            resource.Name = tile.Name;
            resource.Texture = tile.TextureUri.OriginalString;
            resource.Translucent = tile.Translucent;
            resource.DiffuseColor = tile.DiffuseColor.PackedValue;
            resource.EmissiveColor = tile.EmissiveColor.PackedValue;
            resource.SpecularColor = tile.SpecularColor.PackedValue;
            resource.SpecularPower = tile.SpecularPower;

            ResourceSerializer.Serialize<TileDefinition>(uri, resource);

            tile.Uri = uri;
        }

        void UnpackColor(uint packedValue, out Color color)
        {
            color = new Color
            {
                A = (byte) (packedValue >> 24),
                R = (byte) (packedValue >> 16),
                G = (byte) (packedValue >> 8),
                B = (byte) (packedValue)
            };
        }
    }
}
