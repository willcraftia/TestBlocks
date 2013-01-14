#region Using

using System;
using Microsoft.Xna.Framework;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class InterChunkMesh
    {
        // TODO
        //
        // 実行で最適と思われる値を調べて決定する。
        // 恐らくは、最大頂点数となりうる構造は確定できるため、事前に算出できるはずだが、
        // その算出アルゴリズムが分からない。
        public const ushort VertexCapacity = 20000;

        public const ushort IndexCapacity = 6 * VertexCapacity / 4;

        VertexPositionNormalColorTexture[] vertices = new VertexPositionNormalColorTexture[VertexCapacity];

        ushort[] indices = new ushort[IndexCapacity];

        BoundingBox boundingBox;

        Vector3[] corners = new Vector3[8];

        public int VertexCount { get; private set; }

        public int IndexCount { get; private set; }

        public InterChunkMesh()
        {
            vertices = new VertexPositionNormalColorTexture[VertexCapacity];
            indices = new ushort[IndexCapacity];
        }

        public void Populate(ChunkMesh destination)
        {
            if (destination == null) throw new ArgumentNullException("destination");

            // メッシュの BoundingBox。
            var transform = destination.World;
            Vector3.Transform(ref boundingBox.Min, ref transform, out destination.BoundingBox.Min);
            Vector3.Transform(ref boundingBox.Max, ref transform, out destination.BoundingBox.Max);

            // メッシュの BoundingSphere。
            destination.BoundingBox.GetCorners(corners);
            destination.BoundingSphere = BoundingSphere.CreateFromPoints(corners);

            destination.SetVertices(vertices, VertexCount);
            destination.SetIndices(indices, IndexCount);
        }

        public void AddIndex(ushort index)
        {
            indices[IndexCount++] = (ushort) (VertexCount + index);
        }

        public void AddVertex(ref VertexPositionNormalColorTexture vertex)
        {
            vertices[VertexCount++] = vertex;

            // AABB を更新。
            Vector3.Max(ref boundingBox.Max, ref vertex.Position, out boundingBox.Max);
            Vector3.Min(ref boundingBox.Min, ref vertex.Position, out boundingBox.Min);
        }

        public void Clear()
        {
            VertexCount = 0;
            IndexCount = 0;
            boundingBox = BoundingBoxHelper.Empty;
        }
    }
}
