#region Using

using System;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Diagnostics;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class InterChunkMeshPart
    {
        static readonly Logger logger = new Logger(typeof(InterChunkMeshPart).Name);

        // TODO
        //
        // 実行で最適と思われる値を調べて決定する。
        const ushort defaultVertexCapacity = 10000;

        const ushort defaultIndexCapacity = 10000;

        static readonly VertexPositionNormalTexture[] emptyVertices = new VertexPositionNormalTexture[0];

        static readonly ushort[] emptyIndices = new ushort[0];

        VertexPositionNormalTexture[] vertices;

        ushort[] indices;

        int vertexCount;

        int indexCount;

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
                    var newVertices = new VertexPositionNormalTexture[value];
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

        public InterChunkMeshPart()
        {
            vertices = new VertexPositionNormalTexture[defaultVertexCapacity];
            indices = new ushort[defaultIndexCapacity];
        }

        public void Populate(ChunkMeshPart destination)
        {
            if (destination == null) throw new ArgumentNullException("destination");

            destination.SetVertices(vertices, vertexCount);
            destination.SetIndices(indices, indexCount);
        }

        public void AddIndex(ushort index)
        {
            if (indexCount == indices.Length) EnsureIndexCapacity(indexCount + indices.Length);

            indices[indexCount++] = (ushort) (vertexCount + index);
        }

        public void AddVertex(ref VertexPositionNormalTexture vertex)
        {
            if (vertexCount == vertices.Length) EnsureVertexCapacity(vertexCount + 1);

            vertices[vertexCount++] = vertex;
        }

        public void Clear()
        {
            vertexCount = 0;
            indexCount = 0;
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
