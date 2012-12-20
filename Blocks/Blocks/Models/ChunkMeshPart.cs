#region Using

using System;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Diagnostics;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class ChunkMeshPart : IDisposable
    {
        static readonly Logger logger = new Logger(typeof(ChunkMeshPart).Name);

        // TODO
        //
        // 実行で最適と思われる値を調べて決定する。
        const ushort defaultVertexCapacity = 10000;

        const int defaultIndexCapacity = 10000;

        static readonly VertexPositionNormalTexture[] emptyVertices = new VertexPositionNormalTexture[0];
        
        static readonly ushort[] emptyIndices = new ushort[0];

        VertexPositionNormalTexture[] vertices;

        ushort[] indices;

        int vertexCount;

        int indexCount;

        public GraphicsDevice GraphicsDevice { get; private set; }

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

        public DynamicVertexBuffer VertexBuffer { get; private set; }

        public DynamicIndexBuffer IndexBuffer { get; private set; }

        public ChunkMeshPart(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");

            GraphicsDevice = graphicsDevice;

            vertices = new VertexPositionNormalTexture[defaultVertexCapacity];
            indices = new ushort[defaultIndexCapacity];
        }

        public void AddIndices(ushort[] indices)
        {
            foreach (var index in indices) AddIndex(index);
        }

        public void AddIndex(ushort index)
        {
            if (indexCount == indices.Length) EnsureIndexCapacity(indexCount + 1);

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

        public void Draw()
        {
            GraphicsDevice.SetVertexBuffer(VertexBuffer);
            GraphicsDevice.Indices = IndexBuffer;
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexCount, 0, indexCount / 3);
        }

        public void BuildBuffer()
        {
            if (vertexCount == 0 || indexCount == 0) return;

            if (VertexBuffer == null) VertexBuffer = CreateVertexBuffer();
            if (IndexBuffer == null) IndexBuffer = CreateIndexBuffer();

            VertexBuffer.SetData(vertices, 0, vertexCount, SetDataOptions.Discard);
            IndexBuffer.SetData(indices, 0, indexCount, SetDataOptions.Discard);
        }

        DynamicVertexBuffer CreateVertexBuffer()
        {
            return new DynamicVertexBuffer(GraphicsDevice, typeof(VertexPositionNormalTexture), defaultVertexCapacity, BufferUsage.WriteOnly);
        }

        DynamicIndexBuffer CreateIndexBuffer()
        {
            return new DynamicIndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, defaultIndexCapacity, BufferUsage.WriteOnly);
        }

        // TODO
        //
        // 実行で最適と思われる拡張ロジックへ変更する。
        // 少なくとも、頂点数は非常に多いと予想されるため、二倍による拡張は適切ではない。
        void EnsureVertexCapacity(int capacity)
        {
            if (vertices.Length < capacity)
            {
                var newCapacity = vertices.Length == 0 ? defaultVertexCapacity : vertices.Length * 2;
                if (newCapacity < capacity) newCapacity = capacity;
                VertexCapacity = newCapacity;

                logger.Warn("EnsureVertexCapacity: newCapacity={0}", newCapacity);
            }
        }

        void EnsureIndexCapacity(int capacity)
        {
            if (indices.Length < capacity)
            {
                var newCapacity = indices.Length == 0 ? defaultIndexCapacity : indices.Length * 2;
                if (newCapacity < capacity) newCapacity = capacity;
                IndexCapacity = newCapacity;

                logger.Warn("EnsureIndexCapacity: newCapacity={0}", newCapacity);
            }
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed;

        ~ChunkMeshPart()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposed) return;

            if (VertexBuffer != null) VertexBuffer.Dispose();
            if (IndexBuffer != null) IndexBuffer.Dispose();

            disposed = true;
        }

        #endregion
    }
}
