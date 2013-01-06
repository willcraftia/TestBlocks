#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
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

        public const float DefaultAmount = 2.0f;

        RenderTarget2D backingRenderTarget;

        EffectTechnique horizontalBlurTechnique;

        EffectTechnique verticalBlurTechnique;

        public Effect Effect { get; private set; }

        public SpriteBatch SpriteBatch { get; private set; }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public SurfaceFormat Format { get; private set; }

        public int Radius { get; private set; }

        public float Amount { get; private set; }

        public GraphicsDevice GraphicsDevice { get; private set; }

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

            Effect = effect;
            SpriteBatch = spriteBatch;
            Width = width;
            Height = height;
            Radius = radius;
            Amount = amount;

            GraphicsDevice = effect.GraphicsDevice;

            InitializeEffectParameters();
            CacheEffectTechniques();

            backingRenderTarget = new RenderTarget2D(GraphicsDevice, width, height, false, format,
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
            Effect.CurrentTechnique = technique;

            var samplerState = destination.GetPreferredSamplerState();

            GraphicsDevice.SetRenderTarget(destination);
            SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, samplerState, null, null, Effect);
            SpriteBatch.Draw(source, destination.Bounds, Color.White);
            SpriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);
        }

        void CacheEffectTechniques()
        {
            horizontalBlurTechnique = Effect.Techniques["HorizontalBlur"];
            verticalBlurTechnique = Effect.Techniques["VerticalBlur"];
        }

        void InitializeEffectParameters()
        {
            Effect.Parameters["KernelSize"].SetValue(Radius * 2 + 1);
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

            Effect.Parameters["Weights"].SetValue(weights);
        }

        void PopulateOffsetsH()
        {
            Effect.Parameters["OffsetsH"].SetValue(CalculateOffsets(1.0f / (float) Width, 0));
        }

        void PopulateOffsetsV()
        {
            Effect.Parameters["OffsetsV"].SetValue(CalculateOffsets(0, 1.0f / (float) Height));
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
