#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class Dof : IDisposable
    {
        #region DofEffectAdapter

        sealed class DofEffectAdapter
        {
            EffectParameter focusDistance;

            EffectParameter focusRange;

            EffectParameter nearPlaneDistance;

            EffectParameter farPlaneDistance;

            EffectParameter sceneMap;

            EffectParameter depthMap;
            
            EffectParameter bluredSceneMap;

            public Effect Effect { get; private set; }

            public float FocusDistance
            {
                get { return focusDistance.GetValueSingle(); }
                set { focusDistance.SetValue(value); }
            }

            public float FocusRange
            {
                get { return focusRange.GetValueSingle(); }
                set { focusRange.SetValue(value); }
            }

            public float NearPlaneDistance
            {
                get { return nearPlaneDistance.GetValueSingle(); }
                set { nearPlaneDistance.SetValue(value); }
            }

            public float FarPlaneDistance
            {
                get { return farPlaneDistance.GetValueSingle(); }
                set { farPlaneDistance.SetValue(value); }
            }

            public Texture2D SceneMap
            {
                get { return sceneMap.GetValueTexture2D(); }
                set { sceneMap.SetValue(value); }
            }

            public Texture2D DepthMap
            {
                get { return depthMap.GetValueTexture2D(); }
                set { depthMap.SetValue(value); }
            }

            public Texture2D BluredSceneMap
            {
                get { return bluredSceneMap.GetValueTexture2D(); }
                set { bluredSceneMap.SetValue(value); }
            }

            public DofEffectAdapter(Effect effect)
            {
                this.Effect = effect;

                focusDistance = effect.Parameters["FocusDistance"];
                focusRange = effect.Parameters["FocusRange"];
                nearPlaneDistance = effect.Parameters["NearPlaneDistance"];
                farPlaneDistance = effect.Parameters["FarPlaneDistance"];

                sceneMap = effect.Parameters["SceneMap"];
                depthMap = effect.Parameters["DepthMap"];
                bluredSceneMap = effect.Parameters["BluredSceneMap"];
            }

            public void Apply()
            {
                Effect.CurrentTechnique.Passes[0].Apply();
            }
        }

        #endregion

        BasicCamera depthCamera = new BasicCamera("Depth");

        Vector3[] frustumCorners = new Vector3[8];

        BoundingSphere frustumSphere;

        DepthMapEffect depthMapEffect;

        DofEffectAdapter dofEffectAdapter;

        SpriteBatch spriteBatch;

        GaussianBlur blur;

        public GraphicsDevice GraphicsDevice { get; private set; }

        public DofSettings Settings { get; private set; }

        public RenderTarget2D DepthMap { get; private set; }

        public RenderTarget2D BluredSceneMap { get; private set; }

        public Dof(GraphicsDevice graphicsDevice, DofSettings settings,
            SpriteBatch spriteBatch, Effect depthMapEffect, Effect dofEffect, Effect blurEffect)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");
            if (settings == null) throw new ArgumentNullException("settings");
            if (spriteBatch == null) throw new ArgumentNullException("spriteBatch");
            if (depthMapEffect == null) throw new ArgumentNullException("depthMapEffect");
            if (dofEffect == null) throw new ArgumentNullException("dofEffect");
            if (blurEffect == null) throw new ArgumentNullException("blurEffect");

            GraphicsDevice = graphicsDevice;
            Settings = settings;
            this.spriteBatch = spriteBatch;

            //----------------------------------------------------------------
            // エフェクト

            this.depthMapEffect = new DepthMapEffect(depthMapEffect);
            dofEffectAdapter = new DofEffectAdapter(dofEffect);

            //----------------------------------------------------------------
            // レンダ ターゲット

            var pp = GraphicsDevice.PresentationParameters;
            var width = (int) (pp.BackBufferWidth * settings.MapScale);
            var height = (int) (pp.BackBufferHeight * settings.MapScale);

            // 深度マップ
            DepthMap = new RenderTarget2D(GraphicsDevice, width, height,
                false, settings.Format, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

            // ブラー済みシーン マップ
            BluredSceneMap = new RenderTarget2D(GraphicsDevice, width, height,
                false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);

            //----------------------------------------------------------------
            // ブラー機能

            blur = new GaussianBlur(blurEffect, spriteBatch, width, height, SurfaceFormat.Color,
                settings.Blur.Radius, settings.Blur.Amount);
        }

        // TODO
        // シーン マネージャで全ての描画対象を収集しておく。

        public void DrawDepth(ICamera viewerCamera, IEnumerable<SceneObject> sceneObjects)
        {
            if (viewerCamera == null) throw new ArgumentNullException("viewerCamera");
            if (sceneObjects == null) throw new ArgumentNullException("sceneObjects");

            //----------------------------------------------------------------
            // 深度マップ用カメラの準備

            depthCamera.View.Position = viewerCamera.View.Position;
            depthCamera.View.Direction = viewerCamera.View.Direction;
            depthCamera.View.Up = viewerCamera.View.Up;
            depthCamera.Projection.Fov = viewerCamera.Projection.Fov;
            depthCamera.Projection.AspectRatio = viewerCamera.Projection.AspectRatio;
            // TODO: スカイ スフィアの分だけ暫定的に大きめにとっている
            depthCamera.Projection.NearPlaneDistance = viewerCamera.Projection.NearPlaneDistance + 10;
            depthCamera.Update();

            depthCamera.Frustum.GetCorners(frustumCorners);
            frustumSphere = BoundingSphere.CreateFromPoints(frustumCorners);

            //----------------------------------------------------------------
            // 深度マップ用エフェクトの準備

            depthMapEffect.View = depthCamera.View.Matrix;
            depthMapEffect.Projection = depthCamera.Projection.Matrix;

            //----------------------------------------------------------------
            // 描画

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;

            GraphicsDevice.SetRenderTarget(DepthMap);

            foreach (var sceneObject in sceneObjects)
            {
                // 専用カメラの視錐台に含まれるもののみを描画。
                if (IsVisibleObject(sceneObject))
                    sceneObject.Draw(depthMapEffect);
            }

            GraphicsDevice.SetRenderTarget(null);
        }

        public void Filter(ICamera viewerCamera, RenderTarget2D sceneMap, RenderTarget2D result)
        {
            //================================================================
            // シーンにブラーをかける

            blur.Filter(sceneMap, BluredSceneMap);

            //================================================================
            // シーンとブラー済みシーンを深度マップに基いて合成

            //----------------------------------------------------------------
            // エフェクト設定

            var distance = viewerCamera.Projection.FarPlaneDistance - viewerCamera.Projection.NearPlaneDistance;
            dofEffectAdapter.NearPlaneDistance = viewerCamera.Projection.NearPlaneDistance;
            dofEffectAdapter.FarPlaneDistance = viewerCamera.Projection.FarPlaneDistance / distance;
            dofEffectAdapter.FocusDistance = viewerCamera.FocusDistance;
            dofEffectAdapter.FocusRange = viewerCamera.FocusRange;
            dofEffectAdapter.DepthMap = DepthMap;
            dofEffectAdapter.BluredSceneMap = BluredSceneMap;

            //----------------------------------------------------------------
            // 描画

            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.BlendState = BlendState.Opaque;

            GraphicsDevice.SetRenderTarget(result);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, null, null, null, dofEffectAdapter.Effect);
            spriteBatch.Draw(sceneMap, result.Bounds, Color.White);
            spriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);
        }

        bool IsVisibleObject(SceneObject sceneObject)
        {
            // 球同士で判定。
            if (!sceneObject.BoundingSphere.Intersects(frustumSphere)) return false;

            // 球と視錐台で判定。
            if (!sceneObject.BoundingSphere.Intersects(frustumSphere)) return false;

            // 視錐台と AABB で判定。
            return sceneObject.BoundingBox.Intersects(depthCamera.Frustum);
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed;

        ~Dof()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                depthMapEffect.Dispose();
                DepthMap.Dispose();
                BluredSceneMap.Dispose();
                blur.Dispose();
            }

            disposed = true;
        }

        #endregion
    }
}
