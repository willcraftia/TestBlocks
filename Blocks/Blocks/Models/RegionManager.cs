﻿#region Using

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
    public sealed class RegionManager : IDisposable
    {
        static readonly Logger logger = new Logger(typeof(RegionManager).Name);

        IServiceProvider serviceProvider;

        SceneManager sceneManager;

        ResourceManager resourceManager = new ResourceManager();

        AssetManager assetManager;

        List<Region> regions = new List<Region>();

        SkySphere skySphere;

        ChunkEffect chunkEffect;

        ParticleSystem snowParticleSystem;

        ParticleSystem rainParticleSystem;

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
            assetManager.RegisterLoader(typeof(SkySphere), new SkySphereLoader(resourceManager, GraphicsDevice));
            assetManager.RegisterLoader(typeof(ParticleSettings), new ParticleSettingsLoader(resourceManager));

            Monitor = new RegionManagerMonitor(this);
        }

        public void Initialize(SceneSettings sceneSettings)
        {
            if (sceneSettings == null) throw new ArgumentNullException("sceneSettings");

            SceneSettings = sceneSettings;

            //----------------------------------------------------------------
            // スカイ スフィア

            skySphere = LoadAsset<SkySphere>("title:Resources/DefaultSkySphere.json");
            skySphere.SceneSettings = SceneSettings;

            // シーン マネージャへ登録
            sceneManager.SkySphere = skySphere;

            //----------------------------------------------------------------
            // チャンク エフェクト

            chunkEffect = new ChunkEffect(LoadAsset<Effect>("content:Effects/Chunk"));

            //----------------------------------------------------------------
            // 降雪パーティクル

            // TODO
            // これはバイオームで定義できるべきか？
            {
                var particleEffect = LoadAsset<Effect>("content:Effects/Particle");
                var particleSettings = LoadAsset<ParticleSettings>("title:Resources/DefaultSnowParticle.json");

                snowParticleSystem = new ParticleSystem(particleSettings, particleEffect);

                sceneManager.ParticleSystems.Add(snowParticleSystem);

                snowParticleSystem.Enabled = true;
            }

            //----------------------------------------------------------------
            // 降雨パーティクル

            // TODO
            // これはバイオームで定義できるべきか？
            {
                var particleEffect = LoadAsset<Effect>("content:Effects/Particle");
                var particleSettings = LoadAsset<ParticleSettings>("title:Resources/DefaultRainParticle.json");

                rainParticleSystem = new ParticleSystem(particleSettings, particleEffect);

                sceneManager.ParticleSystems.Add(rainParticleSystem);

                rainParticleSystem.Enabled = false;
            }
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

        //
        // TODO
        //
        static Random random = new Random();

        public void Update(GameTime gameTime)
        {
            Monitor.OnBeginUpdate();

            //----------------------------------------------------------------
            // シーン設定

            SceneSettings.Update(gameTime);

            //----------------------------------------------------------------
            // リージョン

            foreach (var region in regions) region.Update();

            //----------------------------------------------------------------
            // 降雪パーティクル

            // TODO
            if (snowParticleSystem.Enabled)
            {
                int boundsX = 128 * 2;
                int boundsZ = 128 * 2;
                int minY = 32;
                int maxY = 64;

                for (int i = 0; i < 40; i++)
                {
                    var randomX = random.Next(boundsX) - boundsX / 2;
                    var randomY = random.Next(maxY - minY) + minY;
                    var randomZ = random.Next(boundsZ) - boundsZ / 2;
                    var position = new Vector3(randomX, randomY, randomZ) + sceneManager.ActiveCamera.View.Position;
                    snowParticleSystem.AddParticle(position, Vector3.Zero);
                }
            }

            //----------------------------------------------------------------
            // 降雨パーティクル

            // TODO
            if (rainParticleSystem.Enabled)
            {
                int boundsX = 128 * 2;
                int boundsZ = 128 * 2;
                int minY = 32;
                int maxY = 64;

                for (int i = 0; i < 80; i++)
                {
                    var randomX = random.Next(boundsX) - boundsX / 2;
                    var randomY = random.Next(maxY - minY) + minY;
                    var randomZ = random.Next(boundsZ) - boundsZ / 2;
                    var position = new Vector3(randomX, randomY, randomZ) + sceneManager.ActiveCamera.View.Position;
                    rainParticleSystem.AddParticle(position, Vector3.Zero);
                }
            }

            Monitor.OnEndUpdate();
        }

        public void PrepareChunkEffect()
        {
            var camera = sceneManager.ActiveCamera;
            var projection = camera.Projection;

            //----------------------------------------------------------------
            // カメラ設定

            chunkEffect.EyePosition = camera.View.Position;
            chunkEffect.View = camera.View.Matrix;
            chunkEffect.Projection = camera.Projection.Matrix;

            //----------------------------------------------------------------
            // ライティング

            chunkEffect.AmbientLightColor = SceneSettings.CurrentAmbientLightColor;

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
            chunkEffect.FogStart = projection.FarPlaneDistance * 0.7f;
            chunkEffect.FogEnd = projection.FarPlaneDistance * 0.9f;
            chunkEffect.FogColor = SceneSettings.CurrentSkyColor;

            //----------------------------------------------------------------
            // ワイヤフレーム設定

            chunkEffect.WireframeEnabled = Wireframe;
        }

        public void Close()
        {
            foreach (var region in regions) region.Close();
        }

        internal void OnShadowMapUpdated(object sender, EventArgs e)
        {
            sceneManager.UpdateEffectShadowMap(chunkEffect);
        }

        T LoadAsset<T>(string uri)
        {
            var resource = resourceManager.Load(uri);
            return assetManager.Load<T>(resource);
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed;

        ~RegionManager()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                chunkEffect.Dispose();
            }

            disposed = true;
        }

        #endregion
    }
}
