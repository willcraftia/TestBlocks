#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class Bloom : PostProcessor, IDisposable
    {
        #region BloomExtractEffect

        sealed class BloomExtractEffect
        {
            EffectParameter threshold;
            
            public Effect Effect { get; private set; }

            public float Threshold
            {
                get { return threshold.GetValueSingle(); }
                set { threshold.SetValue(value); }
            }

            public BloomExtractEffect(Effect effect)
            {
                Effect = effect;

                threshold = effect.Parameters["Threshold"];
            }
        }

        #endregion

        #region BloomEffect

        public sealed class BloomEffect
        {
            public Effect Effect { get; private set; }

            EffectParameter bloomExtractMap;

            EffectParameter bloomIntensity;

            EffectParameter baseIntensity;

            EffectParameter bloomSaturation;

            EffectParameter baseSaturation;

            public Texture2D BloomExtractMap
            {
                get { return bloomExtractMap.GetValueTexture2D(); }
                set { bloomExtractMap.SetValue(value); }
            }

            public float BloomIntensity
            {
                get { return bloomIntensity.GetValueSingle(); }
                set { bloomIntensity.SetValue(value); }
            }

            public float BaseIntensity
            {
                get { return baseIntensity.GetValueSingle(); }
                set { baseIntensity.SetValue(value); }
            }

            public float BloomSaturation
            {
                get { return bloomSaturation.GetValueSingle(); }
                set { bloomSaturation.SetValue(value); }
            }

            public float BaseSaturation
            {
                get { return baseSaturation.GetValueSingle(); }
                set { baseSaturation.SetValue(value); }
            }

            public BloomEffect(Effect effect)
            {
                Effect = effect;

                bloomExtractMap = effect.Parameters["BloomExtractMap"];
                bloomIntensity = effect.Parameters["BloomIntensity"];
                baseIntensity = effect.Parameters["BaseIntensity"];
                bloomSaturation = effect.Parameters["BloomSaturation"];
                baseSaturation = effect.Parameters["BaseSaturation"];
            }
        }

        #endregion

        GraphicsDevice graphicsDevice;

        BloomSettings settings;

        SpriteBatch spriteBatch;

        BloomExtractEffect bloomExtractEffect;

        BloomEffect bloomEffect;

        GaussianBlur blur;

        RenderTarget2D bloomExtractMap;

        public Bloom(GraphicsDevice graphicsDevice, BloomSettings settings, SpriteBatch spriteBatch,
            Effect bloomExtractEffect, Effect bloomEffect, Effect blurEffect)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");
            if (settings == null) throw new ArgumentNullException("settings");
            if (spriteBatch == null) throw new ArgumentNullException("spriteBatch");
            if (bloomExtractEffect == null) throw new ArgumentNullException("bloomExtractEffect");
            if (bloomEffect == null) throw new ArgumentNullException("bloomEffect");
            if (blurEffect == null) throw new ArgumentNullException("blurEffect");

            this.graphicsDevice = graphicsDevice;
            this.settings = settings;
            this.spriteBatch = spriteBatch;

            //----------------------------------------------------------------
            // エフェクト

            this.bloomExtractEffect = new BloomExtractEffect(bloomExtractEffect);
            this.bloomExtractEffect.Threshold = settings.Threshold;
            
            this.bloomEffect = new BloomEffect(bloomEffect);
            this.bloomEffect.BloomIntensity = settings.BloomIntensity;
            this.bloomEffect.BaseIntensity = settings.BaseIntensity;
            this.bloomEffect.BloomSaturation = settings.BloomSaturation;
            this.bloomEffect.BaseSaturation = settings.BaseSaturation;

            //----------------------------------------------------------------
            // レンダ ターゲット

            var pp = graphicsDevice.PresentationParameters;
            var width = (int) (pp.BackBufferWidth * settings.MapScale);
            var height = (int) (pp.BackBufferHeight * settings.MapScale);

            bloomExtractMap = new RenderTarget2D(graphicsDevice, width, height,
                false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);

            //----------------------------------------------------------------
            // ブラー

            blur = new GaussianBlur(blurEffect, spriteBatch, width, height,
                SurfaceFormat.Color, settings.Blur.Radius, settings.Blur.Amount);
        }

        public override void Process(IPostProcessorContext context, RenderTarget2D source, RenderTarget2D destination)
        {
            //----------------------------------------------------------------
            // ブルーム エクストラクト マップ

            graphicsDevice.SetRenderTarget(bloomExtractMap);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, null, null, null, bloomExtractEffect.Effect);
            spriteBatch.Draw(source, bloomExtractMap.Bounds, Color.White);
            spriteBatch.End();
            graphicsDevice.SetRenderTarget(null);

            //----------------------------------------------------------------
            // ブルーム エクストラクト マップへブラーを適用

            blur.Filter(bloomExtractMap);

            //----------------------------------------------------------------
            // ブルーム

            bloomEffect.BloomExtractMap = bloomExtractMap;

            graphicsDevice.SetRenderTarget(destination);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, null, null, null, bloomEffect.Effect);
            spriteBatch.Draw(source, destination.Bounds, Color.White);
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

        ~Bloom()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                bloomExtractMap.Dispose();
                blur.Dispose();
            }

            disposed = true;
        }

        #endregion
    }
}
