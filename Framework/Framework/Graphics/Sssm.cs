#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Diagnostics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    /// <summary>
    /// スクリーン スペース シャドウ マッピング (Screen Space Shadow Mapping) を行うためのクラスです。
    /// </summary>
    public sealed class Sssm : PostProcessor, IDisposable
    {
        SpriteBatch spriteBatch;

        ShadowSceneEffect shadowSceneEffect;

        Effect sssmEffect;

        EffectParameter shadowColorParameter;
        
        EffectParameter shadowSceneMapParameter;

        EffectPass currentPass;

        GaussianBlur blur;

        RenderTarget2D shadowSceneMap;

        public GraphicsDevice GraphicsDevice { get; private set; }

        public ShadowSettings Settings { get; private set; }

        public SssmMonitor Monitor { get; private set; }

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
            shadowSceneMap = new RenderTarget2D(GraphicsDevice, width, height,
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

        public override void Process(IPostProcessorContext context, RenderTarget2D source, RenderTarget2D destination)
        {
            Monitor.OnBeginProcess();

            DrawShadowScene(context);
            Filter(context, source, destination);

            if (DebugMapDisplay.Available) DebugMapDisplay.Instance.Add(shadowSceneMap);

            Monitor.OnEndProcess();
        }

        void DrawShadowScene(IPostProcessorContext context)
        {
            Monitor.OnBeginDrawShadowScene();

            var camera = context.ActiveCamera;
            var shadowMap = context.ShadowMap;
            var visibleSceneObjects = context.VisibleSceneObjects;

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

            GraphicsDevice.SetRenderTarget(shadowSceneMap);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);

            foreach (var sceneObject in visibleSceneObjects)
                sceneObject.Draw(shadowSceneEffect);

            GraphicsDevice.SetRenderTarget(null);

            Monitor.OnEndDrawShadowScene();
        }

        void Filter(IPostProcessorContext context, RenderTarget2D source, RenderTarget2D destination)
        {
            Monitor.OnBeginFilter();

            if (blur != null) blur.Filter(shadowSceneMap);

            //----------------------------------------------------------------
            // エフェクト

            shadowColorParameter.SetValue(context.ShadowColor);
            shadowSceneMapParameter.SetValue(shadowSceneMap);

            //----------------------------------------------------------------
            // 描画

            GraphicsDevice.SetRenderTarget(destination);

            var samplerState = destination.GetPreferredSamplerState();

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, samplerState, null, null, sssmEffect);
            spriteBatch.Draw(source, destination.Bounds, Color.White);
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
                shadowSceneEffect.Dispose();
                shadowSceneMap.Dispose();
                if (blur != null) blur.Dispose();
            }

            disposed = true;
        }

        #endregion
    }
}
