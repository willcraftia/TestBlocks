#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class Dof
    {
        sealed class DepthMapEffect : Effect, IEffectMatrices
        {
            EffectParameter projection;

            EffectParameter view;

            EffectParameter world;

            // I/F
            public Matrix Projection
            {
                get { return projection.GetValueMatrix(); }
                set { projection.SetValue(value); }
            }

            // I/F
            public Matrix View
            {
                get { return view.GetValueMatrix(); }
                set { view.SetValue(value); }
            }

            // I/F
            public Matrix World
            {
                get { return world.GetValueMatrix(); }
                set { world.SetValue(value); }
            }

            public DepthMapEffect(Effect cloneSource)
                : base(cloneSource)
            {
                world = Parameters["World"];
                view = Parameters["View"];
                projection = Parameters["Projection"];
            }
        }

        BasicCamera depthCamera = new BasicCamera("Depth");

        Vector3[] frustumCorners = new Vector3[8];

        BoundingSphere frustumSphere;

        DepthMapEffect depthMapEffect;

        RenderTarget2D depthMapRenderTarget;

        RenderTarget2D blurRenderTarget;

        public GraphicsDevice GraphicsDevice { get; private set; }

        public DofSettings Settings { get; private set; }

        public Dof(GraphicsDevice graphicsDevice, DofSettings settings, Effect depthMapEffect, Effect dofEffect, Effect blurEffect)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");
            if (settings == null) throw new ArgumentNullException("settings");
            if (depthMapEffect == null) throw new ArgumentNullException("depthMapEffect");
            if (dofEffect == null) throw new ArgumentNullException("dofEffect");
            if (blurEffect == null) throw new ArgumentNullException("blurEffect");

            GraphicsDevice = graphicsDevice;
            Settings = settings;

            //----------------------------------------------------------------
            // エフェクト

            this.depthMapEffect = new DepthMapEffect(depthMapEffect);

            //----------------------------------------------------------------
            // レンダ ターゲット

            var pp = GraphicsDevice.PresentationParameters;
            var width = (int) (pp.BackBufferWidth * Settings.MapScale);
            var height = (int) (pp.BackBufferHeight * Settings.MapScale);
            depthMapRenderTarget = new RenderTarget2D(GraphicsDevice, width, height,
                false, Settings.Format, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
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
            depthCamera.Projection.NearPlaneDistance = viewerCamera.Projection.NearPlaneDistance;
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

            GraphicsDevice.SetRenderTarget(depthMapRenderTarget);

            foreach (var sceneObject in sceneObjects)
            {
                // 専用カメラの視錐台に含まれるもののみを描画。
                if (IsVisibleObject(sceneObject))
                    sceneObject.Draw(depthMapEffect);
            }

            GraphicsDevice.SetRenderTarget(null);
        }

        public void Filter(RenderTarget2D source, RenderTarget2D destination)
        {
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
    }
}
