#region Using

using System;
using Willcraftia.Xna.Framework.Assets;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Blocks.Models;
using Willcraftia.Xna.Blocks.Serialization;

#endregion

namespace Willcraftia.Xna.Blocks.Assets
{
    public sealed class MeshLoader : AssetLoaderBase
    {
        DefinitionSerializer serializer = new DefinitionSerializer(typeof(MeshDefinition));

        public MeshLoader(ResourceManager resourceManager)
            : base(resourceManager)
        {
        }

        public override object Load(IResource resource)
        {
            var definition = (MeshDefinition) serializer.Deserialize(resource);

            var mesh = new Mesh();

            mesh.Resource = resource;
            mesh.Name = definition.Name;

            if (definition.Top.Vertices != null || definition.Top.Vertices.Length != 0)
                mesh.Top = CreateMeshPart(definition.Top);
            if (definition.Bottom.Vertices != null || definition.Bottom.Vertices.Length != 0)
                mesh.Bottom = CreateMeshPart(definition.Bottom);
            if (definition.Front.Vertices != null || definition.Front.Vertices.Length != 0)
                mesh.Front = CreateMeshPart(definition.Front);
            if (definition.Back.Vertices != null || definition.Back.Vertices.Length != 0)
                mesh.Back = CreateMeshPart(definition.Back);
            if (definition.Left.Vertices != null || definition.Left.Vertices.Length != 0)
                mesh.Left = CreateMeshPart(definition.Left);
            if (definition.Right.Vertices != null || definition.Right.Vertices.Length != 0)
                mesh.Right = CreateMeshPart(definition.Right);

            return mesh;
        }

        public override void Save(IResource resource, object asset)
        {
            var mesh = asset as Mesh;

            var definition = new MeshDefinition();

            definition.Name = mesh.Name;

            if (mesh.Top != null) definition.Top = CreateMeshPartDefinition(mesh.Top);
            if (mesh.Bottom != null) definition.Bottom = CreateMeshPartDefinition(mesh.Bottom);
            if (mesh.Front != null) definition.Front = CreateMeshPartDefinition(mesh.Front);
            if (mesh.Back != null) definition.Back = CreateMeshPartDefinition(mesh.Back);
            if (mesh.Left != null) definition.Left = CreateMeshPartDefinition(mesh.Left);
            if (mesh.Right != null) definition.Right = CreateMeshPartDefinition(mesh.Right);

            serializer.Serialize(resource, definition);

            mesh.Resource = resource;
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
