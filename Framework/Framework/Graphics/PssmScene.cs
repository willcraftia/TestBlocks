#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class PssmScene
    {
        ShadowSettings shadowSettings;

        RenderTarget2D renderTarget;

        PssmSceneEffect shadowSceneEffect;

        public GraphicsDevice GraphicsDevice { get; private set; }

        public PssmSceneMonitor Monitor { get; private set; }

        public RenderTarget2D ShadowScene
        {
            get { return renderTarget; }
        }

        public PssmScene(GraphicsDevice graphicsDevice, ShadowSettings shadowSettings, Effect shadowSceneEffect)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");
            if (shadowSettings == null) throw new ArgumentNullException("shadowSettings");
            if (shadowSceneEffect == null) throw new ArgumentNullException("shadowSceneEffect");

            GraphicsDevice = graphicsDevice;
            this.shadowSettings = shadowSettings;

            //----------------------------------------------------------------
            // エフェクト

            this.shadowSceneEffect = new PssmSceneEffect(shadowSceneEffect);
            this.shadowSceneEffect.DepthBias = shadowSettings.ShadowMap.DepthBias;
            this.shadowSceneEffect.SplitCount = shadowSettings.LightFrustum.Pssm.SplitCount;

            //----------------------------------------------------------------
            // レンダ ターゲット

            var shadowSceneSettings = shadowSettings.ShadowScene;
            var pp = GraphicsDevice.PresentationParameters;
            var width = (int) (pp.BackBufferWidth * shadowSceneSettings.MapScale);
            var height = (int) (pp.BackBufferHeight * shadowSceneSettings.MapScale);

            // メモ: ブラーをかける場合があるので RenderTargetUsage.PreserveContents で作成。
            renderTarget = new RenderTarget2D(GraphicsDevice, width, height,
                false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.PreserveContents);

            Monitor = new PssmSceneMonitor(this);
        }

        public void Draw(ICamera camera, Pssm pssm, IEnumerable<SceneObject> sceneObjects)
        {
            if (camera == null) throw new ArgumentNullException("camera");
            if (pssm == null) throw new ArgumentNullException("pssm");
            if (sceneObjects == null) throw new ArgumentNullException("sceneObjects");

            Monitor.OnBeginDraw();

            var shadowMapSettings = shadowSettings.ShadowMap;
            var pssmSettings = shadowSettings.LightFrustum.Pssm;

            //----------------------------------------------------------------
            // エフェクト

            shadowSceneEffect.View = camera.View.Matrix;
            shadowSceneEffect.Projection = camera.Projection.Matrix;
            shadowSceneEffect.SplitDistances = pssm.SplitDistances;
            shadowSceneEffect.SplitLightViewProjections = pssm.SplitLightViewProjections;
            shadowSceneEffect.SplitShadowMaps = pssm.SplitShadowMaps;

            //----------------------------------------------------------------
            // 描画

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;

            GraphicsDevice.SetRenderTarget(renderTarget);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);

            foreach (var sceneObject in sceneObjects)
            {
                var support = sceneObject as IShadowSceneSupport;
                if (support != null) support.Draw(shadowSceneEffect);
            }

            GraphicsDevice.SetRenderTarget(null);

            Monitor.OnEndDraw();
        }
    }
}
