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

        GraphicsDeviceManager graphics;

        KeyboardState lastKeyboardState;

        FreeView view = new FreeView();

        PerspectiveFov projection = new PerspectiveFov();

        FreeViewInput viewInput = new FreeViewInput();

        float moveVelocity = 10;
        
        float dashFactor = 2;

        float farPlaneDistance = 150;

        RegionManager regionManager;

        ChunkPartitionManager partitionManager;

        RasterizerState defaultRasterizerState = new RasterizerState();

        Vector4 backgroundColor = Color.CornflowerBlue.ToVector4();

        Region region;

#if DEBUG

        string helpMessage =
            "[F1] Help\r\n" +
            "[F2] Chunk bounding box\r\n" +
            "[F3] Wireframe\r\n" +
            //"[F4] Light\r\n" +
            //"[F5] Fog\r\n" +
            //"[F9] Height color\r\n" +
            //"[F10] Normal\r\n" +
            //"[F11] White solid\r\n" +
            //"[F12] Texture\r\n" +
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

            view.Position = new Vector3(0, 16 * 17, 0);
            //view.Position = new Vector3(0, 16 * 3, 0);
            view.Yaw(MathHelper.Pi);
            view.Update();

            projection.FarPlaneDistance = farPlaneDistance;
            projection.Update();

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
            fpsCounter.Enabled = false;
            fpsCounter.Visible = false;
            Components.Add(fpsCounter);
        }

        protected override void LoadContent()
        {
            logger.Info("LoadContent");

            //================================================================
            // Region

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

            BuildInformationMessage();
            debugInformationTextFontSize = debugFont.MeasureString(stringBuilder);
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

            //================================================================
            // Debug

            DebugUpdate(gameTime);

            //================================================================
            // Keyboard State

            lastKeyboardState = keyboardState;

            base.Update(gameTime);
        }

        [Conditional("DEBUG")]
        void DebugUpdate(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();

            // F1
            if (keyboardState.IsKeyUp(Keys.F1) && lastKeyboardState.IsKeyDown(Keys.F1))
                helpVisible = !helpVisible;
            // F2
            if (keyboardState.IsKeyUp(Keys.F2) && lastKeyboardState.IsKeyDown(Keys.F2))
                ChunkManager.ChunkBoundingBoxVisible = !ChunkManager.ChunkBoundingBoxVisible;
            // F3
            if (keyboardState.IsKeyUp(Keys.F3) && lastKeyboardState.IsKeyDown(Keys.F3))
                ChunkManager.Wireframe = !ChunkManager.Wireframe;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, backgroundColor, 1, 0);
            GraphicsDevice.RasterizerState = defaultRasterizerState;
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            regionManager.Draw(view, projection);

            DrawHelp();

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

        [Conditional("DEBUG")]
        void BuildInformationMessage()
        {
            var sb = stringBuilder;

            sb.Length = 0;
            sb.Append("Screen: ");
            sb.AppendNumber(graphics.PreferredBackBufferWidth).Append('x');
            sb.AppendNumber(graphics.PreferredBackBufferHeight).AppendLine();

            sb.Append("FarPlane: ");
            sb.AppendNumber(farPlaneDistance).AppendLine();
            
            sb.Append("Partition: ");
            sb.Append("A(").AppendNumber(partitionManager.Monitor.ActivePartitionCount).Append(") ");
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

            sb.Append("VisibleChunk: ");
            sb.Append("O(").AppendNumber(region.Monitor.VisibleOpaqueChunkCount).Append(") ");
            sb.Append("T(").AppendNumber(region.Monitor.VisibleTranslucentChunkCount).Append(")").AppendLine();
            
            sb.Append("OccludedChunk: ");
            sb.Append("O(").AppendNumber(region.Monitor.OccludedOpaqueChunkCount).Append(") ").AppendLine();
            
            sb.Append("MoveVelocity: ");
            sb.AppendNumber(viewInput.MoveVelocity).AppendLine();
            
            sb.Append("Eye: ");
            sb.Append("P(");
            sb.AppendNumber(view.Position.X).Append(", ");
            sb.AppendNumber(view.Position.Y).Append(", ");
            sb.AppendNumber(view.Position.Z).Append(") ");
            sb.Append("D(");
            sb.AppendNumber(view.Forward.X).Append(", ");
            sb.AppendNumber(view.Forward.Y).Append(", ");
            sb.AppendNumber(view.Forward.Z).Append(")");
        }

        [Conditional("DEBUG")]
        void DrawHelp()
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
            BuildInformationMessage();
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
