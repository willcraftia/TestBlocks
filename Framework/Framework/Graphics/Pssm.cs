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

        float eyeFarPlaneDistance;

        float inverseSplitCount;

        PssmLightCamera[] splitLightCameras;

        float[] splitDistances;

        Matrix[] splitViewProjections;

        MultiRenderTargets splitRenderTargets;

        Texture2D[] splitShadowMaps;

        Queue<IShadowCaster>[] splitShadowCasters;

        SpriteBatch blurSpriteBatch;

        GaussianBlur blur;

        public GraphicsDevice GraphicsDevice { get; private set; }

        public ShadowSettings ShadowSettings { get; private set; }

        public int SplitCount { get; private set; }

        public float[] SplitDistances
        {
            get { return splitDistances; }
        }

        public Matrix[] SplitViewProjections
        {
            get
            {
                for (int i = 0; i < splitLightCameras.Length; i++)
                    splitViewProjections[i] = splitLightCameras[i].ViewProjection;
                return splitViewProjections;
            }
        }

        public Texture2D[] SplitShadowMaps
        {
            get
            {
                for (int i = 0; i < splitShadowMaps.Length; i++)
                    splitShadowMaps[i] = splitRenderTargets[i];
                return splitShadowMaps;
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
            splitViewProjections = new Matrix[SplitCount];
            splitShadowMaps = new Texture2D[SplitCount];

            splitLightCameras = new PssmLightCamera[SplitCount];
            for (int i = 0; i < splitLightCameras.Length; i++)
                splitLightCameras[i] = new PssmLightCamera(shadowMapSettings.Size);

            // TODO: パラメータ見直し or 外部設定化。
            var pp = GraphicsDevice.PresentationParameters;
            // メモ: ブラーをかける場合があるので RenderTargetUsage.PreserveContents で作成。
            splitRenderTargets = new MultiRenderTargets(GraphicsDevice, "ShadowMap", SplitCount,
                shadowMapSettings.Size, shadowMapSettings.Size,
                false, shadowMapSettings.Format, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.PreserveContents);

            // TODO: 初期容量。
            splitShadowCasters = new Queue<IShadowCaster>[SplitCount];
            for (int i = 0; i < splitShadowCasters.Length; i++)
                splitShadowCasters[i] = new Queue<IShadowCaster>();

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
            var boundingBox = BoundingBox.CreateFromPoints(corners);

            Prepare(camera, ref lightDirection, ref boundingBox);
        }

        public void Prepare(ICamera camera, ref Vector3 lightDirection, ref BoundingBox boundingBox)
        {
            if (camera == null) throw new InvalidOperationException("Camera is null.");

            CalculateEyeFarPlaneDistance(camera, ref boundingBox);
            CalculateSplitDistances(camera);

            for (int i = 0; i < splitDistances.Length - 1; i++)
            {
                var near = splitDistances[i];
                var far = splitDistances[i + 1];

                splitLightCameras[i].LightView.Direction = lightDirection;
                splitLightCameras[i].SplitEyeProjection.Fov = camera.Projection.Fov;
                splitLightCameras[i].SplitEyeProjection.AspectRatio = camera.Projection.AspectRatio;
                splitLightCameras[i].SplitEyeProjection.NearPlaneDistance = near;
                splitLightCameras[i].SplitEyeProjection.FarPlaneDistance = far;

                splitLightCameras[i].Prepare(camera);

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

        public void TryAddShadowCaster(IShadowCaster shadowCaster)
        {
            for (int i = 0; i < splitLightCameras.Length; i++)
            {
                var lightCamera = splitLightCameras[i];
                var shadowCasters = splitShadowCasters[i];

                BoundingSphere casterSphere;
                shadowCaster.GetBoundingSphere(out casterSphere);

                BoundingBox casterBox;
                shadowCaster.GetBoundingBox(out casterBox);
                casterBox.GetCorners(corners);

                bool shouldAdd = false;
                if (casterSphere.Intersects(lightCamera.SplitEyeFrustum))
                {
                    shouldAdd = true;
                }
                else if (casterBox.Intersects(lightCamera.SplitEyeFrustum))
                {
                    shouldAdd = true;
                }
                else
                {
                    //var ray = new Ray
                    //{
                    //    Direction = lightCamera.LightView.Direction
                    //};

                    //// TODO: 要検討。
                    //for (int j = 0; j < 8; j++)
                    //{
                    //    // AABB の頂点から光方向の線。
                    //    ray.Position = corners[j];

                    //    // 分割視錐台と交差するか否か。
                    //    var distance = ray.Intersects(lightCamera.SplitEyeFrustum);
                    //    if (distance == null) continue;

                    //    // TODO
                    //    if (distance < 10)
                    //    {
                    //        shouldAdd = true;
                    //        break;
                    //    }
                    //}
                }

                if (shouldAdd)
                {
                    // 投影オブジェクトとして登録。
                    shadowCasters.Enqueue(shadowCaster);

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

                lightCamera.UpdateViewProjection();

                //------------------------------------------------------------
                // エフェクト

                effect.LightViewProjection = lightCamera.ViewProjection;

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

        void CalculateEyeFarPlaneDistance(ICamera camera, ref BoundingBox boundingBox)
        {
            var viewMatrix = camera.View.Matrix;

            //
            // smaller z, more far
            // z == 0 means the point of view
            //
            var maxFar = 0.0f;
            boundingBox.GetCorners(corners);
            for (int i = 0; i < 8; i++)
            {
                var z =
                    corners[i].X * viewMatrix.M13 +
                    corners[i].Y * viewMatrix.M23 +
                    corners[i].Z * viewMatrix.M33 +
                    viewMatrix.M43;

                if (z < maxFar) maxFar = z;
            }

            eyeFarPlaneDistance = camera.Projection.NearPlaneDistance - maxFar;
        }

        void CalculateSplitDistances(ICamera camera)
        {
            var near = camera.Projection.NearPlaneDistance;
            var far = eyeFarPlaneDistance;
            var farNearRatio = far / near;
            var splitLambda = pssmSettings.SplitLambda;

            for (int i = 0; i < splitDistances.Length; i++)
            {
                float idm = i * inverseSplitCount;
                float log = (float) (near * Math.Pow(farNearRatio, idm));

                // REFERENCE: the version in the main PSSM paper
                float uniform = near + (far - near) * idm;
                // REFERENCE: the version (?) in some actual codes,
                // i think the following is a wrong formula.
                //float uniform = (near + idm) * (far - near);

                splitDistances[i] = log * splitLambda + uniform * (1.0f - splitLambda);
            }

            splitDistances[0] = near;
            splitDistances[splitDistances.Length - 1] = eyeFarPlaneDistance;
        }
    }
}
