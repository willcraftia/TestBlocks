#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Blocks.Models;
using Willcraftia.Xna.Blocks.Serialization;

#endregion

namespace Willcraftia.Xna.Blocks.Assets
{
    public sealed class TileLoader : AssetLoaderBase
    {
        public TileLoader(UriManager uriManager)
            : base(uriManager)
        {
        }

        public override object Load(IUri uri)
        {
            var resource = ResourceSerializer.Deserialize<TileDefinition>(uri);

            var tile = new Tile();

            tile.Uri = uri;
            tile.Name = resource.Name;
            
            //
            // TODO: absolute URI
            //
            
            tile.TextureUri = resource.Texture;
            tile.Texture = LoadTexture(uri.BaseUri, tile.TextureUri);
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

        Texture2D LoadTexture(string baseUri, string textureUri)
        {
            var uri = UriManager.Create(baseUri, textureUri);
            return AssetManager.Load<Texture2D>(uri);
        }

        public override void Save(IUri uri, object asset)
        {
            var tile = asset as Tile;

            var resource = new TileDefinition();

            resource.Name = tile.Name;
            resource.Texture = tile.TextureUri;
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
