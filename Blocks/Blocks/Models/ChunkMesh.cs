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
        //
        // メモ
        //
        // 過去、頂点バッファとインデックス バッファをプーリングしていたが、
        // チャンク メッシュの頂点数には非常に大きなばらつきがあるため、
        // チャンク メッシュの構築毎に、頂点バッファとインデックス バッファを必要十分なサイズで確保する。
        // また、チャンク メッシュの破棄では、頂点バッファとインデックス バッファも破棄する。
        //
        // 過去、チャンク メッシュ自体もプーリングしていたが、
        // 頂点バッファとインデックス バッファのプーリングを行わないならば、
        // チャンク メッシュのプーリングの意味もほぼ失われるため、
        // チャンク メッシュのプーリングも行わない。
        //

        static readonly VectorI3 chunkSize = Chunk.Size;

        GraphicsDevice graphicsDevice;

        Matrix world = Matrix.Identity;

        OcclusionQuery occlusionQuery;

        bool occlusionQueryActive;

        public Chunk Chunk { get; set; }

        public Matrix World
        {
            get { return world; }
            set { world = value; }
        }

        public VertexBuffer VertexBuffer { get; private set; }

        public IndexBuffer IndexBuffer { get; private set; }

        public int VertexCount { get; private set; }

        public int IndexCount { get; private set; }

        public int PrimitiveCount { get; private set; }

        public ChunkMesh(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");

            this.graphicsDevice = graphicsDevice;

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

            // 描画ロックを取得できない場合は終了。
            if (!EnterDraw()) return;

            occlusionQuery.Begin();

            //----------------------------------------------------------------
            // エフェクト

            var effect = Chunk.Region.ChunkEffect;

            effect.EnableOcclusionQueryTechnique();

            effect.World = world;
            effect.CurrentTechnique.Passes[0].Apply();

            //----------------------------------------------------------------
            // 描画

            DrawCore();

            occlusionQuery.End();
            occlusionQueryActive = true;

            // 描画ロックを解放。
            ExitDraw();
        }

        public override void Draw()
        {
            if (Occluded) return;

            // 描画ロックを取得できない場合は終了。
            if (!EnterDraw()) return;

            //----------------------------------------------------------------
            // エフェクト

            var effect = Chunk.Region.ChunkEffect;

            effect.ResolveCurrentTechnique();

            effect.World = world;
            effect.CurrentTechnique.Passes[0].Apply();

            //----------------------------------------------------------------
            // 描画

            DrawCore();

            // 描画ロックを解放。
            ExitDraw();
        }

        public override void Draw(Effect effect)
        {
            if (Occluded) return;

            // 描画ロックを取得できない場合は終了。
            if (!EnterDraw()) return;

            var effectMatrices = effect as IEffectMatrices;
            if (effectMatrices != null) effectMatrices.World = world;

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                DrawCore();
            }

            // 描画ロックを解放。
            ExitDraw();
        }

        public void SetVertices(VertexPositionNormalColorTexture[] vertices, int vertexCount)
        {
            if (vertices == null) throw new ArgumentNullException("vertices");
            if (vertexCount < 0 || vertices.Length < vertexCount) throw new ArgumentOutOfRangeException("vertexCount");

            VertexCount = vertexCount;

            if (vertexCount != 0)
            {
                VertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionNormalColorTexture), vertexCount, BufferUsage.None);
                VertexBuffer.SetData(vertices, 0, vertexCount);
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
                IndexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, indexCount, BufferUsage.None);
                IndexBuffer.SetData(indices, 0, indexCount);
            }
        }

        bool EnterDraw()
        {
            if (Chunk == null || !Chunk.EnterDraw()) return false;

            // 非同期なメッシュ更新により描画不要になっていないかを検査。
            if (disposed || VertexBuffer == null || IndexBuffer == null || VertexCount == 0 || IndexCount == 0)
            {
                // 即座に描画ロックを開放して終了。
                Chunk.ExitDraw();
                return false;
            }

            return true;
        }

        void ExitDraw()
        {
            // 描画ロックを解放。
            Chunk.ExitDraw();
        }

        void DrawCore()
        {
            graphicsDevice.SetVertexBuffer(VertexBuffer);
            graphicsDevice.Indices = IndexBuffer;
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
                if (VertexBuffer != null) VertexBuffer.Dispose();
                if (IndexBuffer != null) IndexBuffer.Dispose();
            }

            disposed = true;
        }

        #endregion
    }
}
