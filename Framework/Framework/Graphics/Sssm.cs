#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    /// <summary>
    /// スクリーン スペース シャドウ マッピング (Screen Space Shadow Mapping) を行うためのクラスです。
    /// </summary>
    public sealed class Sssm : IDisposable
    {
        SpriteBatch spriteBatch;

        ShadowSceneEffect shadowSceneEffect;

        Effect sssmEffect;

        EffectParameter shadowColorParameter;
        
        EffectParameter shadowSceneMapParameter;

        EffectPass currentPass;

        GaussianBlur blur;

        Vector3 shadowColor = Vector3.Zero;

        public GraphicsDevice GraphicsDevice { get; private set; }

        public ShadowSettings Settings { get; private set; }

        public RenderTarget2D ShadowSceneMap { get; private set; }

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

        public Sssm(GraphicsDevice graphicsDevice, ShadowSettings shadowSettings,
            SpriteBatch spriteBatch, Effect shadowSceneEffect, Effect sssmEffect, Effect blurEffect)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");
            if (shadowSettings == null) throw new ArgumentNullException("shadowSettings");
            if (spriteBatch == null) throw new ArgumentNullException("spriteBatch");
            if (shadowSceneEffect == null) throw new ArgumentNullException("shadowSceneEffect");
            if (sssmEffect == null) throw new ArgumentNullException("sssmEffect");
            if (blurEffect == null) throw new ArgumentNullException("blurEffect");

            GraphicsDevice = graphicsDevice;
            Settings = shadowSettings;
            this.spriteBatch = spriteBatch;
            this.sssmEffect = sssmEffect;

            //================================================================
            // シャドウ シーン

            //----------------------------------------------------------------
            // エフェクト

            this.shadowSceneEffect = new ShadowSceneEffect(shadowSceneEffect);
            this.shadowSceneEffect.DepthBias = shadowSettings.ShadowMap.DepthBias;
            this.shadowSceneEffect.SplitCount = shadowSettings.ShadowMap.SplitCount;
            this.shadowSceneEffect.ShadowMapSize = shadowSettings.ShadowMap.Size;
            this.shadowSceneEffect.ShadowMapTechnique = shadowSettings.ShadowMap.Technique;

            //----------------------------------------------------------------
            // レンダ ターゲット

            var sssmSettings = Settings.Sssm;
            var pp = GraphicsDevice.PresentationParameters;
            var width = (int) (pp.BackBufferWidth * sssmSettings.MapScale);
            var height = (int) (pp.BackBufferHeight * sssmSettings.MapScale);

            // メモ: ブラーをかける場合があるので RenderTargetUsage.PreserveContents で作成。
            ShadowSceneMap = new RenderTarget2D(GraphicsDevice, width, height,
                false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);

            //================================================================
            // スクリーン スペース シャドウ マッピング

            //----------------------------------------------------------------
            // エフェクト

            shadowColorParameter = sssmEffect.Parameters["ShadowColor"];
            shadowSceneMapParameter = sssmEffect.Parameters["ShadowSceneMap"];
            currentPass = sssmEffect.CurrentTechnique.Passes[0];

            //----------------------------------------------------------------
            // ブラー

            if (sssmSettings.BlurEnabled)
            {
                var blurSettings = sssmSettings.Blur;
                blur = new GaussianBlur(blurEffect, spriteBatch, width, height, SurfaceFormat.Vector2,
                    blurSettings.Radius, blurSettings.Amount);
            }

            //================================================================
            // モニタ

            Monitor = new SssmMonitor(this);
        }

        public void Draw(ICamera camera, ShadowMap shadowMap, IEnumerable<SceneObject> sceneObjects)
        {
            if (camera == null) throw new ArgumentNullException("camera");
            if (shadowMap == null) throw new ArgumentNullException("shadowMap");
            if (sceneObjects == null) throw new ArgumentNullException("sceneObjects");

            Monitor.OnBeginDraw();

            //----------------------------------------------------------------
            // エフェクト

            shadowSceneEffect.View = camera.View.Matrix;
            shadowSceneEffect.Projection = camera.Projection.Matrix;
            shadowSceneEffect.SplitDistances = shadowMap.SplitDistances;
            shadowSceneEffect.SplitLightViewProjections = shadowMap.SplitLightViewProjections;
            shadowSceneEffect.SplitShadowMaps = shadowMap.SplitShadowMaps;

            //----------------------------------------------------------------
            // 描画

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;

            GraphicsDevice.SetRenderTarget(ShadowSceneMap);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);

            foreach (var sceneObject in sceneObjects)
                sceneObject.Draw(shadowSceneEffect);

            GraphicsDevice.SetRenderTarget(null);

            Monitor.OnEndDraw();
        }

        public void Filter(RenderTarget2D sceneMap, RenderTarget2D result)
        {
            Monitor.OnBeginFilter();

            if (blur != null) blur.Filter(ShadowSceneMap);

            //----------------------------------------------------------------
            // エフェクト

            shadowColorParameter.SetValue(shadowColor);
            shadowSceneMapParameter.SetValue(ShadowSceneMap);

            //----------------------------------------------------------------
            // 描画

            GraphicsDevice.SetRenderTarget(result);

            var samplerState = result.GetPreferredSamplerState();

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, samplerState, null, null, sssmEffect);
            spriteBatch.Draw(sceneMap, result.Bounds, Color.White);
            spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);

            Monitor.OnEndFilter();
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed;

        ~Sssm()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                if (blur != null) blur.Dispose();
            }

            disposed = true;
        }

        #endregion
    }
}
