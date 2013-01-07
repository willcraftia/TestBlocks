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

        public SceneManagerSettings SceneManagerSettings { get; private set; }

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
            // SceneSettings

            // TODO: ワールド設定としてどうするか再検討。
            // いずれにせよ、SceneSettings はワールド設定と一対一。

            var sceneSettingsResource = resourceManager.Load("title:Resources/SceneSettings.json");
            SceneSettings = assetManager.Load<SceneSettings>(sceneSettingsResource);

            //----------------------------------------------------------------
            // SceneManager

            // TODO: リソースから取得する。
            SceneManagerSettings = new SceneManagerSettings();

            // TODO: 暫定的に外部でカメラを設定する。
            SceneManager.Initialize(SceneManagerSettings);
            // 太陽と月を登録。
            SceneManager.AddDirectionalLight(SceneSettings.Sunlight);
            SceneManager.AddDirectionalLight(SceneSettings.Moonlight);

            //----------------------------------------------------------------
            // RegionManager

            RegionManager.Initialize(SceneSettings);

            //----------------------------------------------------------------
            // PartitionManager

            PartitionManager.Initialize(partitionMinActiveRange, partitionMaxActiveRange);
        }

        // TODO: 戻り値を Region にしない。
        public Region Load(string worldUri)
        {
            //----------------------------------------------------------------
            // RegionManager

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
            // PartitionManager

            PartitionManager.Update(ref cameraPosition);

            //----------------------------------------------------------------
            // SceneSettings

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
            // RegionManager

            RegionManager.Update(gameTime);
        }

        public void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1, 0);
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            //----------------------------------------------------------------
            // RegionManager

            // チャンク エフェクトを更新。
            RegionManager.UpdateChunkEffect();

            //----------------------------------------------------------------
            // SceneManager

            SceneManager.BackgroundColor = SceneSettings.SkyColor;
            SceneManager.Draw();
        }
    }
}
