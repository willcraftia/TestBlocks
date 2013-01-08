#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Content;
using Willcraftia.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Blocks.Landscape;
using Willcraftia.Xna.Blocks.Content;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class WorldManager
    {
        const int partitionMinActiveRange = 10;

        const int partitionMaxActiveRange = 12;

        IServiceProvider serviceProvider;

        SpriteBatch spriteBatch;

        ResourceManager resourceManager = new ResourceManager();

        AssetManager assetManager;

        public GraphicsDevice GraphicsDevice { get; private set; }

        public SceneManager SceneManager { get; private set; }

        public SceneManager.Settings SceneManagerSettings { get; private set; }

        public RegionManager RegionManager { get; private set; }

        public ChunkPartitionManager PartitionManager { get; private set; }

        public SceneSettings SceneSettings { get; private set; }

        public ShadowMap ShadowMap { get; private set; }

        public LensFlare LensFlare { get; private set; }

        public Sssm Sssm { get; private set; }

        public Ssao Ssao { get; private set; }

        public Edge Edge { get; private set; }

        public Bloom Bloom { get; private set; }

        public Dof Dof { get; private set; }

        public ColorOverlap ColorOverlap { get; private set; }

        public Monochrome Monochrome { get; private set; }

        public WorldManager(IServiceProvider serviceProvider, GraphicsDevice graphicsDevice)
        {
            if (serviceProvider == null) throw new ArgumentNullException("serviceProvider");
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");

            this.serviceProvider = serviceProvider;
            GraphicsDevice= graphicsDevice;

            spriteBatch = new SpriteBatch(graphicsDevice);
            assetManager = new AssetManager(serviceProvider);
            assetManager.RegisterLoader(typeof(SceneSettings), new SceneSettingsLoader());
            SceneManager = new SceneManager(graphicsDevice);
            RegionManager = new RegionManager(serviceProvider, SceneManager);
            PartitionManager = new ChunkPartitionManager(RegionManager);
        }

        public void Initialize()
        {
            //----------------------------------------------------------------
            // シーン設定

            // TODO: ワールド設定としてどうするか再検討。
            // いずれにせよ、SceneSettings はワールド設定と一対一。

            SceneSettings = LoadAsset<SceneSettings>("title:Resources/SceneSettings.json");

            //----------------------------------------------------------------
            // シーン マネージャ設定

            // TODO: リソースから取得する。
            SceneManagerSettings = new SceneManager.Settings();

            //----------------------------------------------------------------
            // シーン マネージャ

            SceneManager.Initialize(SceneManagerSettings);

            // 太陽と月をディレクショナル ライトとして登録。
            SceneManager.DirectionalLights.Add(SceneSettings.Sunlight);
            SceneManager.DirectionalLights.Add(SceneSettings.Moonlight);

            const bool shadowEnabled = true;
            const bool lensFlareEnabled = true;
            const bool sssmEnabled = false;
            const bool ssaoEnabled = true;
            const bool edgeEnabled = false;
            const bool bloomEnabled = false;
            const bool dofEnabled = true;
            const bool colorOverlapEnabled = false;
            const bool monochromeEnabled = false;

            // シャドウ マップ
            if (shadowEnabled)
            {
                var shadowMapEffect = LoadAsset<Effect>("content:Effects/ShadowMap");
                var blurEffect = LoadAsset<Effect>("content:Effects/GaussianBlur");

                ShadowMap = new ShadowMap(GraphicsDevice, SceneManagerSettings.ShadowMap, spriteBatch, shadowMapEffect, blurEffect);

                SceneManager.ShadowMap = ShadowMap;
            }

            // レンズ フレア
            if (lensFlareEnabled)
            {
                var glowSpite = LoadAsset<Texture2D>("content:Textures/LensFlare/Glow");
                Texture2D[] flareSprites =
                {
                    LoadAsset<Texture2D>("content:Textures/LensFlare/Flare1"),
                    LoadAsset<Texture2D>("content:Textures/LensFlare/Flare2"),
                    LoadAsset<Texture2D>("content:Textures/LensFlare/Flare3")
                };

                LensFlare = new LensFlare(GraphicsDevice, spriteBatch, glowSpite, flareSprites);

                SceneManager.LensFlare = LensFlare;
            }

            // スクリーン スペース シャドウ マッピング
            if (sssmEnabled)
            {
                // スクリーン スペース シャドウ マッピング モジュール
                var shadowSceneEffect = LoadAsset<Effect>("content:Effects/ShadowScene");
                var sssmEffect = LoadAsset<Effect>("content:Effects/Sssm");
                var blurEffect = LoadAsset<Effect>("content:Effects/GaussianBlur");

                Sssm = new Sssm(spriteBatch, SceneManagerSettings.ShadowMap, SceneManagerSettings.Sssm, shadowSceneEffect, sssmEffect, blurEffect);

                SceneManager.PostProcessors.Add(Sssm);
            }

            // スクリーン スペース アンビエント オクルージョン
            if (ssaoEnabled)
            {
                var normalDepthMapEffect = LoadAsset<Effect>("content:Effects/NormalDepthMap");
                var ssaoMapEffect = LoadAsset<Effect>("content:Effects/SsaoMap");
                var ssaoMapBlurEffect = LoadAsset<Effect>("content:Effects/SsaoMapBlur");
                var ssaoEffect = LoadAsset<Effect>("content:Effects/Ssao");
                var randomNormalMap = LoadAsset<Texture2D>("content:Textures/RandomNormal");

                Ssao = new Ssao(spriteBatch, SceneManagerSettings.Ssao,
                    normalDepthMapEffect, ssaoMapEffect, ssaoMapBlurEffect, ssaoEffect, randomNormalMap);

                SceneManager.PostProcessors.Add(Ssao);
            }

            // エッジ強調
            if (edgeEnabled)
            {
                var normalDepthMapEffect = LoadAsset<Effect>("content:Effects/NormalDepthMap");
                var edgeEffect = LoadAsset<Effect>("content:Effects/Edge");

                Edge = new Edge(spriteBatch, SceneManagerSettings.Edge, normalDepthMapEffect, edgeEffect);

                SceneManager.PostProcessors.Add(Edge);
            }

            // ブルーム
            if (bloomEnabled)
            {
                var bloomExtractEffect = LoadAsset<Effect>("content:Effects/BloomExtract");
                var bloomEffect = LoadAsset<Effect>("content:Effects/Bloom");
                var blurEffect = LoadAsset<Effect>("content:Effects/GaussianBlur");

                Bloom = new Bloom(spriteBatch, SceneManagerSettings.Bloom, bloomExtractEffect, bloomEffect, blurEffect);

                SceneManager.PostProcessors.Add(Bloom);
            }

            // 被写界深度
            if (dofEnabled)
            {
                var depthMapEffect = LoadAsset<Effect>("content:Effects/DepthMap");
                var dofEffect = LoadAsset<Effect>("content:Effects/Dof");
                var blurEffect = LoadAsset<Effect>("content:Effects/GaussianBlur");

                Dof = new Dof(spriteBatch, SceneManagerSettings.Dof, depthMapEffect, dofEffect, blurEffect);

                SceneManager.PostProcessors.Add(Dof);
            }

            // カラー オーバラップ
            if (colorOverlapEnabled)
            {
                ColorOverlap = new ColorOverlap(spriteBatch);

                SceneManager.PostProcessors.Add(ColorOverlap);
            }

            // モノクローム
            if (monochromeEnabled)
            {
                var monochromeEffect = LoadAsset<Effect>("content:Effects/Monochrome");

                Monochrome = new Monochrome(spriteBatch, monochromeEffect);

                SceneManager.PostProcessors.Add(Monochrome);
            }

            //----------------------------------------------------------------
            // リージョン マネージャ

            RegionManager.Initialize(SceneSettings);

            //----------------------------------------------------------------
            // パーティション マネージャ

            PartitionManager.Initialize(partitionMinActiveRange, partitionMaxActiveRange);
        }

        // TODO: 戻り値を Region にしない。
        public Region Load(string worldUri)
        {
            // TODO
            return RegionManager.LoadRegion("title:Resources/DefaultRegion.json");
        }

        public void Unload()
        {
            // TODO
        }

        public void Update(GameTime gameTime)
        {
            var camera = SceneManager.ActiveCamera;
            var cameraPosition = camera.View.Position;

            //----------------------------------------------------------------
            // パーティション マネージャ

            PartitionManager.Update(ref cameraPosition);

            //----------------------------------------------------------------
            // シーン設定

            SceneSettings.Update(gameTime);

            if (SceneSettings.SunVisible)
            {
                SceneManager.ActiveDirectionalLightName = SceneSettings.Sunlight.Name;
            }
            else if (SceneSettings.MoonVisible)
            {
                SceneManager.ActiveDirectionalLightName = SceneSettings.Moonlight.Name;
            }
            else
            {
                SceneManager.ActiveDirectionalLightName = null;
            }

            SceneManager.ShadowColor = SceneSettings.ShadowColor;

            //----------------------------------------------------------------
            // リージョン マネージャ

            RegionManager.Update(gameTime);
        }

        public void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1, 0);
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            //----------------------------------------------------------------
            // リージョン マネージャ

            // チャンク エフェクトを更新。
            RegionManager.UpdateChunkEffect();

            //----------------------------------------------------------------
            // シーン マネージャ

            SceneManager.BackgroundColor = SceneSettings.SkyColor;
            SceneManager.Draw(gameTime);
        }

        T LoadAsset<T>(string uri)
        {
            var resource = resourceManager.Load(uri);
            return assetManager.Load<T>(resource);
        }
    }
}
