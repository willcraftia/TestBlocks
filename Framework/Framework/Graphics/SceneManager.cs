#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Diagnostics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class SceneManager : ISceneObjectContext
    {
        #region Settings

        public sealed class Settings
        {
            // TODO
            // 各種初期設定をここへ。
        }

        #endregion

        #region DistanceComparer

        sealed class DistanceComparer : IComparer<SceneObject>
        {
            public static DistanceComparer Instance = new DistanceComparer();

            public Vector3 EyePosition;

            DistanceComparer() { }

            public int Compare(SceneObject o1, SceneObject o2)
            {
                float d1;
                float d2;
                Vector3.DistanceSquared(ref EyePosition, ref o1.Position, out d1);
                Vector3.DistanceSquared(ref EyePosition, ref o2.Position, out d2);

                if (d1 == d2) return 0;
                return (d1 < d2) ? -1 : 1;
            }
        }

        #endregion

        #region PostProcessorContext

        sealed class PostProcessorContext : IPostProcessorContext
        {
            SceneManager sceneManager;

            // I/F
            public ICamera ActiveCamera
            {
                get { return sceneManager.activeCamera; }
            }

            // I/F
            public ShadowMap ShadowMap
            {
                get { return sceneManager.ShadowMap; }
            }

            // I/F
            public IEnumerable<SceneObject> VisibleSceneObjects
            {
                get { return sceneManager.visibleSceneObjects; }
            }

            // I/F
            public RenderTarget2D Source
            {
                get { return sceneManager.renderTarget; }
            }

            // I/F
            public RenderTarget2D Destination
            {
                get { return sceneManager.postProcessRenderTarget; }
            }

            public PostProcessorContext(SceneManager sceneManager)
            {
                this.sceneManager = sceneManager;
            }
        }

        #endregion

        public const int InitialSceneObjectCapacity = 1000;

        public const int InitialCameraCapacity = 10;

        public const int InitialDirectionalLightCapacity = 10;

        public const int InitialVisibleObjectCapacity = 500;

        public const int InitialShadowCasterCapacity = 1000;

        public const int InitialParticleSystemCapacity = 10;

        public const int InitialPostProcessorCapacity = 10;

        public const string MonitorClassifySceneObjects = "SceneManager.ClassifySceneObjects";
        
        public const string MonitorDrawShadowMap = "SceneManager.DrawShadowMap";
        
        public const string MonitorDrawScene = "SceneManager.DrawScene";
        
        public const string MonitorOcclusionQuery = "SceneManager.OcclusionQuery";
        
        public const string MonitorDrawSceneObjects = "SceneManager.DrawSceneObjects";
        
        public const string MonitorDrawParticles = "SceneManager.DrawParticles";
        
        public const string MonitorPostProcess = "SceneManager.PostProcess";

        static readonly BlendState colorWriteDisable = new BlendState
        {
            ColorWriteChannels = ColorWriteChannels.None
        };

        /// <summary>
        /// シャドウ マップの更新完了で発生するイベント。
        /// </summary>
        public event EventHandler ShadowMapUpdated = delegate { };

        List<SceneObject> sceneObjects = new List<SceneObject>(InitialSceneObjectCapacity);

        SceneObject skySphere;

        string activeCameraName;

        ICamera activeCamera;

        Vector3 ambientLightColor;

        string activeDirectionalLightName;

        DirectionalLight activeDirectionalLight;

        Vector3 fogColor;

        Queue<SceneObject> workingSceneObjects = new Queue<SceneObject>(InitialSceneObjectCapacity);

        List<SceneObject> visibleSceneObjects = new List<SceneObject>(InitialVisibleObjectCapacity);

        List<SceneObject> opaqueSceneObjects = new List<SceneObject>(InitialVisibleObjectCapacity);

        List<SceneObject> translucentSceneObjects = new List<SceneObject>(InitialVisibleObjectCapacity);

        List<ShadowCaster> activeShadowCasters = new List<ShadowCaster>(InitialShadowCasterCapacity);

        Vector3[] frustumCorners = new Vector3[8];

        BoundingSphere frustumSphere;

        bool shadowMapAvailable;

        RenderTarget2D renderTarget;

        RenderTarget2D postProcessRenderTarget;

        SpriteBatch spriteBatch;

        PostProcessorContext postProcessorContext;

        Settings settings;

        #region Debug

        BasicEffect debugBoundingBoxEffect;

        BoundingBoxDrawer debugBoundingBoxDrawer;

        public static bool DebugBoundingBoxVisible { get; set; }

        #endregion

        // I/F
        public ICamera ActiveCamera
        {
            get
            {
                if (activeCamera == null) throw new InvalidOperationException("ActiveCamera is null.");

                return activeCamera;
            }
        }

        public DirectionalLight ActiveDirectionalLight
        {
            get { return activeDirectionalLight; }
        }

        public GraphicsDevice GraphicsDevice { get; private set; }

        public CameraCollection Cameras { get; private set; }

        public DirectionalLightCollection DirectionalLights { get; private set; }

        public ShadowMap ShadowMap { get; set; }

        public ParticleSystemCollection ParticleSystems { get; private set; }

        public LensFlare LensFlare { get; set; }

        public PostProcessorCollection PostProcessors { get; private set; }

        public string ActiveCameraName
        {
            get { return activeCameraName; }
            set
            {
                if (value != null && !Cameras.Contains(value))
                    throw new ArgumentException("Camera not found: " + value);

                if (activeCameraName == value) return;

                activeCameraName = value;
                activeCamera = (activeCameraName != null) ? Cameras[activeCameraName] : null;
            }
        }

        public string ActiveDirectionalLightName
        {
            get { return activeDirectionalLightName; }
            set
            {
                if (value != null && !DirectionalLights.Contains(value))
                    throw new ArgumentException("DirectionalLight not found: " + value);

                if (activeDirectionalLightName == value) return;

                activeDirectionalLightName = value;
                activeDirectionalLight = (activeDirectionalLightName != null) ? DirectionalLights[activeDirectionalLightName] : null;
            }
        }

        public Vector3 AmbientLightColor
        {
            get { return ambientLightColor; }
            set { ambientLightColor = value; }
        }

        public bool FogEnabled { get; set; }

        public float FogStart { get; set; }

        public float FogEnd { get; set; }

        public Vector3 FogColor
        {
            get { return fogColor; }
            set { fogColor = value; }
        }

        public Vector3 BackgroundColor { get; set; }

        public SceneObject SkySphere
        {
            get { return skySphere; }
            set
            {
                if (skySphere != null) skySphere.Context = null;

                skySphere = value;

                if (skySphere != null) skySphere.Context = this;
            }
        }

        public bool SssmEnabled { get; set; }

        public int TotalSceneObjectCount { get; internal set; }

        public int VisibleSceneObjectCount { get; internal set; }

        public int OccludedSceneObjectCount { get; internal set; }

        public int RenderedSceneObjectCount
        {
            get { return VisibleSceneObjectCount - OccludedSceneObjectCount; }
        }

        public SceneManager(Settings settings, GraphicsDevice graphicsDevice)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");

            this.settings = settings;
            GraphicsDevice = graphicsDevice;

            spriteBatch = new SpriteBatch(GraphicsDevice);

            Cameras = new CameraCollection(InitialCameraCapacity);
            DirectionalLights = new DirectionalLightCollection(InitialDirectionalLightCapacity);
            ParticleSystems = new ParticleSystemCollection(InitialParticleSystemCapacity);
            PostProcessors = new PostProcessorCollection(InitialPostProcessorCapacity);

            //----------------------------------------------------------------
            // シーン描画のためのレンダ ターゲット

            var pp = GraphicsDevice.PresentationParameters;
            var width = pp.BackBufferWidth;
            var height = pp.BackBufferHeight;
            var format = pp.BackBufferFormat;
            var depthFormat = pp.DepthStencilFormat;
            var multiSampleCount = pp.MultiSampleCount;
            renderTarget = new RenderTarget2D(GraphicsDevice, width, height,
                false, format, depthFormat, multiSampleCount, RenderTargetUsage.PreserveContents);

            //----------------------------------------------------------------
            // ポストプロセッサ コンテキスト

            postProcessorContext = new PostProcessorContext(this);

            //----------------------------------------------------------------
            // ポスト プロセスのためのレンダ ターゲット

            postProcessRenderTarget = new RenderTarget2D(GraphicsDevice, width, height,
                false, format, depthFormat, multiSampleCount, RenderTargetUsage.PreserveContents);

#if DEBUG || TRACE
            debugBoundingBoxEffect = new BasicEffect(GraphicsDevice);
            debugBoundingBoxEffect.AmbientLightColor = Vector3.One;
            debugBoundingBoxEffect.VertexColorEnabled = true;
            debugBoundingBoxDrawer = new BoundingBoxDrawer(GraphicsDevice);
#endif
        }

        public void AddSceneObject(SceneObject sceneObject)
        {
            if (sceneObject == null) throw new ArgumentNullException("sceneObject");

            lock (sceneObjects)
            {
                sceneObject.Context = this;
                sceneObjects.Add(sceneObject);
            }
        }

        public void RemoveSceneObject(SceneObject sceneObject)
        {
            if (sceneObject == null) throw new ArgumentNullException("sceneObject");

            lock (sceneObjects)
            {
                sceneObjects.Remove(sceneObject);
                sceneObject.Context = null;
            }
        }

        public void ClearSceneObjects()
        {
            lock (sceneObjects)
            {
                foreach (var sceneObject in sceneObjects) sceneObject.Context = null;
                sceneObjects.Clear();
            }
        }

        public void Draw(GameTime gameTime)
        {
            if (gameTime == null) throw new ArgumentNullException("gameTime");
            if (activeCamera == null) throw new InvalidOperationException("ActiveCamera is null.");

            activeCamera.Frustum.GetCorners(frustumCorners);
            frustumSphere = BoundingSphere.CreateFromPoints(frustumCorners);

            // 収集リストを初期化。
            visibleSceneObjects.Clear();
            opaqueSceneObjects.Clear();
            translucentSceneObjects.Clear();
            activeShadowCasters.Clear();

            // カウンタをリセット。
            TotalSceneObjectCount = 0;
            VisibleSceneObjectCount = 0;
            OccludedSceneObjectCount = 0;

#if DEBUG || TRACE
            // デバッグ エフェクトへカメラ情報を設定。
            debugBoundingBoxEffect.View = activeCamera.View.Matrix;
            debugBoundingBoxEffect.Projection = activeCamera.Projection.Matrix;
#endif

            // 長時間のロックの回避。
            lock (sceneObjects)
            {
                foreach (var sceneObject in sceneObjects)
                    workingSceneObjects.Enqueue(sceneObject);
            }

            TotalSceneObjectCount = workingSceneObjects.Count;

            //----------------------------------------------------------------
            // 可視オブジェクトの収集と種類による分類

            ClassifySceneObjects();

            // 視点からの距離でソート。
            DistanceComparer.Instance.EyePosition = activeCamera.View.Position;
            visibleSceneObjects.Sort(DistanceComparer.Instance);
            opaqueSceneObjects.Sort(DistanceComparer.Instance);
            translucentSceneObjects.Sort(DistanceComparer.Instance);

            //----------------------------------------------------------------
            // シャドウ マップ

            shadowMapAvailable = false;
            if (ShadowMap != null && activeShadowCasters.Count != 0 &&
                activeDirectionalLight != null && activeDirectionalLight.Enabled)
            {
                DrawShadowMap();
                shadowMapAvailable = true;

                ShadowMapUpdated(this, EventArgs.Empty);
            }

            //----------------------------------------------------------------
            // シーン

            if (shadowMapAvailable && !SssmEnabled)
            {
                DrawScene(ShadowMap);
            }
            else
            {
                DrawScene(null);
            }

            //----------------------------------------------------------------
            // パーティクル

            if (0 < ParticleSystems.Count)
                DrawParticles(gameTime);

            //----------------------------------------------------------------
            // ポスト プロセス

            PostProcess();

            //----------------------------------------------------------------
            // レンズ フレア

            if (LensFlare != null && LensFlare.Enabled)
                DrawLensFlare();

            //----------------------------------------------------------------
            // デバッグのための BoundingBox 描画

            DebugDrawBoundingBoxes();

            //----------------------------------------------------------------
            // レンダ ターゲットの反映

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque);
            spriteBatch.Draw(renderTarget, Vector2.Zero, Color.White);
            spriteBatch.End();
        }

        public void UpdateEffect(Effect effect)
        {
            var matrices = effect as IEffectMatrices;
            if (matrices != null)
            {
                matrices.View = activeCamera.View.Matrix;
                matrices.Projection = activeCamera.Projection.Matrix;
            }

            var eye = effect as IEffectEye;
            if (eye != null)
            {
                eye.EyePosition = activeCamera.View.Position;
            }

            var lights = effect as IEffectLights;
            if (lights != null)
            {
                lights.AmbientLightColor = ambientLightColor;

                if (activeDirectionalLight != null && activeDirectionalLight.Enabled)
                {
                    lights.DirectionalLight0.Enabled = true;
                    lights.DirectionalLight0.Direction = activeDirectionalLight.Direction;
                    lights.DirectionalLight0.DiffuseColor = activeDirectionalLight.DiffuseColor;
                    lights.DirectionalLight0.SpecularColor = activeDirectionalLight.SpecularColor;
                }
                else
                {
                    lights.DirectionalLight0.Enabled = false;
                    lights.DirectionalLight0.Direction = Vector3.Down;
                    lights.DirectionalLight0.DiffuseColor = Vector3.Zero;
                    lights.DirectionalLight0.SpecularColor = Vector3.Zero;
                }
            }

            var fog = effect as IEffectFog;
            if (fog != null)
            {
                if (FogEnabled)
                {
                    fog.FogStart = FogStart;
                    fog.FogEnd = FogEnd;
                    fog.FogColor = fogColor;
                }
                fog.FogEnabled = FogEnabled;
            }
        }

        public void UpdateEffectShadowMap(IEffectShadowMap effect)
        {
            if (effect == null) throw new ArgumentNullException("effect");

            if (SssmEnabled)
            {
                effect.ShadowMapEnabled = false;
            }
            else
            {
                effect.ShadowMapEnabled = true;
                effect.ShadowMapSize = ShadowMap.Size;
                effect.ShadowMapDepthBias = ShadowMap.DepthBias;
                effect.ShadowMapCount = ShadowMap.SplitCount;
                effect.ShadowMapDistances = ShadowMap.SplitDistances;
                effect.ShadowMapLightViewProjections = ShadowMap.SplitLightViewProjections;
                effect.ShadowMaps = ShadowMap.SplitShadowMaps;
            }
        }

        bool IsVisibleObject(SceneObject sceneObject)
        {
            // 球同士で判定。
            if (!sceneObject.BoundingSphere.Intersects(frustumSphere)) return false;

            // 球と視錐台で判定。
            if (!sceneObject.BoundingSphere.Intersects(frustumSphere)) return false;

            // 視錐台と AABB で判定。
            return sceneObject.BoundingBox.Intersects(activeCamera.Frustum);
        }

        bool IsActiveShadowCaster(ShadowCaster shadowCaster)
        {
            if (shadowCaster.Translucent) return false;
            if (!shadowCaster.CastShadow) return false;

            return true;
        }

        void ClassifySceneObjects()
        {
            Monitor.Begin(MonitorClassifySceneObjects);

            // 可視オブジェクトの収集と種類による分類。
            while (workingSceneObjects.Count != 0)
            {
                var sceneObject = workingSceneObjects.Dequeue();

                bool shouldPreDraw = false;

                if (IsVisibleObject(sceneObject))
                {
                    visibleSceneObjects.Add(sceneObject);
                    shouldPreDraw = true;

                    if (sceneObject.Translucent)
                    {
                        translucentSceneObjects.Add(sceneObject);
                    }
                    else
                    {
                        opaqueSceneObjects.Add(sceneObject);
                    }

                    VisibleSceneObjectCount++;
                }

                var shadowCaster = sceneObject as ShadowCaster;
                if (shadowCaster != null && IsActiveShadowCaster(shadowCaster))
                {
                    activeShadowCasters.Add(shadowCaster);
                    shouldPreDraw = true;
                }

                if (shouldPreDraw) sceneObject.PreDraw();
            }

            Monitor.End(MonitorClassifySceneObjects);
        }

        void DrawShadowMap()
        {
            Monitor.Begin(MonitorDrawShadowMap);

            //----------------------------------------------------------------
            // 準備

            ShadowMap.Prepare(activeCamera);

            //----------------------------------------------------------------
            // 投影オブジェクトを収集

            foreach (var shadowCaster in activeShadowCasters)
                ShadowMap.TryAddShadowCaster(shadowCaster);

            //----------------------------------------------------------------
            // シャドウ マップを描画

            var lightDirection = activeDirectionalLight.Direction;
            ShadowMap.Draw(ref lightDirection);

            Monitor.End(MonitorDrawShadowMap);
        }

        void DrawScene(ShadowMap shadowMap)
        {
            Monitor.Begin(MonitorDrawScene);

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            
            GraphicsDevice.SetRenderTarget(renderTarget);
            GraphicsDevice.Clear(new Color(BackgroundColor));

            //================================================================
            //
            // オクルージョン クエリ
            //

            Monitor.Begin(MonitorOcclusionQuery);

            GraphicsDevice.BlendState = colorWriteDisable;

            foreach (var opaque in opaqueSceneObjects)
                opaque.UpdateOcclusion();

            foreach (var translucent in translucentSceneObjects)
                translucent.UpdateOcclusion();

            Monitor.End(MonitorOcclusionQuery);

            //================================================================
            //
            // 描画
            //

            Monitor.Begin(MonitorDrawSceneObjects);

            //----------------------------------------------------------------
            // 不透明オブジェクト

            GraphicsDevice.BlendState = BlendState.Opaque;

            foreach (var opaque in opaqueSceneObjects)
            {
                if (opaque.Occluded)
                {
                    OccludedSceneObjectCount++;
                    continue;
                }

                opaque.Draw();
            }

            //----------------------------------------------------------------
            // 半透明オブジェクト

            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            foreach (var translucent in translucentSceneObjects)
            {
                if (translucent.Occluded)
                {
                    OccludedSceneObjectCount++;
                    continue;
                }

                translucent.Draw();
            }

            //----------------------------------------------------------------
            // スカイ スフィア

            if (SkySphere != null && SkySphere.Visible)
            {
                SkySphere.PreDraw();
                SkySphere.Draw();
            }

            Monitor.End(MonitorDrawSceneObjects);

            GraphicsDevice.SetRenderTarget(null);

            Monitor.End(MonitorDrawScene);
        }

        void DrawParticles(GameTime gameTime)
        {
            Monitor.Begin(MonitorDrawParticles);

            GraphicsDevice.SetRenderTarget(renderTarget);

            foreach (var particleSystem in ParticleSystems)
            {
                if (particleSystem.Enabled)
                    particleSystem.Draw(gameTime, activeCamera);
            }

            GraphicsDevice.SetRenderTarget(null);

            Monitor.End(MonitorDrawParticles);
        }

        void PostProcess()
        {
            Monitor.Begin(MonitorPostProcess);

            foreach (var postProcessor in PostProcessors)
            {
                if (postProcessor.Enabled)
                {
                    postProcessor.Process(postProcessorContext);
                    SwapRenderTargets();
                }
            }

            Monitor.End(MonitorPostProcess);
        }

        void SwapRenderTargets()
        {
            var temp = renderTarget;
            renderTarget = postProcessRenderTarget;
            postProcessRenderTarget = temp;
        }

        void DrawLensFlare()
        {
            if (activeDirectionalLight == null) return;

            GraphicsDevice.SetRenderTarget(renderTarget);

            LensFlare.Draw(activeCamera, activeDirectionalLight.Direction);

            GraphicsDevice.SetRenderTarget(null);
        }

        [Conditional("DEBUG"), Conditional("TRACE")]
        void DebugDrawBoundingBoxes()
        {
            if (!DebugBoundingBoxVisible) return;

            GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            GraphicsDevice.SetRenderTarget(renderTarget);

            foreach (var sceneObject in visibleSceneObjects)
            {
                DebugDrawBoundingBox(sceneObject);
            }

            GraphicsDevice.SetRenderTarget(null);
        }

        [Conditional("DEBUG"), Conditional("TRACE")]
        void DebugDrawBoundingBox(SceneObject sceneObject)
        {
            var color = (sceneObject.Occluded) ? Color.Gray : Color.White;
            debugBoundingBoxDrawer.Draw(ref sceneObject.BoundingBox, debugBoundingBoxEffect, ref color);
        }
    }
}
