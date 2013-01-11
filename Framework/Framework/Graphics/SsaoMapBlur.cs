#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Diagnostics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    /// <summary>
    /// バイラテラル ブラー (バイラテラル フィルタ) を行うクラスです。
    /// このクラスのバイラテラル ブラーでは、法線深度マップを用い、
    /// 法線のなす角と深度の差から重み付けの度合いを変化させてブラー結果を算出します。
    /// </summary>
    public sealed class SsaoMapBlur
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

        EffectParameter colorMap;

        EffectParameter normalDepthMap;

        EffectTechnique horizontalBlurTechnique;

        EffectTechnique verticalBlurTechnique;

        Effect effect;

        SpriteBatch spriteBatch;

        GraphicsDevice graphicsDevice;

        FullscreenQuad fullscreenQuad;

        public int Width { get; private set; }

        public int Height { get; private set; }

        public SurfaceFormat Format { get; private set; }

        public int Radius { get; private set; }

        public float Amount { get; private set; }

        public SsaoMapBlur(Effect effect, SpriteBatch spriteBatch, int width, int height, SurfaceFormat format)
            : this(effect, spriteBatch, width, height, format, DefaultRadius, DefaultAmount)
        {
        }

        public SsaoMapBlur(Effect effect, SpriteBatch spriteBatch, int width, int height, SurfaceFormat format, int radius, float amount)
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

            colorMap = effect.Parameters["ColorMap"];
            normalDepthMap = effect.Parameters["NormalDepthMap"];
            horizontalBlurTechnique = effect.Techniques["HorizontalBlur"];
            verticalBlurTechnique = effect.Techniques["VerticalBlur"];

            InitializeEffectParameters();

            backingRenderTarget = new RenderTarget2D(graphicsDevice, width, height, false, format,
                DepthFormat.None, 0, RenderTargetUsage.PlatformContents);

            fullscreenQuad = new FullscreenQuad(graphicsDevice);
        }

        public void Filter(RenderTarget2D source, Texture2D normalDepthMap)
        {
            Filter(source, normalDepthMap, source);
        }

        public void Filter(Texture2D source, Texture2D normalDepthMap, RenderTarget2D destination)
        {
            this.colorMap.SetValue(source);
            this.normalDepthMap.SetValue(normalDepthMap);

            Draw(horizontalBlurTechnique, source, backingRenderTarget);
            Draw(verticalBlurTechnique, backingRenderTarget, destination);
        }

        void Draw(EffectTechnique technique, Texture2D source, RenderTarget2D destination)
        {
            effect.CurrentTechnique = technique;

            graphicsDevice.SetRenderTarget(destination);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, null, null, effect);
            spriteBatch.Draw(source, destination.Bounds, Color.White);
            spriteBatch.End();
            graphicsDevice.SetRenderTarget(null);

            graphicsDevice.SetRenderTarget(null);
        }

        void InitializeEffectParameters()
        {
            //------------------------------------------------------------
            // スプライト バッチで描画するための行列の初期化

            effect.Parameters["MatrixTransform"].SetValue(EffectHelper.CreateSpriteBatchMatrixTransform(Width, Height));

            //----------------------------------------------------------------
            // カーネルの初期化

            effect.Parameters["KernelSize"].SetValue(Radius * 2 + 1);
            PopulateWeights();
            PopulateOffsetsH();
            PopulateOffsetsV();
        }

        void PopulateWeights()
        {
            // ガウシアン ブラーとは異なり、シェーダ内で深度や法線の関係から重みが変化するため、
            // 正規化せずにシェーダへ設定する。

            var weights = new float[Radius * 2 + 1];
            var sigma = Radius / Amount;

            int index = 0;
            for (int i = -Radius; i <= Radius; i++)
            {
                weights[index] = MathExtension.CalculateGaussian(sigma, i);
                index++;
            }

            effect.Parameters["Weights"].SetValue(weights);
        }

        void PopulateOffsetsH()
        {
            effect.Parameters["OffsetsH"].SetValue(CalculateOffsets(1.0f / (float) Width, 0));
        }

        void PopulateOffsetsV()
        {
            effect.Parameters["OffsetsV"].SetValue(CalculateOffsets(0, 1.0f / (float) Height));
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

        ~SsaoMapBlur()
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
