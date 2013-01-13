#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Diagnostics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    /// <summary>
    /// シーンへ色をブレンドするポスト プロセッサです。
    /// </summary>
    public sealed class ColorOverlap : PostProcessor, IDisposable
    {
        public const string MonitorProcess = "ColorOverlap.Process";

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

        public ColorOverlap(SpriteBatch spriteBatch)
            : base(spriteBatch)
        {
            fillTexture = Texture2DHelper.CreateFillTexture(GraphicsDevice);
        }

        public override void Process(IPostProcessorContext context)
        {
            Monitor.Begin(MonitorProcess);

            GraphicsDevice.SetRenderTarget(context.Destination);
            SpriteBatch.Begin();
            SpriteBatch.Draw(context.Source, context.Destination.Bounds, Color.White);
            SpriteBatch.Draw(fillTexture, context.Destination.Bounds, Color);
            SpriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);

            Monitor.End(MonitorProcess);
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
