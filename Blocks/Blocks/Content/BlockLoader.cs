#region Using

using System;
using Willcraftia.Xna.Framework;
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
            
            var block = new Block
            {
                Name = definition.Name,
                MeshPrototype = Load<Mesh>(resource, definition.Mesh),
                Fluid = definition.Fluid,
                ShadowCasting = definition.ShadowCasting,
                Shape = definition.Shape,
                Mass = definition.Mass,
                StaticFriction = definition.StaticFriction,
                DynamicFriction = definition.DynamicFriction,
                Restitution = definition.Restitution
            };
            block.Tiles[CubicSide.Top] = Load<Tile>(resource, definition.TopTile);
            block.Tiles[CubicSide.Bottom] = Load<Tile>(resource, definition.BottomTile);
            block.Tiles[CubicSide.Front] = Load<Tile>(resource, definition.FrontTile);
            block.Tiles[CubicSide.Back] = Load<Tile>(resource, definition.BackTile);
            block.Tiles[CubicSide.Left] = Load<Tile>(resource, definition.LeftTile);
            block.Tiles[CubicSide.Right] = Load<Tile>(resource, definition.RightTile);
            block.BuildMesh();

            return block;
        }

        // I/F
        public void Save(IResource resource, object asset)
        {
            var block = asset as Block;

            var definition = new BlockDefinition
            {
                Name = block.Name,
                Mesh = ToUri(resource, block.MeshPrototype),
                TopTile = ToUri(resource, block.Tiles[CubicSide.Top]),
                BottomTile = ToUri(resource, block.Tiles[CubicSide.Bottom]),
                FrontTile = ToUri(resource, block.Tiles[CubicSide.Front]),
                BackTile = ToUri(resource, block.Tiles[CubicSide.Back]),
                LeftTile = ToUri(resource, block.Tiles[CubicSide.Left]),
                RightTile = ToUri(resource, block.Tiles[CubicSide.Right]),
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
