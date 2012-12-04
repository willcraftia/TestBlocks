#region Using

using System;
using Willcraftia.Xna.Framework.Assets;
using Willcraftia.Xna.Framework.Serialization;
using Willcraftia.Xna.Blocks.Models;
using Willcraftia.Xna.Blocks.Serialization;

#endregion

namespace Willcraftia.Xna.Blocks.Assets
{
    public sealed class MeshLoader : IAssetLoader
    {
        public object Load(AssetManager assetManager, Uri uri)
        {
            var resource = ResourceManager.Instance.Load<MeshDefinition>(uri);

            var mesh = new Mesh();

            mesh.Uri = uri;
            mesh.Name = resource.Name;

            if (resource.Top != null) mesh.Top = CreateMeshPart(resource.Top);
            if (resource.Bottom != null) mesh.Bottom = CreateMeshPart(resource.Bottom);
            if (resource.Front != null) mesh.Front = CreateMeshPart(resource.Front);
            if (resource.Back != null) mesh.Back = CreateMeshPart(resource.Back);
            if (resource.Left != null) mesh.Left = CreateMeshPart(resource.Left);
            if (resource.Right != null) mesh.Right = CreateMeshPart(resource.Right);

            return mesh;
        }

        public void Unload(AssetManager assetManager, Uri uri, object asset)
        {
            // 他のアセットへの参照なし。
            // Dispose が必要なプロパティもなし。
        }

        public void Save(AssetManager assetManager, Uri uri, object asset)
        {
            var mesh = asset as Mesh;

            var resource = new MeshDefinition();

            resource.Name = mesh.Name;

            if (mesh.Top != null) resource.Top = CreateMeshPartDefinition(mesh.Top);
            if (mesh.Bottom != null) resource.Bottom = CreateMeshPartDefinition(mesh.Bottom);
            if (mesh.Front != null) resource.Front = CreateMeshPartDefinition(mesh.Front);
            if (mesh.Back != null) resource.Back = CreateMeshPartDefinition(mesh.Back);
            if (mesh.Left != null) resource.Left = CreateMeshPartDefinition(mesh.Left);
            if (mesh.Right != null) resource.Right = CreateMeshPartDefinition(mesh.Right);

            ResourceManager.Instance.Save(uri, resource);
            mesh.Uri = uri;
        }

        MeshPart CreateMeshPart(MeshPartDefinition meshPartDefinition)
        {
            return new MeshPart(meshPartDefinition.Vertices, meshPartDefinition.Indices);
        }

        MeshPartDefinition CreateMeshPartDefinition(MeshPart meshPart)
        {
            return new MeshPartDefinition
            {
                Vertices = meshPart.Vertices,
                Indices = meshPart.Indices
            };
        }
    }
}
