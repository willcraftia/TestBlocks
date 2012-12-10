#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Blocks.Models;
using Willcraftia.Xna.Blocks.Serialization;

#endregion

namespace Willcraftia.Xna.Blocks.Assets
{
    public sealed class TileLoader : AssetLoaderBase
    {
        DefinitionSerializer serializer = new DefinitionSerializer(typeof(TileDefinition));

        public TileLoader(ResourceManager resourceManager)
            : base(resourceManager)
        {
        }

        public override object Load(IResource resource)
        {
            var definition = (TileDefinition) serializer.Deserialize(resource);

            var tile = new Tile();

            tile.Resource = resource;
            tile.Name = definition.Name;
            
            //
            // TODO: absolute URI
            //
            
            tile.TextureUri = definition.Texture;
            tile.Texture = LoadTexture(resource, tile.TextureUri);
            tile.Translucent = definition.Translucent;

            Color color;

            UnpackColor(definition.DiffuseColor, out color);
            tile.DiffuseColor = color;

            UnpackColor(definition.EmissiveColor, out color);
            tile.EmissiveColor = color;

            UnpackColor(definition.SpecularColor, out color);
            tile.SpecularColor = color;

            tile.SpecularPower = definition.SpecularPower;

            return tile;
        }

        Texture2D LoadTexture(IResource baseResource, string textureUri)
        {
            var resource = ResourceManager.Load(baseResource, textureUri);
            return AssetManager.Load<Texture2D>(resource);
        }

        public override void Save(IResource resource, object asset)
        {
            var tile = asset as Tile;

            var definition = new TileDefinition();

            definition.Name = tile.Name;
            definition.Texture = tile.TextureUri;
            definition.Translucent = tile.Translucent;
            definition.DiffuseColor = tile.DiffuseColor.PackedValue;
            definition.EmissiveColor = tile.EmissiveColor.PackedValue;
            definition.SpecularColor = tile.SpecularColor.PackedValue;
            definition.SpecularPower = tile.SpecularPower;

            serializer.Serialize(resource, definition);

            tile.Resource = resource;
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
