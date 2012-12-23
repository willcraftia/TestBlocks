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
    public sealed class MeshLoader : IAssetLoader
    {
        DefinitionSerializer serializer = new DefinitionSerializer(typeof(MeshDefinition));

        // I/F
        public object Load(IResource resource)
        {
            var definition = (MeshDefinition) serializer.Deserialize(resource);
            
            var mesh = new Mesh
            {
                Name = definition.Name
            };
            mesh.MeshParts[CubicSide.Top] = ToMeshPart(definition.Top);
            mesh.MeshParts[CubicSide.Bottom] = ToMeshPart(definition.Bottom);
            mesh.MeshParts[CubicSide.Front] = ToMeshPart(definition.Front);
            mesh.MeshParts[CubicSide.Back] = ToMeshPart(definition.Back);
            mesh.MeshParts[CubicSide.Left] = ToMeshPart(definition.Left);
            mesh.MeshParts[CubicSide.Right] = ToMeshPart(definition.Right);

            return mesh;
        }

        // I/F
        public void Save(IResource resource, object asset)
        {
            var mesh = asset as Mesh;

            var definition = new MeshDefinition
            {
                Name = mesh.Name,
                Top = ToMeshPartDefinition(mesh.MeshParts[CubicSide.Top]),
                Bottom = ToMeshPartDefinition(mesh.MeshParts[CubicSide.Bottom]),
                Front = ToMeshPartDefinition(mesh.MeshParts[CubicSide.Front]),
                Back = ToMeshPartDefinition(mesh.MeshParts[CubicSide.Back]),
                Left = ToMeshPartDefinition(mesh.MeshParts[CubicSide.Left]),
                Right = ToMeshPartDefinition(mesh.MeshParts[CubicSide.Right])
            };

            serializer.Serialize(resource, definition);
        }

        MeshPart ToMeshPart(MeshPartDefinition meshPartDefinition)
        {
            if (meshPartDefinition.Vertices == null || meshPartDefinition.Vertices.Length == 0 ||
                meshPartDefinition.Indices == null || meshPartDefinition.Indices.Length == 0)
                return null;

            return new MeshPart(meshPartDefinition.Vertices, meshPartDefinition.Indices);
        }

        MeshPartDefinition ToMeshPartDefinition(MeshPart meshPart)
        {
            if (meshPart == null) return new MeshPartDefinition();

            return new MeshPartDefinition
            {
                Vertices = meshPart.Vertices,
                Indices = meshPart.Indices
            };
        }
    }
}
