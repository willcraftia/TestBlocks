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

        public const int InitialVisibleObjectCapacity = 500;

        public const int InitialShadowCasterCapacity = 1000;

        static readonly BlendState colorWriteDisable = new BlendState
        {
            ColorWriteChannels = ColorWriteChannels.None
        };

        ISceneModuleFactory moduleFactory;

        List<ISceneObject> sceneObjects = new List<ISceneObject>(InitialSceneObjectCapacity);

        Dictionary<string, ICamera> cameraMap = new Dictionary<string, ICamera>(InitialCameraCapacity);

        string activeCameraName;

        ICamera activeCamera;

        Queue<ISceneObject> workingSceneObjects = new Queue<ISceneObject>(InitialSceneObjectCapacity);

        List<ISceneObject> opaqueSceneObjects = new List<ISceneObject>(InitialVisibleObjectCapacity);

        List<ISceneObject> translucentSceneObjects = new List<ISceneObject>(InitialVisibleObjectCapacity);

        List<IShadowCaster> activeShadowCasters = new List<IShadowCaster>(InitialShadowCasterCapacity);

        Vector3[] frustumCorners = new Vector3[8];

        BoundingSphere frustumSphere;

        ShadowMapEffect shadowMapEffect;

        Pssm pssm;

        bool shadowingAvailable;

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

        public GraphicsDevice GraphicsDevice { get; private set; }

        public SceneSettings SceneSettings { get; private set; }

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

        public bool ShadowEnabled { get; set; }

        public int ShadowMapSize { get; set; }

        public LightFrustumTypes LightFrustumType { get; set; }

        public SceneManager(GraphicsDevice graphicsDevice, SceneSettings sceneSettings, ISceneModuleFactory moduleFactory)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");
            if (sceneSettings == null) throw new ArgumentNullException("sceneSettings");
            if (moduleFactory == null) throw new ArgumentNullException("moduleFactory");

            GraphicsDevice = graphicsDevice;
            SceneSettings = sceneSettings;
            this.moduleFactory = moduleFactory;

            Monitor = new SceneManagerMonitor(this);
        }

        public void Initialize()
        {
            var shadowSettings = SceneSettings.Shadow;

            shadowMapEffect = moduleFactory.CreateShadowMapEffect();
            shadowMapEffect.Technique = shadowSettings.ShadowMap.Technique;

            pssm = moduleFactory.CreatePssm(shadowSettings);

            // TODO
            if (shadowMapEffect != null && pssm != null)
                shadowingAvailable = true;

#if DEBUG
            //debugEffect = new BasicEffect(GraphicsDevice);
            //debugEffect.AmbientLightColor = Vector3.One;
            //debugEffect.VertexColorEnabled = true;
            //debugBoundingBoxDrawer = new BoundingBoxDrawer(GraphicsDevice);
            debugBoundingBoxEffect = moduleFactory.CreateDebugBoundingBoxEffect();
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

#if DEBUG
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

            // シャドウ マップの描画。
            if (shadowingAvailable && ShadowEnabled && activeShadowCasters.Count != 0)
            {
                DrawShadowMap();
            }

            // シーンの描画。
            DrawScene();
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

            return false;
        }

        void DrawShadowMap()
        {
            if (LightFrustumType == LightFrustumTypes.Pssm)
            {
                // PSSM の状態を準備。
                pssm.Prepare();

                // 投影オブジェクトを収集。
                foreach (var shadowCaster in activeShadowCasters)
                    pssm.TryAddShadowCaster(shadowCaster);

                // シャドウ マップを描画。
                pssm.Draw(shadowMapEffect);
            }
        }

        void DrawScene()
        {
            Monitor.OnBeginDrawScene();

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

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

                // TODO
                opaque.Draw(null);

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
                // 半透明に対しては常に null で良いか？
                translucent.Draw(null);

                DebugDrawBoundingBox(translucent);
            }

            Monitor.OnEndDrawSceneRendering();

            Monitor.OnEndDrawScene();
        }

        [Conditional("DEBUG")]
        void DebugDrawBoundingBox(ISceneObject sceneObject)
        {
            if (!DebugBoundingBoxVisible) return;

            BoundingBox boundingBox;
            sceneObject.GetBoundingBox(out boundingBox);
            debugBoundingBoxDrawer.Draw(ref boundingBox, debugBoundingBoxEffect);
        }
    }
}
