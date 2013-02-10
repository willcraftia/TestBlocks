#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Collections;
using Willcraftia.Xna.Framework.Graphics;
using Willcraftia.Xna.Blocks.Models;

#endregion

namespace Willcraftia.Xna.Blocks.Edit
{
    public sealed class BrushMesh : SceneObject, IDisposable
    {
        GraphicsDevice graphicsDevice;

        Mesh mesh;

        BasicEffect effect;

        CubicCollection<VertexBuffer> vertexBuffers = new CubicCollection<VertexBuffer>();

        CubicCollection<IndexBuffer> indexBuffers = new CubicCollection<IndexBuffer>();

        Texture2D fillTexture;

        Matrix scale;

        public Vector3 Color
        {
            get { return effect.DiffuseColor; }
            set { effect.DiffuseColor = value; }
        }

        public float Alpha
        {
            get { return effect.Alpha; }
            set { effect.Alpha = value; }
        }

        public bool VisibleAllFaces { get; set; }

        public CubicSide VisibleFace { get; set; }

        public BrushMesh(string name, GraphicsDevice graphicsDevice, Mesh mesh)
            : base(name)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");
            if (mesh == null) throw new ArgumentNullException("mesh");

            this.graphicsDevice = graphicsDevice;
            this.mesh = mesh;

            effect = new BasicEffect(graphicsDevice);

            Translucent = true;

            for (int i = 0; i < CubicSide.Count; i++)
            {
                var meshPart = mesh.MeshParts[i];
                if (meshPart == null) continue;

                var vertexCount = meshPart.Vertices.Length;
                var indexCount = meshPart.Indices.Length;

                if (vertexCount == 0 || indexCount == 0) continue;

                vertexBuffers[i] = new VertexBuffer(graphicsDevice, typeof(VertexPositionNormalTexture), vertexCount, BufferUsage.WriteOnly);
                vertexBuffers[i].SetData(meshPart.Vertices);

                indexBuffers[i] = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, indexCount, BufferUsage.WriteOnly);
                indexBuffers[i].SetData(meshPart.Indices);
            }

            fillTexture = Texture2DHelper.CreateFillTexture(graphicsDevice);
            effect.Texture = fillTexture;
            effect.TextureEnabled = true;

            Matrix.CreateScale(1.001f, out scale);
        }

        public override void Draw()
        {
            Parent.Manager.UpdateEffect(effect);

            Matrix translation;
            Matrix.CreateTranslation(ref PositionWorld, out translation);

            Matrix world;
            Matrix.Multiply(ref scale, ref translation, out world);

            effect.World = world;

            effect.CurrentTechnique.Passes[0].Apply();

            for (int i = 0; i < CubicSide.Count; i++)
            {
                if (!VisibleAllFaces && VisibleFace != CubicSide.Items[i]) continue;

                var vertexBuffer = vertexBuffers[i];
                var indexBuffer = indexBuffers[i];

                if (vertexBuffer == null || indexBuffer == null) continue;

                graphicsDevice.SetVertexBuffer(vertexBuffer);
                graphicsDevice.Indices = indexBuffer;
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexBuffer.VertexCount, 0, indexBuffer.IndexCount / 3);
            }
        }

        public override void Draw(Effect effect)
        {
            // 専用エフェクトによる特殊描画には参加しない。
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed;

        ~BrushMesh()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                for (int i = 0; i < CubicSide.Count; i++)
                {
                    if (vertexBuffers[i] != null) vertexBuffers[i].Dispose();
                    if (indexBuffers[i] != null) indexBuffers[i].Dispose();
                }
                fillTexture.Dispose();
                effect.Dispose();
            }

            disposed = true;
        }

        #endregion
    }
}
