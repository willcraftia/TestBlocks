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

        public const int InitialSceneObjectCapacity = 1000;

        public const int InitialCameraCapacity = 10;

        public const int InitialDirectionalLightCapacity = 10;

        public const int InitialVisibleObjectCapacity = 500;

        public const int InitialShadowCasterCapacity = 1000;

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

        Dof dof;

        Edge edge;

        Ssao ssao;

        LensFlare lensFlare;

        bool shadowMapAvailable;

        RenderTarget2D renderTarget;

        RenderTarget2D postProcessRenderTarget;

        SpriteBatch spriteBatch;

        PostProcessorContext postProcessorContext;

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

        public SceneManagerSettings Settings { get; private set; }

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

            Monitor = new SceneManagerMonitor(this);
        }

        public void Initialize(SceneManagerSettings settings)
        {
            if (settings == null) throw new ArgumentNullException("settings");

            Settings = settings;

            //----------------------------------------------------------------
            // シャドウ モジュール

            var shadowSettings = Settings.Shadow;

            if (shadowSettings.Enabled)
            {
                // シャドウ マップ モジュール
                var shadowMapEffect = moduleFactory.CreateShadowMapEffect();
                var blurEffect = moduleFactory.CreateGaussianBlurEffect();

                shadowMap = new ShadowMap(GraphicsDevice, shadowSettings.ShadowMap, spriteBatch, shadowMapEffect, blurEffect);
                Monitor.ShadowMap = shadowMap.Monitor;

                if (shadowSettings.Sssm.Enabled)
                {
                    // スクリーン スペース シャドウ マッピング モジュール
                    var shadowSceneEffect = moduleFactory.CreateShadowSceneEffect();
                    var sssmEffect = moduleFactory.CreateSssmEffect();

                    sssm = new Sssm(GraphicsDevice, shadowSettings, spriteBatch, shadowSceneEffect, sssmEffect, blurEffect);
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
            // 被写界深度

            if (settings.Dof.Enabled)
            {
                var depthMapEffect = moduleFactory.CreateDepthMapEffect();
                var dofEffect = moduleFactory.CreateDofEffect();
                var blurEffect = moduleFactory.CreateGaussianBlurEffect();

                dof = new Dof(GraphicsDevice, settings.Dof, spriteBatch, depthMapEffect, dofEffect, blurEffect);
            }

            //----------------------------------------------------------------
            // エッジ強調

            if (settings.Edge.Enabled)
            {
                var normalDepthMapEffect = moduleFactory.CreateNormalDepthMapEffect();
                var edgeEffect = moduleFactory.CreateEdgeEffect();

                edge = new Edge(GraphicsDevice, settings.Edge, spriteBatch, normalDepthMapEffect, edgeEffect);
            }

            //----------------------------------------------------------------
            // スクリーン スペース アンビエント オクルージョン

            if (settings.Ssao.Enabled)
            {
                var normalDepthMapEffect = moduleFactory.CreateNormalDepthMapEffect();
                var ssaoMapEffect = moduleFactory.CreateSsaoMapEffect();
                var ssaoMapBlurEffect = moduleFactory.CreateSsaoMapBlurEffect();
                var ssaoEffect = moduleFactory.CreateSsaoEffect();
                var randomNormalMap = moduleFactory.CreateRandomNormalMap();

                ssao = new Ssao(GraphicsDevice, settings.Ssao, spriteBatch,
                    normalDepthMapEffect, ssaoMapEffect, ssaoMapBlurEffect, ssaoEffect, randomNormalMap);
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

        public void Draw()
        {
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
            // 被写界深度

            if (dof != null)
            {
                dof.Process(postProcessorContext, renderTarget, postProcessRenderTarget);
                SwapRenderTargets();
            }

            //----------------------------------------------------------------
            // レンダ ターゲットの反映

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque);
            spriteBatch.Draw(renderTarget, Vector2.Zero, Color.White);
            spriteBatch.End();
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
            //----------------------------------------------------------------
            // シャドウ マッピング用カメラの更新

            shadowCamera.View.Position = activeCamera.View.Position;
            shadowCamera.View.Direction = activeCamera.View.Direction;
            shadowCamera.View.Up = activeCamera.View.Up;
            shadowCamera.Projection.Fov = activeCamera.Projection.Fov;
            shadowCamera.Projection.AspectRatio = activeCamera.Projection.AspectRatio;
            shadowCamera.Projection.NearPlaneDistance = Settings.Shadow.ShadowMap.NearPlaneDistance;
            shadowCamera.Projection.FarPlaneDistance = Settings.Shadow.ShadowMap.FarPlaneDistance;
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
