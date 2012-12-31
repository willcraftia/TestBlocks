#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Content;
using Willcraftia.Xna.Framework.Diagnostics;
using Willcraftia.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Framework.Noise;
using Willcraftia.Xna.Framework.Serialization;
using Willcraftia.Xna.Blocks.Content;
using Willcraftia.Xna.Blocks.Serialization;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class RegionManager
    {
        static readonly Logger logger = new Logger(typeof(RegionManager).Name);

        IServiceProvider serviceProvider;

        SceneManager sceneManager;

        ResourceManager resourceManager = new ResourceManager();

        AssetManager assetManager;

        List<Region> regions = new List<Region>();

        SkySphere skySphere;

        ChunkEffect chunkEffect;

        public static bool Wireframe { get; set; }

        public GraphicsDevice GraphicsDevice { get; private set; }

        public SceneSettings SceneSettings { get; private set; }

        public RegionManagerMonitor Monitor { get; private set; }

        public RegionManager(IServiceProvider serviceProvider, SceneManager sceneManager)
        {
            if (serviceProvider == null) throw new ArgumentNullException("serviceProvider");
            if (sceneManager == null) throw new ArgumentNullException("sceneManager");

            this.serviceProvider = serviceProvider;
            this.sceneManager = sceneManager;

            GraphicsDevice = sceneManager.GraphicsDevice;

            assetManager = new AssetManager(serviceProvider);
            assetManager.RegisterLoader(typeof(Image2D), new Image2DLoader(GraphicsDevice));

            Monitor = new RegionManagerMonitor(this);
        }

        public void Initialize()
        {
            skySphere = new SkySphere(GraphicsDevice);
            var effectResource = resourceManager.Load("content:Effects/SkySphere");
            skySphere.Effect = new SkySphereEffect(assetManager.Load<Effect>(effectResource));
            // シーン マネージャへ登録
            sceneManager.AddSceneObject(skySphere);

            var chunkEffectResource = resourceManager.Load("content:Effects/Chunk");
            chunkEffect = new ChunkEffect(assetManager.Load<Effect>(chunkEffectResource));
        }

        public void SetSceneSettings(SceneSettings sceneSettings)
        {
            if (sceneSettings == null) throw new ArgumentNullException("sceneSettings");

            SceneSettings = sceneSettings;

            skySphere.SceneSettings = SceneSettings;
        }

        //
        // AssetManager の扱い
        //
        // エディタ:
        //      エディタで一つの AssetManager。
        //      各モデルは、必ずしも Region に関連付けられるとは限らない。
        //
        // ゲーム環境:
        //      Region で一つの AssetManager。
        //      各モデルは、必ず一つの Region に関連付けられる。
        //
        // ※
        // 寿命としては ResourceManager も同様。
        // ただし、ResourceManager は AssetManager から Region をロードするに先駆け、
        // Region の Resource を解決するために用いられる点に注意する。
        //

        // 非同期呼び出しを想定。
        // Region 丸ごと別スレッドでロードするつもり。
        public Region LoadRegion(string uri)
        {
            logger.Info("LoadRegion: {0}", uri);

            var resourceManager = new ResourceManager();

            var resource = resourceManager.Load(uri);

            var assetManager = new AssetManager(serviceProvider);
            assetManager.RegisterLoader(typeof(Region), new RegionLoader(resourceManager));
            assetManager.RegisterLoader(typeof(Image2D), new Image2DLoader(GraphicsDevice));
            assetManager.RegisterLoader(typeof(Mesh), new MeshLoader());
            assetManager.RegisterLoader(typeof(Tile), new TileLoader(resourceManager));
            assetManager.RegisterLoader(typeof(TileCatalog), new TileCatalogLoader(resourceManager, GraphicsDevice));
            assetManager.RegisterLoader(typeof(Block), new BlockLoader(resourceManager));
            assetManager.RegisterLoader(typeof(BlockCatalog), new BlockCatalogLoader(resourceManager));
            assetManager.RegisterLoader(typeof(IBiome), new BiomeLoader(resourceManager));
            assetManager.RegisterLoader(typeof(IBiomeManager), new BiomeManagerLoader(resourceManager));
            assetManager.RegisterLoader(typeof(BiomeCatalog), new BiomeCatalogLoader(resourceManager));
            assetManager.RegisterLoader(typeof(IChunkProcedure), new ChunkProcedureLoader());
            assetManager.RegisterLoader(typeof(INoiseSource), new NoiseLoader(resourceManager));

            var region = assetManager.Load<Region>(resource);
            region.Initialize(sceneManager, SceneSettings, assetManager, chunkEffect);

            lock (regions)
            {
                regions.Add(region);
            }

            return region;
        }

        public bool RegionExists(ref VectorI3 position)
        {
            Region region;
            return TryGetRegion(ref position, out region);
        }

        public bool TryGetRegion(ref VectorI3 position, out Region region)
        {
            lock (regions)
            {
                foreach (var r in regions)
                {
                    if (r.ContainsPosition(ref position))
                    {
                        region = r;
                        return true;
                    }
                }
            }

            region = null;
            return false;
        }

        public void Update(GameTime gameTime)
        {
            Monitor.OnBeginUpdate();

            SceneSettings.Update(gameTime);

            foreach (var region in regions) region.Update();

            Monitor.OnEndUpdate();
        }

        public void UpdateChunkEffect()
        {
            var camera = sceneManager.ActiveCamera;
            var projection = camera.Projection;

            //----------------------------------------------------------------
            // カメラ設定

            chunkEffect.EyePosition = camera.Position;
            chunkEffect.ViewProjection = camera.Frustum.Matrix;

            //----------------------------------------------------------------
            // ライティング

            chunkEffect.AmbientLightColor = SceneSettings.AmbientLightColor;

            //
            // TODO
            //
            // ChunkEffect に DirectionalLight プロパティを作る。
            //
            var activeDirectionalLight = sceneManager.ActiveDirectionalLight;
            if (activeDirectionalLight != null && activeDirectionalLight.Enabled)
            {
                chunkEffect.LightDirection = activeDirectionalLight.Direction;
                chunkEffect.LightDiffuseColor = activeDirectionalLight.DiffuseColor;
                chunkEffect.LightSpecularColor = activeDirectionalLight.SpecularColor;
            }
            else
            {
                chunkEffect.LightDirection = Vector3.Down;
                chunkEffect.LightDiffuseColor = Vector3.Zero;
                chunkEffect.LightSpecularColor = Vector3.Zero;
            }

            //----------------------------------------------------------------
            // フォグ

            chunkEffect.FogEnabled = GlobalSceneSettings.FogEnabled;
            chunkEffect.FogStart = projection.FarPlaneDistance * 0.6f;
            chunkEffect.FogEnd = projection.FarPlaneDistance * 0.9f;
            chunkEffect.FogColor = SceneSettings.SkyColor;

            //----------------------------------------------------------------
            // テクニック

            chunkEffect.EnableDefaultTechnique();
            
            if (Wireframe) chunkEffect.EnableWireframeTechnique();
        }

        public void Close()
        {
            foreach (var region in regions) region.Close();
        }
    }
}
