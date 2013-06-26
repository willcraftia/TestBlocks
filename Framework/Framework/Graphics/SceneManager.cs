#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Collections;
using Willcraftia.Xna.Framework.Diagnostics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class SceneManager
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
                Vector3.DistanceSquared(ref EyePosition, ref o1.PositionWorld, out d1);
                Vector3.DistanceSquared(ref EyePosition, ref o2.PositionWorld, out d2);

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
            public List<SceneObject> OpaqueObjects
            {
                get { return sceneManager.opaqueObjects; }
            }

            // I/F
            public List<SceneObject> TranslucentObjects
            {
                get { return sceneManager.translucentObjects; }
            }

            // I/F
            public ShadowMap ShadowMap
            {
                get { return sceneManager.ShadowMap; }
            }

            // I/F
            public RenderTarget2D Source
            {
                get { return sceneManager.RenderTarget; }
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

        #region CameraCollection

        public sealed class CameraCollection : KeyedList<string, ICamera>
        {
            internal CameraCollection(int capacity) : base(capacity) { }

            protected override string GetKeyForItem(ICamera item)
            {
                return item.Name;
            }
        }

        #endregion

        #region DirectionalLightCollection

        public sealed class DirectionalLightCollection : KeyedList<string, DirectionalLight>
        {
            internal DirectionalLightCollection(int capacity) : base(capacity) { }

            protected override string GetKeyForItem(DirectionalLight item)
            {
                return item.Name;
            }
        }

        #endregion

        #region ParticleSystemCollection

        public sealed class ParticleSystemCollection : KeyedList<string, ParticleSystem>
        {
            internal ParticleSystemCollection(int capacity) : base(capacity) { }

            protected override string GetKeyForItem(ParticleSystem item)
            {
                return item.Name;
            }
        }

        #endregion

        #region PostProcessorCollection

        public sealed class PostProcessorCollection : ListBase<PostProcessor>
        {
            internal PostProcessorCollection(int capacity) : base(capacity) { }
        }

        #endregion

        public const int InitialCameraCapacity = 10;

        public const int InitialDirectionalLightCapacity = 10;

        public const int InitialSceneObjectCapacity = 500;

        public const int InitialShadowCasterCapacity = 500;

        public const int InitialParticleSystemCapacity = 10;

        public const int InitialPostProcessorCapacity = 10;

        public const string InstrumentDraw = "SceneManager.Draw";

        public const string InstrumentDrawShadowMap = "SceneManager.DrawShadowMap";
        
        public const string InstrumentDrawScene = "SceneManager.DrawScene";
        
        public const string InstrumentOcclusionQuery = "SceneManager.OcclusionQuery";
        
        public const string InstrumentDrawSceneObjects = "SceneManager.DrawSceneObjects";
        
        public const string InstrumentDrawParticles = "SceneManager.DrawParticles";
        
        public const string InstrumentPostProcess = "SceneManager.PostProcess";

        static readonly BlendState colorWriteDisable = new BlendState
        {
            ColorWriteChannels = ColorWriteChannels.None
        };

        /// <summary>
        /// シャドウ マップの更新完了で発生するイベント。
        /// </summary>
        public event EventHandler ShadowMapUpdated = delegate { };

        OctreeManager octreeManager;

        SceneNode skySphereNode;

        string activeCameraName;

        ICamera activeCamera;

        Vector3 ambientLightColor;

        string activeDirectionalLightName;

        DirectionalLight activeDirectionalLight;

        Vector3 fogColor;

        Vector3[] frustumCorners = new Vector3[8];

        BoundingSphere frustumSphere;

        bool shadowMapAvailable;

        RenderTarget2D postProcessRenderTarget;

        SpriteBatch spriteBatch;

        PostProcessorContext postProcessorContext;

        Settings settings;

        List<SceneObject> opaqueObjects;

        List<SceneObject> translucentObjects;

        List<ShadowCaster> shadowCasters;

        Action<Octree> collectObjectsAction;

        #region Debug

        BasicEffect debugBoxEffect;

        BoundingBoxDrawer debugBoxDrawer;

        public static bool DebugBoxVisible { get; set; }

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

        public SceneNode RootNode { get; private set; }

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

        public SceneNode SkySphere
        {
            get { return skySphereNode; }
            set { skySphereNode = value; }
        }

        /// <summary>
        /// レンダ ターゲットを取得します。
        /// シーン マネージャは、このレンダ ターゲットへ最終的なシーンを描画します。
        /// 呼び出し元は、シーン マネージャの描画が完了したら、
        /// このレンダ ターゲットをスクリーンへ反映させる必要があります。
        /// </summary>
        public RenderTarget2D RenderTarget { get; private set; }

        public bool SssmEnabled { get; set; }

        public int SceneObjectCount { get; internal set; }

        public int OccludedSceneObjectCount { get; internal set; }

        public int RenderedSceneObjectCount
        {
            get { return SceneObjectCount - OccludedSceneObjectCount; }
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

            opaqueObjects = new List<SceneObject>(InitialSceneObjectCapacity);
            translucentObjects = new List<SceneObject>(InitialSceneObjectCapacity);
            shadowCasters = new List<ShadowCaster>(InitialShadowCasterCapacity);
            collectObjectsAction = new Action<Octree>(CollectObjects);

            //----------------------------------------------------------------
            // シーン描画のためのレンダ ターゲット

            var pp = GraphicsDevice.PresentationParameters;
            var width = pp.BackBufferWidth;
            var height = pp.BackBufferHeight;
            var format = pp.BackBufferFormat;
            var depthFormat = pp.DepthStencilFormat;
            var multiSampleCount = pp.MultiSampleCount;
            RenderTarget = new RenderTarget2D(GraphicsDevice, width, height,
                false, format, depthFormat, multiSampleCount, RenderTargetUsage.PreserveContents);

            //----------------------------------------------------------------
            // ポストプロセッサ コンテキスト

            postProcessorContext = new PostProcessorContext(this);

            //----------------------------------------------------------------
            // ポスト プロセスのためのレンダ ターゲット

            postProcessRenderTarget = new RenderTarget2D(GraphicsDevice, width, height,
                false, format, depthFormat, multiSampleCount, RenderTargetUsage.PreserveContents);

            // TODO
            octreeManager = new OctreeManager(new Vector3(256), 3);

            // TODO
            RootNode = new SceneNode(this, "Root");

#if DEBUG || TRACE
            debugBoxEffect = new BasicEffect(GraphicsDevice);
            debugBoxEffect.AmbientLightColor = Vector3.One;
            debugBoxEffect.VertexColorEnabled = true;
            debugBoxDrawer = new BoundingBoxDrawer(GraphicsDevice);
#endif
        }

        public SceneNode CreateSceneNode(string name)
        {
            return new SceneNode(this, name);
        }

        public void UpdateOctreeSceneNode(SceneNode node)
        {
            octreeManager.Update(node);
        }

        public void RemoveOctreeSceneNode(SceneNode node)
        {
            octreeManager.Remove(node);
        }

        public void Draw(GameTime gameTime)
        {
            if (gameTime == null) throw new ArgumentNullException("gameTime");
            if (activeCamera == null) throw new InvalidOperationException("ActiveCamera is null.");

            Instrument.Begin(InstrumentDraw);

            activeCamera.Frustum.GetCorners(frustumCorners);
            frustumSphere = BoundingSphere.CreateFromPoints(frustumCorners);

            // カウンタをリセット。
            SceneObjectCount = 0;
            OccludedSceneObjectCount = 0;

#if DEBUG || TRACE
            // デバッグ エフェクトへカメラ情報を設定。
            debugBoxEffect.View = activeCamera.View.Matrix;
            debugBoxEffect.Projection = activeCamera.Projection.Matrix;
#endif

            octreeManager.Execute(activeCamera.Frustum, collectObjectsAction);

            SceneObjectCount = opaqueObjects.Count + translucentObjects.Count;

            // 視点からの距離でソート。
            DistanceComparer.Instance.EyePosition = activeCamera.View.Position;
            // TODO
            // 不透明オブジェクトのソートは除外して良いかも。
            opaqueObjects.Sort(DistanceComparer.Instance);
            translucentObjects.Sort(DistanceComparer.Instance);

            //----------------------------------------------------------------
            // シャドウ マップ

            shadowMapAvailable = false;
            if (ShadowMap != null && shadowCasters.Count != 0 &&
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
            // シーン オブジェクトの境界ボックスの描画

            DebugDrawBoundingBoxes();

            //----------------------------------------------------------------
            // 後処理

            // 分類リストを初期化。
            opaqueObjects.Clear();
            translucentObjects.Clear();
            shadowCasters.Clear();

            Instrument.End();
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

        void CollectObjects(Octree octree)
        {
            if (octree.Nodes.Count == 0) return;

            for (int i = 0; i < octree.Nodes.Count; i++)
            {
                var node = octree.Nodes[i];

                if (node.Objects.Count == 0) continue;

                foreach (var obj in node.Objects)
                {
                    // Visible = false は除外。
                    if (!obj.Visible) continue;

                    // 半透明と不透明で分類。
                    if (obj.Translucent)
                    {
                        translucentObjects.Add(obj);
                    }
                    else
                    {
                        opaqueObjects.Add(obj);
                    }

                    // 投影可か否か。
                    var shadowCaster = obj as ShadowCaster;
                    if (shadowCaster != null)
                    {
                        shadowCasters.Add(shadowCaster);
                    }
                }
            }
        }

        void DrawShadowMap()
        {
            Instrument.Begin(InstrumentDrawShadowMap);

            //----------------------------------------------------------------
            // 準備

            ShadowMap.Prepare(activeCamera);

            //----------------------------------------------------------------
            // 投影オブジェクトを収集

            for (int i = 0; i < shadowCasters.Count; i++)
                ShadowMap.TryAddShadowCaster(shadowCasters[i]);

            //----------------------------------------------------------------
            // シャドウ マップを描画

            var lightDirection = activeDirectionalLight.Direction;
            ShadowMap.Draw(ref lightDirection);

            Instrument.End();
        }

        void DrawScene(ShadowMap shadowMap)
        {
            Instrument.Begin(InstrumentDrawScene);

            //GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            GraphicsDevice.SetRenderTarget(RenderTarget);
            GraphicsDevice.Clear(new Color(BackgroundColor));

            //================================================================
            //
            // オクルージョン クエリ
            //

            Instrument.Begin(InstrumentOcclusionQuery);

            GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            GraphicsDevice.BlendState = colorWriteDisable;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            for (int i = 0; i < opaqueObjects.Count; i++)
                opaqueObjects[i].UpdateOcclusion();

            for (int i = 0; i < translucentObjects.Count; i++)
                translucentObjects[i].UpdateOcclusion();

            Instrument.End();

            //================================================================
            //
            // 描画
            //

            Instrument.Begin(InstrumentDrawSceneObjects);

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            //----------------------------------------------------------------
            // 不透明オブジェクト

            GraphicsDevice.BlendState = BlendState.Opaque;

            for (int i = 0; i < opaqueObjects.Count; i++)
            {
                var opaque = opaqueObjects[i];

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

            for (int i = 0; i < translucentObjects.Count; i++)
            {
                var translucent = translucentObjects[i];

                if (translucent.Occluded)
                {
                    OccludedSceneObjectCount++;
                    continue;
                }

                translucent.Draw();
            }

            //----------------------------------------------------------------
            // スカイ スフィア

            foreach (var obj in skySphereNode.Objects)
            {
                if (obj.Visible) obj.Draw();
            }

            //if (skySphere != null && skySphere.Visible)
            //    skySphere.Draw();

            Instrument.End();

            GraphicsDevice.SetRenderTarget(null);

            Instrument.End();
        }

        void DrawParticles(GameTime gameTime)
        {
            Instrument.Begin(InstrumentDrawParticles);

            GraphicsDevice.SetRenderTarget(RenderTarget);

            for (int i = 0; i < ParticleSystems.Count; i++)
            {
                var particleSystem = ParticleSystems[i];
                if (particleSystem.Enabled)
                    particleSystem.Draw(gameTime, activeCamera);
            }

            GraphicsDevice.SetRenderTarget(null);

            Instrument.End();
        }

        void PostProcess()
        {
            Instrument.Begin(InstrumentPostProcess);

            for (int i = 0; i < PostProcessors.Count; i++)
            {
                var postProcessor = PostProcessors[i];
                if (postProcessor.Enabled)
                {
                    postProcessor.Process(postProcessorContext);
                    SwapRenderTargets();
                }
            }

            Instrument.End();
        }

        void SwapRenderTargets()
        {
            var temp = RenderTarget;
            RenderTarget = postProcessRenderTarget;
            postProcessRenderTarget = temp;
        }

        void DrawLensFlare()
        {
            if (activeDirectionalLight == null) return;

            GraphicsDevice.SetRenderTarget(RenderTarget);

            LensFlare.Draw(activeCamera, activeDirectionalLight.Direction);

            GraphicsDevice.SetRenderTarget(null);
        }

        [Conditional("DEBUG"), Conditional("TRACE")]
        void DebugDrawBoundingBoxes()
        {
            if (!DebugBoxVisible) return;

            GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            GraphicsDevice.SetRenderTarget(RenderTarget);

            for (int i = 0; i < opaqueObjects.Count; i++)
                DebugDrawBoundingBox(opaqueObjects[i]);

            for (int i = 0; i < translucentObjects.Count; i++)
                DebugDrawBoundingBox(translucentObjects[i]);

            GraphicsDevice.SetRenderTarget(null);
        }

        [Conditional("DEBUG"), Conditional("TRACE")]
        void DebugDrawBoundingBox(SceneObject sceneObject)
        {
            var color = (sceneObject.Occluded) ? Color.Gray : Color.White;
            debugBoxDrawer.Draw(ref sceneObject.BoxWorld, debugBoxEffect, ref color);
        }
    }
}
