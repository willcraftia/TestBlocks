#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public abstract class PrimitiveMesh : IDisposable
    {
        VertexPositionNormal[] vertices;

        ushort[] indices;

        int primitiveCount;

        int currentVertexCount;

        int currentIndexCount;

        VertexBuffer vertexBuffer;
        
        IndexBuffer indexBuffer;

        protected int CurrentVertex
        {
            get { return currentVertexCount; }
        }

        protected PrimitiveMesh() { }

        public void Draw(Effect effect)
        {
            var graphicsDevice = effect.GraphicsDevice;

            graphicsDevice.SetVertexBuffer(vertexBuffer);
            graphicsDevice.Indices = indexBuffer;

            foreach (var effectPass in effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertices.Length, 0, primitiveCount);
            }
        }

        protected void Allocate(int vertexCount, int indexCount)
        {
            vertices = new VertexPositionNormal[vertexCount];
            indices = new ushort[indexCount];
            primitiveCount = indices.Length / 3;
        }

        protected void AddVertex(Vector3 position, Vector3 normal)
        {
            vertices[currentVertexCount++] = new VertexPositionNormal(position, normal);
        }

        protected void AddIndex(int index)
        {
            if (ushort.MaxValue < index) throw new ArgumentOutOfRangeException("index");

            indices[currentIndexCount++] = (ushort) index;
        }

        protected void Build(GraphicsDevice graphicsDevice)
        {
            vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionNormal), vertices.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices);

            indexBuffer = new IndexBuffer(graphicsDevice, typeof(ushort), indices.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices);
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed;

        ~PrimitiveMesh()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposed) return;

            if (vertexBuffer != null) vertexBuffer.Dispose();
            if (indexBuffer != null) indexBuffer.Dispose();

            disposed = true;
        }

        #endregion
    }
}
