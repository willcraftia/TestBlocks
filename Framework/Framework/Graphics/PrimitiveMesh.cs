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

        int currentVertexCount;

        int currentIndexCount;

        public VertexBuffer VertexBuffer { get; private set; }

        public IndexBuffer IndexBuffer { get; private set; }

        public PrimitiveType PrimitiveType
        {
            get { return PrimitiveType.TriangleList; }
        }

        public int NumVertices { get; private set; }

        public int PrimitiveCount { get; private set; }

        protected int CurrentVertex
        {
            get { return currentVertexCount; }
        }

        protected PrimitiveMesh() { }

        public void Draw(Effect effect)
        {
            var graphicsDevice = effect.GraphicsDevice;

            graphicsDevice.SetVertexBuffer(VertexBuffer);
            graphicsDevice.Indices = IndexBuffer;

            for (int i = 0; i < effect.CurrentTechnique.Passes.Count; i++)
            {
                effect.CurrentTechnique.Passes[i].Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType, 0, 0, NumVertices, 0, PrimitiveCount);
            }
        }

        protected void Allocate(int vertexCount, int indexCount)
        {
            vertices = new VertexPositionNormal[vertexCount];
            indices = new ushort[indexCount];
            NumVertices = vertexCount;
            PrimitiveCount = indices.Length / 3;
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
            VertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionNormal), vertices.Length, BufferUsage.WriteOnly);
            VertexBuffer.SetData(vertices);

            IndexBuffer = new IndexBuffer(graphicsDevice, typeof(ushort), indices.Length, BufferUsage.WriteOnly);
            IndexBuffer.SetData(indices);
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

            if (VertexBuffer != null) VertexBuffer.Dispose();
            if (IndexBuffer != null) IndexBuffer.Dispose();

            disposed = true;
        }

        #endregion
    }
}
