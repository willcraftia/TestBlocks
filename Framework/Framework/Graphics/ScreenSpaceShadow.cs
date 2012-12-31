#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class ScreenSpaceShadow
    {
        ShadowSceneSettings shadowSceneSettings;

        SpriteBatch spriteBatch;

        Effect screenSpaceShadowEffect;

        EffectParameter shadowColorParameter;
        
        EffectParameter shadowSceneMapParameter;

        EffectPass currentPass;

        GaussianBlur blur;

        Vector3 shadowColor = Vector3.Zero;

        public GraphicsDevice GraphicsDevice { get; private set; }

        public ScreenSpaceShadowMonitor Monitor { get; private set; }

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

        public ScreenSpaceShadow(GraphicsDevice graphicsDevice, ShadowSceneSettings shadowSceneSettings,
            Effect screenSpaceShadowEffect, Effect blurEffect)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");
            if (shadowSceneSettings == null) throw new ArgumentNullException("shadowSceneSettings");
            if (screenSpaceShadowEffect == null) throw new ArgumentNullException("screenSpaceShadowEffect");
            if (blurEffect == null) throw new ArgumentNullException("blurEffect");

            GraphicsDevice = graphicsDevice;
            this.shadowSceneSettings = shadowSceneSettings;
            this.screenSpaceShadowEffect = screenSpaceShadowEffect;

            //----------------------------------------------------------------
            // スプライト バッチ

            spriteBatch = new SpriteBatch(GraphicsDevice);

            //----------------------------------------------------------------
            // エフェクト

            shadowColorParameter = screenSpaceShadowEffect.Parameters["ShadowColor"];
            shadowSceneMapParameter = screenSpaceShadowEffect.Parameters["ShadowSceneMap"];
            currentPass = screenSpaceShadowEffect.CurrentTechnique.Passes[0];

            //----------------------------------------------------------------
            // ブラー

            var pp = GraphicsDevice.PresentationParameters;
            var width = (int) (pp.BackBufferWidth * shadowSceneSettings.MapScale);
            var height = (int) (pp.BackBufferHeight * shadowSceneSettings.MapScale);

            var blurSettings = shadowSceneSettings.Blur;
            blur = new GaussianBlur(blurEffect, spriteBatch, width, height, SurfaceFormat.Vector2,
                blurSettings.Radius, blurSettings.Amount);

            //----------------------------------------------------------------
            // モニタ

            Monitor = new ScreenSpaceShadowMonitor(this);
        }

        public void Filter(RenderTarget2D scene, RenderTarget2D shadowScene, RenderTarget2D result)
        {
            Monitor.OnBeginFilter();

            if (shadowSceneSettings.Blur.Enabled) blur.Filter(shadowScene);

            //----------------------------------------------------------------
            // エフェクト

            shadowSceneMapParameter.SetValue(shadowScene);

            //----------------------------------------------------------------
            // 描画

            GraphicsDevice.SetRenderTarget(result);

            var samplerState = result.GetPreferredSamplerState();

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, samplerState, null, null, screenSpaceShadowEffect);
            spriteBatch.Draw(scene, result.Bounds, Color.White);
            spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);

            Monitor.OnEndFilter();
        }
    }
}
