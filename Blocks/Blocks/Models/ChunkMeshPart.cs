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

        public int VertexCount
        {
            get
            {
                return VertexBuffer != null ? VertexBuffer.VertexCount : 0;
            }
        }

        public int IndexCount
        {
            get
            {
                return IndexBuffer != null ? IndexBuffer.IndexCount : 0;
            }
        }

        public bool Occluded { get; private set; }

        OcclusionQuery occlusionQuery;

        bool occlusionQueryActive;

        public ChunkMeshPart(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");

            GraphicsDevice = graphicsDevice;

            occlusionQuery = new OcclusionQuery(graphicsDevice);
        }

        public void UpdateOcclusion()
        {
            if (VertexBuffer == null || IndexBuffer == null) return;

            Occluded = false;

            if (occlusionQueryActive)
            {
                if (!occlusionQuery.IsComplete) return;

                Occluded = (occlusionQuery.PixelCount == 0);
            }

            occlusionQuery.Begin();

            GraphicsDevice.SetVertexBuffer(VertexBuffer);
            GraphicsDevice.Indices = IndexBuffer;
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, VertexBuffer.VertexCount, 0, IndexBuffer.IndexCount / 3);

            occlusionQuery.End();
            occlusionQueryActive = true;
        }

        public void Draw()
        {
            if (VertexBuffer == null || IndexBuffer == null) return;
            if (Occluded) return;

            GraphicsDevice.SetVertexBuffer(VertexBuffer);
            GraphicsDevice.Indices = IndexBuffer;
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, VertexBuffer.VertexCount, 0, IndexBuffer.IndexCount / 3);
        }
    }
}
