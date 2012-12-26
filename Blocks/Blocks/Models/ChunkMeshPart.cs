#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Diagnostics;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class ChunkMeshPart
    {
        GraphicsDevice graphicsDevice;

        VertexBuffer vertexBuffer;

        IndexBuffer indexBuffer;

        int vertexCount;

        int indexCount;

        int primitiveCount;

        bool occluded;

        OcclusionQuery occlusionQuery;

        bool occlusionQueryActive;

        public VertexBuffer VertexBuffer
        {
            get { return vertexBuffer; }
            set
            {
                vertexBuffer = value;
                VertexCount = 0;
            }
        }

        public IndexBuffer IndexBuffer
        {
            get { return indexBuffer; }
            set
            {
                indexBuffer = value;
                IndexCount = 0;
            }
        }

        public int VertexCount
        {
            get { return vertexCount; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");

                vertexCount = value;
            }
        }

        public int IndexCount
        {
            get { return indexCount; }
            internal set
            {
                if (value < 0 || ushort.MaxValue < value) throw new ArgumentOutOfRangeException("value");

                indexCount = value;
                primitiveCount = indexCount / 3;
            }
        }

        public bool Occluded
        {
            get { return occluded; }
        }

        public ChunkMeshPart(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");

            this.graphicsDevice = graphicsDevice;

            occlusionQuery = new OcclusionQuery(graphicsDevice);
        }

        public void SetVertices(VertexPositionNormalTexture[] vertices, int vertexCount)
        {
            if (vertices == null) throw new ArgumentNullException("vertices");
            if (vertexCount < 0 || vertices.Length < vertexCount) throw new ArgumentOutOfRangeException("vertexCount");
            if (VertexBuffer == null) throw new InvalidOperationException("VertexBuffer is null.");

            if (vertexCount != 0 && vertices.Length != 0)
            {
                VertexBuffer.SetData(vertices, 0, vertexCount);
                this.vertexCount = vertexCount;
            }
            else
            {
                this.vertexCount = 0;
            }
        }

        public void SetIndices(ushort[] indices, int indexCount)
        {
            if (indices == null) throw new ArgumentNullException("indices");
            if (indexCount < 0 || indices.Length < indexCount) throw new ArgumentOutOfRangeException("vertexCount");
            if (IndexBuffer == null) throw new InvalidOperationException("IndexBuffer is null.");

            if (indexCount != 0 && indices.Length != 0)
            {
                IndexBuffer.SetData(indices, 0, indexCount);
                IndexCount = indexCount;
            }
            else
            {
                IndexCount = 0;
            }
        }

        public void UpdateOcclusion()
        {
            if (vertexBuffer == null || indexBuffer == null || vertexCount == 0 || indexCount == 0)
                return;

            occluded = false;

            if (occlusionQueryActive)
            {
                if (!occlusionQuery.IsComplete) return;

                occluded = (occlusionQuery.PixelCount == 0);
            }

            occlusionQuery.Begin();

            graphicsDevice.SetVertexBuffer(vertexBuffer);
            graphicsDevice.Indices = indexBuffer;
            graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexCount, 0, primitiveCount);

            occlusionQuery.End();
            occlusionQueryActive = true;
        }

        public void Draw(ChunkEffect effect, ref Matrix world)
        {
            if (vertexBuffer == null || indexBuffer == null || vertexCount == 0 || indexCount == 0)
                return;
            if (occluded) return;

            effect.World = world;
            effect.Apply();

            graphicsDevice.SetVertexBuffer(vertexBuffer);
            graphicsDevice.Indices = indexBuffer;
            graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexCount, 0, primitiveCount);
        }
    }
}
