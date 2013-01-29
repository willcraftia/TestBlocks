#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Blocks.Edit
{
    public sealed class BrushMesh : SceneObject, IDisposable
    {
        CubeMesh cube;

        public BasicEffect Effect { get; private set; }

        public BrushMesh(string name, GraphicsDevice graphicsDevice)
            : base(name)
        {
            cube = new CubeMesh(graphicsDevice);
            Effect = new BasicEffect(graphicsDevice);
            Effect.VertexColorEnabled = true;
        }

        public override void Draw()
        {
            var graphicsDevice = Effect.GraphicsDevice;

            graphicsDevice.BlendState = BlendState.AlphaBlend;
            graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

            cube.Draw(Effect);

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
                cube.Dispose();
                Effect.Dispose();
            }

            disposed = true;
        }

        #endregion
    }
}
