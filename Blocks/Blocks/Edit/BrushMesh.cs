#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Blocks.Edit
{
    public sealed class BrushMesh : SceneObject, IDisposable
    {
        CubeMesh mesh;

        BasicEffect effect;

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

        public BrushMesh(string name, GraphicsDevice graphicsDevice)
            : base(name)
        {
            mesh = new CubeMesh(graphicsDevice);
            effect = new BasicEffect(graphicsDevice);
            effect.VertexColorEnabled = true;
        }

        public override void Draw()
        {
            var graphicsDevice = effect.GraphicsDevice;

            graphicsDevice.BlendState = BlendState.AlphaBlend;
            graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

            mesh.Draw(effect);

            graphicsDevice.BlendState = BlendState.Opaque;
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
                mesh.Dispose();
                effect.Dispose();
            }

            disposed = true;
        }

        #endregion
    }
}
