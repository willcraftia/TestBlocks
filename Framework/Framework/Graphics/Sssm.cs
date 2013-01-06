#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    /// <summary>
    /// スクリーン スペース シャドウ マッピング (Screen Space Shadow Mapping) を行うためのクラスです。
    /// </summary>
    public sealed class Sssm
    {
        SpriteBatch spriteBatch;

        Effect sssmEffect;

        EffectParameter shadowColorParameter;
        
        EffectParameter shadowSceneMapParameter;

        EffectPass currentPass;

        GaussianBlur blur;

        Vector3 shadowColor = Vector3.Zero;

        public GraphicsDevice GraphicsDevice { get; private set; }

        public SssmSettings Settings { get; private set; }

        public SssmMonitor Monitor { get; private set; }

        public Vector3 ShadowColor
        {
            get { return shadowColor; }
            set
            {
                if (shadowColor == value) return;

                shadowColor = value;
                shadowColorParameter.SetValue(shadowColor);
            }
        }

        public Sssm(GraphicsDevice graphicsDevice, SssmSettings sssmSettings,
            SpriteBatch spriteBatch, Effect sssmEffect, Effect blurEffect)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");
            if (sssmSettings == null) throw new ArgumentNullException("sssmSettings");
            if (spriteBatch == null) throw new ArgumentNullException("spriteBatch");
            if (sssmEffect == null) throw new ArgumentNullException("sssmEffect");
            if (blurEffect == null) throw new ArgumentNullException("blurEffect");

            GraphicsDevice = graphicsDevice;
            Settings = sssmSettings;
            this.spriteBatch = spriteBatch;
            this.sssmEffect = sssmEffect;

            //----------------------------------------------------------------
            // エフェクト

            shadowColorParameter = sssmEffect.Parameters["ShadowColor"];
            shadowSceneMapParameter = sssmEffect.Parameters["ShadowSceneMap"];
            currentPass = sssmEffect.CurrentTechnique.Passes[0];

            //----------------------------------------------------------------
            // ブラー

            var pp = GraphicsDevice.PresentationParameters;
            var width = (int) (pp.BackBufferWidth * sssmSettings.MapScale);
            var height = (int) (pp.BackBufferHeight * sssmSettings.MapScale);

            var blurSettings = sssmSettings.Blur;
            blur = new GaussianBlur(blurEffect, spriteBatch, width, height, SurfaceFormat.Vector2,
                blurSettings.Radius, blurSettings.Amount);

            //----------------------------------------------------------------
            // モニタ

            Monitor = new SssmMonitor(this);
        }

        public void Filter(RenderTarget2D scene, RenderTarget2D shadowScene, RenderTarget2D result)
        {
            Monitor.OnBeginFilter();

            if (Settings.Blur.Enabled) blur.Filter(shadowScene);

            //----------------------------------------------------------------
            // エフェクト

            shadowColorParameter.SetValue(shadowColor);
            shadowSceneMapParameter.SetValue(shadowScene);

            //----------------------------------------------------------------
            // 描画

            GraphicsDevice.SetRenderTarget(result);

            var samplerState = result.GetPreferredSamplerState();

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, samplerState, null, null, sssmEffect);
            spriteBatch.Draw(scene, result.Bounds, Color.White);
            spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);

            Monitor.OnEndFilter();
        }
    }
}
