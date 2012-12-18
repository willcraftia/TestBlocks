﻿#region Using

using System;
using Willcraftia.Xna.Framework.Content;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Blocks.Models;
using Willcraftia.Xna.Blocks.Serialization;

#endregion

namespace Willcraftia.Xna.Blocks.Content
{
    public sealed class BlockLoader : IAssetLoader, IAssetManagerAware
    {
        ResourceManager resourceManager;

        DefinitionSerializer serializer = new DefinitionSerializer(typeof(BlockDefinition));

        // I/F
        public AssetManager AssetManager { private get; set; }

        public BlockLoader(ResourceManager resourceManager)
        {
            if (resourceManager == null) throw new ArgumentNullException("resourceManager");

            this.resourceManager = resourceManager;
        }

        // I/F
        public object Load(IResource resource)
        {
            var definition = (BlockDefinition) serializer.Deserialize(resource);
            return new Block
            {
                Name = definition.Name,
                Mesh = Load<Mesh>(resource, definition.Mesh),
                TopTile = Load<Tile>(resource, definition.TopTile),
                BottomTile = Load<Tile>(resource, definition.BottomTile),
                FrontTile = Load<Tile>(resource, definition.FrontTile),
                BackTile = Load<Tile>(resource, definition.BackTile),
                LeftTile = Load<Tile>(resource, definition.LeftTile),
                RightTile = Load<Tile>(resource, definition.RightTile),
                Fluid = definition.Fluid,
                ShadowCasting = definition.ShadowCasting,
                Shape = definition.Shape,
                Mass = definition.Mass,
                StaticFriction = definition.StaticFriction,
                DynamicFriction = definition.DynamicFriction,
                Restitution = definition.Restitution
            };
        }

        // I/F
        public void Save(IResource resource, object asset)
        {
            var block = asset as Block;

            var definition = new BlockDefinition
            {
                Name = block.Name,
                Mesh = ToUri(resource, block.Mesh),
                TopTile = ToUri(resource, block.TopTile),
                BottomTile = ToUri(resource, block.BottomTile),
                FrontTile = ToUri(resource, block.FrontTile),
                BackTile = ToUri(resource, block.BackTile),
                LeftTile = ToUri(resource, block.LeftTile),
                RightTile = ToUri(resource, block.RightTile),
                Fluid = block.Fluid,
                ShadowCasting = block.ShadowCasting,
                Shape = block.Shape,
                Mass = block.Mass,
                StaticFriction = block.StaticFriction,
                DynamicFriction = block.DynamicFriction,
                Restitution = block.Restitution
            };

            serializer.Serialize(resource, definition);
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
