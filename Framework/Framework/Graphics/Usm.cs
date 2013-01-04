#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class Usm
    {
        ShadowMapSettings shadowMapSettings;

        VsmSettings vsmSettings;

        UsmLightVolume lightVolume;

        SpriteBatch blurSpriteBatch;

        GaussianBlur blur;

        RenderTarget2D renderTarget;

        BasicCamera lightCamera = new BasicCamera("UsmLight");

        Vector3[] corners = new Vector3[8];

        BoundingBox frustumBoundingBox;

        // TODO: 初期容量。
        Queue<ShadowCaster> shadowCasters = new Queue<ShadowCaster>();

        public GraphicsDevice GraphicsDevice { get; private set; }

        public ShadowSettings ShadowSettings { get; private set; }

        public Matrix LightViewProjection
        {
            get { return lightVolume.LightViewProjection; }
        }

        public Texture2D ShadowMap
        {
            get { return renderTarget; }
        }

        public UsmMonitor Monitor { get; private set; }

        public Usm(GraphicsDevice graphicsDevice, ShadowSettings shadowSettings, Effect blurEffect)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");
            if (shadowSettings == null) throw new ArgumentNullException("shadowSettings");
            if (blurEffect == null) throw new ArgumentNullException("blurEffect");

            GraphicsDevice = graphicsDevice;
            ShadowSettings = shadowSettings;

            shadowMapSettings = shadowSettings.ShadowMap;
            vsmSettings = shadowMapSettings.Vsm;

            lightVolume = new UsmLightVolume(shadowMapSettings.Size);

            // TODO: パラメータ見直し or 外部設定化。
            var pp = GraphicsDevice.PresentationParameters;
            // メモ: ブラーをかける場合があるので RenderTargetUsage.PreserveContents で作成。
            renderTarget = new RenderTarget2D(GraphicsDevice, shadowMapSettings.Size, shadowMapSettings.Size,
                false, shadowMapSettings.Format, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);
            renderTarget.Name = "ShadowMap";

            blurSpriteBatch = new SpriteBatch(GraphicsDevice);
            blur = new GaussianBlur(blurEffect, blurSpriteBatch, shadowMapSettings.Size, shadowMapSettings.Size, SurfaceFormat.Vector2,
                vsmSettings.Blur.Radius, vsmSettings.Blur.Amount);

            Monitor = new UsmMonitor();
        }

        public void Prepare(ICamera camera)
        {
            lightCamera.View.Position = camera.View.Position;
            lightCamera.View.Direction = camera.View.Direction;
            lightCamera.View.Up = camera.View.Up;
            lightCamera.Projection.Fov = camera.Projection.Fov;
            lightCamera.Projection.AspectRatio = camera.Projection.AspectRatio;
            lightCamera.Projection.NearPlaneDistance = camera.Projection.NearPlaneDistance;
            // TODO: 調整。
            lightCamera.Projection.FarPlaneDistance = camera.Projection.FarPlaneDistance;
            lightCamera.Update();

            camera.Frustum.GetCorners(corners);
            frustumBoundingBox = BoundingBox.CreateFromPoints(corners);
        }

        public void TryAddShadowCaster(ShadowCaster shadowCaster)
        {
            if (!shadowCaster.BoundingSphere.Intersects(frustumBoundingBox)) return;
            if (!shadowCaster.BoundingBox.Intersects(frustumBoundingBox)) return;

            bool shouldAdd = false;
            if (shadowCaster.BoundingSphere.Intersects(lightCamera.Frustum))
            {
                shouldAdd = true;
            }
            else if (shadowCaster.BoundingBox.Intersects(lightCamera.Frustum))
            {
                shouldAdd = true;
            }

            if (shouldAdd)
            {
                // 投影オブジェクトとして登録。
                shadowCasters.Enqueue(shadowCaster);

                // AABB の頂点を包含座標として登録。
                lightVolume.AddLightVolumePoints(corners);

                Monitor.ShadowCasterCount++;

                //break;
            }
        }

        // シャドウ マップを描画。
        public void Draw(ShadowMapEffect effect, ref Vector3 lightDirection)
        {
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;

            //------------------------------------------------------------
            // ライトのビュー×射影行列の更新

            lightVolume.Update(lightCamera, ref lightDirection);

            //------------------------------------------------------------
            // エフェクト

            effect.LightViewProjection = lightVolume.LightViewProjection;

            //------------------------------------------------------------
            // 描画

            GraphicsDevice.SetRenderTarget(renderTarget);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);

            while (0 < shadowCasters.Count)
            {
                var shadowCaster = shadowCasters.Dequeue();
                shadowCaster.Draw(effect);
            }

            if (effect.Technique == ShadowMapTechniques.Vsm && vsmSettings.Blur.Enabled)
                blur.Filter(renderTarget);

            GraphicsDevice.SetRenderTarget(null);
        }
    }
}
