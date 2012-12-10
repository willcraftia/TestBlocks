#region Using

using System;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Blocks.Models;
using Willcraftia.Xna.Blocks.Serialization;

#endregion

namespace Willcraftia.Xna.Blocks.Assets
{
    public sealed class BlockLoader : AssetLoaderBase
    {
        DefinitionSerializer serializer = new DefinitionSerializer(typeof(BlockDefinition));

        public BlockLoader(UriManager uriManager)
            : base(uriManager)
        {
        }

        public override object Load(IUri uri)
        {
            var resource = (BlockDefinition) serializer.Deserialize(uri);

            var block = new Block();

            block.Uri = uri;
            block.Name = resource.Name;
            
            // ※注意
            // AssetManager のキャッシュを前提に、同一 URI ならば同一の Tile を得られるため、
            // TileCatalog を用いないならば、単に Index 未使用として Tile をバインドし、
            // TileCatalog を用いるならば、そこへの Tile の登録で Index が確定することを利用する。
            if (resource.Mesh != null)
            {
                var meshUri = UriManager.Create(uri.BaseUri, resource.Mesh);
                block.Mesh = AssetManager.Load<Mesh>(meshUri);
            }

            block.TopTile = LoadTile(uri.BaseUri, resource.TopTile);
            block.BottomTile = LoadTile(uri.BaseUri, resource.BottomTile);
            block.FrontTile = LoadTile(uri.BaseUri, resource.FrontTile);
            block.BackTile = LoadTile(uri.BaseUri, resource.BackTile);
            block.LeftTile = LoadTile(uri.BaseUri, resource.LeftTile);
            block.RightTile = LoadTile(uri.BaseUri, resource.RightTile);

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

        Tile LoadTile(string baseUri, string tileUri)
        {
            if (tileUri == null) return null;

            var uri = UriManager.Create(baseUri, tileUri);
            return AssetManager.Load<Tile>(uri);
        }

        public override void Save(IUri uri, object asset)
        {
            var block = asset as Block;

            var resource = new BlockDefinition();

            resource.Name = block.Name;

            if (block.Mesh != null && block.Mesh.Uri != null)
                resource.Mesh = UriManager.CreateRelativeUri(uri.BaseUri, block.Mesh.Uri);

            resource.TopTile = CreateTileUri(uri.BaseUri, block.TopTile.Uri);
            resource.BottomTile = CreateTileUri(uri.BaseUri, block.BottomTile.Uri);
            resource.FrontTile = CreateTileUri(uri.BaseUri, block.FrontTile.Uri);
            resource.BackTile = CreateTileUri(uri.BaseUri, block.BackTile.Uri);
            resource.LeftTile = CreateTileUri(uri.BaseUri, block.LeftTile.Uri);
            resource.RightTile = CreateTileUri(uri.BaseUri, block.RightTile.Uri);

            resource.Fluid = block.Fluid;
            resource.ShadowCasting = block.ShadowCasting;
            resource.Shape = block.Shape;
            resource.Mass = block.Mass;
            //resource.Immovable = block.Immovable;
            resource.StaticFriction = block.StaticFriction;
            resource.DynamicFriction = block.DynamicFriction;
            resource.Restitution = block.Restitution;

            serializer.Serialize(uri, resource);

            block.Uri = uri;
        }

        string CreateTileUri(string baseUri, IUri tileUri)
        {
            if (tileUri == null) return null;

            return UriManager.CreateRelativeUri(baseUri, tileUri);
        }
    }
}
