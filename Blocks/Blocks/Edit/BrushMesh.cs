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

        RasterizerState wireframe;

        public BasicEffect Effect { get; private set; }

        public BrushMesh(string name, GraphicsDevice graphicsDevice)
            : base(name)
        {
            cube = new CubeMesh(graphicsDevice, 1.001f);
            Effect = new BasicEffect(graphicsDevice);

            Translucent = true;

            wireframe = new RasterizerState
            {
                CullMode = CullMode.CullCounterClockwiseFace,
                FillMode = FillMode.WireFrame
            };
        }

        public override void Draw()
        {
            var graphicsDevice = Effect.GraphicsDevice;

            var prevBlendState = graphicsDevice.BlendState;
            var prevAlpha = Effect.Alpha;

            graphicsDevice.RasterizerState = wireframe;
            graphicsDevice.BlendState = BlendState.Opaque;
            Effect.Alpha = 1;
            cube.Draw(Effect);

            graphicsDevice.BlendState = prevBlendState;
            Effect.Alpha = prevAlpha;

            graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            cube.Draw(Effect);

            graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
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
