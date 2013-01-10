#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Content;
using Willcraftia.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Framework.Landscape;
using Willcraftia.Xna.Blocks.Landscape;
using Willcraftia.Xna.Blocks.Content;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class WorldManager
    {
        // TODO
        public const int PartitionMinActiveRange = 10;
        public const int PartitionMaxActiveRange = 12;

        IServiceProvider serviceProvider;

        SpriteBatch spriteBatch;

        ResourceManager resourceManager = new ResourceManager();

        AssetManager assetManager;

        public GraphicsDevice GraphicsDevice { get; private set; }

        public SceneManager SceneManager { get; private set; }

        public SceneManager.Settings SceneManagerSettings { get; private set; }

        public RegionManager RegionManager { get; private set; }

        public ChunkPartitionManager PartitionManager { get; private set; }

        public GraphicsSettings GraphicsSettings { get; private set; }

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
            assetManager.RegisterLoader(typeof(GraphicsSettings), new GraphicsSettingsLoader());
            assetManager.RegisterLoader(typeof(SceneSettings), new SceneSettingsLoader());
        }

        public void Initialize()
        {
            //----------------------------------------------------------------
            // グラフィックス設定

            GraphicsSettings = LoadAsset<GraphicsSettings>("title:Resources/GraphicsSettings.json");

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

            SceneManager = new SceneManager(SceneManagerSettings, GraphicsDevice);

            // 太陽と月をディレクショナル ライトとして登録。
            SceneManager.DirectionalLights.Add(SceneSettings.Sunlight);
            SceneManager.DirectionalLights.Add(SceneSettings.Moonlight);

            // シャドウ マップ
            if (GraphicsSettings.ShadowMapEnabled)
            {
                var shadowMapEffect = LoadAsset<Effect>("content:Effects/ShadowMap");
                var blurEffect = LoadAsset<Effect>("content:Effects/GaussianBlur");

                ShadowMap = new ShadowMap(GraphicsDevice, GraphicsSettings.ShadowMap, spriteBatch, shadowMapEffect, blurEffect);

                SceneManager.ShadowMap = ShadowMap;
            }

            // レンズ フレア
            if (GraphicsSettings.LensFlareEnabled)
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
            if (GraphicsSettings.SssmEnabled)
            {
                // スクリーン スペース シャドウ マッピング モジュール
                var shadowSceneEffect = LoadAsset<Effect>("content:Effects/ShadowScene");
                var sssmEffect = LoadAsset<Effect>("content:Effects/Sssm");
                var blurEffect = LoadAsset<Effect>("content:Effects/GaussianBlur");

                Sssm = new Sssm(spriteBatch, GraphicsSettings.ShadowMap, GraphicsSettings.Sssm, shadowSceneEffect, sssmEffect, blurEffect);
                Sssm.ShadowColor = SceneSettings.ShadowColor;

                SceneManager.PostProcessors.Add(Sssm);

                // SSSM は直接的なシャドウ描画を回避しなければならないため明示。
                SceneManager.SssmEnabled = true;
            }

            // スクリーン スペース アンビエント オクルージョン
            if (GraphicsSettings.SsaoEnabled)
            {
                var normalDepthMapEffect = LoadAsset<Effect>("content:Effects/NormalDepthMap");
                var ssaoMapEffect = LoadAsset<Effect>("content:Effects/SsaoMap");
                var blurEffect = LoadAsset<Effect>("content:Effects/BilateralBlur");
                var ssaoEffect = LoadAsset<Effect>("content:Effects/Ssao");
                var randomNormalMap = LoadAsset<Texture2D>("content:Textures/RandomNormal");

                Ssao = new Ssao(spriteBatch, GraphicsSettings.Ssao,
                    normalDepthMapEffect, ssaoMapEffect, blurEffect, ssaoEffect, randomNormalMap);

                SceneManager.PostProcessors.Add(Ssao);
            }

            // エッジ強調
            if (GraphicsSettings.EdgeEnabled)
            {
                var normalDepthMapEffect = LoadAsset<Effect>("content:Effects/NormalDepthMap");
                var edgeEffect = LoadAsset<Effect>("content:Effects/Edge");

                Edge = new Edge(spriteBatch, GraphicsSettings.Edge, normalDepthMapEffect, edgeEffect);

                SceneManager.PostProcessors.Add(Edge);
            }

            // ブルーム
            if (GraphicsSettings.BloomEnabled)
            {
                var bloomExtractEffect = LoadAsset<Effect>("content:Effects/BloomExtract");
                var bloomEffect = LoadAsset<Effect>("content:Effects/Bloom");
                var blurEffect = LoadAsset<Effect>("content:Effects/GaussianBlur");

                Bloom = new Bloom(spriteBatch, GraphicsSettings.Bloom, bloomExtractEffect, bloomEffect, blurEffect);

                SceneManager.PostProcessors.Add(Bloom);
            }

            // 被写界深度
            if (GraphicsSettings.DofEnabled)
            {
                var depthMapEffect = LoadAsset<Effect>("content:Effects/DepthMap");
                var dofEffect = LoadAsset<Effect>("content:Effects/Dof");
                var blurEffect = LoadAsset<Effect>("content:Effects/GaussianBlur");

                Dof = new Dof(spriteBatch, GraphicsSettings.Dof, depthMapEffect, dofEffect, blurEffect);

                SceneManager.PostProcessors.Add(Dof);
            }

            // カラー オーバラップ
            if (GraphicsSettings.ColorOverlapEnabled)
            {
                ColorOverlap = new ColorOverlap(spriteBatch);

                SceneManager.PostProcessors.Add(ColorOverlap);
            }

            // モノクローム
            if (GraphicsSettings.MonochromeEnabled)
            {
                var monochromeEffect = LoadAsset<Effect>("content:Effects/Monochrome");

                Monochrome = new Monochrome(spriteBatch, monochromeEffect);

                SceneManager.PostProcessors.Add(Monochrome);
            }

            //----------------------------------------------------------------
            // リージョン マネージャ

            RegionManager = new RegionManager(serviceProvider, SceneManager);
            RegionManager.Initialize(SceneSettings);

            //----------------------------------------------------------------
            // パーティション マネージャ

            // TODO
            var partitionManagerSettings = new PartitionManager.Settings
            {
                PartitionSize = Chunk.Size.ToVector3(),
                MinLandscapeVolume = new DefaultLandscapeVolume(VectorI3.Zero, PartitionMinActiveRange),
                MaxLandscapeVolume = new DefaultLandscapeVolume(VectorI3.Zero, PartitionMaxActiveRange)
            };

            PartitionManager = new ChunkPartitionManager(partitionManagerSettings, RegionManager);
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

            //----------------------------------------------------------------
            // リージョン マネージャ

            RegionManager.Update(gameTime);
        }

        public void Draw(GameTime gameTime)
        {
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
