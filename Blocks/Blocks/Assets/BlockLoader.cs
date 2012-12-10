#region Using

using System;
using Willcraftia.Xna.Framework.Assets;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Blocks.Models;
using Willcraftia.Xna.Blocks.Serialization;

#endregion

namespace Willcraftia.Xna.Blocks.Assets
{
    public sealed class BlockLoader : IAssetLoader, IAssetManagerAware, IResourceManagerAware
    {
        DefinitionSerializer serializer = new DefinitionSerializer(typeof(BlockDefinition));

        // I/F
        public AssetManager AssetManager { private get; set; }

        // I/F
        public ResourceManager ResourceManager { private get; set; }

        // I/F
        public object Load(IResource resource)
        {
            var definition = (BlockDefinition) serializer.Deserialize(resource);

            var block = new Block();

            block.Resource = resource;
            block.Name = definition.Name;
            
            // ※注意
            // AssetManager のキャッシュを前提に、同一 URI ならば同一の Tile を得られるため、
            // TileCatalog を用いないならば、単に Index 未使用として Tile をバインドし、
            // TileCatalog を用いるならば、そこへの Tile の登録で Index が確定することを利用する。
            if (definition.Mesh != null)
            {
                var meshResource = ResourceManager.Load(resource, definition.Mesh);
                block.Mesh = AssetManager.Load<Mesh>(meshResource);
            }

            block.TopTile = LoadTile(resource, definition.TopTile);
            block.BottomTile = LoadTile(resource, definition.BottomTile);
            block.FrontTile = LoadTile(resource, definition.FrontTile);
            block.BackTile = LoadTile(resource, definition.BackTile);
            block.LeftTile = LoadTile(resource, definition.LeftTile);
            block.RightTile = LoadTile(resource, definition.RightTile);

            block.Fluid = definition.Fluid;
            block.ShadowCasting = definition.ShadowCasting;
            block.Shape = definition.Shape;
            block.Mass = definition.Mass;
            //block.Immovable = resource.Immovable;
            block.StaticFriction = definition.StaticFriction;
            block.DynamicFriction = definition.DynamicFriction;
            block.Restitution = definition.Restitution;

            return block;
        }

        Tile LoadTile(IResource baseResource, string tileUri)
        {
            if (tileUri == null) return null;

            var resource = ResourceManager.Load(baseResource, tileUri);
            return AssetManager.Load<Tile>(resource);
        }

        // I/F
        public void Unload(IResource resource, object asset) { }

        // I/F
        public void Save(IResource resource, object asset)
        {
            var block = asset as Block;

            var definition = new BlockDefinition();

            definition.Name = block.Name;

            if (block.Mesh != null && block.Mesh.Resource != null)
                definition.Mesh = ResourceManager.CreateRelativeUri(resource, block.Mesh.Resource);

            definition.TopTile = CreateTileUri(resource, block.TopTile.Resource);
            definition.BottomTile = CreateTileUri(resource, block.BottomTile.Resource);
            definition.FrontTile = CreateTileUri(resource, block.FrontTile.Resource);
            definition.BackTile = CreateTileUri(resource, block.BackTile.Resource);
            definition.LeftTile = CreateTileUri(resource, block.LeftTile.Resource);
            definition.RightTile = CreateTileUri(resource, block.RightTile.Resource);

            definition.Fluid = block.Fluid;
            definition.ShadowCasting = block.ShadowCasting;
            definition.Shape = block.Shape;
            definition.Mass = block.Mass;
            //resource.Immovable = block.Immovable;
            definition.StaticFriction = block.StaticFriction;
            definition.DynamicFriction = block.DynamicFriction;
            definition.Restitution = block.Restitution;

            serializer.Serialize(resource, definition);

            block.Resource = resource;
        }

        string CreateTileUri(IResource baseResource, IResource tileUri)
        {
            if (tileUri == null) return null;

            return ResourceManager.CreateRelativeUri(baseResource, tileUri);
        }
    }
}
