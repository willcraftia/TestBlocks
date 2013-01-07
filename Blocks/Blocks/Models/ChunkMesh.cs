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
    public sealed class ChunkMesh : ShadowCaster
    {
        static readonly VectorI3 chunkSize = Chunk.Size;

        Region region;

        Matrix world = Matrix.Identity;

        VertexBuffer vertexBuffer;

        IndexBuffer indexBuffer;

        int vertexCount;

        int indexCount;

        int primitiveCount;

        OcclusionQuery occlusionQuery;

        bool occlusionQueryActive;

        public GraphicsDevice GraphicsDevice { get; private set; }

        public Chunk Chunk { get; set; }

        public Matrix World
        {
            get { return world; }
            set { world = value; }
        }

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

        public ChunkMesh(Region region)
        {
            if (region == null) throw new ArgumentNullException("region");

            this.region = region;

            GraphicsDevice = region.GraphicsDevice;
            occlusionQuery = new OcclusionQuery(GraphicsDevice);
        }

        public override void PreDraw()
        {
            // ワールド行列を更新。
            Matrix world;
            Matrix.CreateTranslation(ref Position, out world);

            base.PreDraw();
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

            var effect = region.ChunkEffect;
            effect.World = world;
            effect.Apply();

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

            var effect = region.ChunkEffect;
            effect.World = world;
            effect.Apply();

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

        public override void Draw(ShadowMap shadowMap)
        {
            if (Occluded) return;

            var effect = region.ChunkEffect;

            //----------------------------------------------------------------
            // エフェクトへシャドウ マップを設定

            effect.DepthBias = shadowMap.Settings.DepthBias;
            effect.SplitCount = shadowMap.Settings.SplitCount;
            effect.ShadowMapSize = shadowMap.Settings.Size;
            effect.SplitDistances = shadowMap.SplitDistances;
            effect.SplitLightViewProjections = shadowMap.SplitLightViewProjections;
            effect.SplitShadowMaps = shadowMap.SplitShadowMaps;

            //----------------------------------------------------------------
            // シャドウ マップ対応テクニックを設定

            effect.EnableShadowTechnique(shadowMap.Settings.Technique);

            //----------------------------------------------------------------
            // 変換行列

            effect.World = world;
            effect.Apply();

            //----------------------------------------------------------------
            // 描画

            DrawCore();
        }

        void DrawCore()
        {
            // チャンクに描画ロックを要求。
            if (Chunk != null && !Chunk.EnterDraw()) return;

            // 非同期なメッシュ更新により描画不要になっていないかを検査。
            if (vertexBuffer == null || indexBuffer == null || vertexCount == 0 || indexCount == 0)
            {
                // 描画をせずに描画ロックを開放して終了。
                Chunk.ExitDraw();
                return;
            }

            GraphicsDevice.SetVertexBuffer(vertexBuffer);
            GraphicsDevice.Indices = indexBuffer;
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexCount, 0, primitiveCount);

            // 描画ロックを解放。
            Chunk.ExitDraw();
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
    }
}
