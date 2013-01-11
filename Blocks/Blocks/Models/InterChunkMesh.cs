#region Using

using System;
using Microsoft.Xna.Framework;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Diagnostics;
using Willcraftia.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class InterChunkMesh
    {
        static readonly Logger logger = new Logger(typeof(InterChunkMesh).Name);

        // TODO
        //
        // 実行で最適と思われる値を調べて決定する。
        const ushort defaultVertexCapacity = 10000;

        const ushort defaultIndexCapacity = 10000;

        static readonly VertexPositionNormalColorTexture[] emptyVertices = new VertexPositionNormalColorTexture[0];

        static readonly ushort[] emptyIndices = new ushort[0];

        VertexPositionNormalColorTexture[] vertices;

        ushort[] indices;

        int vertexCount;

        int indexCount;

        BoundingBox boundingBox;

        Vector3[] corners = new Vector3[8];

        public int VertexCount
        {
            get { return vertexCount; }
        }

        public int IndexCount
        {
            get { return indexCount; }
        }

        public int VertexCapacity
        {
            get { return vertices.Length; }
            set
            {
                if (value < vertexCount) throw new ArgumentOutOfRangeException("value");

                if (value == vertices.Length) return;

                if (0 < value)
                {
                    var newVertices = new VertexPositionNormalColorTexture[value];
                    if (0 < vertexCount) Array.Copy(vertices, 0, newVertices, 0, vertexCount);
                    vertices = newVertices;
                }
                else
                {
                    vertices = emptyVertices;
                }
            }
        }

        public int IndexCapacity
        {
            get { return indices.Length; }
            set
            {
                if (value < indexCount) throw new ArgumentOutOfRangeException("value");

                if (value == indices.Length) return;

                if (0 < value)
                {
                    var newIndices = new ushort[value];
                    if (0 < indexCount) Array.Copy(indices, 0, newIndices, 0, indexCount);
                    indices = newIndices;
                }
                else
                {
                    indices = emptyIndices;
                }
            }
        }

        public InterChunkMesh()
        {
            vertices = new VertexPositionNormalColorTexture[defaultVertexCapacity];
            indices = new ushort[defaultIndexCapacity];
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

            destination.SetVertices(vertices, vertexCount);
            destination.SetIndices(indices, indexCount);
        }

        public void AddIndex(ushort index)
        {
            if (indexCount == indices.Length) EnsureIndexCapacity(indexCount + indices.Length);

            indices[indexCount++] = (ushort) (vertexCount + index);
        }

        public void AddVertex(ref VertexPositionNormalColorTexture vertex)
        {
            if (vertexCount == vertices.Length) EnsureVertexCapacity(vertexCount + 1);

            vertices[vertexCount++] = vertex;

            // AABB を更新。
            Vector3.Max(ref boundingBox.Max, ref vertex.Position, out boundingBox.Max);
            Vector3.Min(ref boundingBox.Min, ref vertex.Position, out boundingBox.Min);
        }

        public void Clear()
        {
            vertexCount = 0;
            indexCount = 0;
            boundingBox = BoundingBoxHelper.Empty;
        }

        // TODO
        //
        // 実行で最適と思われる拡張ロジックへ変更する。
        // 少なくとも、頂点数は非常に多いと予想されるため、二倍による拡張は適切ではない。
        void EnsureVertexCapacity(int capacity)
        {
            if (vertices.Length < capacity)
            {
                //var newCapacity = vertices.Length == 0 ? defaultVertexCapacity : vertices.Length * 2;
                var newCapacity = vertices.Length == 0 ? defaultVertexCapacity : vertices.Length + 100;
                if (newCapacity < capacity) newCapacity = capacity;
                VertexCapacity = newCapacity;

                logger.Warn("EnsureVertexCapacity: newCapacity={0}", newCapacity);
            }
        }

        void EnsureIndexCapacity(int capacity)
        {
            if (indices.Length < capacity)
            {
                //var newCapacity = indices.Length == 0 ? defaultIndexCapacity : indices.Length * 2;
                var newCapacity = indices.Length == 0 ? defaultIndexCapacity : indices.Length + 100;
                if (newCapacity < capacity) newCapacity = capacity;
                IndexCapacity = newCapacity;

                logger.Warn("EnsureIndexCapacity: newCapacity={0}", newCapacity);
            }
        }
    }
}
