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
        ShadowSettings shadowSettings;

        ShadowMapSettings shadowMapSettings;

        PssmSettings pssmSettings;

        Vector3 lightDirection = Vector3.Down;

        Vector3[] corners = new Vector3[8];

        float eyeFarPlaneDistance;

        float inverseSplitCount;

        PssmLightCamera[] splitLightCameras;

        float[] splitDistances;

        Matrix[] splitViewProjections;

        MultiRenderTargets splitRenderTargets;

        Queue<IShadowCaster>[] splitShadowCasters;

        List<Vector3>[] splitLightVolumePoints;

        public GraphicsDevice GraphicsDevice { get; private set; }

        // PSSM で参照する視点カメラ。
        // 実視点カメラである必要はない。
        // PSSM 用に状態を設定した視点カメラを設定。
        public ICamera Camera { get; set; }

        public Vector3 LightDirection
        {
            get { return lightDirection; }
            set { lightDirection = value; }
        }

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

        public Pssm(GraphicsDevice graphicsDevice, ShadowSettings shadowSettings)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");
            if (shadowSettings == null) throw new ArgumentNullException("shadowSettings");

            GraphicsDevice = graphicsDevice;
            this.shadowSettings = shadowSettings;

            shadowMapSettings = shadowSettings.ShadowMap;
            pssmSettings = shadowSettings.LightFrustum.Pssm;

            inverseSplitCount = 1.0f / (float) pssmSettings.SplitCount;
            splitDistances = new float[pssmSettings.SplitCount + 1];
            splitViewProjections = new Matrix[pssmSettings.SplitCount];

            splitLightCameras = new PssmLightCamera[pssmSettings.SplitCount];
            for (int i = 0; i < splitLightCameras.Length; i++)
                splitLightCameras[i] = new PssmLightCamera(shadowMapSettings.Size);

            // TODO: パラメータ見直し or 外部設定化。
            splitRenderTargets = new MultiRenderTargets(GraphicsDevice, "ShadowMap", pssmSettings.SplitCount,
                shadowMapSettings.Size, shadowMapSettings.Size,
                false, SurfaceFormat.Vector2, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.PlatformContents);

            // TODO: 初期容量。
            splitShadowCasters = new Queue<IShadowCaster>[pssmSettings.SplitCount];
            for (int i = 0; i < splitShadowCasters.Length; i++)
                splitShadowCasters[i] = new Queue<IShadowCaster>();

            // TODO: 初期容量。
            splitLightVolumePoints = new List<Vector3>[pssmSettings.SplitCount];
            for (int i = 0; i < splitLightVolumePoints.Length; i++)
                splitLightVolumePoints[i] = new List<Vector3>();
        }

        public void Prepare()
        {
            if (Camera == null) throw new InvalidOperationException("Camera is null.");

            // デフォルトでは視錐台を含む AABB で準備する。
            Camera.Frustum.GetCorners(corners);
            var boundingBox = BoundingBox.CreateFromPoints(corners);

            Prepare(ref boundingBox);
        }

        public void Prepare(ref BoundingBox boundingBox)
        {
            if (Camera == null) throw new InvalidOperationException("Camera is null.");

            CalculateEyeFarPlaneDistance(ref boundingBox);
            CalculateSplitDistances();

            for (int i = 0; i < splitDistances.Length; i++)
            {
                var near = splitDistances[i];
                var far = splitDistances[i + 1];

                splitLightCameras[i].LightView.Direction = lightDirection;
                splitLightCameras[i].SplitEyeProjection.Fov = Camera.Projection.Fov;
                splitLightCameras[i].SplitEyeProjection.AspectRatio = Camera.Projection.AspectRatio;
                splitLightCameras[i].SplitEyeProjection.NearPlaneDistance = near;
                splitLightCameras[i].SplitEyeProjection.FarPlaneDistance = far;
                
                splitLightCameras[i].Prepare(Camera);
            }
        }

        public void TryAddShadowCaster(IShadowCaster shadowCaster)
        {
            for (int i = 0; i < splitLightCameras.Length; i++)
            {
                var lightCamera = splitLightCameras[i];
                var shadowCasters = splitShadowCasters[i];
                var lightVolumePoints = splitLightVolumePoints[i];

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
                else
                {
                    var ray = new Ray
                    {
                        Direction = lightCamera.LightView.Direction
                    };

                    // TODO: 要検討。
                    for (int j = 0; j < 8; j++)
                    {
                        // AABB の頂点から光方向の線。
                        ray.Position = corners[j];

                        // 分割視錐台と交差するか否か。
                        var distance = ray.Intersects(lightCamera.SplitEyeFrustum);
                        if (distance == null) continue;

                        // TODO
                        if (distance < 10)
                        {
                            shouldAdd = true;
                            break;
                        }
                    }
                }

                if (shouldAdd)
                {
                    // 投影オブジェクトとして登録。
                    shadowCasters.Enqueue(shadowCaster);

                    // AABB の頂点を包含座標として登録。
                    lightCamera.AddLightVolumePoints(corners);

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
                // エフェクト

                lightCamera.UpdateViewProjection();

                effect.LightViewProjection = lightCamera.ViewProjection;

                //------------------------------------------------------------
                // 描画

                GraphicsDevice.SetRenderTarget(renderTarget);

                while (0 < shadowCasters.Count)
                {
                    var shadowCaster = shadowCasters.Dequeue();
                    shadowCaster.DrawShadow(effect);
                }

                GraphicsDevice.SetRenderTarget(null);
            }
        }

        void CalculateEyeFarPlaneDistance(ref BoundingBox boundingBox)
        {
            var viewMatrix = Camera.View.Matrix;

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

            eyeFarPlaneDistance = Camera.Projection.NearPlaneDistance - maxFar;
        }

        void CalculateSplitDistances()
        {
            var near = Camera.Projection.NearPlaneDistance;
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
