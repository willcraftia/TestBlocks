#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class ShadowScene
    {
        RenderTarget2D renderTarget;

        ShadowSceneEffect shadowSceneEffect;

        public GraphicsDevice GraphicsDevice { get; private set; }

        public ShadowSceneMonitor Monitor { get; private set; }

        public RenderTarget2D RenderTarget
        {
            get { return renderTarget; }
        }

        public ShadowScene(GraphicsDevice graphicsDevice, ShadowSettings shadowSettings, Effect shadowSceneEffect)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");
            if (shadowSettings == null) throw new ArgumentNullException("shadowSettings");
            if (shadowSceneEffect == null) throw new ArgumentNullException("shadowSceneEffect");

            GraphicsDevice = graphicsDevice;

            //----------------------------------------------------------------
            // エフェクト

            this.shadowSceneEffect = new ShadowSceneEffect(shadowSceneEffect);
            this.shadowSceneEffect.DepthBias = shadowSettings.ShadowMap.DepthBias;
            this.shadowSceneEffect.SplitCount = shadowSettings.ShadowMap.SplitCount;
            this.shadowSceneEffect.ShadowMapSize = shadowSettings.ShadowMap.Size;
            this.shadowSceneEffect.Technique = shadowSettings.ShadowMap.Technique;

            //----------------------------------------------------------------
            // レンダ ターゲット

            var shadowSceneSettings = shadowSettings.Sssm;
            var pp = GraphicsDevice.PresentationParameters;
            var width = (int) (pp.BackBufferWidth * shadowSceneSettings.MapScale);
            var height = (int) (pp.BackBufferHeight * shadowSceneSettings.MapScale);

            // メモ: ブラーをかける場合があるので RenderTargetUsage.PreserveContents で作成。
            renderTarget = new RenderTarget2D(GraphicsDevice, width, height,
                false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);

            Monitor = new ShadowSceneMonitor(this);
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
