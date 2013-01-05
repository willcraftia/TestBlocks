#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class Pssm
    {
        ShadowMapSettings shadowMapSettings;

        VsmSettings vsmSettings;

        Vector3[] corners = new Vector3[8];

        float inverseSplitCount;

        BasicCamera[] splitCameras;

        float[] splitDistances;

        float[] safeSplitDistances;

        LightCamera[] splitLightCameras;

        Matrix[] safeSplitLightViewProjections;

        RenderTarget2D[] splitRenderTargets;

        Texture2D[] safeSplitShadowMaps;

        Queue<ShadowCaster>[] splitShadowCasters;

        SpriteBatch blurSpriteBatch;

        GaussianBlur blur;

        BoundingBox frustumBoundingBox;

        public GraphicsDevice GraphicsDevice { get; private set; }

        public ShadowSettings ShadowSettings { get; private set; }

        public int SplitCount { get; private set; }

        public float[] SplitDistances
        {
            get
            {
                Array.Copy(splitDistances, safeSplitDistances, splitDistances.Length);
                return safeSplitDistances;
            }
        }

        public Matrix[] SplitLightViewProjections
        {
            get
            {
                for (int i = 0; i < splitLightCameras.Length; i++)
                    safeSplitLightViewProjections[i] = splitLightCameras[i].LightViewProjection;
                return safeSplitLightViewProjections;
            }
        }

        public Texture2D[] SplitShadowMaps
        {
            get
            {
                for (int i = 0; i < safeSplitShadowMaps.Length; i++)
                    safeSplitShadowMaps[i] = splitRenderTargets[i];
                return safeSplitShadowMaps;
            }
        }

        #region Debug

        public PssmMonitor Monitor;

        #endregion

        public Pssm(GraphicsDevice graphicsDevice, ShadowSettings shadowSettings, Effect blurEffect)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");
            if (shadowSettings == null) throw new ArgumentNullException("shadowSettings");
            if (blurEffect == null) throw new ArgumentNullException("blurEffect");

            GraphicsDevice = graphicsDevice;
            ShadowSettings = shadowSettings;

            shadowMapSettings = shadowSettings.ShadowMap;
            vsmSettings = shadowMapSettings.Vsm;

            SplitCount = shadowMapSettings.SplitCount;
            inverseSplitCount = 1.0f / (float) SplitCount;
            splitDistances = new float[SplitCount + 1];
            safeSplitDistances = new float[SplitCount + 1];
            safeSplitLightViewProjections = new Matrix[SplitCount];
            safeSplitShadowMaps = new Texture2D[SplitCount];

            splitCameras = new BasicCamera[SplitCount];
            for (int i = 0; i < splitCameras.Length; i++)
                splitCameras[i] = new BasicCamera("PssmLight" + i);

            splitLightCameras = new LightCamera[SplitCount];
            for (int i = 0; i < splitLightCameras.Length; i++)
                splitLightCameras[i] = new LightCamera(shadowMapSettings.Size);

            // TODO: パラメータ見直し or 外部設定化。
            var pp = GraphicsDevice.PresentationParameters;
            // メモ: ブラーをかける場合があるので RenderTargetUsage.PreserveContents で作成。
            splitRenderTargets = new RenderTarget2D[SplitCount];
            for (int i = 0; i < splitRenderTargets.Length; i++)
            {
                splitRenderTargets[i] = new RenderTarget2D(GraphicsDevice, shadowMapSettings.Size, shadowMapSettings.Size,
                    false, shadowMapSettings.Format, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);
                splitRenderTargets[i].Name = "ShadowMap" + i;
            }

            // TODO: 初期容量。
            splitShadowCasters = new Queue<ShadowCaster>[SplitCount];
            for (int i = 0; i < splitShadowCasters.Length; i++)
                splitShadowCasters[i] = new Queue<ShadowCaster>();

            blurSpriteBatch = new SpriteBatch(GraphicsDevice);
            blur = new GaussianBlur(blurEffect, blurSpriteBatch, shadowMapSettings.Size, shadowMapSettings.Size, SurfaceFormat.Vector2,
                vsmSettings.Blur.Radius, vsmSettings.Blur.Amount);

            Monitor = new PssmMonitor(SplitCount);
        }

        public void PrepareSplitCameras(ICamera camera)
        {
            if (camera == null) throw new ArgumentNullException("camera");

            // 視錐台を含む AABB をシーン領域のデフォルトとしておく。
            camera.Frustum.GetCorners(corners);
            var sceneBoundingBox = BoundingBox.CreateFromPoints(corners);

            PrepareSplitCameras(camera, ref sceneBoundingBox);
        }

        public void PrepareSplitCameras(ICamera camera, ref BoundingBox sceneBoundingBox)
        {
            if (camera == null) throw new ArgumentNullException("camera");

            camera.Frustum.GetCorners(corners);
            frustumBoundingBox = BoundingBox.CreateFromPoints(corners);

            var far = CalculateFarPlaneDistance(camera, ref sceneBoundingBox);
            CalculateSplitDistances(camera, far);

            for (int i = 0; i < SplitCount; i++)
            {
                splitCameras[i].View.Position = camera.View.Position;
                splitCameras[i].View.Direction = camera.View.Direction;
                splitCameras[i].View.Up = camera.View.Up;
                splitCameras[i].Projection.Fov = camera.Projection.Fov;
                splitCameras[i].Projection.AspectRatio = camera.Projection.AspectRatio;
                splitCameras[i].Projection.NearPlaneDistance = splitDistances[i];
                splitCameras[i].Projection.FarPlaneDistance = splitDistances[i + 1];
                splitCameras[i].Update();

                Monitor[i].ShadowCasterCount = 0;
            }

            Monitor.TotalShadowCasterCount = 0;
        }

        public void TryAddShadowCaster(ShadowCaster shadowCaster)
        {
            if (!shadowCaster.BoundingSphere.Intersects(frustumBoundingBox)) return;
            if (!shadowCaster.BoundingBox.Intersects(frustumBoundingBox)) return;

            for (int i = 0; i < splitCameras.Length; i++)
            {
                var lightCamera = splitCameras[i];

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
                    splitShadowCasters[i].Enqueue(shadowCaster);

                    // AABB の頂点を包含座標として登録。
                    splitLightCameras[i].AddLightVolumePoints(corners);

                    Monitor[i].ShadowCasterCount++;
                    Monitor.TotalShadowCasterCount++;

                    //break;
                }
            }
        }

        // シャドウ マップを描画。
        public void Draw(ShadowMapEffect effect, ref Vector3 lightDirection)
        {
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;

            // 各ライト カメラで描画。
            for (int i = 0; i < splitCameras.Length; i++)
            {
                var camera = splitCameras[i];
                var renderTarget = splitRenderTargets[i];
                var shadowCasters = splitShadowCasters[i];

                //------------------------------------------------------------
                // ライトのビュー×射影行列の更新

                splitLightCameras[i].Update(camera, ref lightDirection);

                //------------------------------------------------------------
                // エフェクト

                effect.LightViewProjection = splitLightCameras[i].LightViewProjection;

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

        public Texture2D GetShadowMap(int index)
        {
            if (index < 0 || splitRenderTargets.Length < index) throw new ArgumentOutOfRangeException("index");

            return splitRenderTargets[index];
        }

        float CalculateFarPlaneDistance(ICamera camera, ref BoundingBox sceneBoundingBox)
        {
            var viewMatrix = camera.View.Matrix;

            // 領域の最も遠い点を探す。
            // z = 0 は視点。
            // より小さな z がより遠い点。
            var maxFar = 0.0f;
            sceneBoundingBox.GetCorners(corners);
            for (int i = 0; i < 8; i++)
            {
                // ビュー座標へ変換。
                var z =
                    corners[i].X * viewMatrix.M13 +
                    corners[i].Y * viewMatrix.M23 +
                    corners[i].Z * viewMatrix.M33 +
                    viewMatrix.M43;

                if (z < maxFar) maxFar = z;
            }

            // 見つかった最も遠い点の z で farPlaneDistance を決定。
            return camera.Projection.NearPlaneDistance - maxFar;
        }

        void CalculateSplitDistances(ICamera camera, float farPlaneDistance)
        {
            var near = camera.Projection.NearPlaneDistance;
            var far = farPlaneDistance;
            var farNearRatio = far / near;
            var splitLambda = shadowMapSettings.SplitLambda;

            for (int i = 0; i < splitDistances.Length; i++)
            {
                float idm = i * inverseSplitCount;

                // CL = n * (f / n)^(i / m)
                float log = (float) (near * Math.Pow(farNearRatio, idm));

                // CU = n + (f - n) * (i / m)
                float uniform = near + (far - near) * idm;
                // REFERENCE: the version (?) in some actual codes,
                //float uniform = (near + idm) * (far - near);

                // C = CL * lambda + CU * (1 - lambda)
                splitDistances[i] = log * splitLambda + uniform * (1.0f - splitLambda);
            }

            splitDistances[0] = near;
            splitDistances[splitDistances.Length - 1] = farPlaneDistance;
        }
    }
}
