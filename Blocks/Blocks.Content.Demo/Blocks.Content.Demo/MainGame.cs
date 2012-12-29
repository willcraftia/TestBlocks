#region Using

using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Diagnostics;
using Willcraftia.Xna.Framework.Graphics;
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

        const int partitionMinActiveRange = 13;

        const int partitionMaxActiveRange = 15;

        GraphicsDeviceManager graphics;

        KeyboardState lastKeyboardState;

        FreeCamera camera = new FreeCamera("Default");

        FreeViewInput viewInput = new FreeViewInput();

        float moveVelocity = 10;
        
        float dashFactor = 2;

        float farPlaneDistance = (partitionMinActiveRange - 1) * 16;

        SceneManager sceneManager;

        RegionManager regionManager;

        ChunkPartitionManager partitionManager;

        RasterizerState defaultRasterizerState = new RasterizerState();

        Region region;

#if DEBUG

        TimeRuler timeRuler;

        TimeRulerMarker updateMarker;

        TimeRulerMarker drawMarker;

        string helpMessage =
            "[F1] Help\r\n" +
            "[F2] Chunk bounding box\r\n" +
            "[F3] Wireframe\r\n" +
            "[F4] Fog\r\n" +
            "[w][s][a][d][q][z] Movement\r\n" +
            "[Mouse] Camera orientation\r\n" +
            "[PageUp][PageDown] Move velocity";

        Vector2 debugHelpMessageFontSize;

        Vector2 debugInformationTextFontSize;

        bool helpVisible = true;

        StringBuilder stringBuilder = new StringBuilder();

        SpriteBatch debugSpriteBatch;

        SpriteFont debugFont;

        Texture2D debugFillTexture;

