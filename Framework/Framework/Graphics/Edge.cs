#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Diagnostics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class Edge : PostProcessor, IDisposable
    {
        #region Settings

        public sealed class Settings
        {
            // 精度を下げる程にエッジのギザギザが目立つようになるため、1 が無難。
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
        }

        #endregion

        #region EdgeDetectionEffect

        sealed class EdgeEffect
        {
            public Effect Effect { get; private set; }

            float edgeWidth = 1;

            int mapWidth = 1;

            int mapHeight = 1;

            EffectParameter edgeOffset;

            EffectParameter edgeIntensity;

            EffectParameter normalThreshold;

            EffectParameter depthThreshold;

            EffectParameter normalSensitivity;

            EffectParameter depthSensitivity;

            EffectParameter edgeColor;

            EffectParameter normalDepthMap;

            EffectParameter sceneMap;

            public float EdgeWidth
            {
                get { return edgeWidth; }
                set
                {
                    if (edgeWidth == value) return;

                    edgeWidth = value;
                    UpdateEdgeOffset();
                }
            }

            public float EdgeIntensity
            {
                get { return edgeIntensity.GetValueSingle(); }
                set { edgeIntensity.SetValue(value); }
            }

            public float NormalThreshold
            {
                get { return normalThreshold.GetValueSingle(); }
                set { normalThreshold.SetValue(value); }
            }

            public float DepthThreshold
            {
                get { return depthThreshold.GetValueSingle(); }
                set { depthThreshold.SetValue(value); }
            }

            public float NormalSensitivity
            {
                get { return normalSensitivity.GetValueSingle(); }
                set { normalSensitivity.SetValue(value); }
            }

            public float DepthSensitivity
            {
                get { return depthSensitivity.GetValueSingle(); }
                set { depthSensitivity.SetValue(value); }
            }

            public int MapWidth
            {
                get { return mapWidth; }
                set
                {
                    if (mapWidth == value) return;

                    mapWidth = value;
                    UpdateEdgeOffset();
                }
            }

            public int MapHeight
            {
                get { return mapHeight; }
                set
                {
                    if (mapHeight == value) return;

                    mapHeight = value;
                    UpdateEdgeOffset();
                }
            }

            public Vector3 EdgeColor
            {
                get { return edgeColor.GetValueVector3(); }
                set { edgeColor.SetValue(value); }
            }

            public Texture2D NormalDepthMap
            {
                get { return normalDepthMap.GetValueTexture2D(); }
                set { normalDepthMap.SetValue(value); }
            }

            public Texture2D SceneMap
            {
                get { return sceneMap.GetValueTexture2D(); }
                set { sceneMap.SetValue(value); }
            }

            public EdgeEffect(Effect effect)
            {
                Effect = effect;

                edgeOffset = effect.Parameters["EdgeOffset"];
                edgeIntensity = effect.Parameters["EdgeIntensity"];
                normalThreshold = effect.Parameters["NormalThreshold"];
                depthThreshold = effect.Parameters["DepthThreshold"];
                normalSensitivity = effect.Parameters["NormalSensitivity"];
                depthSensitivity = effect.Parameters["DepthSensitivity"];
                edgeColor = effect.Parameters["EdgeColor"];
                sceneMap = effect.Parameters["SceneMap"];
                normalDepthMap = effect.Parameters["NormalDepthMap"];

                UpdateEdgeOffset();
            }

            void UpdateEdgeOffset()
            {
                var offset = new Vector2(edgeWidth, edgeWidth);
                offset.X /= (float) mapWidth;
                offset.Y /= (float) mapHeight;
                edgeOffset.SetValue(offset);
            }
        }

        #endregion

        #region EdgeMonitor

        public sealed class EdgeMonitor : PostProcessorMonitor
        {
            public event EventHandler BeingDrawNormalDepth = delegate { };

            public event EventHandler EndDrawNormalDepth = delegate { };

            public event EventHandler BeginFilter = delegate { };

            public event EventHandler EndFilter = delegate { };

            internal EdgeMonitor(Edge edge) : base(edge) { }

            internal void OnBeingDrawNormalDepth()
            {
                BeingDrawNormalDepth(PostProcessor, EventArgs.Empty);
            }

            internal void OnEndDrawNormalDepth()
            {
                EndDrawNormalDepth(PostProcessor, EventArgs.Empty);
            }

            internal void OnBeginFilter()
            {
                BeginFilter(PostProcessor, EventArgs.Empty);
            }

            internal void OnEndFilter()
            {
                EndFilter(PostProcessor, EventArgs.Empty);
            }
        }

        #endregion

        public const float DefaultEdgeWidth = 1;

        public const float DefaultEdgeIntensity = 20;

        public const float DefaultNormalThreshold = 0.1f;

        public const float DefaultDepthThreshold = 0;

        public const float DefaultNormalSensitivity = 1;

        public const float DefaultDepthSensitivity = 1000;

        NormalDepthMapEffect normalDepthMapEffect;

        EdgeEffect edgeEffect;

        BasicCamera internalCamera = new BasicCamera("EdgeInternal");

        Vector3[] frustumCorners = new Vector3[8];

        BoundingSphere frustumSphere;

        RenderTarget2D normalDepthMap;

        Settings settings;

        float edgeWidth = DefaultEdgeWidth;

        float edgeIntensity = DefaultEdgeIntensity;

        float normalThreshold = DefaultNormalThreshold;

        float depthThreshold = DefaultDepthThreshold;

        float normalSensitivity = DefaultNormalSensitivity;

        float depthSensitivity = DefaultDepthSensitivity;

        Vector3 edgeColor = Vector3.Zero;

        // TODO
        float farPlaneDistance = 128;

        public float EdgeWidth
        {
            get { return edgeWidth; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");

                edgeWidth = value;
            }
        }

        public float EdgeIntensity
        {
            get { return edgeIntensity; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");

                edgeIntensity = value;
            }
        }

        public float NormalThreshold
        {
            get { return normalThreshold; }
            set
            {
                if (value < 0 || 1 < value) throw new ArgumentOutOfRangeException("value");

                normalThreshold = value;
            }
        }

        public float DepthThreshold
        {
            get { return depthThreshold; }
            set
            {
                if (value < 0 || 1 < value) throw new ArgumentOutOfRangeException("value");

                depthThreshold = value;
            }
        }

        public float NormalSensitivity
        {
            get { return normalSensitivity; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");

                normalSensitivity = value;
            }
        }

        public float DepthSensitivity
        {
            get { return depthSensitivity; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");

                depthSensitivity = value;
            }
        }

        public Vector3 EdgeColor
        {
            get { return edgeColor; }
            set { edgeColor = value; }
        }

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

        public EdgeMonitor Monitor { get; private set; }

        public Edge(SpriteBatch spriteBatch, Settings settings, Effect normalDepthMapEffect, Effect edgeEffect)
            : base(spriteBatch)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            if (normalDepthMapEffect == null) throw new ArgumentNullException("normalDepthMapEffect");
            if (edgeEffect == null) throw new ArgumentNullException("edgeEffect");

            this.settings = settings;

            var pp = GraphicsDevice.PresentationParameters;
            var width = (int) (pp.BackBufferWidth * settings.MapScale);
            var height = (int) (pp.BackBufferHeight * settings.MapScale);

            //----------------------------------------------------------------
            // エフェクト

            // 法線深度マップ
            this.normalDepthMapEffect = new NormalDepthMapEffect(normalDepthMapEffect);

            // エッジ強調
            this.edgeEffect = new EdgeEffect(edgeEffect);
            this.edgeEffect.MapWidth = width;
            this.edgeEffect.MapHeight = height;

            //----------------------------------------------------------------
            // レンダ ターゲット

            normalDepthMap = new RenderTarget2D(GraphicsDevice, width, height,
                false, SurfaceFormat.Vector4, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

            //----------------------------------------------------------------
            // モニタ

            Monitor = new EdgeMonitor(this);
        }

        public override void Process(IPostProcessorContext context)
        {
            Monitor.OnBeginProcess();

            DrawNormalDepth(context);
            Filter(context);

            if (DebugMapDisplay.Available) DebugMapDisplay.Instance.Add(normalDepthMap);

            Monitor.OnEndProcess();
        }

        void DrawNormalDepth(IPostProcessorContext context)
        {
            Monitor.OnBeingDrawNormalDepth();

            var viewerCamera = context.ActiveCamera;
            var visibleSceneObjects = context.VisibleSceneObjects;

            //----------------------------------------------------------------
            // 内部カメラの準備

            internalCamera.View.Position = viewerCamera.View.Position;
            internalCamera.View.Direction = viewerCamera.View.Direction;
            internalCamera.View.Up = viewerCamera.View.Up;
            internalCamera.Projection.Fov = viewerCamera.Projection.Fov;
            internalCamera.Projection.AspectRatio = viewerCamera.Projection.AspectRatio;
            internalCamera.Projection.NearPlaneDistance = viewerCamera.Projection.NearPlaneDistance;
            internalCamera.Projection.FarPlaneDistance = FarPlaneDistance;
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

            Monitor.OnEndDrawNormalDepth();
        }

        void Filter(IPostProcessorContext context)
        {
            Monitor.OnBeginFilter();

            //----------------------------------------------------------------
            // エフェクト

            edgeEffect.EdgeWidth = EdgeWidth;
            edgeEffect.EdgeIntensity = EdgeIntensity;
            edgeEffect.NormalThreshold = NormalThreshold;
            edgeEffect.DepthThreshold = DepthThreshold;
            edgeEffect.NormalSensitivity = NormalSensitivity;
            edgeEffect.DepthSensitivity = DepthSensitivity;
            edgeEffect.EdgeColor = EdgeColor;
            edgeEffect.NormalDepthMap = normalDepthMap;

            //----------------------------------------------------------------
            // 描画

            GraphicsDevice.SetRenderTarget(context.Destination);
            SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, null, null, null, edgeEffect.Effect);
            SpriteBatch.Draw(context.Source, context.Destination.Bounds, Color.White);
            SpriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);

            Monitor.OnEndFilter();
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

        ~Edge()
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
            }

            disposed = true;
        }

        #endregion
    }
}
