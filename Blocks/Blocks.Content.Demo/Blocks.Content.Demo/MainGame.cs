#region Using

using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Diagnostics;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Framework.Serialization;
using Willcraftia.Xna.Framework.Serialization.Json;
using Willcraftia.Xna.Blocks.Landscape;
using Willcraftia.Xna.Blocks.Models;

#endregion

namespace Willcraftia.Xna.Blocks.Content.Demo
{
    public class MainGame : Game
    {
        static readonly Logger logger = new Logger(typeof(MainGame).Name);

        GraphicsDeviceManager graphics;

        FreeView view = new FreeView();

        PerspectiveFov projection = new PerspectiveFov();

        FreeViewInput viewInput = new FreeViewInput();

        float moveVelocity = 10;
        
        float dashFactor = 2;

        float farPlaneDistance = 200;

        RegionManager regionManager;

        ChunkPartitionManager partitionManager;

        RasterizerState defaultRasterizerState = new RasterizerState();

        Vector4 backgroundColor = Color.CornflowerBlue.ToVector4();

        public MainGame()
        {
            //================================================================
            // GraphicsDeviceManager

            graphics = new GraphicsDeviceManager(this);
            //graphics.PreferMultiSampling = true;

            //================================================================
            // Logging

            //FileTraceListenerManager.Add(@"Logs\App.log", false);
            logger.InfoGameStarted();

            Thread.GetDomain().UnhandledException += new UnhandledExceptionEventHandler(OnUnhandledException);
        }

        void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            logger.Fatal(e.ExceptionObject as Exception, "UnhandledException");
        }

        protected override void Initialize()
        {
            //================================================================
            // Logging
            logger.Info("Initialize");

            EnvironmentLog.Info();
            GraphicsAdapterLog.Info();

            //================================================================
            // StorageManager

            StorageManager.SelectStorageContainer("Blocks.Demo.MainGame");

            //================================================================
            // ResourceLoader

            ResourceLoader.Register(ContentResourceLoader.Instance);
            ResourceLoader.Register(TitleResourceLoader.Instance);
            ResourceLoader.Register(StorageResourceLoader.Instance);
            ResourceLoader.Register(FileResourceLoader.Instance);

            //================================================================
            // RegionManager

            regionManager = new RegionManager(Services);

            //================================================================
            // ChunkPartitionManager

            partitionManager = new ChunkPartitionManager(regionManager);
            //partitionManager.TaskQueueSlotCount = 20;

            //================================================================
            // Camera Settings

            var viewport = GraphicsDevice.Viewport;
            viewInput.InitialMousePositionX = viewport.Width / 2;
            viewInput.InitialMousePositionY = viewport.Height / 2;
            viewInput.FreeView = view;
            viewInput.MoveVelocity = moveVelocity;
            viewInput.DashFactor = dashFactor;

            view.Position = new Vector3(0, 16 * 16, 0);
            //view.Position = new Vector3(0, 16 * 3, 0);
            view.Yaw(MathHelper.Pi);
            view.Update();

            projection.FarPlaneDistance = farPlaneDistance;
            projection.Update();

            //================================================================
            // Default RasterizerState

            defaultRasterizerState.CullMode = CullMode.CullCounterClockwiseFace;
            defaultRasterizerState.FillMode = FillMode.Solid;

#if DEBUG
            //================================================================
            // DEBUG: FpsCounter

            var fpsCounter = new FpsCounter(this);
            fpsCounter.Content.RootDirectory = "Content";
            fpsCounter.HorizontalAlignment = DebugHorizontalAlignment.Right;
            fpsCounter.SampleSpan = TimeSpan.FromSeconds(2);
            //fpsCounter.Enabled = false;
            //fpsCounter.Visible = false;
            Components.Add(fpsCounter);
#endif

            //================================================================
            // Others

            base.Initialize();
        }

        protected override void LoadContent()
        {
            logger.Info("LoadContent");

            //================================================================
            // Region

            regionManager.LoadRegion("title:Resources/DefaultRegion.json");
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (!IsActive) return;

            var keyboardState = Keyboard.GetState();

            //================================================================
            // Exit

            if (!partitionManager.Closing && !partitionManager.Closed && keyboardState.IsKeyDown(Keys.Escape))
            {
                partitionManager.Close();

                logger.Info("Wait until all partitions are passivated...");
            }

            if (partitionManager.Closed)
                Exit();

            //================================================================
            // ViewInput

            viewInput.Update(gameTime);

            if (keyboardState.IsKeyDown(Keys.PageUp))
                viewInput.MoveVelocity += 10;
            if (keyboardState.IsKeyDown(Keys.PageDown))
            {
                viewInput.MoveVelocity -= 10;
                if (viewInput.MoveVelocity < 10) viewInput.MoveVelocity = 10;
            }

            //================================================================
            // View/Projection

            view.Update();
            projection.Update();

            //================================================================
            // PartitionManager

            var eyePosition = view.Position;
            partitionManager.Update(ref eyePosition);

            //================================================================
            // RegionManager

            regionManager.Update();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, backgroundColor, 1, 0);
            GraphicsDevice.RasterizerState = defaultRasterizerState;
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            if (!partitionManager.Closing && !partitionManager.Closed)
                regionManager.Draw(view, projection);

            base.Draw(gameTime);
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            logger.InfoGameExited();

            base.OnExiting(sender, args);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                FileTraceListenerManager.Clear();

            base.Dispose(disposing);
        }
    }
}
