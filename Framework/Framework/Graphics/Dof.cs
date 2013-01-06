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
        #region DofEffect

        sealed class DofEffect
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

            public DofEffect(Effect effect)
            {
                Effect = effect;

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

        BasicCamera internalCamera = new BasicCamera("DofInternal");

        Vector3[] frustumCorners = new Vector3[8];

        BoundingSphere frustumSphere;

        DepthMapEffect depthMapEffect;

        DofEffect dofEffect;

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
            this.dofEffect = new DofEffect(dofEffect);

            //----------------------------------------------------------------
            // レンダ ターゲット

            var pp = GraphicsDevice.PresentationParameters;
            var width = (int) (pp.BackBufferWidth * settings.MapScale);
            var height = (int) (pp.BackBufferHeight * settings.MapScale);

            // 深度マップ
            DepthMap = new RenderTarget2D(GraphicsDevice, width, height,
                false, SurfaceFormat.Single, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

            // ブラー済みシーン マップ
            BluredSceneMap = new RenderTarget2D(GraphicsDevice, width, height,
                false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);

            //----------------------------------------------------------------
            // ブラー

            blur = new GaussianBlur(blurEffect, spriteBatch, width, height, SurfaceFormat.Color,
                settings.Blur.Radius, settings.Blur.Amount);
        }

        public void DrawDepth(ICamera viewerCamera, IEnumerable<SceneObject> sceneObjects)
        {
            if (viewerCamera == null) throw new ArgumentNullException("viewerCamera");
            if (sceneObjects == null) throw new ArgumentNullException("sceneObjects");

            //----------------------------------------------------------------
            // 内部カメラの準備

            internalCamera.View.Position = viewerCamera.View.Position;
            internalCamera.View.Direction = viewerCamera.View.Direction;
            internalCamera.View.Up = viewerCamera.View.Up;
            internalCamera.Projection.Fov = viewerCamera.Projection.Fov;
            internalCamera.Projection.AspectRatio = viewerCamera.Projection.AspectRatio;
            internalCamera.Projection.NearPlaneDistance = viewerCamera.Projection.NearPlaneDistance;
            internalCamera.Projection.FarPlaneDistance = viewerCamera.Projection.FarPlaneDistance;
            internalCamera.Update();

            internalCamera.Frustum.GetCorners(frustumCorners);
            frustumSphere = BoundingSphere.CreateFromPoints(frustumCorners);

            //----------------------------------------------------------------
            // エフェクト

            depthMapEffect.View = internalCamera.View.Matrix;
            depthMapEffect.Projection = internalCamera.Projection.Matrix;

            //----------------------------------------------------------------
            // 描画

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;

            GraphicsDevice.SetRenderTarget(DepthMap);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);

            foreach (var sceneObject in sceneObjects)
            {
                // 専用カメラの視錐台に含まれるもののみを描画。
                if (IsVisibleObject(sceneObject))
                    sceneObject.Draw(depthMapEffect);
            }

            GraphicsDevice.SetRenderTarget(null);
        }

        public void Filter(RenderTarget2D sceneMap, RenderTarget2D result)
        {
            if (sceneMap == null) throw new ArgumentNullException("sceneMap");
            if (result == null) throw new ArgumentNullException("result");

            //================================================================
            // シーンにブラーを適用

            blur.Filter(sceneMap, BluredSceneMap);

            //================================================================
            // シーンとブラー済みシーンを深度マップに基いて合成

            //----------------------------------------------------------------
            // エフェクト

            var distance = internalCamera.Projection.FarPlaneDistance - internalCamera.Projection.NearPlaneDistance;
            dofEffect.NearPlaneDistance = internalCamera.Projection.NearPlaneDistance;
            dofEffect.FarPlaneDistance = internalCamera.Projection.FarPlaneDistance / distance;
            dofEffect.FocusDistance = internalCamera.FocusDistance;
            dofEffect.FocusRange = internalCamera.FocusRange;
            dofEffect.DepthMap = DepthMap;
            dofEffect.BluredSceneMap = BluredSceneMap;

            //----------------------------------------------------------------
            // 描画

            GraphicsDevice.SetRenderTarget(result);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, null, null, null, dofEffect.Effect);
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
            return sceneObject.BoundingBox.Intersects(internalCamera.Frustum);
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
