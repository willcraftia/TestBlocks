#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Assets;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Blocks.Models;
using Willcraftia.Xna.Blocks.Serialization;

#endregion

namespace Willcraftia.Xna.Blocks.Assets
{
    public sealed class TileLoader : IAssetLoader, IAssetManagerAware, IResourceManagerAware
    {
        DefinitionSerializer serializer = new DefinitionSerializer(typeof(TileDefinition));

        // I/F
        public AssetManager AssetManager { private get; set; }

        // I/F
        public ResourceManager ResourceManager { private get; set; }

        // I/F
        public object Load(IResource resource)
        {
            var definition = (TileDefinition) serializer.Deserialize(resource);
            return new Tile
            {
                Resource = resource,
                Name = definition.Name,
                TextureUri = definition.Texture,
                Texture = Load<Texture2D>(resource, definition.Texture),
                Translucent = definition.Translucent,
                DiffuseColor = ToColor(definition.DiffuseColor),
                EmissiveColor = ToColor(definition.EmissiveColor),
                SpecularColor = ToColor(definition.SpecularColor),
                SpecularPower = definition.SpecularPower
            };
        }

        // I/F
        public void Save(IResource resource, object asset)
        {
            var tile = asset as Tile;

            var definition = new TileDefinition
            {
                Name = tile.Name,
                Texture = tile.TextureUri,
                Translucent = tile.Translucent,
                DiffuseColor = tile.DiffuseColor.PackedValue,
                EmissiveColor = tile.EmissiveColor.PackedValue,
                SpecularColor = tile.SpecularColor.PackedValue,
                SpecularPower = tile.SpecularPower
            };

            serializer.Serialize(resource, definition);

            tile.Resource = resource;
        }

        T Load<T>(IResource baseResource, string uri) where T : class
        {
            if (string.IsNullOrEmpty(uri)) return null;

            var resource = ResourceManager.Load(baseResource, uri);
            return AssetManager.Load<T>(resource);
        }

        Color ToColor(uint packedValue)
        {
            return new Color
            {
                A = (byte) (packedValue >> 24),
                R = (byte) (packedValue >> 16),
                G = (byte) (packedValue >> 8),
                B = (byte) (packedValue)
            };
        }
    }
}
