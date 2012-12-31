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

        PssmSettings pssmSettings;

        Vector3[] corners = new Vector3[8];

        float inverseSplitCount;

        PssmLightCamera[] splitLightCameras;

        float[] splitDistances;

        float[] safeSplitDistances;

        Matrix[] safeSplitLightViewProjections;

        MultiRenderTargets splitRenderTargets;

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
            pssmSettings = shadowSettings.LightFrustum.Pssm;

            SplitCount = pssmSettings.SplitCount;
            inverseSplitCount = 1.0f / (float) SplitCount;
            splitDistances = new float[SplitCount + 1];
            safeSplitDistances = new float[SplitCount + 1];
            safeSplitLightViewProjections = new Matrix[SplitCount];
            safeSplitShadowMaps = new Texture2D[SplitCount];

            splitLightCameras = new PssmLightCamera[SplitCount];
            for (int i = 0; i < splitLightCameras.Length; i++)
                splitLightCameras[i] = new PssmLightCamera("PssmLight" + i, shadowMapSettings.Size);

            // TODO: パラメータ見直し or 外部設定化。
            var pp = GraphicsDevice.PresentationParameters;
            // メモ: ブラーをかける場合があるので RenderTargetUsage.PreserveContents で作成。
            splitRenderTargets = new MultiRenderTargets(GraphicsDevice, "ShadowMap", SplitCount,
                shadowMapSettings.Size, shadowMapSettings.Size,
                false, shadowMapSettings.Format, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.PreserveContents);

            // TODO: 初期容量。
            splitShadowCasters = new Queue<ShadowCaster>[SplitCount];
            for (int i = 0; i < splitShadowCasters.Length; i++)
                splitShadowCasters[i] = new Queue<ShadowCaster>();

            blurSpriteBatch = new SpriteBatch(GraphicsDevice);
            blur = new GaussianBlur(blurEffect, blurSpriteBatch, shadowMapSettings.Size, shadowMapSettings.Size, SurfaceFormat.Vector2,
                vsmSettings.Blur.Radius, vsmSettings.Blur.Amount);

            Monitor = new PssmMonitor(this, SplitCount);
        }

        public void Prepare(ICamera camera, ref Vector3 lightDirection)
        {
            if (camera == null) throw new ArgumentNullException("camera");

            // デフォルトでは視錐台を含む AABB で準備する。
            camera.Frustum.GetCorners(corners);
            frustumBoundingBox = BoundingBox.CreateFromPoints(corners);

            Prepare(camera, ref lightDirection, ref frustumBoundingBox);
        }

        public void Prepare(ICamera camera, ref Vector3 lightDirection, ref BoundingBox sceneBoundingBox)
        {
            if (camera == null) throw new ArgumentNullException("camera");

            var far = CalculateFarPlaneDistance(camera, ref sceneBoundingBox);
            CalculateSplitDistances(camera, far);

            for (int i = 0; i < splitDistances.Length - 1; i++)
            {
                splitLightCameras[i].LightView.Direction = lightDirection;
                splitLightCameras[i].View.Position = camera.View.Position;
                splitLightCameras[i].View.Direction = camera.View.Direction;
                splitLightCameras[i].View.Up = camera.View.Up;
                splitLightCameras[i].Projection.Fov = camera.Projection.Fov;
                splitLightCameras[i].Projection.AspectRatio = camera.Projection.AspectRatio;
                splitLightCameras[i].Projection.NearPlaneDistance = splitDistances[i];
                splitLightCameras[i].Projection.FarPlaneDistance = splitDistances[i + 1];

                // 分割カメラのビュー行列と射影行列を更新。
                splitLightCameras[i].Update();

                Monitor[i].ShadowCasterCount = 0;
            }

            // TODO: 必要？
            //Matrix invertView;
            //Matrix.Invert(ref camera.View.Matrix, out invertView);
            //Vector3 position = camera.Position;
            //var back = position + invertView.Backward * 10;
            //var left = position + invertView.Left * 10;
            //var right = position + invertView.Right * 10;
            //var up = position + invertView.Up * 10;
            //splitLightCameras[0].AddLightVolumePoint(ref back);
            //splitLightCameras[0].AddLightVolumePoint(ref left);
            //splitLightCameras[0].AddLightVolumePoint(ref right);
            //splitLightCameras[0].AddLightVolumePoint(ref up);

            Monitor.TotalShadowCasterCount = 0;
        }

        public void TryAddShadowCaster(ShadowCaster shadowCaster)
        {
            if (!shadowCaster.BoundingSphere.Intersects(frustumBoundingBox)) return;
            if (!shadowCaster.BoundingBox.Intersects(frustumBoundingBox)) return;

            for (int i = 0; i < splitLightCameras.Length; i++)
            {
                var lightCamera = splitLightCameras[i];

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
                    lightCamera.AddLightVolumePoints(corners);

                    Monitor[i].ShadowCasterCount++;
                    Monitor.TotalShadowCasterCount++;

                    break;
                }
            }
        }

        // シャドウ マップを描画。
        public void Draw(ShadowMapEffect effect)
        {
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;

            // 各ライト カメラで描画。
            for (int i = 0; i < splitLightCameras.Length; i++)
            {
                var lightCamera = splitLightCameras[i];
                var renderTarget = splitRenderTargets[i];
                var shadowCasters = splitShadowCasters[i];

                //------------------------------------------------------------
                // ライトのビュー×射影行列の更新

                lightCamera.UpdateLightViewProjection();

                //------------------------------------------------------------
                // エフェクト

                effect.LightViewProjection = lightCamera.LightViewProjection;

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
            if (index < 0 || splitRenderTargets.Count < index) throw new ArgumentOutOfRangeException("index");

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
            var splitLambda = pssmSettings.SplitLambda;

            for (int i = 0; i < splitDistances.Length; i++)
            {
                float idm = i * inverseSplitCount;
                float log = (float) (near * Math.Pow(farNearRatio, idm));

                // REFERENCE: the version in the main PSSM paper
                float uniform = near + (far - near) * idm;
                // REFERENCE: the version (?) in some actual codes,
                //float uniform = (near + idm) * (far - near);

                splitDistances[i] = log * splitLambda + uniform * (1.0f - splitLambda);
            }

            splitDistances[0] = near;
            splitDistances[splitDistances.Length - 1] = farPlaneDistance;
        }
    }
}
