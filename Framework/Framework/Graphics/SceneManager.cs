﻿#region Using

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
            public const bool DefaultShadowEnabled = true;

            public const bool DefaultSssmEnabled = false;

            public const bool DefaultSsaoEnabled = true;

            public const bool DefaultEdgeEnabled = false;

            public const bool DefaultBloomEnabled = false;

            public const bool DefaultDofEnabled = true;

            public const bool DefaultColorOverlapEnabled = false;

            public const bool DefaultMonochromeEnabled = false;

            /// <summary>
            /// シャドウ処理が有効かどうかを示す値を取得または設定します。
            /// </summary>
            /// <value>
            /// true (シャドウ処理が有効)、false (それ以外の場合)。
            /// </value>
            public bool ShadowEnabled { get; set; }

            /// <summary>
            /// シャドウ マップ設定を取得します。
            /// </summary>
            public ShadowMap.Settings ShadowMap { get; private set; }

            /// <summary>
            /// スクリーン スペース シャドウ マッピングが有効か否かを示す値を取得または設定します。
            /// </summary>
            /// <value>
            /// true (スクリーン スペース シャドウ マッピングが有効な場合)、false (それ以外の場合)。
            /// </value>
            public bool SssmEnabled { get; set; }

            /// <summary>
            /// スクリーン スペース シャドウ マッピング設定を取得します。
            /// </summary>
            public Sssm.Settings Sssm { get; private set; }

            /// <summary>
            /// スクリーン スペース アンビエント オクルージョンが有効か否かを示す値を取得または設定します。
            /// </summary>
            /// <value>
            /// true (スクリーン スペース アンビエント オクルージョンが有効な場合)、false (それ以外の場合)。
            /// </value>
            public bool SsaoEnabled { get; set; }

            /// <summary>
            /// スクリーン スペース アンビエント オクルージョン設定を取得します。
            /// </summary>
            public Ssao.Settings Ssao { get; private set; }

            /// <summary>
            /// エッジ強調が有効か否かを示す値を取得または設定します。
            /// </summary>
            /// <value>
            /// true (エッジ強調が有効な場合)、false (それ以外の場合)。
            /// </value>
            public bool EdgeEnabled { get; set; }

            /// <summary>
            /// エッジ強調設定を取得します。
            /// </summary>
            public Edge.Settings Edge { get; private set; }

            /// <summary>
            /// ブルームが有効か否かを示す値を取得または設定します。
            /// </summary>
            /// <value>
            /// true (ブルームが有効な場合)、false (それ以外の場合)。
            /// </value>
            public bool BloomEnabled { get; set; }

            /// <summary>
            /// ブルーム設定を取得します。
            /// </summary>
            public Bloom.Settings Bloom { get; private set; }

            /// <summary>
            /// 被写界深度が有効か否かを示す値を取得または設定します。
            /// </summary>
            /// <value>
            /// true (被写界深度が有効な場合)、false (それ以外の場合)。
            /// </value>
            public bool DofEnabled { get; set; }

            /// <summary>
            /// 被写界深度設定を取得します。
            /// </summary>
            public Dof.Settings Dof { get; private set; }

            /// <summary>
            /// カラー オーバラップが有効か否かを示す値を取得または設定します。
            /// </summary>
            /// <value>
            /// true (カラー オーバラップが有効な場合)、false (それ以外の場合)。
            /// </value>
            public bool ColorOverlapEnabled { get; set; }

            /// <summary>
            /// モノクロームが有効か否かを示す値を取得または設定します。
            /// </summary>
            /// <value>
            /// true (モノクロームが有効な場合)、false (それ以外の場合)。
            /// </value>
            public bool MonochromeEnabled { get; set; }

            public Settings()
            {
                ShadowEnabled = DefaultShadowEnabled;

                ShadowMap = new ShadowMap.Settings();

                SssmEnabled = DefaultSssmEnabled;
                Sssm = new Sssm.Settings();

                SsaoEnabled = DefaultSsaoEnabled;
                Ssao = new Ssao.Settings();

                EdgeEnabled = DefaultEdgeEnabled;
                Edge = new Edge.Settings();

                BloomEnabled = DefaultBloomEnabled;
                Bloom = new Bloom.Settings();

                DofEnabled = DefaultDofEnabled;
                Dof = new Dof.Settings();

                ColorOverlapEnabled = DefaultColorOverlapEnabled;

                MonochromeEnabled = DefaultMonochromeEnabled;
            }
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
                get { return sceneManager.shadowMap; }
            }

            // I/F
            public Vector3 ShadowColor
            {
                get { return sceneManager.ShadowColor; }
            }

            // I/F
            public IEnumerable<SceneObject> VisibleSceneObjects
            {
                get { return sceneManager.visibleSceneObjects; }
            }

            public PostProcessorContext(SceneManager sceneManager)
            {
                this.sceneManager = sceneManager;
            }
        }

        #endregion

        #region SceneManagerMonitor

        public sealed class SceneManagerMonitor
        {
            public event EventHandler BeginClassifySceneObjects = delegate { };

            public event EventHandler EndClassifySceneObjects = delegate { };

            public event EventHandler BeginDrawShadowMap = delegate { };

            public event EventHandler EndDrawShadowMap = delegate { };

            public event EventHandler BeginDrawScene = delegate { };

            public event EventHandler EndDrawScene = delegate { };

            public event EventHandler BeginDrawSceneOcclusionQuery = delegate { };

            public event EventHandler EndDrawSceneOcclusionQuery = delegate { };

            public event EventHandler BeginDrawSceneRendering = delegate { };

            public event EventHandler EndDrawSceneRendering = delegate { };

            public event EventHandler BeginPostProcess = delegate { };

            public event EventHandler EndPostProcess = delegate { };

            SceneManager sceneManager;

            public int TotalSceneObjectCount { get; internal set; }

            public int VisibleSceneObjectCount { get; internal set; }

            public int OccludedSceneObjectCount { get; internal set; }

            public int RenderedSceneObjectCount
            {
                get { return VisibleSceneObjectCount - OccludedSceneObjectCount; }
            }

            public ShadowMap.ShadowMapMonitor ShadowMap { get; internal set; }

            public Sssm.SssmMonitor Sssm { get; internal set; }

            public Ssao.SsaoMonitor Ssao { get; internal set; }

            public Edge.EdgeMonitor Edge { get; internal set; }

            public Bloom.BloomMonitor Bloom { get; internal set; }

            public Dof.DofMonitor Dof { get; internal set; }

            public ColorOverlap.ColorOverlapMonitor ColorOverlap { get; internal set; }

            public Monochrome.MonochromeMonitor Monochrome { get; internal set; }

            internal SceneManagerMonitor(SceneManager sceneManager)
            {
                if (sceneManager == null) throw new ArgumentNullException("sceneManager");

                this.sceneManager = sceneManager;
            }

            internal void OnBeingClassifySceneObjects()
            {
                BeginClassifySceneObjects(sceneManager, EventArgs.Empty);
            }

            internal void OnEndClassifySceneObjects()
            {
                EndClassifySceneObjects(sceneManager, EventArgs.Empty);
            }

            internal void OnBeingDrawShadowMap()
            {
                BeginDrawShadowMap(sceneManager, EventArgs.Empty);
            }

            internal void OnEndDrawShadowMap()
            {
                EndDrawShadowMap(sceneManager, EventArgs.Empty);
            }

            internal void OnBeginDrawScene()
            {
                BeginDrawScene(sceneManager, EventArgs.Empty);
            }

            internal void OnEndDrawScene()
            {
                EndDrawScene(sceneManager, EventArgs.Empty);
            }

            internal void OnBeginDrawSceneOcclusionQuery()
            {
                BeginDrawSceneOcclusionQuery(sceneManager, EventArgs.Empty);
            }

            internal void OnEndDrawSceneOcclusionQuery()
            {
                EndDrawSceneOcclusionQuery(sceneManager, EventArgs.Empty);
            }

            internal void OnBeginDrawSceneRendering()
            {
                BeginDrawSceneRendering(sceneManager, EventArgs.Empty);
            }

            internal void OnEndDrawSceneRendering()
            {
                EndDrawSceneRendering(sceneManager, EventArgs.Empty);
            }

            internal void OnBeginPostProcess()
            {
                BeginPostProcess(sceneManager, EventArgs.Empty);
            }

            internal void OnEndPostProcess()
            {
                EndPostProcess(sceneManager, EventArgs.Empty);
            }
        }

        #endregion

        public const int InitialSceneObjectCapacity = 1000;

        public const int InitialCameraCapacity = 10;

        public const int InitialDirectionalLightCapacity = 10;

        public const int InitialVisibleObjectCapacity = 500;

        public const int InitialShadowCasterCapacity = 1000;

        public const int InitialParticleSystemCapacity = 10;

        static readonly BlendState colorWriteDisable = new BlendState
        {
            ColorWriteChannels = ColorWriteChannels.None
        };

        ISceneModuleFactory moduleFactory;

        List<SceneObject> sceneObjects = new List<SceneObject>(InitialSceneObjectCapacity);

        SceneObject skySphere;

        Dictionary<string, ICamera> cameraMap = new Dictionary<string, ICamera>(InitialCameraCapacity);

        Dictionary<string, DirectionalLight> directionalLightMap = new Dictionary<string, DirectionalLight>(InitialDirectionalLightCapacity);

        string activeCameraName;

        ICamera activeCamera;

        string activeDirectionalLightName;

        DirectionalLight activeDirectionalLight;

        Queue<SceneObject> workingSceneObjects = new Queue<SceneObject>(InitialSceneObjectCapacity);

        List<SceneObject> visibleSceneObjects = new List<SceneObject>(InitialVisibleObjectCapacity);

        List<SceneObject> opaqueSceneObjects = new List<SceneObject>(InitialVisibleObjectCapacity);

        List<SceneObject> translucentSceneObjects = new List<SceneObject>(InitialVisibleObjectCapacity);

        List<ShadowCaster> activeShadowCasters = new List<ShadowCaster>(InitialShadowCasterCapacity);

        Vector3[] frustumCorners = new Vector3[8];

        BoundingSphere frustumSphere;

        BasicCamera shadowCamera = new BasicCamera("Shadow");

        ShadowMap shadowMap;

        Sssm sssm;

        Ssao ssao;

        Edge edge;

        Bloom bloom;

        Dof dof;

        ColorOverlap colorOverlap;

        Monochrome monochrome;

        LensFlare lensFlare;

        bool shadowMapAvailable;

        RenderTarget2D renderTarget;

        RenderTarget2D postProcessRenderTarget;

        SpriteBatch spriteBatch;

        PostProcessorContext postProcessorContext;

        Settings settings;

        public ParticleSystemCollection ParticleSystems { get; private set; }

        public SceneManagerMonitor Monitor { get; private set; }

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

        // I/F
        public DirectionalLight ActiveDirectionalLight
        {
            get { return activeDirectionalLight; }
        }

        public GraphicsDevice GraphicsDevice { get; private set; }

        public string ActiveCameraName
        {
            get { return activeCameraName; }
            set
            {
                if (value != null && !cameraMap.ContainsKey(value))
                    throw new ArgumentException("Camera not found: " + value);

                if (activeCameraName == value) return;

                activeCameraName = value;
                activeCamera = (activeCameraName != null) ? cameraMap[activeCameraName] : null;
            }
        }

        public string ActiveDirectionalLightName
        {
            get { return activeDirectionalLightName; }
            set
            {
                if (value != null && !directionalLightMap.ContainsKey(value))
                    throw new ArgumentException("DirectionalLight not found: " + value);

                if (activeDirectionalLightName == value) return;

                activeDirectionalLightName = value;
                activeDirectionalLight = (activeDirectionalLightName != null) ? directionalLightMap[activeDirectionalLightName] : null;
            }
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

        public int ShadowMapSize { get; set; }

        public Vector3 ShadowColor { get; set; }

        public SceneManager(GraphicsDevice graphicsDevice, ISceneModuleFactory moduleFactory)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");
            if (moduleFactory == null) throw new ArgumentNullException("moduleFactory");

            GraphicsDevice = graphicsDevice;
            this.moduleFactory = moduleFactory;

            spriteBatch = new SpriteBatch(GraphicsDevice);

            ParticleSystems = new ParticleSystemCollection(InitialParticleSystemCapacity);

            Monitor = new SceneManagerMonitor(this);
        }

        public void Initialize(Settings settings)
        {
            if (settings == null) throw new ArgumentNullException("settings");

            this.settings = settings;

            //----------------------------------------------------------------
            // シャドウ モジュール

            if (settings.ShadowEnabled)
            {
                // シャドウ マップ モジュール
                var shadowMapEffect = moduleFactory.CreateShadowMapEffect();
                var blurEffect = moduleFactory.CreateGaussianBlurEffect();

                shadowMap = new ShadowMap(GraphicsDevice, settings.ShadowMap, spriteBatch, shadowMapEffect, blurEffect);
                Monitor.ShadowMap = shadowMap.Monitor;

                if (settings.SssmEnabled)
                {
                    // スクリーン スペース シャドウ マッピング モジュール
                    var shadowSceneEffect = moduleFactory.CreateShadowSceneEffect();
                    var sssmEffect = moduleFactory.CreateSssmEffect();

                    sssm = new Sssm(spriteBatch, settings.ShadowMap, settings.Sssm, shadowSceneEffect, sssmEffect, blurEffect);
                    Monitor.Sssm = sssm.Monitor;
                }
            }

            //----------------------------------------------------------------
            // シャドウ マッピング用カメラの登録

            AddCamera(shadowCamera);

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
            // シーンへのポスト プロセスのためのレンダ ターゲット

            postProcessRenderTarget = new RenderTarget2D(GraphicsDevice, width, height,
                false, format, depthFormat, multiSampleCount, RenderTargetUsage.PreserveContents);

            //----------------------------------------------------------------
            // スクリーン スペース アンビエント オクルージョン

            if (settings.SsaoEnabled)
            {
                var normalDepthMapEffect = moduleFactory.CreateNormalDepthMapEffect();
                var ssaoMapEffect = moduleFactory.CreateSsaoMapEffect();
                var ssaoMapBlurEffect = moduleFactory.CreateSsaoMapBlurEffect();
                var ssaoEffect = moduleFactory.CreateSsaoEffect();
                var randomNormalMap = moduleFactory.CreateRandomNormalMap();

                ssao = new Ssao(spriteBatch, settings.Ssao,
                    normalDepthMapEffect, ssaoMapEffect, ssaoMapBlurEffect, ssaoEffect, randomNormalMap);
                Monitor.Ssao = ssao.Monitor;
            }

            //----------------------------------------------------------------
            // エッジ強調

            if (settings.EdgeEnabled)
            {
                var normalDepthMapEffect = moduleFactory.CreateNormalDepthMapEffect();
                var edgeEffect = moduleFactory.CreateEdgeEffect();

                edge = new Edge(spriteBatch, settings.Edge, normalDepthMapEffect, edgeEffect);
                Monitor.Edge = edge.Monitor;
            }

            //----------------------------------------------------------------
            // ブルーム

            if (settings.BloomEnabled)
            {
                var bloomExtractEffect = moduleFactory.CreateBloomExtractEffect();
                var bloomEffect = moduleFactory.CreateBloomEffect();
                var blurEffect = moduleFactory.CreateGaussianBlurEffect();

                bloom = new Bloom(spriteBatch, settings.Bloom, bloomExtractEffect, bloomEffect, blurEffect);
                Monitor.Bloom = bloom.Monitor;
            }

            //----------------------------------------------------------------
            // 被写界深度

            if (settings.DofEnabled)
            {
                var depthMapEffect = moduleFactory.CreateDepthMapEffect();
                var dofEffect = moduleFactory.CreateDofEffect();
                var blurEffect = moduleFactory.CreateGaussianBlurEffect();

                dof = new Dof(spriteBatch, settings.Dof, depthMapEffect, dofEffect, blurEffect);
                Monitor.Dof = dof.Monitor;
            }

            //----------------------------------------------------------------
            // カラー オーバラップ

            if (settings.ColorOverlapEnabled)
            {
                colorOverlap = new ColorOverlap(spriteBatch);
                Monitor.ColorOverlap = colorOverlap.Monitor;
            }

            //----------------------------------------------------------------
            // モノクローム

            if (settings.MonochromeEnabled)
            {
                var monochromeEffect = moduleFactory.CreateMonochromeEffect();

                monochrome = new Monochrome(spriteBatch, monochromeEffect);
                Monitor.Monochrome = monochrome.Monitor;
            }

            //----------------------------------------------------------------
            // レンズ フレア

            {
                var glowSpite = moduleFactory.CreateLensFlareGlowSprite();
                var flareSprites = moduleFactory.CreateLensFlareFlareSprites();
                lensFlare = new LensFlare(GraphicsDevice, spriteBatch, glowSpite, flareSprites);
            }

#if DEBUG || TRACE
            debugBoundingBoxEffect = new BasicEffect(GraphicsDevice);
            debugBoundingBoxEffect.AmbientLightColor = Vector3.One;
            debugBoundingBoxEffect.VertexColorEnabled = true;
            debugBoundingBoxDrawer = new BoundingBoxDrawer(GraphicsDevice);
#endif
        }

        public void AddCamera(ICamera camera)
        {
            if (camera == null) throw new ArgumentNullException("camera");
            
            cameraMap[camera.Name] = camera;
        }

        public void RemoveCamera(string name)
        {
            if (name == null) throw new ArgumentNullException("name");
            
            cameraMap.Remove(name);
        }

        public void ClearCameras()
        {
            cameraMap.Clear();
        }

        public void AddDirectionalLight(DirectionalLight directionalLight)
        {
            if (directionalLight == null) throw new ArgumentNullException("directionalLight");

            directionalLightMap[directionalLight.Name] = directionalLight;
        }

        public void RemoveDirectionalLight(string name)
        {
            if (name == null) throw new ArgumentNullException("name");

            directionalLightMap.Remove(name);
        }

        public void ClearDirectionalLights()
        {
            directionalLightMap.Clear();
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

            // カメラ更新。
            activeCamera.Update();

            activeCamera.Frustum.GetCorners(frustumCorners);
            frustumSphere = BoundingSphere.CreateFromPoints(frustumCorners);

            // 収集リストを初期化。
            visibleSceneObjects.Clear();
            opaqueSceneObjects.Clear();
            translucentSceneObjects.Clear();
            activeShadowCasters.Clear();

            // カウンタをリセット。
            Monitor.TotalSceneObjectCount = 0;
            Monitor.VisibleSceneObjectCount = 0;
            Monitor.OccludedSceneObjectCount = 0;

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

            Monitor.TotalSceneObjectCount = workingSceneObjects.Count;

            Monitor.OnBeingClassifySceneObjects();

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

                    Monitor.VisibleSceneObjectCount++;
                }

                var shadowCaster = sceneObject as ShadowCaster;
                if (shadowCaster != null && IsActiveShadowCaster(shadowCaster))
                {
                    activeShadowCasters.Add(shadowCaster);
                    shouldPreDraw = true;
                }

                if (shouldPreDraw) sceneObject.PreDraw();
            }

            Monitor.OnEndClassifySceneObjects();

            // 視点からの距離でソート。
            DistanceComparer.Instance.EyePosition = activeCamera.View.Position;
            visibleSceneObjects.Sort(DistanceComparer.Instance);
            opaqueSceneObjects.Sort(DistanceComparer.Instance);
            translucentSceneObjects.Sort(DistanceComparer.Instance);

            //----------------------------------------------------------------
            // シャドウ マップ

            shadowMapAvailable = false;
            if (shadowMap != null && activeShadowCasters.Count != 0 &&
                activeDirectionalLight != null && activeDirectionalLight.Enabled)
            {
                DrawShadowMap();
                shadowMapAvailable = true;
            }

            //----------------------------------------------------------------
            // シーン

            if (shadowMapAvailable && sssm == null)
            {
                DrawScene(shadowMap);
            }
            else
            {
                DrawScene();
            }

            //----------------------------------------------------------------
            // パーティクル

            if (0 < ParticleSystems.Count)
                DrawParticles(gameTime);

            //----------------------------------------------------------------
            // ポスト プロセス

            PostProcess();

            //----------------------------------------------------------------
            // レンダ ターゲットの反映

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque);
            spriteBatch.Draw(renderTarget, Vector2.Zero, Color.White);
            spriteBatch.End();
        }

        void DrawParticles(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(renderTarget);

            foreach (var particleSystem in ParticleSystems)
            {
                if (particleSystem.Enabled)
                    particleSystem.Draw(gameTime, activeCamera);
            }

            GraphicsDevice.SetRenderTarget(null);
        }

        void PostProcess()
        {
            Monitor.OnBeginPostProcess();

            //----------------------------------------------------------------
            // スクリーン スペース シャドウ マッピング

            if (sssm != null)
            {
                sssm.Process(postProcessorContext, renderTarget, postProcessRenderTarget);
                SwapRenderTargets();
            }

            //----------------------------------------------------------------
            // スクリーン スペース アンビエント オクルージョン

            if (ssao != null)
            {
                ssao.Process(postProcessorContext, renderTarget, postProcessRenderTarget);
                SwapRenderTargets();
            }

            //----------------------------------------------------------------
            // エッジ強調

            if (edge != null)
            {
                edge.Process(postProcessorContext, renderTarget, postProcessRenderTarget);
                SwapRenderTargets();
            }

            //----------------------------------------------------------------
            // ブルーム

            if (bloom != null)
            {
                bloom.Process(postProcessorContext, renderTarget, postProcessRenderTarget);
                SwapRenderTargets();
            }

            //----------------------------------------------------------------
            // 被写界深度

            if (dof != null)
            {
                dof.Process(postProcessorContext, renderTarget, postProcessRenderTarget);
                SwapRenderTargets();
            }

            //----------------------------------------------------------------
            // カラー オーバラップ

            if (colorOverlap != null)
            {
                colorOverlap.Process(postProcessorContext, renderTarget, postProcessRenderTarget);
                SwapRenderTargets();
            }

            //----------------------------------------------------------------
            // モノクローム

            if (monochrome != null)
            {
                monochrome.Process(postProcessorContext, renderTarget, postProcessRenderTarget);
                SwapRenderTargets();
            }

            Monitor.OnEndPostProcess();
        }

        void SwapRenderTargets()
        {
            var temp = renderTarget;
            renderTarget = postProcessRenderTarget;
            postProcessRenderTarget = temp;
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

        void DrawShadowMap()
        {
            Monitor.OnBeingDrawShadowMap();

            //----------------------------------------------------------------
            // シャドウ マッピング用カメラの更新

            shadowCamera.View.Position = activeCamera.View.Position;
            shadowCamera.View.Direction = activeCamera.View.Direction;
            shadowCamera.View.Up = activeCamera.View.Up;
            shadowCamera.Projection.Fov = activeCamera.Projection.Fov;
            shadowCamera.Projection.AspectRatio = activeCamera.Projection.AspectRatio;
            shadowCamera.Projection.NearPlaneDistance = settings.ShadowMap.NearPlaneDistance;
            shadowCamera.Projection.FarPlaneDistance = settings.ShadowMap.FarPlaneDistance;
            shadowCamera.Update();

            //----------------------------------------------------------------
            // 分割カメラを準備

            shadowMap.PrepareSplitCameras(shadowCamera);

            //----------------------------------------------------------------
            // 投影オブジェクトを収集

            foreach (var shadowCaster in activeShadowCasters)
                shadowMap.TryAddShadowCaster(shadowCaster);

            //----------------------------------------------------------------
            // シャドウ マップを描画

            var lightDirection = activeDirectionalLight.Direction;
            shadowMap.Draw(ref lightDirection);

            if (DebugMapDisplay.Available)
            {
                for (int i = 0; i < shadowMap.SplitCount; i++)
                {
                    DebugMapDisplay.Instance.Add(shadowMap.GetShadowMap(i));
                }
            }

            Monitor.OnEndDrawShadowMap();
        }

        void DrawScene()
        {
            Monitor.OnBeginDrawScene();

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            GraphicsDevice.SetRenderTarget(renderTarget);
            GraphicsDevice.Clear(new Color(BackgroundColor));

            //================================================================
            //
            // オクルージョン クエリ
            //

            Monitor.OnBeginDrawSceneOcclusionQuery();

            GraphicsDevice.BlendState = colorWriteDisable;

            //----------------------------------------------------------------
            // 不透明オブジェクト

            foreach (var opaque in opaqueSceneObjects)
                opaque.UpdateOcclusion();

            //----------------------------------------------------------------
            // 半透明オブジェクト

            foreach (var translucent in translucentSceneObjects)
                translucent.UpdateOcclusion();

            Monitor.OnEndDrawSceneOcclusionQuery();

            //================================================================
            //
            // 描画
            //

            Monitor.OnBeginDrawSceneRendering();

            //----------------------------------------------------------------
            // 不透明オブジェクト

            GraphicsDevice.BlendState = BlendState.Opaque;

            foreach (var opaque in opaqueSceneObjects)
            {
                if (opaque.Occluded)
                {
                    Monitor.OccludedSceneObjectCount++;
                    DebugDrawBoundingBox(opaque);
                    continue;
                }

                opaque.Draw();

                DebugDrawBoundingBox(opaque);
            }

            //----------------------------------------------------------------
            // 半透明オブジェクト

            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            foreach (var translucent in translucentSceneObjects)
            {
                if (translucent.Occluded)
                {
                    Monitor.OccludedSceneObjectCount++;
                    DebugDrawBoundingBox(translucent);
                    continue;
                }

                // TODO
                // 半透明に対してシャドウ マップは必要か？
                translucent.Draw();

                DebugDrawBoundingBox(translucent);
            }

            //----------------------------------------------------------------
            // レンズ フレア

            // TODO: 描画位置がおかしいか？ここはオクルージョン クエリのみで良いかも。
            lensFlare.Draw(activeCamera, activeDirectionalLight.Direction);

            //----------------------------------------------------------------
            // スカイ スフィア

            if (SkySphere != null && SkySphere.Visible) SkySphere.Draw();

            Monitor.OnEndDrawSceneRendering();

            GraphicsDevice.SetRenderTarget(null);

            Monitor.OnEndDrawScene();
        }

        void DrawScene(ShadowMap shadowMap)
        {
            Monitor.OnBeginDrawScene();

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            
            GraphicsDevice.SetRenderTarget(renderTarget);
            GraphicsDevice.Clear(new Color(BackgroundColor));

            //================================================================
            //
            // オクルージョン クエリ
            //

            Monitor.OnBeginDrawSceneOcclusionQuery();

            GraphicsDevice.BlendState = colorWriteDisable;

            foreach (var opaque in opaqueSceneObjects)
                opaque.UpdateOcclusion();

            foreach (var translucent in translucentSceneObjects)
                translucent.UpdateOcclusion();

            Monitor.OnEndDrawSceneOcclusionQuery();

            //================================================================
            //
            // 描画
            //

            Monitor.OnBeginDrawSceneRendering();

            //----------------------------------------------------------------
            // 不透明オブジェクト

            GraphicsDevice.BlendState = BlendState.Opaque;

            foreach (var opaque in opaqueSceneObjects)
            {
                if (opaque.Occluded)
                {
                    Monitor.OccludedSceneObjectCount++;
                    DebugDrawBoundingBox(opaque);
                    continue;
                }

                opaque.Draw(shadowMap);

                DebugDrawBoundingBox(opaque);
            }

            //----------------------------------------------------------------
            // 半透明オブジェクト

            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            foreach (var translucent in translucentSceneObjects)
            {
                if (translucent.Occluded)
                {
                    Monitor.OccludedSceneObjectCount++;
                    DebugDrawBoundingBox(translucent);
                    continue;
                }

                // TODO
                // 半透明に対してシャドウ マップは必要か？
                translucent.Draw();

                DebugDrawBoundingBox(translucent);
            }

            //----------------------------------------------------------------
            // レンズ フレア

            // TODO: 描画位置がおかしいか？ここはオクルージョン クエリのみで良いかも。
            lensFlare.Draw(activeCamera, activeDirectionalLight.Direction);

            Monitor.OnEndDrawSceneRendering();

            GraphicsDevice.SetRenderTarget(null);

            Monitor.OnEndDrawScene();
        }

        [Conditional("DEBUG"), Conditional("TRACE")]
        void DebugDrawBoundingBox(SceneObject sceneObject)
        {
            if (!DebugBoundingBoxVisible) return;

            var color = (sceneObject.Occluded) ? Color.Gray : Color.White;
            debugBoundingBoxDrawer.Draw(ref sceneObject.BoundingBox, debugBoundingBoxEffect, ref color);
        }
    }
}
