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

        sealed class DistanceComparer : IComparer<ISceneObject>
        {
            public static DistanceComparer Instance = new DistanceComparer();

            public Vector3 EyePosition;

            DistanceComparer() { }

            public int Compare(ISceneObject o1, ISceneObject o2)
            {
                float d1;
                float d2;
                o1.GetDistanceSquared(ref EyePosition, out d1);
                o2.GetDistanceSquared(ref EyePosition, out d2);

                if (d1 == d2) return 0;
                return (d1 < d2) ? -1 : 1;
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

        List<ISceneObject> sceneObjects = new List<ISceneObject>(InitialSceneObjectCapacity);

        Dictionary<string, ICamera> cameraMap = new Dictionary<string, ICamera>(InitialCameraCapacity);

        Dictionary<string, DirectionalLight> directionalLightMap = new Dictionary<string, DirectionalLight>(InitialDirectionalLightCapacity);

        string activeCameraName;

        ICamera activeCamera;

        string activeDirectionalLightName;

        DirectionalLight activeDirectionalLight;

        Queue<ISceneObject> workingSceneObjects = new Queue<ISceneObject>(InitialSceneObjectCapacity);

        List<ISceneObject> opaqueSceneObjects = new List<ISceneObject>(InitialVisibleObjectCapacity);

        List<ISceneObject> translucentSceneObjects = new List<ISceneObject>(InitialVisibleObjectCapacity);

        List<IShadowCaster> activeShadowCasters = new List<IShadowCaster>(InitialShadowCasterCapacity);

        Vector3[] frustumCorners = new Vector3[8];

        BoundingSphere frustumSphere;

        ShadowMapEffect shadowMapEffect;

        Pssm pssm;

        PssmScene pssmScene;

        ScreenSpaceShadow screenSpaceShadow;

        bool shadowMapAvailable;

        RenderTarget2D renderTarget;

        RenderTarget2D postProcessRenderTarget;

        SpriteBatch spriteBatch;

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

        public int ShadowMapSize { get; set; }

        public LightFrustumTypes LightFrustumType { get; set; }

        public SceneManager(GraphicsDevice graphicsDevice, ISceneModuleFactory moduleFactory)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");
            if (moduleFactory == null) throw new ArgumentNullException("moduleFactory");

            GraphicsDevice = graphicsDevice;
            this.moduleFactory = moduleFactory;

            Monitor = new SceneManagerMonitor(this);
        }

        public void Initialize(SceneManagerSettings settings)
        {
            if (settings == null) throw new ArgumentNullException("settings");

            Settings = settings;

            //----------------------------------------------------------------
            // シャドウ モジュール

            var shadowSettings = Settings.Shadow;

            shadowMapEffect = new ShadowMapEffect(moduleFactory.CreateShadowMapEffect());
            shadowMapEffect.Technique = shadowSettings.ShadowMap.Technique;

            pssm = moduleFactory.CreatePssm(shadowSettings);
            if (pssm != null) Monitor.Pssm = pssm.Monitor;

            pssmScene = moduleFactory.CreatePssmScene(shadowSettings);
            if (pssmScene != null) Monitor.PssmScene = pssmScene.Monitor;

            screenSpaceShadow = moduleFactory.CreateScreenSpaceShadow(shadowSettings.ShadowScene);
            if (screenSpaceShadow != null) Monitor.ScreenSpaceShadow = screenSpaceShadow.Monitor;

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
            // シーンへのポスト プロセスのためのレンダ ターゲット

            postProcessRenderTarget = new RenderTarget2D(GraphicsDevice, width, height,
                false, format, depthFormat, multiSampleCount, RenderTargetUsage.PreserveContents);

            //----------------------------------------------------------------
            // シーン描画のためのスプライト バッチ

            spriteBatch = new SpriteBatch(GraphicsDevice);

#if DEBUG || TRACE
            debugBoundingBoxEffect = moduleFactory.CreateDebugBoundingBoxEffect();
            debugBoundingBoxEffect.AmbientLightColor = Vector3.One;
            debugBoundingBoxEffect.VertexColorEnabled = true;
            debugBoundingBoxDrawer = moduleFactory.CreateDebugBoundingBoxDrawer();
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

        public void AddSceneObject(ISceneObject sceneObject)
        {
            if (sceneObject == null) throw new ArgumentNullException("sceneObject");

            lock (sceneObjects)
            {
                sceneObject.Context = this;
                sceneObjects.Add(sceneObject);
            }
        }

        public void RemoveSceneObject(ISceneObject sceneObject)
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
            opaqueSceneObjects.Clear();
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

                if (IsVisibleObject(sceneObject))
                {
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

                var shadowCaster = sceneObject as IShadowCaster;
                if (shadowCaster != null &&IsActiveShadowCaster(shadowCaster))
                    activeShadowCasters.Add(shadowCaster);
            }

            // 視点からの距離でソート。
            DistanceComparer.Instance.EyePosition = activeCamera.Position;
            opaqueSceneObjects.Sort(DistanceComparer.Instance);
            translucentSceneObjects.Sort(DistanceComparer.Instance);

            //----------------------------------------------------------------
            // シャドウ マップ

            shadowMapAvailable = false;
            if (shadowMapEffect != null && Settings.Shadow.Enabled &&
                activeShadowCasters.Count != 0 &&
                activeDirectionalLight != null && activeDirectionalLight.Enabled)
            {
                DrawShadowMap();
            }

            //----------------------------------------------------------------
            // シャドウ シーン

            RenderTarget2D shadowScene = null;
            if (shadowMapAvailable && Settings.Shadow.ShadowScene.Enabled)
            {
                shadowScene = DrawShadowScene();
            }

            //----------------------------------------------------------------
            // シーン

            DrawScene();

            //----------------------------------------------------------------
            // スクリーン スペース シャドウ

            if (shadowScene != null && screenSpaceShadow != null && Settings.Shadow.ShadowScene.Enabled)
            {
                // TODO: ShadowColor
                screenSpaceShadow.Filter(renderTarget, shadowScene, postProcessRenderTarget);

                // TODO: スワップ管理クラスを作る？
                var temp = renderTarget;
                renderTarget = postProcessRenderTarget;
                postProcessRenderTarget = temp;
            }

            //----------------------------------------------------------------
            // レンダ ターゲットの反映

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque);
            spriteBatch.Draw(renderTarget, Vector2.Zero, Color.White);
            spriteBatch.End();
        }

        RenderTarget2D DrawShadowScene()
        {
            if (Settings.Shadow.LightFrustum.Type == LightFrustumTypes.Pssm && pssmScene != null)
            {
                // TODO: Transculent は要らない？
                pssmScene.Draw(activeCamera, pssm, opaqueSceneObjects);

                if (DebugMapDisplay.Available) DebugMapDisplay.Instance.Add(pssmScene.ShadowScene);

                return pssmScene.ShadowScene;
            }

            return null;
        }

        bool IsVisibleObject(ISceneObject sceneObject)
        {
            BoundingSphere objectSphere;
            sceneObject.GetBoundingSphere(out objectSphere);

            // 球同士で大きく判定。
            if (!objectSphere.Intersects(frustumSphere)) return false;

            // 視錐台と AABB で判定。
            BoundingBox objectBox;
            sceneObject.GetBoundingBox(out objectBox);
            return objectBox.Intersects(activeCamera.Frustum);
        }

        bool IsActiveShadowCaster(IShadowCaster shadowCaster)
        {
            if (shadowCaster.Translucent) return false;
            if (!shadowCaster.CastShadow) return false;

            return true;
        }

        void DrawShadowMap()
        {
            if (LightFrustumType == LightFrustumTypes.Pssm)
            {
                // PSSM の状態を準備。
                var lightDirection = activeDirectionalLight.Direction;
                pssm.Prepare(activeCamera, ref lightDirection);

                // 投影オブジェクトを収集。
                foreach (var shadowCaster in activeShadowCasters)
                    pssm.TryAddShadowCaster(shadowCaster);

                // シャドウ マップを描画。
                pssm.Draw(shadowMapEffect);

                if (DebugMapDisplay.Available)
                {
                    for (int i = 0; i < Settings.Shadow.LightFrustum.Pssm.SplitCount; i++)
                    {
                        DebugMapDisplay.Instance.Add(pssm.GetShadowMap(i));
                    }
                }

                shadowMapAvailable = true;
            }
        }

        void DrawScene()
        {
            Monitor.OnBeginDrawScene();

            GraphicsDevice.SetRenderTarget(renderTarget);

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.Clear(Color.White);

            //================================================================
            //
            // OcclusionQuery
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
            // Rendering
            //

            Monitor.OnBeginDrawSceneRendering();

            //----------------------------------------------------------------
            // Opaque Objects

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
            // Translucent Objects

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

            Monitor.OnEndDrawSceneRendering();

            GraphicsDevice.SetRenderTarget(null);

            Monitor.OnEndDrawScene();
        }

        [Conditional("DEBUG"), Conditional("TRACE")]
        void DebugDrawBoundingBox(ISceneObject sceneObject)
        {
            if (!DebugBoundingBoxVisible) return;

            var color = (sceneObject.Occluded) ? Color.DimGray : Color.Yellow;

            BoundingBox boundingBox;
            sceneObject.GetBoundingBox(out boundingBox);
            debugBoundingBoxDrawer.Draw(ref boundingBox, debugBoundingBoxEffect, ref color);
        }
    }
}
