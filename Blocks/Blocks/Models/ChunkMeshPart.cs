#region Using

using System;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Diagnostics;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class ChunkMeshPart : IDisposable
    {
        // TODO
        //
        // 実行で最適と思われる値を調べて決定する。
        const ushort defaultVertexCapacity = 10000;

        const ushort defaultIndexCapacity = 10000;

        public GraphicsDevice GraphicsDevice { get; private set; }

        public DynamicVertexBuffer VertexBuffer { get; private set; }

        public DynamicIndexBuffer IndexBuffer { get; private set; }

        public int VertexCount { get; private set; }

        public int IndexCount { get; private set; }

        internal InterChunkMeshPart InterChunkMeshPart { get; set; }

        public ChunkMeshPart(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");

            GraphicsDevice = graphicsDevice;
        }

        public void Draw()
        {
            GraphicsDevice.SetVertexBuffer(VertexBuffer);
            GraphicsDevice.Indices = IndexBuffer;
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, VertexBuffer.VertexCount, 0, IndexBuffer.IndexCount / 3);
        }

        public void BuildBuffer()
        {
            if (InterChunkMeshPart == null) return;

            VertexCount = InterChunkMeshPart.VertexCount;
            IndexCount = InterChunkMeshPart.IndexCount;

            if (VertexCount == 0 || IndexCount == 0) return;

            if (VertexBuffer == null) VertexBuffer = CreateVertexBuffer();
            if (IndexBuffer == null) IndexBuffer = CreateIndexBuffer();

            InterChunkMeshPart.PopulateVertexBuffer(VertexBuffer);
            InterChunkMeshPart.PopulateIndexBuffer(IndexBuffer);
        }

        DynamicVertexBuffer CreateVertexBuffer()
        {
            return new DynamicVertexBuffer(GraphicsDevice, typeof(VertexPositionNormalTexture), defaultVertexCapacity, BufferUsage.WriteOnly);
        }

        DynamicIndexBuffer CreateIndexBuffer()
        {
            return new DynamicIndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, defaultIndexCapacity, BufferUsage.WriteOnly);
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
