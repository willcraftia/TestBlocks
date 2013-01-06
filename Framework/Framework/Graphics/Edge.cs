#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class Edge : IDisposable
    {
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

        SpriteBatch spriteBatch;

        NormalDepthMapEffect normalDepthMapEffect;

        EdgeEffect edgeEffect;

        BasicCamera internalCamera = new BasicCamera("EdgeInternal");

        Vector3[] frustumCorners = new Vector3[8];

        BoundingSphere frustumSphere;

        public GraphicsDevice GraphicsDevice { get; private set; }

        public EdgeSettings Settings { get; private set; }

        public RenderTarget2D NormalDepthMap { get; private set; }

        public Edge(GraphicsDevice graphicsDevice, EdgeSettings settings, SpriteBatch spriteBatch,
            Effect normalDepthMapEffect, Effect edgeEffect)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");
            if (settings == null) throw new ArgumentNullException("settings");
            if (spriteBatch == null) throw new ArgumentNullException("spriteBatch");

            GraphicsDevice = graphicsDevice;
            Settings = settings;
            this.spriteBatch = spriteBatch;

            var pp = GraphicsDevice.PresentationParameters;
            var width = (int) (pp.BackBufferWidth * settings.MapScale);
            var height = (int) (pp.BackBufferHeight * settings.MapScale);

            //----------------------------------------------------------------
            // エフェクト

            // 法線深度マップ
            this.normalDepthMapEffect = new NormalDepthMapEffect(normalDepthMapEffect);

            // エッジ強調
            this.edgeEffect = new EdgeEffect(edgeEffect);
            this.edgeEffect.EdgeWidth = settings.EdgeWidth;
            this.edgeEffect.EdgeIntensity = settings.EdgeIntensity;
            this.edgeEffect.NormalThreshold = settings.NormalThreshold;
            this.edgeEffect.DepthThreshold = settings.DepthThreshold;
            this.edgeEffect.NormalSensitivity = settings.NormalSensitivity;
            this.edgeEffect.DepthSensitivity = settings.DepthSensitivity;
            this.edgeEffect.EdgeColor = settings.EdgeColor;
            this.edgeEffect.MapWidth = width;
            this.edgeEffect.MapHeight = height;

            //----------------------------------------------------------------
            // レンダ ターゲット

            NormalDepthMap = new RenderTarget2D(GraphicsDevice, width, height,
                false, SurfaceFormat.Vector4, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
        }

        public void DrawNormalDepth(ICamera viewerCamera, IEnumerable<SceneObject> sceneObjects)
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
            internalCamera.Projection.FarPlaneDistance = Settings.FarPlaneDistance;
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

            GraphicsDevice.SetRenderTarget(NormalDepthMap);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);

            foreach (var sceneObject in sceneObjects)
            {
                // 専用カメラの視錐台に含まれるもののみを描画。
                if (IsVisibleObject(sceneObject))
                    sceneObject.Draw(normalDepthMapEffect);
            }

            GraphicsDevice.SetRenderTarget(null);
        }

        public void Filter(RenderTarget2D sceneMap, RenderTarget2D result)
        {
            if (sceneMap == null) throw new ArgumentNullException("sceneMap");
            if (result == null) throw new ArgumentNullException("result");

            //----------------------------------------------------------------
            // エフェクト

            edgeEffect.NormalDepthMap = NormalDepthMap;

            //----------------------------------------------------------------
            // 描画

            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.BlendState = BlendState.Opaque;

            GraphicsDevice.SetRenderTarget(result);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, null, null, null, edgeEffect.Effect);
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
                NormalDepthMap.Dispose();
            }

            disposed = true;
        }

        #endregion
    }
}
