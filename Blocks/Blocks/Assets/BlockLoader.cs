#region Using

using System;
using Willcraftia.Xna.Framework.Assets;
using Willcraftia.Xna.Framework.Serialization;
using Willcraftia.Xna.Blocks.Models;
using Willcraftia.Xna.Blocks.Serialization;

#endregion

namespace Willcraftia.Xna.Blocks.Assets
{
    public sealed class BlockLoader : IAssetLoader
    {
        public object Load(AssetManager assetManager, Uri uri)
        {
            var resource = ResourceManager.Instance.Load<BlockDefinition>(uri);

            var block = new Block();

            block.Uri = uri;
            block.Name = resource.Name;
            
            // ※注意
            // AssetManager のキャッシュを前提に、同一 URI ならば同一の Tile を得られるため、
            // TileCatalog を用いないならば、単に Index 未使用として Tile をバインドし、
            // TileCatalog を用いるならば、そこへの Tile の登録で Index が確定することを利用する。
            if (resource.Mesh != null) block.Mesh = assetManager.Load<Mesh>(resource.Mesh);
            if (resource.TopTile != null) block.TopTile = assetManager.Load<Tile>(resource.TopTile);
            if (resource.BottomTile != null) block.BottomTile = assetManager.Load<Tile>(resource.BottomTile);
            if (resource.FrontTile != null) block.FrontTile = assetManager.Load<Tile>(resource.FrontTile);
            if (resource.BackTile != null) block.BackTile = assetManager.Load<Tile>(resource.BackTile);
            if (resource.LeftTile != null) block.LeftTile = assetManager.Load<Tile>(resource.LeftTile);
            if (resource.RightTile != null) block.RightTile = assetManager.Load<Tile>(resource.RightTile);

            block.Fluid = resource.Fluid;
            block.ShadowCasting = resource.ShadowCasting;
            block.Shape = resource.Shape;
            block.Mass = resource.Mass;
            //block.Immovable = resource.Immovable;
            block.StaticFriction = resource.StaticFriction;
            block.DynamicFriction = resource.DynamicFriction;
            block.Restitution = resource.Restitution;

            return block;
        }

        public void Unload(AssetManager assetManager, Uri uri, object asset)
        {
            var block = asset as Block;

            block.Uri = null;
            block.Name = null;
            block.Mesh = null;
            block.TopTile = null;
            block.BottomTile = null;
            block.FrontTile = null;
            block.BackTile = null;
            block.LeftTile = null;
            block.RightTile = null;
            block.Fluid = false;
            block.ShadowCasting = false;
            block.Shape = BlockShape.Cube;
            block.Mass = 0;
            block.StaticFriction = 0;
            block.DynamicFriction = 0;
            block.Restitution = 0;
        }

        public void Save(AssetManager assetManager, Uri uri, object asset)
        {
            var block = asset as Block;

            var resource = new BlockDefinition();

            resource.Name = block.Name;

            if (block.Mesh != null) resource.Mesh = block.Mesh.Uri.OriginalString;
            if (block.TopTile != null) resource.TopTile = block.TopTile.Uri.OriginalString;
            if (block.BottomTile != null) resource.BottomTile = block.BottomTile.Uri.OriginalString;
            if (block.FrontTile != null) resource.FrontTile = block.FrontTile.Uri.OriginalString;
            if (block.BackTile != null) resource.BackTile = block.BackTile.Uri.OriginalString;
            if (block.LeftTile != null) resource.LeftTile = block.LeftTile.Uri.OriginalString;
            if (block.RightTile != null) resource.RightTile = block.RightTile.Uri.OriginalString;

            resource.Fluid = block.Fluid;
            resource.ShadowCasting = block.ShadowCasting;
            resource.Shape = block.Shape;
            resource.Mass = block.Mass;
            //resource.Immovable = block.Immovable;
            resource.StaticFriction = block.StaticFriction;
            resource.DynamicFriction = block.DynamicFriction;
            resource.Restitution = block.Restitution;

            ResourceManager.Instance.Save(uri, resource);
            block.Uri = uri;
        }
    }
}
