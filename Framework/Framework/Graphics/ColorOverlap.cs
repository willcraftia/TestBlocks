#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    /// <summary>
    /// シーンへ色をブレンドするポスト プロセッサです。
    /// </summary>
    public sealed class ColorOverlap : PostProcessor, IDisposable
    {
        GraphicsDevice graphicsDevice;

        SpriteBatch spriteBatch;

        Texture2D fillTexture;

        Color color = Color.Black * 0.5f;

        /// <summary>
        /// シーンへブレンドする色を取得または設定します。
        /// 乗算済みアルファの色を設定する必要があります。
        /// </summary>
        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        public ColorOverlap(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");
            if (spriteBatch == null) throw new ArgumentNullException("spriteBatch");

            this.graphicsDevice = graphicsDevice;
            this.spriteBatch = spriteBatch;

            fillTexture = Texture2DHelper.CreateFillTexture(graphicsDevice);
        }

        public override void Process(IPostProcessorContext context, RenderTarget2D source, RenderTarget2D destination)
        {
            graphicsDevice.SetRenderTarget(destination);
            spriteBatch.Begin();
            spriteBatch.Draw(source, destination.Bounds, Color.White);
            spriteBatch.Draw(fillTexture, destination.Bounds, Color);
            spriteBatch.End();
            graphicsDevice.SetRenderTarget(null);
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed;

        ~ColorOverlap()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                fillTexture.Dispose();
            }

            disposed = true;
        }

        #endregion
    }
}
