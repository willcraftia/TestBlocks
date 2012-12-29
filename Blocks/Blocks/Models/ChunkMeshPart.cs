#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Diagnostics;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class ChunkMeshPart : IShadowCaster
    {
        Region region;

        VertexBuffer vertexBuffer;

        IndexBuffer indexBuffer;

        int vertexCount;

        int indexCount;

        int primitiveCount;

        BoundingSphere boundingSphere;

        BoundingBox boundingBox;

        bool occluded;

        OcclusionQuery occlusionQuery;

        bool occlusionQueryActive;

        Vector3 position;

        Matrix world;

        // I/F
        public bool CastShadow
        {
            // チャンクはブロックの設定とは異なり、常に一定の結果を返す。
            get { return true; }
        }

        // I/F
        public ISceneObjectContext Context
        {
            set { }
        }

        // I/F
        public bool Visible
        {
            // 常に true。
            // 頂点を持たない場合でも true だが、その場合はシーン マネージャに登録されず、
            // 結果として描画されない。
            get { return true; }
        }

        // I/F
        public bool Translucent { get; private set; }

        // I/F
        public bool Occluded
        {
            get { return occluded; }
        }

        public GraphicsDevice GraphicsDevice { get; private set; }

        public Chunk Chunk { get; set; }

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

        public Vector3 Position
        {
            get { return position; }
            set
            {
                position = value;
                Matrix.CreateTranslation(ref position, out world);
            }
        }

        public ChunkMeshPart(Region region, bool translucent)
        {
            if (region == null) throw new ArgumentNullException("region");

            this.region = region;
            Translucent = translucent;

            GraphicsDevice = region.GraphicsDevice;

            occlusionQuery = new OcclusionQuery(GraphicsDevice);
        }

        // I/F
        public void DrawShadow()
        {
            throw new NotImplementedException();
        }

        // I/F
        public void GetDistanceSquared(ref Vector3 eyePosition, out float result)
        {
            // 中心座標を算出。
            var chunkSize = region.ChunkSize;
            var halfSize = new Vector3(chunkSize.X * 0.5f, chunkSize.Y * 0.5f, chunkSize.Z * 0.5f);
            var centerPosition = position + halfSize;

            Vector3.DistanceSquared(ref eyePosition, ref centerPosition, out result);
        }

        // I/F
        public void GetBoundingSphere(out BoundingSphere result)
        {
            result = boundingSphere;
        }

        // I/F
        public void GetBoundingBox(out BoundingBox result)
        {
            result = boundingBox;
        }

        // I/F
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

            var effect = region.ChunkEffect;

            //----------------------------------------------------------------
            // タイル カタログのテクスチャ

            var tileCatalog = region.TileCatalog;
            effect.TileMap = tileCatalog.TileMap;
            effect.DiffuseMap = tileCatalog.DiffuseColorMap;
            effect.EmissiveMap = tileCatalog.EmissiveColorMap;
            effect.SpecularMap = tileCatalog.SpecularColorMap;

            //----------------------------------------------------------------
            // 変換行列

            effect.World = world;
            effect.Apply();

            GraphicsDevice.SetVertexBuffer(vertexBuffer);
            GraphicsDevice.Indices = indexBuffer;
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexCount, 0, primitiveCount);

            occlusionQuery.End();
            occlusionQueryActive = true;
        }

        // I/F
        public void Draw(Texture2D shadowMap)
        {
            if (vertexBuffer == null || indexBuffer == null || vertexCount == 0 || indexCount == 0)
                return;
            if (occluded) return;

            // チャンクに描画ロックを要求。
            if (Chunk != null && !Chunk.EnterDraw()) return;

            var effect = region.ChunkEffect;

            //----------------------------------------------------------------
            // タイル カタログのテクスチャ

            var tileCatalog = region.TileCatalog;
            effect.TileMap = tileCatalog.TileMap;
            effect.DiffuseMap = tileCatalog.DiffuseColorMap;
            effect.EmissiveMap = tileCatalog.EmissiveColorMap;
            effect.SpecularMap = tileCatalog.SpecularColorMap;

            //----------------------------------------------------------------
            // 変換行列

            effect.World = world;
            effect.Apply();

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

        internal void SetBoundingSphere(ref BoundingSphere boundingSphere)
        {
            this.boundingSphere = boundingSphere;
        }

        internal void SetBoundingBox(ref BoundingBox boundingBox)
        {
            this.boundingBox = boundingBox;
        }
    }
}
