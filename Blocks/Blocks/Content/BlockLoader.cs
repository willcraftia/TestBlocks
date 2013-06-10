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

            block.Tiles[Side.Top] = Load<Tile>(resource, definition.TopTile);
            block.Tiles[Side.Bottom] = Load<Tile>(resource, definition.BottomTile);
            block.Tiles[Side.Front] = Load<Tile>(resource, definition.FrontTile);
            block.Tiles[Side.Back] = Load<Tile>(resource, definition.BackTile);
            block.Tiles[Side.Left] = Load<Tile>(resource, definition.LeftTile);
            block.Tiles[Side.Right] = Load<Tile>(resource, definition.RightTile);
            block.BuildMesh();

            // 1 つでも半透明タイルを含んでいたら半透明ブロックとする。
            bool translucent = false;
            foreach (var tile in block.Tiles)
            {
                if (tile.Translucent)
                {
                    translucent = true;
                    break;
                }
            }
            block.Translucent = translucent;

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
                TopTile = ToUri(resource, block.Tiles[Side.Top]),
                BottomTile = ToUri(resource, block.Tiles[Side.Bottom]),
                FrontTile = ToUri(resource, block.Tiles[Side.Front]),
                BackTile = ToUri(resource, block.Tiles[Side.Back]),
                LeftTile = ToUri(resource, block.Tiles[Side.Left]),
                RightTile = ToUri(resource, block.Tiles[Side.Right]),
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
