#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class PssmScreenSpaceShadow : ScreenSpaceShadow
    {
        public const ShadowTests DefaultShadowTest = ShadowTests.Vsm;

        public const bool DefaultVsmBlurEnabled = true;

        public const int DefaultVsmBlurRadius = 1;

        public const float DefaultVsmBlurAmount = 1;

        public const int DefaultPcfKernelSize = 2;

        public const float DefaultShadowDepthBias = 0.005f;

        BoundingFrustum frustum = new BoundingFrustum(Matrix.Identity);

        Vector3[] corners = new Vector3[8];

        BoundingSphere testSphere;

        // TODO: 初期容量。
        List<Vector3> lightVolumePoints = new List<Vector3>();

        MultiRenderTargets shadowMaps;

        GaussianBlur vsmBlur;

        public int SplitCount { get; set; }

        public Pssm Pssm { get; private set; }

        public ShadowTests ShadowTest { get; private set; }

        public bool VsmBlurEnabled { get; set; }

        public int VsmBlurRadius { get; set; }

        public float VsmBlurAmount { get; set; }

        public ShadowMapEffect ShadowMapEffect { get; set; }

        public PssmSceneEffect PssmSceneEffect { get; set; }

        public int PcfKernelSize { get; set; }

        public float ShadowDepthBias { get; set; }

        public PssmScreenSpaceShadow(GraphicsDevice graphicsDevice)
            : base(graphicsDevice)
        {
            ShadowTest = DefaultShadowTest;
            VsmBlurEnabled = DefaultVsmBlurEnabled;
            PcfKernelSize = DefaultPcfKernelSize;
            ShadowDepthBias = DefaultShadowDepthBias;
        }

        protected override void InitializeOverride()
        {
            if (ShadowMapEffect == null) throw new InvalidOperationException("ShadowMapEffect is null.");
            if (PssmSceneEffect == null) throw new InvalidOperationException("PssmSceneEffect is null.");

            Pssm = new Pssm(SplitCount);

            shadowMaps = new MultiRenderTargets(GraphicsDevice, "ShadowMap", SplitCount, ShadowMapSize, ShadowMapSize,
                false, ShadowMapFormat, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.PlatformContents);

            vsmBlur = new GaussianBlur(GaussianBlurEffect, SpriteBatch, ShadowMapSize, ShadowMapSize, ShadowMapFormat,
                VsmBlurRadius, VsmBlurAmount);

            base.InitializeOverride();
        }

        public override void Prepare(View eyeView, PerspectiveFov eyeProjection)
        {
            // ライト ボリュームを初期化。
            //
            // TODO: PSSM 用の射影行列を設定しているかどうかを確認。
            //
            Matrix viewProjection;
            Matrix.Multiply(ref eyeView.Matrix, ref eyeProjection.Matrix, out viewProjection);
            frustum.Matrix = viewProjection;
            corners = frustum.GetCorners();
            var boundingBox = BoundingBox.CreateFromPoints(corners);

            // 非投影オブジェクトを大きく除外するための球。
            testSphere = BoundingSphere.CreateFromPoints(corners);
            testSphere.Radius *= 1.2f;

            // PSSM を準備。
            Pssm.Prepare(eyeView, eyeProjection, ref boundingBox);
        }

        // Prepare 呼び出し後に呼び出す前提。
        public void TryAddShadowCaster(IShadowCaster shadowCaster)
        {
            if (shadowCaster == null) throw new ArgumentNullException("shadowCaster");

            // TODO: 本当にこれで良いのだろうか？
            BoundingSphere casterSphere;
            shadowCaster.GetShadowTestBoundingSphere(out casterSphere);
            if (!casterSphere.Intersects(testSphere)) return;

            Pssm.TryAddShadowCaster(shadowCaster);
        }

        protected override void DrawShadowMap(View eyeView, PerspectiveFov eyeProjection)
        {
            // TODO: シェーダで設定できるのでは？
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;

            // シャドウ テストの種類に応じたテクニックを設定。
            ShadowMapEffect.EnableTechnique(ShadowTest);

            for (int i = 0; i < SplitCount; i++)
            {
                var camera = Pssm.GetCamera(i);

                // カメラのビュー行列と射影行列を更新。
                camera.Update(eyeView);
                // カメラのビュー×射影行列。
                Matrix lightViewProjection;
                Matrix.Multiply(ref camera.View.Matrix, ref camera.Projection.Matrix, out lightViewProjection);

                // エフェクトに設定。
                ShadowMapEffect.LightViewProjection = lightViewProjection;

                // シャドウ マップを描画。
                GraphicsDevice.SetRenderTarget(shadowMaps[i]);
                GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);

                foreach (var caster in camera.ShadowCasters) caster.Draw(ShadowMapEffect);
                
                GraphicsDevice.SetRenderTarget(null);

                // VSM の場合はブラーを適用。
                if (ShadowTest == ShadowTests.Vsm && VsmBlurEnabled)
                    vsmBlur.Filter(shadowMaps[i]);

                // 投影オブジェクトなどを削除。
                //camera.Clear();
            }
        }

        protected override void DrawShadowSceneMap(View eyeView, PerspectiveFov eyeProjection)
        {
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;

            PssmSceneEffect.EnableTechnique(ShadowTest);
            if (ShadowTest == ShadowTests.Pcf)
            {
                PssmSceneEffect.ConfigurePcf(ShadowMapSize, PcfKernelSize);
            }

            PssmSceneEffect.View = eyeView.Matrix;
            PssmSceneEffect.Projection = eyeProjection.Matrix;
            PssmSceneEffect.DepthBias = ShadowDepthBias;
            PssmSceneEffect.SplitCount = SplitCount;
            PssmSceneEffect.SplitDistances = Pssm.SplitDistances;
            PssmSceneEffect.SplitViewProjections = Pssm.SplitViewProjections;
            PssmSceneEffect.SetShadowMaps(shadowMaps);

            GraphicsDevice.SetRenderTarget(ShadowSceneMap);

            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);

            for (int i = 0; i < SplitCount; i++)
            {
                var camera = Pssm.GetCamera(i);

                foreach (var caster in camera.ShadowCasters) caster.Draw(PssmSceneEffect);

                // PSSM カメラはこれ以降使わないので投影オブジェクトなどをリストから削除。
                camera.Clear();
            }

            GraphicsDevice.SetRenderTarget(null);

            // シャドウ シーンにブラーを適用。
            BlurShadowSceneMap();
        }
    }
}
