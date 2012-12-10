#region Using

using System;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Assets;
using Willcraftia.Xna.Blocks.Models;
using Willcraftia.Xna.Blocks.Serialization;

#endregion

namespace Willcraftia.Xna.Blocks.Assets
{
    public sealed class MeshLoader : AssetLoaderBase
    {
        DefinitionSerializer serializer = new DefinitionSerializer(typeof(MeshDefinition));

        public MeshLoader(UriManager uriManager)
            : base(uriManager)
        {
        }

        public override object Load(IUri uri)
        {
            var resource = (MeshDefinition) serializer.Deserialize(uri);

            var mesh = new Mesh();

            mesh.Uri = uri;
            mesh.Name = resource.Name;

            if (resource.Top.Vertices != null || resource.Top.Vertices.Length != 0)
                mesh.Top = CreateMeshPart(resource.Top);
            if (resource.Bottom.Vertices != null || resource.Bottom.Vertices.Length != 0)
                mesh.Bottom = CreateMeshPart(resource.Bottom);
            if (resource.Front.Vertices != null || resource.Front.Vertices.Length != 0)
                mesh.Front = CreateMeshPart(resource.Front);
            if (resource.Back.Vertices != null || resource.Back.Vertices.Length != 0)
                mesh.Back = CreateMeshPart(resource.Back);
            if (resource.Left.Vertices != null || resource.Left.Vertices.Length != 0)
                mesh.Left = CreateMeshPart(resource.Left);
            if (resource.Right.Vertices != null || resource.Right.Vertices.Length != 0)
                mesh.Right = CreateMeshPart(resource.Right);

            return mesh;
        }

        public override void Save(IUri uri, object asset)
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

            serializer.Serialize(uri, resource);

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
