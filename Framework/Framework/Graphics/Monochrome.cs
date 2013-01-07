#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class Monochrome : PostProcessor
    {
        #region MonochromeEffect

        sealed class MonochromeEffect
        {
            EffectParameter cb;
            
            EffectParameter cr;

            public Effect Effect { get; private set; }

            public float Cb
            {
                get { return cb.GetValueSingle(); }
                set { cb.SetValue(value); }
            }

            public float Cr
            {
                get { return cr.GetValueSingle(); }
                set { cr.SetValue(value); }
            }

            public MonochromeEffect(Effect effect)
            {
                Effect = effect;

                cb = effect.Parameters["Cb"];
                cr = effect.Parameters["Cr"];
            }
        }

        #endregion

        public static Vector2 Grayscale
        {
            get { return new Vector2(0.0f, 0.0f); }
        }

        public static Vector2 SepiaTone
        {
            get { return new Vector2(-0.1f, 0.1f); }
        }

        GraphicsDevice graphicsDevice;

        SpriteBatch spriteBatch;

        MonochromeEffect monochromeEffect;

        Vector2 cbCr = Grayscale;

        public Vector2 CbCr
        {
            get { return cbCr; }
            set { cbCr = value; }
        }

        public Monochrome(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, Effect monochromeEffect)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");
            if (spriteBatch == null) throw new ArgumentNullException("spriteBatch");

            this.graphicsDevice = graphicsDevice;
            this.spriteBatch = spriteBatch;

            this.monochromeEffect = new MonochromeEffect(monochromeEffect);
        }

        public override void Process(IPostProcessorContext context, RenderTarget2D source, RenderTarget2D destination)
        {
            monochromeEffect.Cb = cbCr.X;
            monochromeEffect.Cr = cbCr.Y;

            graphicsDevice.SetRenderTarget(destination);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, null, null, null, monochromeEffect.Effect);
            spriteBatch.Draw(source, destination.Bounds, Color.White);
            spriteBatch.End();
            graphicsDevice.SetRenderTarget(null);
        }
    }
}
