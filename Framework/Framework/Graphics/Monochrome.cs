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

        MonochromeEffect monochromeEffect;

        Vector2 cbCr = Grayscale;

        public Vector2 CbCr
        {
            get { return cbCr; }
            set { cbCr = value; }
        }

        public Monochrome(SpriteBatch spriteBatch, Effect monochromeEffect)
            : base(spriteBatch)
        {
            this.monochromeEffect = new MonochromeEffect(monochromeEffect);
        }

        public override void Process(IPostProcessorContext context, RenderTarget2D source, RenderTarget2D destination)
        {
            monochromeEffect.Cb = cbCr.X;
            monochromeEffect.Cr = cbCr.Y;

            GraphicsDevice.SetRenderTarget(destination);
            SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, null, null, null, monochromeEffect.Effect);
            SpriteBatch.Draw(source, destination.Bounds, Color.White);
            SpriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);
        }
    }
}
