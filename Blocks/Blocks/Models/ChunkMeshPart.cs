#region Using

using System;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Diagnostics;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class ChunkMeshPart
    {
        public GraphicsDevice GraphicsDevice { get; private set; }

        public DynamicVertexBuffer VertexBuffer { get; internal set; }

        public DynamicIndexBuffer IndexBuffer { get; internal set; }

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
            if (VertexBuffer == null || IndexBuffer == null) return;

            GraphicsDevice.SetVertexBuffer(VertexBuffer);
            GraphicsDevice.Indices = IndexBuffer;
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, VertexBuffer.VertexCount, 0, IndexBuffer.IndexCount / 3);
        }

        public void BuildBuffer()
        {
            if (InterChunkMeshPart == null || VertexBuffer == null || IndexBuffer == null) return;

            VertexCount = InterChunkMeshPart.VertexCount;
            IndexCount = InterChunkMeshPart.IndexCount;

            InterChunkMeshPart.PopulateVertexBuffer(VertexBuffer);
            InterChunkMeshPart.PopulateIndexBuffer(IndexBuffer);
        }
    }
}
