#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    /// <summary>
    /// ガウシアン ブラー (ガウシアン フィルタ) を行うクラスです。
    /// </summary>
    public sealed class GaussianBlur : IDisposable
    {
#if SHADER_3_0
        public const int MaxRadius = 7;
#else
        public const int MaxRadius = 4;
#endif
        public const int MinRadius = 1;

        public const float MinAmount = 0.001f;

        public const int DefaultRadius = 1;

        public const float DefaultAmount = 2;

        RenderTarget2D backingRenderTarget;

        EffectTechnique horizontalBlurTechnique;

        EffectTechnique verticalBlurTechnique;

        Effect effect;

        SpriteBatch spriteBatch;

        GraphicsDevice graphicsDevice;

        public int Width { get; private set; }

        public int Height { get; private set; }

        public SurfaceFormat Format { get; private set; }

        public int Radius { get; private set; }

        public float Amount { get; private set; }

        public GaussianBlur(Effect effect, SpriteBatch spriteBatch, int width, int height, SurfaceFormat format)
            : this(effect, spriteBatch, width, height, format, DefaultRadius, DefaultAmount)
        {
        }

        public GaussianBlur(Effect effect, SpriteBatch spriteBatch, int width, int height, SurfaceFormat format, int radius, float amount)
        {
            if (effect == null) throw new ArgumentNullException("effect");
            if (spriteBatch == null) throw new ArgumentNullException("spriteBatch");
            if (width < 1) throw new ArgumentOutOfRangeException("width");
            if (height < 1) throw new ArgumentOutOfRangeException("height");
            if (radius < MinAmount || MaxRadius < radius) throw new ArgumentOutOfRangeException("value");
            if (amount < MinAmount) throw new ArgumentOutOfRangeException("value");

            this.effect = effect;
            this.spriteBatch = spriteBatch;
            Width = width;
            Height = height;
            Radius = radius;
            Amount = amount;

            graphicsDevice = effect.GraphicsDevice;

            InitializeEffectParameters();
            CacheEffectTechniques();

            backingRenderTarget = new RenderTarget2D(graphicsDevice, width, height, false, format,
                DepthFormat.None, 0, RenderTargetUsage.PlatformContents);
        }

        public void Filter(RenderTarget2D source)
        {
            Filter(source, source);
        }

        public void Filter(Texture2D source, RenderTarget2D destination)
        {
            Draw(horizontalBlurTechnique, source, backingRenderTarget);
            Draw(verticalBlurTechnique, backingRenderTarget, destination);
        }

        void Draw(EffectTechnique technique, Texture2D source, RenderTarget2D destination)
        {
            effect.CurrentTechnique = technique;

            var samplerState = destination.GetPreferredSamplerState();

            graphicsDevice.SetRenderTarget(destination);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, samplerState, null, null, effect);
            spriteBatch.Draw(source, destination.Bounds, Color.White);
            spriteBatch.End();
            graphicsDevice.SetRenderTarget(null);
        }

        void CacheEffectTechniques()
        {
            horizontalBlurTechnique = effect.Techniques["HorizontalBlur"];
            verticalBlurTechnique = effect.Techniques["VerticalBlur"];
        }

        void InitializeEffectParameters()
        {
            effect.Parameters["KernelSize"].SetValue(Radius * 2 + 1);
            PopulateWeights();
            PopulateOffsetsH();
            PopulateOffsetsV();
        }

        void PopulateWeights()
        {
            var weights = new float[Radius * 2 + 1];
            var totalWeight = 0.0f;
            var sigma = Radius / Amount;

            int index = 0;
            for (int i = -Radius; i <= Radius; i++)
            {
                weights[index] = MathExtension.CalculateGaussian(sigma, i);
                totalWeight += weights[index];
                index++;
            }

            // Normalize
            for (int i = 0; i < weights.Length; i++)
            {
                weights[i] /= totalWeight;
            }

            effect.Parameters["Weights"].SetValue(weights);
        }

        void PopulateOffsetsH()
        {
            effect.Parameters["OffsetsH"].SetValue(CalculateOffsets(1 / (float) Width, 0));
        }

        void PopulateOffsetsV()
        {
            effect.Parameters["OffsetsV"].SetValue(CalculateOffsets(0, 1 / (float) Height));
        }

        Vector2[] CalculateOffsets(float dx, float dy)
        {
            var offsets = new Vector2[Radius * 2 + 1];

            int index = 0;
            for (int i = -Radius; i <= Radius; i++)
            {
                offsets[index] = new Vector2(i * dx, i * dy);
                index++;
            }

            return offsets;
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed;

        ~GaussianBlur()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                backingRenderTarget.Dispose();
            }

            disposed = true;
        }

        #endregion
    }
}
