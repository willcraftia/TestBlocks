#region Using

using System;
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
            return new Mesh
            {
                Resource = resource,
                Name = definition.Name,
                Top = ToMeshPart(definition.Top),
                Bottom = ToMeshPart(definition.Bottom),
                Front = ToMeshPart(definition.Front),
                Back = ToMeshPart(definition.Back),
                Left = ToMeshPart(definition.Left),
                Right = ToMeshPart(definition.Right)
            };
        }

        // I/F
        public void Save(IResource resource, object asset)
        {
            var mesh = asset as Mesh;

            var definition = new MeshDefinition
            {
                Name = mesh.Name,
                Top = ToMeshPartDefinition(mesh.Top),
                Bottom = ToMeshPartDefinition(mesh.Bottom),
                Front = ToMeshPartDefinition(mesh.Front),
                Back = ToMeshPartDefinition(mesh.Back),
                Left = ToMeshPartDefinition(mesh.Left),
                Right = ToMeshPartDefinition(mesh.Right)
            };

            serializer.Serialize(resource, definition);

            mesh.Resource = resource;
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
