#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Content;
using Willcraftia.Xna.Framework.Diagnostics;
using Willcraftia.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Framework.Landscape;
using Willcraftia.Xna.Blocks.Content;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class WorldManager
    {
        public const string MonitorUpdate = "WorldManager.Update";

        IServiceProvider serviceProvider;

        SpriteBatch spriteBatch;

        ResourceManager resourceManager = new ResourceManager();

        AssetManager assetManager;

        BasicCamera defaultCamera = new BasicCamera("Default");

        public GraphicsDevice GraphicsDevice { get; private set; }

        public SceneManager SceneManager { get; private set; }

        public SceneManager.Settings SceneManagerSettings { get; private set; }

        public ChunkManager ChunkManager { get; private set; }

        public RegionManager RegionManager { get; private set; }

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

        public Scanline Scanline { get; private set; }

        public WorldManager(IServiceProvider serviceProvider, GraphicsDevice graphicsDevice)
        {
            if (serviceProvider == null) throw new ArgumentNullException("serviceProvider");
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");

            this.serviceProvider = serviceProvider;
            GraphicsDevice= graphicsDevice;

            spriteBatch = new SpriteBatch(graphicsDevice);
            assetManager = new AssetManager(serviceProvider);
            assetManager.RegisterLoader(typeof(GraphicsSettings), new GraphicsSettingsLoader());
            assetManager.RegisterLoader(typeof(LandscapeSettings), new LandscapeSettingsLoader());
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
                var blurEffect = LoadAsset<Effect>("content:Effects/SsaoMapBlur");
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

            // 走査線
            if (GraphicsSettings.ScanlineEnabled)
            {
                var effect = LoadAsset<Effect>("content:Effects/Scanline");

                Scanline = new Scanline(spriteBatch, effect);

                SceneManager.PostProcessors.Add(Scanline);
            }

            //----------------------------------------------------------------
            // リージョン マネージャ

            RegionManager = new RegionManager(serviceProvider, SceneManager);
            RegionManager.Initialize(SceneSettings);

            // イベント ハンドラ
            // シャドウ マップ更新にあわせて、リージョン マネージャで管理しているエフェクトを準備する。
            SceneManager.ShadowMapUpdated += RegionManager.OnShadowMapUpdated;

            //----------------------------------------------------------------
            // チャンク マネージャ

            var landscapeSettings = LoadAsset<LandscapeSettings>("title:Resources/LandscapeSettings.json");

            ChunkManager = new ChunkManager(landscapeSettings.PartitionManager, GraphicsDevice, RegionManager, SceneManager);

            //----------------------------------------------------------------
            // デフォルト カメラ

            //camera.View.Position = new Vector3(0, 16 * 18, 0);
            defaultCamera.View.Position = new Vector3(0, 16 * 16, 0);
            //camera.View.Position = new Vector3(0, 16 * 3, 0);
            //camera.View.Position = new Vector3(0, 16 * 2, 0);
            defaultCamera.Projection.AspectRatio = GraphicsDevice.Viewport.AspectRatio;

            // 最小アクティブ範囲を超えない位置へ FarPlaneDistance を設定。
            // パーティション (チャンク) のサイズを掛けておく。
            defaultCamera.Projection.FarPlaneDistance = (landscapeSettings.MinActiveRange - 1) * 16;

            // 念のためここで一度更新。
            defaultCamera.Update();

            // シーン マネージャへ登録してアクティブ化。
            SceneManager.Cameras.Add(defaultCamera);
            SceneManager.ActiveCameraName = defaultCamera.Name;
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
            Monitor.Begin(MonitorUpdate);

            //----------------------------------------------------------------
            // カメラ更新

            SceneManager.ActiveCamera.Update();

            //----------------------------------------------------------------
            // シーン設定

            SceneSettings.Update(gameTime);

            SceneManager.AmbientLightColor = SceneSettings.CurrentAmbientLightColor;

            if (SceneSettings.Sunlight.Enabled && SceneSettings.SunAboveHorizon)
            {
                SceneManager.ActiveDirectionalLightName = SceneSettings.Sunlight.Name;
            }
            else if (SceneSettings.Moonlight.Enabled && SceneSettings.MoonAboveHorizon)
            {
                SceneManager.ActiveDirectionalLightName = SceneSettings.Moonlight.Name;
            }
            else
            {
                SceneManager.ActiveDirectionalLightName = null;
            }

            if (SceneSettings.FogEnabled)
            {
                var currentFarPlaneDistance = SceneManager.ActiveCamera.Projection.FarPlaneDistance;
                SceneManager.FogStart = currentFarPlaneDistance * SceneSettings.FogStartScale;
                SceneManager.FogEnd = currentFarPlaneDistance * SceneSettings.FogEndScale;
                SceneManager.FogColor = SceneSettings.CurrentSkyColor;
            }
            SceneManager.FogEnabled = SceneSettings.FogEnabled;

            // 太陽が見える場合にのみレンズ フレアを描画。
            LensFlare.Enabled = SceneSettings.SunAboveHorizon;

            //----------------------------------------------------------------
            // チャンク マネージャ

            ChunkManager.Update(SceneManager.ActiveCamera.View.Position);

            //----------------------------------------------------------------
            // リージョン マネージャ

            RegionManager.Update(gameTime);

            Monitor.End(MonitorUpdate);
        }

        public void Draw(GameTime gameTime)
        {
            //----------------------------------------------------------------
            // リージョン マネージャ

            // チャンク エフェクトを更新。
            RegionManager.PrepareChunkEffects();

            //----------------------------------------------------------------
            // シーン マネージャ

            SceneManager.BackgroundColor = SceneSettings.CurrentSkyColor;
            SceneManager.Draw(gameTime);
        }

        T LoadAsset<T>(string uri)
        {
            var resource = resourceManager.Load(uri);
            return assetManager.Load<T>(resource);
        }
    }
}
