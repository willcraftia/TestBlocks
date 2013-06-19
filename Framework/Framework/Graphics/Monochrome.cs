#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Diagnostics;

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

        public const string InstrumentProcess = "Monochrome.Process";

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

        public override void Process(IPostProcessorContext context)
        {
            Instrument.Begin(InstrumentProcess);

            monochromeEffect.Cb = cbCr.X;
            monochromeEffect.Cr = cbCr.Y;

            GraphicsDevice.SetRenderTarget(context.Destination);
            SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, null, null, null, monochromeEffect.Effect);
            SpriteBatch.Draw(context.Source, context.Destination.Bounds, Color.White);
            SpriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);

            Instrument.End();
        }
    }
}
