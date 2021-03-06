﻿#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Diagnostics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class Dof : PostProcessor, IDisposable
    {
        #region Settings

        public sealed class Settings
        {
            float mapScale = 0.5f;

            /// <summary>
            /// 実スクリーンに対する深度マップのスケールを取得または設定します。
            /// </summary>
            public float MapScale
            {
                get { return mapScale; }
                set
                {
                    if (value <= 0) throw new ArgumentOutOfRangeException("value");

                    mapScale = value;
                }
            }

            /// <summary>
            /// ブラー設定を取得します。
            /// </summary>
            public BlurSettings Blur { get; private set; }

            public Settings()
            {
                Blur = new BlurSettings();
            }
        }

        #endregion

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

        public const string InstrumentProcess = "Dof.Process";

        public const string InstrumentDrawDepth = "Dof.DrawDepth";

        public const string InstrumentFilter = "Dof.Filter";

        BasicCamera internalCamera = new BasicCamera("DofInternal");

        Vector3[] frustumCorners = new Vector3[8];

        BoundingSphere frustumSphere;

        DepthMapEffect depthMapEffect;

        DofEffect dofEffect;

        GaussianBlur blur;

        RenderTarget2D depthMap;

        RenderTarget2D bluredSceneMap;

        Settings settings;

        float farPlaneDistance = PerspectiveFov.DefaultFarPlaneDistance;

        /// <summary>
        /// 深度マップ描画で使用するカメラの FarPlaneDistance を取得または設定します。
        /// </summary>
        public float FarPlaneDistance
        {
            get { return farPlaneDistance; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");

                farPlaneDistance = value;
            }
        }

        public Dof(SpriteBatch spriteBatch, Settings settings, Effect depthMapEffect, Effect dofEffect, Effect blurEffect)
            : base(spriteBatch)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            if (depthMapEffect == null) throw new ArgumentNullException("depthMapEffect");
            if (dofEffect == null) throw new ArgumentNullException("dofEffect");
            if (blurEffect == null) throw new ArgumentNullException("blurEffect");

            this.settings = settings;

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
            depthMap = new RenderTarget2D(GraphicsDevice, width, height,
                false, SurfaceFormat.Single, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

            // ブラー済みシーン マップ
            bluredSceneMap = new RenderTarget2D(GraphicsDevice, width, height,
                false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);

            //----------------------------------------------------------------
            // ブラー

            blur = new GaussianBlur(blurEffect, spriteBatch, width, height, SurfaceFormat.Color,
                settings.Blur.Radius, settings.Blur.Amount);
        }

        public override void Process(IPostProcessorContext context)
        {
            Instrument.Begin(InstrumentProcess);

            DrawDepth(context);
            Filter(context);

            TextureDisplay.Add(depthMap);
            TextureDisplay.Add(bluredSceneMap);

            Instrument.End();
        }

        void DrawDepth(IPostProcessorContext context)
        {
            Instrument.Begin(InstrumentDrawDepth);

            var viewerCamera = context.ActiveCamera;

            //----------------------------------------------------------------
            // 内部カメラの準備

            internalCamera.View.Position = viewerCamera.View.Position;
            internalCamera.View.Direction = viewerCamera.View.Direction;
            internalCamera.View.Up = viewerCamera.View.Up;
            internalCamera.Projection.Fov = viewerCamera.Projection.Fov;
            internalCamera.Projection.AspectRatio = viewerCamera.Projection.AspectRatio;
            internalCamera.Projection.NearPlaneDistance = viewerCamera.Projection.NearPlaneDistance;
            //internalCamera.Projection.FarPlaneDistance = viewerCamera.Projection.FarPlaneDistance;
            internalCamera.Projection.FarPlaneDistance = farPlaneDistance;
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

            GraphicsDevice.SetRenderTarget(depthMap);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);

            for (int i = 0; i < context.OpaqueObjects.Count; i++)
            {
                var opaque = context.OpaqueObjects[i];

                // 専用カメラの視錐台に含まれるもののみを描画。
                if (IsVisibleObject(opaque))
                    opaque.Draw(depthMapEffect);
            }

            for (int i = 0; i < context.TranslucentObjects.Count; i++)
            {
                var translucent = context.TranslucentObjects[i];

                // 専用カメラの視錐台に含まれるもののみを描画。
                if (IsVisibleObject(translucent))
                    translucent.Draw(depthMapEffect);
            }

            GraphicsDevice.SetRenderTarget(null);

            Instrument.End();
        }

        void Filter(IPostProcessorContext context)
        {
            Instrument.Begin(InstrumentFilter);

            //================================================================
            // シーンにブラーを適用

            blur.Filter(context.Source, bluredSceneMap);

            //================================================================
            // シーンとブラー済みシーンを深度マップに基いて合成

            //----------------------------------------------------------------
            // エフェクト

            var distance = internalCamera.Projection.FarPlaneDistance - internalCamera.Projection.NearPlaneDistance;
            dofEffect.NearPlaneDistance = internalCamera.Projection.NearPlaneDistance;
            dofEffect.FarPlaneDistance = internalCamera.Projection.FarPlaneDistance / distance;
            dofEffect.FocusDistance = internalCamera.FocusDistance;
            dofEffect.FocusRange = internalCamera.FocusRange;
            dofEffect.DepthMap = depthMap;
            dofEffect.BluredSceneMap = bluredSceneMap;

            //----------------------------------------------------------------
            // 描画

            GraphicsDevice.SetRenderTarget(context.Destination);
            SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, null, null, null, dofEffect.Effect);
            SpriteBatch.Draw(context.Source, context.Destination.Bounds, Color.White);
            SpriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);

            Instrument.End();
        }

        bool IsVisibleObject(SceneObject sceneObject)
        {
            // 球同士で判定。
            if (!sceneObject.SphereWorld.Intersects(frustumSphere)) return false;

            // 球と視錐台で判定。
            if (!sceneObject.SphereWorld.Intersects(frustumSphere)) return false;

            // 視錐台と AABB で判定。
            return sceneObject.BoxWorld.Intersects(internalCamera.Frustum);
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
                depthMap.Dispose();
                bluredSceneMap.Dispose();
                blur.Dispose();
            }

            disposed = true;
        }

        #endregion
    }
}
