#region Using

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Diagnostics;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class ChunkMesh : ShadowCaster, IDisposable
    {
        ChunkEffect chunkEffect;

        GraphicsDevice graphicsDevice;

        Matrix world = Matrix.Identity;

        OcclusionQuery occlusionQuery;

        bool occlusionQueryActive;

        DynamicVertexBuffer vertexBuffer;

        DynamicIndexBuffer indexBuffer;

        public Matrix World
        {
            get { return world; }
            set { world = value; }
        }

        public int VertexCount { get; private set; }

        public int IndexCount { get; private set; }

        public int PrimitiveCount { get; private set; }

        public ChunkMesh(string name, ChunkEffect chunkEffect)
            : base(name)
        {
            if (chunkEffect == null) throw new ArgumentNullException("chunkEffect");

            this.chunkEffect = chunkEffect;
            this.graphicsDevice = chunkEffect.GraphicsDevice;

            occlusionQuery = new OcclusionQuery(graphicsDevice);
        }

        public override void UpdateOcclusion()
        {
            Occluded = false;

            if (occlusionQueryActive)
            {
                if (!occlusionQuery.IsComplete) return;

                Occluded = (occlusionQuery.PixelCount == 0);
            }

            occlusionQuery.Begin();

            //----------------------------------------------------------------
            // エフェクト

            chunkEffect.EnableOcclusionQueryTechnique();

            chunkEffect.World = world;
            chunkEffect.CurrentTechnique.Passes[0].Apply();

            //----------------------------------------------------------------
            // 描画

            DrawCore();

            occlusionQuery.End();
            occlusionQueryActive = true;
        }

        public override void Draw()
        {
            if (Occluded) return;

            //----------------------------------------------------------------
            // エフェクト

            chunkEffect.ResolveCurrentTechnique();

            chunkEffect.World = world;
            chunkEffect.CurrentTechnique.Passes[0].Apply();

            //----------------------------------------------------------------
            // 描画

            DrawCore();
        }

        public override void Draw(Effect effect)
        {
            if (Occluded) return;

            var effectMatrices = effect as IEffectMatrices;
            if (effectMatrices != null) effectMatrices.World = world;

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                DrawCore();
            }
        }

        public void SetVertices(VertexPositionNormalColorTexture[] vertices, int vertexCount)
        {
            if (vertices == null) throw new ArgumentNullException("vertices");
            if (vertexCount < 0 || vertices.Length < vertexCount) throw new ArgumentOutOfRangeException("vertexCount");

            VertexCount = vertexCount;

            if (vertexCount != 0)
            {
                if (vertexBuffer != null && vertexBuffer.VertexCount != vertexCount)
                {
                    vertexBuffer.Dispose();
                    vertexBuffer = null;
                }

                if (vertexBuffer == null)
                    vertexBuffer = new DynamicVertexBuffer(
                        graphicsDevice, typeof(VertexPositionNormalColorTexture), vertexCount, BufferUsage.None);
                
                vertexBuffer.SetData(vertices, 0, vertexCount, SetDataOptions.Discard);
            }
            else
            {
                if (vertexBuffer != null)
                {
                    vertexBuffer.Dispose();
                    vertexBuffer = null;
                }
            }
        }

        public void SetIndices(ushort[] indices, int indexCount)
        {
            if (indices == null) throw new ArgumentNullException("indices");
            if (indexCount < 0 || indices.Length < indexCount) throw new ArgumentOutOfRangeException("vertexCount");

            IndexCount = indexCount;
            PrimitiveCount = indexCount / 3;

            if (indexCount != 0)
            {
                if (indexBuffer != null && indexBuffer.IndexCount != indexCount)
                {
                    indexBuffer.Dispose();
                    indexBuffer = null;
                }

                if (indexBuffer == null)
                    indexBuffer = new DynamicIndexBuffer(
                        graphicsDevice, IndexElementSize.SixteenBits, indexCount, BufferUsage.None);

                indexBuffer.SetData(indices, 0, indexCount, SetDataOptions.Discard);
            }
            else
            {
                if (indexBuffer != null)
                {
                    indexBuffer.Dispose();
                    indexBuffer = null;
                }
            }
        }

        void DrawCore()
        {
            graphicsDevice.SetVertexBuffer(vertexBuffer);
            graphicsDevice.Indices = indexBuffer;
            graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, VertexCount, 0, PrimitiveCount);
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed;

        ~ChunkMesh()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                if (vertexBuffer != null) vertexBuffer.Dispose();
                if (indexBuffer != null) indexBuffer.Dispose();
            }

            disposed = true;
        }

        #endregion
    }
}
