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

        ResourceManager resourceManager = new ResourceManager();

        AssetManager assetManager;

        SceneModuleFactory sceneModuleFactory;

        public GraphicsDevice GraphicsDevice { get; private set; }

        public SceneManager SceneManager { get; private set; }

        public SceneManager.Settings SceneManagerSettings { get; private set; }

        public RegionManager RegionManager { get; private set; }

        public ChunkPartitionManager PartitionManager { get; private set; }

        public SceneSettings SceneSettings { get; private set; }

        public WorldManager(IServiceProvider serviceProvider, GraphicsDevice graphicsDevice)
        {
            if (serviceProvider == null) throw new ArgumentNullException("serviceProvider");
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");

            this.serviceProvider = serviceProvider;
            GraphicsDevice= graphicsDevice;

            assetManager = new AssetManager(serviceProvider);
            assetManager.RegisterLoader(typeof(SceneSettings), new SceneSettingsLoader());
            sceneModuleFactory = new SceneModuleFactory(resourceManager, assetManager);
            SceneManager = new SceneManager(GraphicsDevice, sceneModuleFactory);
            RegionManager = new RegionManager(serviceProvider, SceneManager);
            PartitionManager = new ChunkPartitionManager(RegionManager);
        }

        public void Initialize()
        {
            //----------------------------------------------------------------
            // シーン設定

            // TODO: ワールド設定としてどうするか再検討。
            // いずれにせよ、SceneSettings はワールド設定と一対一。

            var sceneSettingsResource = resourceManager.Load("title:Resources/SceneSettings.json");
            SceneSettings = assetManager.Load<SceneSettings>(sceneSettingsResource);

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
    }
}
