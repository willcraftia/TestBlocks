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

        #region BloomMonitor

        public sealed class BloomMonitor : PostProcessorMonitor
        {
            internal BloomMonitor(Bloom bloom) : base(bloom) { }
        }

        #endregion

        public const float DefaultThreshold = 0.25f;

        public const float DefaultBloomIntensity = 1.25f;

        public const float DefaultBaseIntensity = 1;

        public const float DefaultBloomSaturation = 1;

        public const float DefaultBaseSaturation = 1;

        BloomSettings settings;

        BloomExtractEffect bloomExtractEffect;

        BloomEffect bloomEffect;

        GaussianBlur blur;

        RenderTarget2D bloomExtractMap;

        float threshold = DefaultThreshold;

        float bloomIntensity = DefaultBloomIntensity;

        float baseIntensity = DefaultBaseIntensity;

        float bloomSaturation = DefaultBloomSaturation;

        float baseSaturation = DefaultBaseSaturation;

        public float Threshold
        {
            get { return threshold; }
            set
            {
                if (value < 0 || 1 < value) throw new ArgumentOutOfRangeException("value");

                threshold = value;
            }
        }

        public float BloomIntensity
        {
            get { return bloomIntensity; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");

                bloomIntensity = value;
            }
        }

        public float BaseIntensity
        {
            get { return baseIntensity; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");

                baseIntensity = value;
            }
        }

        public float BloomSaturation
        {
            get { return bloomSaturation; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");

                bloomSaturation = value;
            }
        }

        public float BaseSaturation
        {
            get { return baseSaturation; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");

                baseSaturation = value;
            }
        }

        public BloomMonitor Monitor { get; private set; }

        public Bloom(SpriteBatch spriteBatch, BloomSettings settings, Effect bloomExtractEffect, Effect bloomEffect, Effect blurEffect)
            : base(spriteBatch)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            if (bloomExtractEffect == null) throw new ArgumentNullException("bloomExtractEffect");
            if (bloomEffect == null) throw new ArgumentNullException("bloomEffect");
            if (blurEffect == null) throw new ArgumentNullException("blurEffect");

            this.settings = settings;

            //----------------------------------------------------------------
            // エフェクト

            this.bloomExtractEffect = new BloomExtractEffect(bloomExtractEffect);
            this.bloomEffect = new BloomEffect(bloomEffect);

            //----------------------------------------------------------------
            // レンダ ターゲット

            var pp = GraphicsDevice.PresentationParameters;
            var width = (int) (pp.BackBufferWidth * settings.MapScale);
            var height = (int) (pp.BackBufferHeight * settings.MapScale);

            bloomExtractMap = new RenderTarget2D(GraphicsDevice, width, height,
                false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);

            //----------------------------------------------------------------
            // ブラー

            blur = new GaussianBlur(blurEffect, spriteBatch, width, height,
                SurfaceFormat.Color, settings.Blur.Radius, settings.Blur.Amount);

            //----------------------------------------------------------------
            // モニタ

            Monitor = new BloomMonitor(this);
        }

        public override void Process(IPostProcessorContext context, RenderTarget2D source, RenderTarget2D destination)
        {
            Monitor.OnBeginProcess();

            //----------------------------------------------------------------
            // ブルーム エクストラクト マップ

            bloomExtractEffect.Threshold = Threshold;

            GraphicsDevice.SetRenderTarget(bloomExtractMap);
            SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, null, null, null, bloomExtractEffect.Effect);
            SpriteBatch.Draw(source, bloomExtractMap.Bounds, Color.White);
            SpriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);

            //----------------------------------------------------------------
            // ブルーム エクストラクト マップへブラーを適用

            blur.Filter(bloomExtractMap);

            //----------------------------------------------------------------
            // ブルーム

            bloomEffect.BloomIntensity = BloomIntensity;
            bloomEffect.BaseIntensity = BaseIntensity;
            bloomEffect.BloomSaturation = BloomSaturation;
            bloomEffect.BaseSaturation = BaseSaturation;
            bloomEffect.BloomExtractMap = bloomExtractMap;

            GraphicsDevice.SetRenderTarget(destination);
            SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, null, null, null, bloomEffect.Effect);
            SpriteBatch.Draw(source, destination.Bounds, Color.White);
            SpriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);

            Monitor.OnEndProcess();
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
