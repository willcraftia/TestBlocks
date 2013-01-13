#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Diagnostics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class Ssao : PostProcessor, IDisposable
    {
        #region Settings

        public sealed class Settings
        {
            // 精度の低下により見栄えが大きく劣化するため、1 が無難。
            float mapScale = 1;

            /// <summary>
            /// 実スクリーンに対する法線深度マップのスケールを取得または設定します。
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

        #region SsaoMapEffect

        sealed class SsaoMapEffect
        {
            EffectParameter viewportSize;

            EffectParameter totalStrength;
            
            EffectParameter strength;
            
            EffectParameter randomOffset;
            
            EffectParameter falloff;
            
            EffectParameter radius;
            
            public Effect Effect { get; private set; }

            public Vector2 ViewportSize
            {
                get { return viewportSize.GetValueVector2(); }
                set { viewportSize.SetValue(value); }
            }

            public float TotalStrength
            {
                get { return totalStrength.GetValueSingle(); }
                set { totalStrength.SetValue(value); }
            }

            public float Strength
            {
                get { return strength.GetValueSingle(); }
                set { strength.SetValue(value); }
            }

            public float RandomOffset
            {
                get { return randomOffset.GetValueSingle(); }
                set { randomOffset.SetValue(value); }
            }

            public float Falloff
            {
                get { return falloff.GetValueSingle(); }
                set { falloff.SetValue(value); }
            }

            public float Radius
            {
                get { return radius.GetValueSingle(); }
                set { radius.SetValue(value); }
            }

            public SsaoMapEffect(Effect effect)
            {
                Effect = effect;

                viewportSize = effect.Parameters["ViewportSize"];
                totalStrength = effect.Parameters["TotalStrength"];
                strength = effect.Parameters["Strength"];
                randomOffset = effect.Parameters["RandomOffset"];
                falloff = effect.Parameters["Falloff"];
                radius = effect.Parameters["Radius"];
            }

            public void Initialize(int width, int height, Texture2D randomNormalMap)
            {
                //------------------------------------------------------------
                // スプライト バッチで描画するための行列の初期化

                Effect.Parameters["MatrixTransform"].SetValue(EffectHelper.CreateSpriteBatchMatrixTransform(width, height));

                //------------------------------------------------------------
                // ランダム法線マップ

                Effect.Parameters["RandomNormalMap"].SetValue(randomNormalMap);
            }
        }

        #endregion

        #region SsaoEffect

        public sealed class SsaoEffect
        {
            EffectParameter shadowColor;
            
            EffectParameter ssaoMap;

            public Effect Effect { get; private set; }

            public Vector3 ShadowColor
            {
                get { return shadowColor.GetValueVector3(); }
                set { shadowColor.SetValue(value); }
            }

            public Texture2D SsaoMap
            {
                get { return ssaoMap.GetValueTexture2D(); }
                set { ssaoMap.SetValue(value); }
            }

            public SsaoEffect(Effect effect)
            {
                Effect = effect;

                shadowColor = effect.Parameters["ShadowColor"];
                ssaoMap = effect.Parameters["SsaoMap"];
            }
        }

        #endregion

        public const string MonitorProcess = "Ssao.Process";

        public const string MonitorDrawSsaoMap = "Ssao.DrawSsaoMap";

        public const string MonitorFilter = "Ssao.Filter";

        public const float DefaultTotalStrength = 5;

        public const float DefaultStrength = 0.01f;

        public const float DefaultFalloff = 0.00001f;

        public const float DefaultRadius = 5;

        SpriteBatch spriteBatch;

        NormalDepthMapEffect normalDepthMapEffect;

        SsaoMapEffect ssaoMapEffect;

        SsaoEffect ssaoEffect;

        BasicCamera internalCamera = new BasicCamera("SsaoInternal");

        Vector3[] frustumCorners = new Vector3[8];

        BoundingSphere frustumSphere;

        SsaoMapBlur blur;

        RenderTarget2D normalDepthMap;

        RenderTarget2D ssaoMap;

        Settings settings;

        float farPlaneDistance = 64;

        float totalStrength = DefaultTotalStrength;

        float strength = DefaultStrength;

        float falloff = DefaultFalloff;

        float radius = DefaultRadius;

        /// <summary>
        /// 法線深度マップ描画で使用するカメラの、遠くのビュー プレーンとの距離。
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

        public float TotalStrength
        {
            get { return totalStrength; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");

                totalStrength = value;
            }
        }

        public float Strength
        {
            get { return strength; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");

                strength = value;
            }
        }

        public float Falloff
        {
            get { return falloff; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");

                falloff = value;
            }
        }

        public float Radius
        {
            get { return radius; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");

                radius = value;
            }
        }

        public Ssao(SpriteBatch spriteBatch, Settings settings,
            Effect normalDepthMapEffect, Effect ssaoMapEffect, Effect blurEffect, Effect ssaoEffect,
            Texture2D randomNormalMap)
            : base(spriteBatch)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            if (normalDepthMapEffect == null) throw new ArgumentNullException("normalDepthMapEffect");
            if (ssaoMapEffect == null) throw new ArgumentNullException("ssaoMapEffect");
            if (blurEffect == null) throw new ArgumentNullException("blurEffect");
            if (ssaoEffect == null) throw new ArgumentNullException("ssaoEffect");
            if (randomNormalMap == null) throw new ArgumentNullException("randomNormalMap");

            this.settings = settings;
            this.spriteBatch = spriteBatch;

            var pp = GraphicsDevice.PresentationParameters;
            var width = (int) (pp.BackBufferWidth * settings.MapScale);
            var height = (int) (pp.BackBufferHeight * settings.MapScale);

            //----------------------------------------------------------------
            // エフェクト

            // 法線深度マップ
            this.normalDepthMapEffect = new NormalDepthMapEffect(normalDepthMapEffect);
            
            // SSAO マップ
            this.ssaoMapEffect = new SsaoMapEffect(ssaoMapEffect);
            this.ssaoMapEffect.Initialize(width, height, randomNormalMap);

            // SSAO
            this.ssaoEffect = new SsaoEffect(ssaoEffect);

            //----------------------------------------------------------------
            // レンダ ターゲット

            // 法線深度マップ
            normalDepthMap = new RenderTarget2D(GraphicsDevice, width, height,
                false, SurfaceFormat.Vector4, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

            // SSAO マップ
            ssaoMap = new RenderTarget2D(GraphicsDevice, width, height,
                false, SurfaceFormat.Single, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);

            //----------------------------------------------------------------
            // SSAO マップブラー

            blur = new SsaoMapBlur(blurEffect, spriteBatch, width, height, SurfaceFormat.Single,
                settings.Blur.Radius, settings.Blur.Amount);
        }

        public override void Process(IPostProcessorContext context)
        {
            Monitor.Begin(MonitorProcess);

            DrawSsaoMap(context);
            Filter(context);

            if (DebugMapDisplay.Available)
            {
                DebugMapDisplay.Instance.Add(normalDepthMap);
                DebugMapDisplay.Instance.Add(ssaoMap);
            }

            Monitor.End(MonitorProcess);
        }

        void DrawSsaoMap(IPostProcessorContext context)
        {
            Monitor.Begin(MonitorDrawSsaoMap);

            var viewerCamera = context.ActiveCamera;
            var visibleSceneObjects = context.VisibleSceneObjects;

            //================================================================
            //
            // 法線深度マップを描画
            //

            //----------------------------------------------------------------
            // 内部カメラの準備

            internalCamera.View.Position = viewerCamera.View.Position;
            internalCamera.View.Direction = viewerCamera.View.Direction;
            internalCamera.View.Up = viewerCamera.View.Up;
            internalCamera.Projection.Fov = viewerCamera.Projection.Fov;
            internalCamera.Projection.AspectRatio = viewerCamera.Projection.AspectRatio;
            internalCamera.Projection.NearPlaneDistance = viewerCamera.Projection.NearPlaneDistance;
            // TODO
            // 状況に応じて FarPlaneDistance を変化させられるように。
            //internalCamera.Projection.FarPlaneDistance = viewerCamera.Projection.FarPlaneDistance;
            internalCamera.Projection.FarPlaneDistance = 32;
            internalCamera.Update();

            internalCamera.Frustum.GetCorners(frustumCorners);
            frustumSphere = BoundingSphere.CreateFromPoints(frustumCorners);

            //----------------------------------------------------------------
            // エフェクト

            normalDepthMapEffect.View = internalCamera.View.Matrix;
            normalDepthMapEffect.Projection = internalCamera.Projection.Matrix;

            //----------------------------------------------------------------
            // 描画

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;

            GraphicsDevice.SetRenderTarget(normalDepthMap);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);

            foreach (var sceneObject in visibleSceneObjects)
            {
                // 専用カメラの視錐台に含まれるもののみを描画。
                if (IsVisibleObject(sceneObject))
                    sceneObject.Draw(normalDepthMapEffect);
            }

            GraphicsDevice.SetRenderTarget(null);

            //================================================================
            //
            // SSAO マップを描画
            //

            //----------------------------------------------------------------
            // エフェクト

            var randomOffsetVector = viewerCamera.View.Position + viewerCamera.View.Direction;

            ssaoMapEffect.TotalStrength = TotalStrength;
            ssaoMapEffect.Strength = Strength;
            ssaoMapEffect.Falloff = Falloff;
            ssaoMapEffect.Radius = Radius;
            ssaoMapEffect.RandomOffset = randomOffsetVector.LengthSquared();

            //----------------------------------------------------------------
            // 描画

            GraphicsDevice.SetRenderTarget(ssaoMap);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, null, null, ssaoMapEffect.Effect);
            spriteBatch.Draw(normalDepthMap, ssaoMap.Bounds, Color.White);
            spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);

            //================================================================
            //
            // SSAO マップへブラーを適用
            //

            // TODO
            for (int i = 0; i < 3; i++)
                blur.Filter(ssaoMap, normalDepthMap);

            Monitor.End(MonitorDrawSsaoMap);
        }

        void Filter(IPostProcessorContext context)
        {
            Monitor.Begin(MonitorFilter);

            //----------------------------------------------------------------
            // エフェクト

            ssaoEffect.SsaoMap = ssaoMap;

            //----------------------------------------------------------------
            // 描画

            GraphicsDevice.SetRenderTarget(context.Destination);
            SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, null, null, null, ssaoEffect.Effect);
            SpriteBatch.Draw(context.Source, context.Destination.Bounds, Color.White);
            SpriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);

            Monitor.End(MonitorFilter);
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

        ~Ssao()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                normalDepthMapEffect.Dispose();
                normalDepthMap.Dispose();
                ssaoMap.Dispose();
                blur.Dispose();
            }

            disposed = true;
        }

        #endregion
    }
}