#endif

        public MainGame()
        {
            //================================================================
            // GraphicsDeviceManager

            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
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
            // SceneManager

            sceneManager = new SceneManager(GraphicsDevice);
            sceneManager.AddCamera(camera);
            sceneManager.ActiveCameraName = camera.Name;

            //================================================================
            // RegionManager

            regionManager = new RegionManager(Services, sceneManager);

            //================================================================
            // ChunkPartitionManager

            partitionManager = new ChunkPartitionManager(regionManager);
            partitionManager.Initialize(partitionMinActiveRange, partitionMaxActiveRange);

            //================================================================
            // Camera Settings

            var viewport = GraphicsDevice.Viewport;
            viewInput.InitialMousePositionX = viewport.Width / 2;
            viewInput.InitialMousePositionY = viewport.Height / 2;
            viewInput.FreeView = camera.FreeView;
            viewInput.MoveVelocity = moveVelocity;
            viewInput.DashFactor = dashFactor;

            //camera.FreeView.Position = new Vector3(0, 16 * 18, 0);
            camera.FreeView.Position = new Vector3(0, 16 * 16, 0);
            //camera.FreeView.Position = new Vector3(0, 16 * 3, 0);
            //camera.FreeView.Position = new Vector3(0, 16 * 2, 0);
            camera.FreeView.Yaw(MathHelper.Pi);
            camera.Projection.FarPlaneDistance = farPlaneDistance;

            camera.Update();

            //================================================================
            // Default RasterizerState

            defaultRasterizerState.CullMode = CullMode.CullCounterClockwiseFace;
            defaultRasterizerState.FillMode = FillMode.Solid;

            //================================================================
            // Debug

            DebugInitialize();

            //================================================================
            // Others

            base.Initialize();
        }

        [Conditional("DEBUG")]
        void DebugInitialize()
        {
            //================================================================
            // FpsCounter

            var fpsCounter = new FpsCounter(this);
            fpsCounter.Content.RootDirectory = "Content";
            fpsCounter.HorizontalAlignment = DebugHorizontalAlignment.Right;
            fpsCounter.SampleSpan = TimeSpan.FromSeconds(2);
            //fpsCounter.Enabled = false;
            //fpsCounter.Visible = false;
            Components.Add(fpsCounter);

            //================================================================
            // TimeRuler

            timeRuler = new TimeRuler(this);
            //timeRuler.Enabled = false;
            //timeRuler.Visible = false;
            Components.Add(timeRuler);

            updateMarker = timeRuler.CreateMarker();
            updateMarker.Name = "Draw";
            updateMarker.BarIndex = 0;
            updateMarker.Color = Color.Cyan;

            drawMarker = timeRuler.CreateMarker();
            drawMarker.Name = "Draw";
            drawMarker.BarIndex = 1;
            drawMarker.Color = Color.Yellow;
        }

        protected override void LoadContent()
        {
            logger.Info("LoadContent");

            //================================================================
            // Region

            regionManager.LoadGrobalSettings();

            region = regionManager.LoadRegion("title:Resources/DefaultRegion.json");

            //================================================================
            // Debug

            DebugLoadContent();
        }

        [Conditional("DEBUG")]
        void DebugLoadContent()
        {
            Content.RootDirectory = "Content";

            debugSpriteBatch = new SpriteBatch(GraphicsDevice);
            debugFont = Content.Load<SpriteFont>("Fonts/Debug");
            debugFillTexture = Texture2DHelper.CreateFillTexture(GraphicsDevice);
            debugHelpMessageFontSize = debugFont.MeasureString(helpMessage);

            DebugBuildInfoMessage();
            debugInformationTextFontSize = debugFont.MeasureString(stringBuilder);
        }

        protected override void UnloadContent()
        {
            //================================================================
            // Debug

            DebugUnloadContent();
        }

        [Conditional("DEBUG")]
        void DebugUnloadContent()
        {
            timeRuler.ReleaseMarker(updateMarker);
            timeRuler.ReleaseMarker(drawMarker);

            debugSpriteBatch.Dispose();
            debugFillTexture.Dispose();
        }

        protected override void Update(GameTime gameTime)
        {
            if (!IsActive) return;

            //================================================================
            // TimeRuler (DEBUG)

            DebugBeginUpdate();

            //================================================================
            // Keyboard State

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
            // Camera

            camera.Update();

            //================================================================
            // PartitionManager

            var eyePosition = camera.FreeView.Position;
            partitionManager.Update(ref eyePosition);

            //================================================================
            // RegionManager

            regionManager.Update(gameTime);

            //================================================================
            // Debug

            DebugUpdate(gameTime, ref keyboardState);

            //================================================================
            // Keyboard State

            lastKeyboardState = keyboardState;

            base.Update(gameTime);

            //================================================================
            // TimeRuler (DEBUG)

            DebugEndUpdate();
        }

        [Conditional("DEBUG")]
        void DebugBeginUpdate()
        {
            timeRuler.StartFrame();
            updateMarker.Begin();
        }

        [Conditional("DEBUG")]
        void DebugEndUpdate()
        {
            updateMarker.End();
        }

        [Conditional("DEBUG")]
        void DebugUpdate(GameTime gameTime, ref KeyboardState keyboardState)
        {
            // F1
            if (keyboardState.IsKeyUp(Keys.F1) && lastKeyboardState.IsKeyDown(Keys.F1))
                helpVisible = !helpVisible;
            // F2
            if (keyboardState.IsKeyUp(Keys.F2) && lastKeyboardState.IsKeyDown(Keys.F2))
                SceneManager.DebugBoundingBoxVisible = !SceneManager.DebugBoundingBoxVisible;
            // F3
            if (keyboardState.IsKeyUp(Keys.F3) && lastKeyboardState.IsKeyDown(Keys.F3))
                RegionManager.DebugWireframe = !RegionManager.DebugWireframe;
            // F4
            if (keyboardState.IsKeyUp(Keys.F4) && lastKeyboardState.IsKeyDown(Keys.F4))
                GlobalSceneSettings.FogEnabled = !GlobalSceneSettings.FogEnabled;
        }

        protected override void Draw(GameTime gameTime)
        {
            //================================================================
            // TimeRuler (DEBUG)

            DebugBeginDraw();

            //================================================================
            // SceneManager

            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1, 0);
            GraphicsDevice.RasterizerState = defaultRasterizerState;
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            // チャンク エフェクトを更新。
            regionManager.UpdateChunkEffect();

            sceneManager.Draw();

            //================================================================
            // Help HUD (DEBUG)

            DebugDrawHelp();

            //================================================================
            // TimeRuler (DEBUG)

            DebugEndDraw();

            base.Draw(gameTime);
        }

        [Conditional("DEBUG")]
        void DebugBeginDraw()
        {
            if (IsActive) drawMarker.Begin();
        }

        [Conditional("DEBUG")]
        void DebugEndDraw()
        {
            if (IsActive) drawMarker.End();
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

        [Conditional("DEBUG")]
        void DebugBuildInfoMessage()
        {
            var sb = stringBuilder;

            sb.Length = 0;
            sb.Append("Screen: ");
            sb.AppendNumber(graphics.PreferredBackBufferWidth).Append('x');
            sb.AppendNumber(graphics.PreferredBackBufferHeight).AppendLine();

            sb.Append("FarPlane: ");
            sb.AppendNumber(farPlaneDistance).AppendLine();
            
            sb.Append("Partition: ");
            sb.Append("A(").AppendNumber(partitionManager.Monitor.ActiveClusterCount).Append(":");
            sb.AppendNumber(partitionManager.Monitor.ActivePartitionCount).Append(") ");
            sb.Append("W(").AppendNumber(partitionManager.Monitor.ActivatingPartitionCount).Append(") ");
            sb.Append("P(").AppendNumber(partitionManager.Monitor.PassivatingPartitionCount).Append(")").AppendLine();
            
            sb.Append("Chunk: ").AppendNumber(region.Monitor.ActiveChunkCount).Append("/");
            sb.AppendNumber(region.Monitor.TotalChunkCount).Append(" ");
            sb.Append("Mesh: ").AppendNumber(region.Monitor.ActiveChunkMeshCount).Append("/");
            sb.AppendNumber(region.Monitor.TotalChunkMeshCount).Append(" ");
            sb.Append("InterMesh: ").AppendNumber(region.Monitor.ActiveInterChunkMeshCount).Append("/");
            sb.AppendNumber(region.Monitor.TotalInterChunkMeshCount).AppendLine();
            
            sb.Append("VertexBuffer(IndexBuffer): ").AppendNumber(region.Monitor.ActiveVertexBufferCount).Append("/");
            sb.AppendNumber(region.Monitor.TotalVertexBufferCount).AppendLine();

            sb.Append("UpdatingChunk: ").AppendNumber(region.Monitor.UpdatingChunkCount).AppendLine();

            sb.Append("ChunkVertex: ");
            sb.Append("Max(").AppendNumber(region.Monitor.MaxChunkVertexCount).Append(") ");
            sb.Append("Total(").AppendNumber(region.Monitor.TotalChunkVertexCount).Append(")").AppendLine();

            sb.Append("ChunkIndex: ");
            sb.Append("Max(").AppendNumber(region.Monitor.MaxChunkIndexCount).Append(") ");
            sb.Append("Total(").AppendNumber(region.Monitor.TotalChunkIndexCount).Append(")").AppendLine();

            sb.Append("SceneObejcts: ").AppendNumber(sceneManager.DebugRenderedSceneObjectCount).Append("/");
            sb.AppendNumber(sceneManager.DebugVisibleSceneObjectCount).Append("/");
            sb.AppendNumber(sceneManager.DebugTotalSceneObjectCount).AppendLine();
            
            sb.Append("MoveVelocity: ");
            sb.AppendNumber(viewInput.MoveVelocity).AppendLine();
            
            sb.Append("Eye: ");
            sb.Append("P(");
            sb.AppendNumber(camera.Position.X).Append(", ");
            sb.AppendNumber(camera.Position.Y).Append(", ");
            sb.AppendNumber(camera.Position.Z).Append(") ");
            sb.Append("D(");
            sb.AppendNumber(camera.Forward.X).Append(", ");
            sb.AppendNumber(camera.Forward.Y).Append(", ");
            sb.AppendNumber(camera.Forward.Z).Append(")");
        }

        [Conditional("DEBUG")]
        void DebugDrawHelp()
        {
            if (!helpVisible) return;

            debugSpriteBatch.Begin();

            var layout = new DebugLayout();

            int informationWidth = 380;

            // calculate the background area for information.
            layout.ContainerBounds = GraphicsDevice.Viewport.TitleSafeArea;
            layout.Width = informationWidth + 4;
            layout.Height = (int) debugInformationTextFontSize.Y + 2;
            layout.HorizontalMargin = 8;
            layout.VerticalMargin = 8;
            layout.HorizontalAlignment = DebugHorizontalAlignment.Left;
            layout.VerticalAlignment = DebugVerticalAlignment.Top;
            layout.Arrange();
            // draw the rectangle.
            debugSpriteBatch.Draw(debugFillTexture, layout.ArrangedBounds, Color.Black * 0.5f);

            // calculate the text area for help messages.
            layout.ContainerBounds = layout.ArrangedBounds;
            layout.Width = informationWidth;
            layout.Height = (int) debugInformationTextFontSize.Y;
            layout.HorizontalMargin = 2;
            layout.VerticalMargin = 0;
            layout.HorizontalAlignment = DebugHorizontalAlignment.Center;
            layout.VerticalAlignment = DebugVerticalAlignment.Center;
            layout.Arrange();
            // draw the text.
            DebugBuildInfoMessage();
            debugSpriteBatch.DrawString(debugFont, stringBuilder, new Vector2(layout.ArrangedBounds.X, layout.ArrangedBounds.Y), Color.Yellow);

            // calculate the background area for help messages.
            layout.ContainerBounds = GraphicsDevice.Viewport.TitleSafeArea;
            layout.Width = (int) debugHelpMessageFontSize.X + 4;
            layout.Height = (int) debugHelpMessageFontSize.Y + 2;
            layout.HorizontalMargin = 8;
            layout.VerticalMargin = 8;
            layout.HorizontalAlignment = DebugHorizontalAlignment.Left;
            layout.VerticalAlignment = DebugVerticalAlignment.Bottom;
            layout.Arrange();
            // draw the rectangle.
            debugSpriteBatch.Draw(debugFillTexture, layout.ArrangedBounds, Color.Black * 0.5f);

            // calculate the text area for help messages.
            layout.ContainerBounds = layout.ArrangedBounds;
            layout.Width = (int) debugHelpMessageFontSize.X;
            layout.Height = (int) debugHelpMessageFontSize.Y;
            layout.HorizontalMargin = 2;
            layout.VerticalMargin = 0;
            layout.HorizontalAlignment = DebugHorizontalAlignment.Center;
            layout.VerticalAlignment = DebugVerticalAlignment.Center;
            layout.Arrange();
            // draw the text.
            debugSpriteBatch.DrawString(debugFont, helpMessage, new Vector2(layout.ArrangedBounds.X, layout.ArrangedBounds.Y), Color.Yellow);

            debugSpriteBatch.End();
        }
    }
}
