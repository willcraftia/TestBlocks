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

        ShadowMapEffect shadowMapEffect;

        BasicCamera shadowCamera = new BasicCamera("Shadow");

        ShadowMap shadowMap;

        ShadowScene shadowScene;

        Sssm sssm;

        Dof dof;

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

        public Vector3 ShadowColor { get; set; }

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

            shadowMap = moduleFactory.CreateShadowMap(shadowSettings.ShadowMap);
            if (shadowMap != null) Monitor.Pssm = shadowMap.Monitor;

            if (shadowSettings.Sssm.Enabled)
            {
                shadowScene = moduleFactory.CreateShadowScene(shadowSettings);
                if (shadowScene != null) Monitor.ShadowScene = shadowScene.Monitor;

                sssm = moduleFactory.CreateSssm(shadowSettings.Sssm);
                if (sssm != null) Monitor.ScreenSpaceShadow = sssm.Monitor;
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
            // シーンへのポスト プロセスのためのレンダ ターゲット

            postProcessRenderTarget = new RenderTarget2D(GraphicsDevice, width, height,
                false, format, depthFormat, multiSampleCount, RenderTargetUsage.PreserveContents);

            //----------------------------------------------------------------
            // シーン描画のためのスプライト バッチ

            spriteBatch = new SpriteBatch(GraphicsDevice);

            //----------------------------------------------------------------
            // 被写界深度

            if (settings.Dof.Enabled)
                dof = moduleFactory.CreateDof(settings.Dof);

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

                if (IsVisibleObject(sceneObject))
                {
                    visibleSceneObjects.Add(sceneObject);

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
                    activeShadowCasters.Add(shadowCaster);
            }

            // 視点からの距離でソート。
            DistanceComparer.Instance.EyePosition = activeCamera.View.Position;
            visibleSceneObjects.Sort(DistanceComparer.Instance);
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
            if (shadowMapAvailable && Settings.Shadow.Sssm.Enabled)
            {
                shadowScene = DrawShadowScene();
            }

            //----------------------------------------------------------------
            // シーン

            if (shadowMapAvailable && !Settings.Shadow.Sssm.Enabled)
            {
                DrawScene(shadowMap);
            }
            else
            {
                DrawScene();
            }

            //----------------------------------------------------------------
            // スクリーン スペース シャドウ マッピング

            if (shadowScene != null && sssm != null)
            {
                sssm.ShadowColor = ShadowColor;
                sssm.Filter(renderTarget, shadowScene, postProcessRenderTarget);

                SwapRenderTargets();
            }

            //----------------------------------------------------------------
            // 被写界深度

            if (dof != null)
            {
                dof.DrawDepth(activeCamera, visibleSceneObjects);
                dof.Filter(activeCamera, renderTarget, postProcessRenderTarget);

                if (DebugMapDisplay.Available)
                {
                    DebugMapDisplay.Instance.Add(dof.DepthMap);
                    DebugMapDisplay.Instance.Add(dof.BluredSceneMap);
                }

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

        RenderTarget2D DrawShadowScene()
        {
            if (shadowScene != null)
            {
                // TODO: Transculent は要らない？
                shadowScene.Draw(activeCamera, shadowMap, opaqueSceneObjects);

                if (DebugMapDisplay.Available) DebugMapDisplay.Instance.Add(shadowScene.RenderTarget);

                return shadowScene.RenderTarget;
            }

            return null;
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
            shadowMap.Draw(shadowMapEffect, ref lightDirection);

            if (DebugMapDisplay.Available)
            {
                for (int i = 0; i < Settings.Shadow.ShadowMap.SplitCount; i++)
                {
                    DebugMapDisplay.Instance.Add(shadowMap.GetShadowMap(i));
                }
            }

            shadowMapAvailable = true;
        }

        void DrawScene()
        {
            Monitor.OnBeginDrawScene();

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            GraphicsDevice.SetRenderTarget(renderTarget);
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

        void DrawScene(ShadowMap shadowMap)
        {
            Monitor.OnBeginDrawScene();

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            
            GraphicsDevice.SetRenderTarget(renderTarget);
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

                opaque.Draw(shadowMap);

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
        void DebugDrawBoundingBox(SceneObject sceneObject)
        {
            if (!DebugBoundingBoxVisible) return;

            var color = (sceneObject.Occluded) ? Color.Gray : Color.White;
            debugBoundingBoxDrawer.Draw(ref sceneObject.BoundingBox, debugBoundingBoxEffect, ref color);
        }
    }
}
