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
using Willcraftia.Xna.Blocks.Models;

#endregion

namespace Willcraftia.Xna.Blocks.Content.Demo
{
    public class MainGame : Game
    {
        static readonly Logger logger = new Logger(typeof(MainGame).Name);

        GraphicsDeviceManager graphics;

        KeyboardState lastKeyboardState;

        FreeViewInput viewInput = new FreeViewInput();

        float moveVelocity = 10;
        
        float dashFactor = 2;

        WorldManager worldManager;

        Region region;

        #region Trace

        TimeRuler timeRuler;

        TimeRulerMarker updateMarker;

        TimeRulerMarker drawMarker;

        TimeRulerMarker partitionManagerUpdateMarker;

        TimeRulerMarker partitionManagerCheckPassivationCompletedMarker;

        TimeRulerMarker partitionManagerCheckActivationCompletedMarker;

        TimeRulerMarker partitionManagerPassivatePartitionsMarker;

        TimeRulerMarker partitionManagerActivatePartitionsMarker;

        TimeRulerMarker regionUpdateMarker;

        TimeRulerMarker classifySceneObjectsMarker;

        TimeRulerMarker drawShadowMapMarker;

        TimeRulerMarker drawSceneMarker;

        TimeRulerMarker drawSceneOcclusionQueryMarker;

        TimeRulerMarker drawSceneRenderingMarker;

        TimeRulerMarker ssaoProcessMarker;

        TimeRulerMarker dofProcessMarker;

        string helpMessage =
            "[F1] Help\r\n" +
            "[F2] Chunk bounding box\r\n" +
            "[F3] Wireframe\r\n" +
            "[F4] Fog\r\n" +
            "[F5] InterMap\r\n" +
            "[w][s][a][d][q][z] Movement\r\n" +
            "[Mouse] Camera orientation\r\n" +
            "[PageUp][PageDown] Move velocity";

        Vector2 helpMessageFontSize;

        Vector2 informationTextFontSize;

        bool helpVisible = true;

        StringBuilder stringBuilder = new StringBuilder();

        SpriteBatch spriteBatch;

        SpriteFont font;

        Texture2D fillTexture;

        #endregion

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
            #region Logging

            //================================================================
            // Logging

            logger.Info("Initialize");

            EnvironmentLog.Info();
            GraphicsAdapterLog.Info();

            #endregion

            #region FpsCounter

            //================================================================
            // FpsCounter

            var fpsCounter = new FpsCounter(this);
            fpsCounter.Content.RootDirectory = "Content";
            fpsCounter.HorizontalAlignment = DebugHorizontalAlignment.Right;
            fpsCounter.SampleSpan = TimeSpan.FromSeconds(2);
            //fpsCounter.Enabled = false;
            //fpsCounter.Visible = false;
            Components.Add(fpsCounter);

            #endregion

            #region TimeRuler

            //================================================================
            // TimeRuler

            timeRuler = new TimeRuler(this);
            timeRuler.BackgroundColor = Color.Black;
            //timeRuler.Enabled = false;
            //timeRuler.Visible = false;
            Components.Add(timeRuler);

            updateMarker = timeRuler.CreateMarker();
            updateMarker.Name = "Update";
            updateMarker.BarIndex = 0;
            updateMarker.Color = Color.White;

            partitionManagerUpdateMarker = timeRuler.CreateMarker();
            partitionManagerUpdateMarker.Name = "PartitionManagerUpdate";
            partitionManagerUpdateMarker.BarIndex = 1;
            partitionManagerUpdateMarker.Color = Color.Cyan;

            partitionManagerCheckPassivationCompletedMarker = timeRuler.CreateMarker();
            partitionManagerCheckPassivationCompletedMarker.Name = "PartitionManagerCheckPassivationCompleted";
            partitionManagerCheckPassivationCompletedMarker.BarIndex = 1;
            partitionManagerCheckPassivationCompletedMarker.Color = Color.LawnGreen;

            partitionManagerCheckActivationCompletedMarker = timeRuler.CreateMarker();
            partitionManagerCheckActivationCompletedMarker.Name = "PartitionManagerCheckActivationCompleted";
            partitionManagerCheckActivationCompletedMarker.BarIndex = 1;
            partitionManagerCheckActivationCompletedMarker.Color = Color.Green;

            partitionManagerPassivatePartitionsMarker = timeRuler.CreateMarker();
            partitionManagerPassivatePartitionsMarker.Name = "PartitionManagerPassivatePartitions";
            partitionManagerPassivatePartitionsMarker.BarIndex = 1;
            partitionManagerPassivatePartitionsMarker.Color = Color.Yellow;

            partitionManagerActivatePartitionsMarker = timeRuler.CreateMarker();
            partitionManagerActivatePartitionsMarker.Name = "PartitionManagerActivatePartitions";
            partitionManagerActivatePartitionsMarker.BarIndex = 1;
            partitionManagerActivatePartitionsMarker.Color = Color.Orange;

            regionUpdateMarker = timeRuler.CreateMarker();
            regionUpdateMarker.Name = "RegionManagerUpdate";
            regionUpdateMarker.BarIndex = 1;
            regionUpdateMarker.Color = Color.Blue;

            drawMarker = timeRuler.CreateMarker();
            drawMarker.Name = "Draw";
            drawMarker.BarIndex = 2;
            drawMarker.Color = Color.White;

            classifySceneObjectsMarker = timeRuler.CreateMarker();
            classifySceneObjectsMarker.Name = "DrawShadowMapMarker";
            classifySceneObjectsMarker.BarIndex = 3;
            classifySceneObjectsMarker.Color = Color.Cyan;

            drawShadowMapMarker = timeRuler.CreateMarker();
            drawShadowMapMarker.Name = "DrawShadowMapMarker";
            drawShadowMapMarker.BarIndex = 3;
            drawShadowMapMarker.Color = Color.LawnGreen;

            drawSceneMarker = timeRuler.CreateMarker();
            drawSceneMarker.Name = "DrawScene";
            drawSceneMarker.BarIndex = 3;
            drawSceneMarker.Color = Color.Green;

            drawSceneOcclusionQueryMarker = timeRuler.CreateMarker();
            drawSceneOcclusionQueryMarker.Name = "DrawSceneOcclusionQuery";
            drawSceneOcclusionQueryMarker.BarIndex = 3;
            drawSceneOcclusionQueryMarker.Color = Color.Yellow;

            drawSceneRenderingMarker = timeRuler.CreateMarker();
            drawSceneRenderingMarker.Name = "DrawSceneRendering";
            drawSceneRenderingMarker.BarIndex = 3;
            drawSceneRenderingMarker.Color = Color.Orange;

            ssaoProcessMarker = timeRuler.CreateMarker();
            ssaoProcessMarker.Name = "SsaoProcessMarker";
            ssaoProcessMarker.BarIndex = 4;
            ssaoProcessMarker.Color = Color.Cyan;

            dofProcessMarker = timeRuler.CreateMarker();
            dofProcessMarker.Name = "DofProcessMarker";
            dofProcessMarker.BarIndex = 4;
            dofProcessMarker.Color = Color.LawnGreen;

            #endregion

            #region DebugMapDisplay

            //================================================================
            // DebugMapDisplay

            var debugMapDisplay = new DebugMapDisplay(this);
            debugMapDisplay.Visible = false;
            Components.Add(debugMapDisplay);

            #endregion

            base.Initialize();
        }

        protected override void LoadContent()
        {
            logger.Info("LoadContent");

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
            // Camera

            var viewport = GraphicsDevice.Viewport;
            viewInput.InitialMousePositionX = viewport.Width / 2;
            viewInput.InitialMousePositionY = viewport.Height / 2;
            viewInput.MoveVelocity = moveVelocity;
            viewInput.DashFactor = dashFactor;
            viewInput.Yaw(MathHelper.Pi);

            //================================================================
            // WorldManager

            worldManager = new WorldManager(Services, GraphicsDevice);
            worldManager.Initialize();

            // TODO: 暫定

            #region Monitor

            //----------------------------------------------------------------
            // シーン マネージャ

            worldManager.SceneManager.Monitor.BeginClassifySceneObjects += OnBeginClassifySceneObjects;
            worldManager.SceneManager.Monitor.EndClassifySceneObjects += OnEndClassifySceneObjects;
            worldManager.SceneManager.Monitor.BeginDrawShadowMap += OnBeginDrawShadowMap;
            worldManager.SceneManager.Monitor.EndDrawShadowMap += OnEndDrawShadowMap;
            worldManager.SceneManager.Monitor.BeginDrawScene += OnBeginDrawScene;
            worldManager.SceneManager.Monitor.EndDrawScene += OnEndDrawScene;
            worldManager.SceneManager.Monitor.BeginDrawSceneOcclusionQuery += OnBeginDrawSceneOcclusionQuery;
            worldManager.SceneManager.Monitor.EndDrawSceneOcclusionQuery += OnEndDrawSceneOcclusionQuery;
            worldManager.SceneManager.Monitor.BeginDrawSceneRendering += OnBeginDrawSceneRendering;
            worldManager.SceneManager.Monitor.EndDrawSceneRendering += OnEndDrawSceneRendering;

            //----------------------------------------------------------------
            // スクリーン スペース アンビエント オクルージョン

            if (worldManager.Ssao != null)
            {
                worldManager.Ssao.Monitor.BeginProcess += OnSsaoBeginProcess;
                worldManager.Ssao.Monitor.EndProcess += OnSsaoEndProcess;
            }

            //----------------------------------------------------------------
            // 被写界深度

            if (worldManager.Dof != null)
            {
                worldManager.Dof.Monitor.BeginProcess += OnDofBeginProcess;
                worldManager.Dof.Monitor.EndProcess += OnDofEndProcess;
            }

            //----------------------------------------------------------------
            // パーティション マネージャ

            worldManager.PartitionManager.Monitor.BeginUpdate += OnPartitionManagerMonitorBeginUpdate;
            worldManager.PartitionManager.Monitor.EndUpdate += OnPartitionManagerMonitorEndUpdate;
            worldManager.PartitionManager.Monitor.BeginCheckPassivationCompleted += OnPartitionManagerMonitorBeginCheckPassivationCompleted;
            worldManager.PartitionManager.Monitor.EndCheckPassivationCompleted += OnPartitionManagerMonitorEndCheckPassivationCompleted;
            worldManager.PartitionManager.Monitor.BeginCheckActivationCompleted += OnPartitionManagerBeginCheckActivationCompleted;
            worldManager.PartitionManager.Monitor.EndCheckActivationCompleted += OnPartitionManagerEndCheckActivationCompleted;
            worldManager.PartitionManager.Monitor.BeginPassivatePartitions += OnPartitionManagerMonitorBeginPassivatePartitions;
            worldManager.PartitionManager.Monitor.EndPassivatePartitions += OnPartitionManagerMonitorEndPassivatePartitions;
            worldManager.PartitionManager.Monitor.BeginActivatePartitions += OnPartitionManagerMonitorBeginActivatePartitions;
            worldManager.PartitionManager.Monitor.EndActivatePartitions += OnPartitionManagerEndActivatePartitions;

            #endregion

            //================================================================
            // Region

            region = worldManager.Load("dummy");

            //================================================================
            // Others

            Content.RootDirectory = "Content";

            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Fonts/Debug");
            fillTexture = Texture2DHelper.CreateFillTexture(GraphicsDevice);
            helpMessageFontSize = font.MeasureString(helpMessage);

            BuildInfoMessage();
            informationTextFontSize = font.MeasureString(stringBuilder);
        }

        protected override void UnloadContent()
        {
            worldManager.Unload();

            #region TimeRuler

            timeRuler.ReleaseMarker(updateMarker);
            timeRuler.ReleaseMarker(partitionManagerUpdateMarker);
            timeRuler.ReleaseMarker(partitionManagerCheckPassivationCompletedMarker);
            timeRuler.ReleaseMarker(partitionManagerCheckActivationCompletedMarker);
            timeRuler.ReleaseMarker(partitionManagerCheckPassivationCompletedMarker);
            timeRuler.ReleaseMarker(partitionManagerCheckActivationCompletedMarker);
            timeRuler.ReleaseMarker(regionUpdateMarker);
            timeRuler.ReleaseMarker(drawMarker);
            timeRuler.ReleaseMarker(drawSceneMarker);
            timeRuler.ReleaseMarker(drawShadowMapMarker);
            timeRuler.ReleaseMarker(drawSceneOcclusionQueryMarker);
            timeRuler.ReleaseMarker(drawSceneRenderingMarker);
            timeRuler.ReleaseMarker(ssaoProcessMarker);
            timeRuler.ReleaseMarker(dofProcessMarker);

            #endregion

            spriteBatch.Dispose();
            fillTexture.Dispose();
        }

        protected override void Update(GameTime gameTime)
        {
            //================================================================
            // TimeRuler

            timeRuler.StartFrame();

            // これをしないとデバッグできない。
            if (!IsActive) return;

            //================================================================
            // TimeRuler

            updateMarker.Begin();

            //================================================================
            // Keyboard State

            var keyboardState = Keyboard.GetState();

            //================================================================
            // Exit

            // TODO
            if (!worldManager.PartitionManager.Closing && !worldManager.PartitionManager.Closed &&
                keyboardState.IsKeyDown(Keys.Escape))
            {
                worldManager.PartitionManager.Close();

                logger.Info("Wait until all partitions are passivated...");
            }

            if (worldManager.PartitionManager.Closed)
                Exit();

            //================================================================
            // ViewInput

            viewInput.Update(gameTime, worldManager.SceneManager.ActiveCamera.View);

            if (keyboardState.IsKeyDown(Keys.PageUp))
                viewInput.MoveVelocity += 10;
            if (keyboardState.IsKeyDown(Keys.PageDown))
            {
                viewInput.MoveVelocity -= 10;
                if (viewInput.MoveVelocity < 10) viewInput.MoveVelocity = 10;
            }

            //================================================================
            // WorldManager

            worldManager.Update(gameTime);

            //================================================================
            // Others

            // F1
            if (keyboardState.IsKeyUp(Keys.F1) && lastKeyboardState.IsKeyDown(Keys.F1))
                helpVisible = !helpVisible;
            // F2
            if (keyboardState.IsKeyUp(Keys.F2) && lastKeyboardState.IsKeyDown(Keys.F2))
                SceneManager.DebugBoundingBoxVisible = !SceneManager.DebugBoundingBoxVisible;
            // F3
            if (keyboardState.IsKeyUp(Keys.F3) && lastKeyboardState.IsKeyDown(Keys.F3))
                RegionManager.Wireframe = !RegionManager.Wireframe;
            // F4
            if (keyboardState.IsKeyUp(Keys.F4) && lastKeyboardState.IsKeyDown(Keys.F4))
                worldManager.SceneSettings.FogEnabled = !worldManager.SceneSettings.FogEnabled;
            // F5
            if (DebugMapDisplay.Available && keyboardState.IsKeyUp(Keys.F5) && lastKeyboardState.IsKeyDown(Keys.F5))
                DebugMapDisplay.Instance.Visible = !DebugMapDisplay.Instance.Visible;

            //================================================================
            // Keyboard State

            lastKeyboardState = keyboardState;

            base.Update(gameTime);

            //================================================================
            // TimeRuler

            updateMarker.End();
        }

        protected override void Draw(GameTime gameTime)
        {
            //================================================================
            // TimeRuler

            drawMarker.Begin();

            //================================================================
            // WorldManager

            worldManager.Draw(gameTime);

            //================================================================
            // Help HUD

            DrawHelp();

            //================================================================
            // TimeRuler

            drawMarker.End();

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

        void BuildInfoMessage()
        {
            var sb = stringBuilder;

            sb.Length = 0;
            sb.Append("Screen: ");
            sb.AppendNumber(graphics.PreferredBackBufferWidth).Append('x');
            sb.AppendNumber(graphics.PreferredBackBufferHeight).AppendLine();

            var partitionManagerMonitor = worldManager.PartitionManager.Monitor;
            sb.Append("Partition: ");
            sb.Append("A(").AppendNumber(partitionManagerMonitor.ActiveClusterCount).Append(":");
            sb.AppendNumber(partitionManagerMonitor.ActivePartitionCount).Append(") ");
            sb.Append("W(").AppendNumber(partitionManagerMonitor.ActivatingPartitionCount).Append(") ");
            sb.Append("P(").AppendNumber(partitionManagerMonitor.PassivatingPartitionCount).Append(")").AppendLine();

            var regionMonitor = region.Monitor;
            sb.Append("Chunk: ").AppendNumber(regionMonitor.ActiveChunkCount).Append("/");
            sb.AppendNumber(regionMonitor.TotalChunkCount).Append(" ");
            sb.Append("Mesh: ").AppendNumber(regionMonitor.ActiveChunkMeshCount).Append("/");
            sb.AppendNumber(regionMonitor.TotalChunkMeshCount).Append(" ");
            sb.Append("InterMesh: ").AppendNumber(regionMonitor.ActiveInterChunkMeshCount).Append("/");
            sb.AppendNumber(regionMonitor.TotalInterChunkMeshCount).AppendLine();

            sb.Append("VertexBuffer(IndexBuffer): ").AppendNumber(regionMonitor.ActiveVertexBufferCount).Append("/");
            sb.AppendNumber(regionMonitor.TotalVertexBufferCount).AppendLine();

            sb.Append("UpdatingChunk: ").AppendNumber(regionMonitor.UpdatingChunkCount).AppendLine();

            sb.Append("ChunkVertex: ");
            sb.Append("Max(").AppendNumber(regionMonitor.MaxChunkVertexCount).Append(") ");
            sb.Append("Total(").AppendNumber(regionMonitor.TotalChunkVertexCount).Append(")").AppendLine();

            sb.Append("ChunkIndex: ");
            sb.Append("Max(").AppendNumber(regionMonitor.MaxChunkIndexCount).Append(") ");
            sb.Append("Total(").AppendNumber(regionMonitor.TotalChunkIndexCount).Append(")").AppendLine();

            var sceneManagerMonitor = worldManager.SceneManager.Monitor;
            sb.Append("SceneObejcts: ").AppendNumber(sceneManagerMonitor.RenderedSceneObjectCount).Append("/");
            sb.AppendNumber(sceneManagerMonitor.VisibleSceneObjectCount).Append("/");
            sb.AppendNumber(sceneManagerMonitor.TotalSceneObjectCount).AppendLine();

            if (worldManager.ShadowMap != null)
            {
                var shadowMapMonitor = worldManager.ShadowMap.Monitor;
                sb.Append("ShadowCaster: ");
                for (int i = 0; i < shadowMapMonitor.SplitCount; i++)
                {
                    if (0 < i) sb.Append(":");
                    sb.AppendNumber(shadowMapMonitor[i].ShadowCasterCount);
                }
                sb.Append("/").AppendNumber(shadowMapMonitor.TotalShadowCasterCount).AppendLine();
            }

            sb.Append("MoveVelocity: ");
            sb.AppendNumber(viewInput.MoveVelocity).AppendLine();

            var camera = worldManager.SceneManager.ActiveCamera;
            sb.Append("Eye: ");
            sb.Append("P(");
            sb.AppendNumber(camera.View.Position.X).Append(", ");
            sb.AppendNumber(camera.View.Position.Y).Append(", ");
            sb.AppendNumber(camera.View.Position.Z).Append(") ");
            sb.Append("D(");
            sb.AppendNumber(camera.View.Direction.X).Append(", ");
            sb.AppendNumber(camera.View.Direction.Y).Append(", ");
            sb.AppendNumber(camera.View.Direction.Z).Append(")").AppendLine();

            sb.Append("Near/Far: ").AppendNumber(camera.Projection.NearPlaneDistance).Append("/");
            sb.AppendNumber(camera.Projection.FarPlaneDistance);
        }

        void DrawHelp()
        {
            if (!helpVisible) return;

            spriteBatch.Begin();

            var layout = new DebugLayout();

            int informationWidth = 380;

            // calculate the background area for information.
            layout.ContainerBounds = GraphicsDevice.Viewport.TitleSafeArea;
            layout.Width = informationWidth + 4;
            layout.Height = (int) informationTextFontSize.Y + 2;
            layout.HorizontalMargin = 8;
            layout.VerticalMargin = 8;
            layout.HorizontalAlignment = DebugHorizontalAlignment.Left;
            layout.VerticalAlignment = DebugVerticalAlignment.Top;
            layout.Arrange();
            // draw the rectangle.
            spriteBatch.Draw(fillTexture, layout.ArrangedBounds, Color.Black * 0.5f);

            // calculate the text area for help messages.
            layout.ContainerBounds = layout.ArrangedBounds;
            layout.Width = informationWidth;
            layout.Height = (int) informationTextFontSize.Y;
            layout.HorizontalMargin = 2;
            layout.VerticalMargin = 0;
            layout.HorizontalAlignment = DebugHorizontalAlignment.Center;
            layout.VerticalAlignment = DebugVerticalAlignment.Center;
            layout.Arrange();
            // draw the text.
            BuildInfoMessage();
            spriteBatch.DrawString(font, stringBuilder, new Vector2(layout.ArrangedBounds.X, layout.ArrangedBounds.Y), Color.Yellow);

            // calculate the background area for help messages.
            layout.ContainerBounds = GraphicsDevice.Viewport.TitleSafeArea;
            layout.Width = (int) helpMessageFontSize.X + 4;
            layout.Height = (int) helpMessageFontSize.Y + 2;
            layout.HorizontalMargin = 8;
            layout.VerticalMargin = 8;
            layout.HorizontalAlignment = DebugHorizontalAlignment.Left;
            layout.VerticalAlignment = DebugVerticalAlignment.Bottom;
            layout.Arrange();
            // draw the rectangle.
            spriteBatch.Draw(fillTexture, layout.ArrangedBounds, Color.Black * 0.5f);

            // calculate the text area for help messages.
            layout.ContainerBounds = layout.ArrangedBounds;
            layout.Width = (int) helpMessageFontSize.X;
            layout.Height = (int) helpMessageFontSize.Y;
            layout.HorizontalMargin = 2;
            layout.VerticalMargin = 0;
            layout.HorizontalAlignment = DebugHorizontalAlignment.Center;
            layout.VerticalAlignment = DebugVerticalAlignment.Center;
            layout.Arrange();
            // draw the text.
            spriteBatch.DrawString(font, helpMessage, new Vector2(layout.ArrangedBounds.X, layout.ArrangedBounds.Y), Color.Yellow);

            spriteBatch.End();
        }

        #region Monitor Callback

        void OnPartitionManagerMonitorEndUpdate(object sender, EventArgs e)
        {
            partitionManagerUpdateMarker.End();
        }

        void OnPartitionManagerMonitorBeginUpdate(object sender, EventArgs e)
        {
            partitionManagerUpdateMarker.Begin();
        }

        void OnPartitionManagerMonitorBeginCheckPassivationCompleted(object sender, EventArgs e)
        {
            partitionManagerCheckPassivationCompletedMarker.Begin();
        }

        void OnPartitionManagerMonitorEndCheckPassivationCompleted(object sender, EventArgs e)
        {
            partitionManagerCheckPassivationCompletedMarker.End();
        }

        void OnPartitionManagerBeginCheckActivationCompleted(object sender, EventArgs e)
        {
            partitionManagerCheckActivationCompletedMarker.Begin();
        }

        void OnPartitionManagerEndCheckActivationCompleted(object sender, EventArgs e)
        {
            partitionManagerCheckActivationCompletedMarker.End();
        }

        void OnPartitionManagerMonitorBeginPassivatePartitions(object sender, EventArgs e)
        {
            partitionManagerPassivatePartitionsMarker.Begin();
        }

        void OnPartitionManagerMonitorEndPassivatePartitions(object sender, EventArgs e)
        {
            partitionManagerPassivatePartitionsMarker.End();
        }

        void OnPartitionManagerMonitorBeginActivatePartitions(object sender, EventArgs e)
        {
            partitionManagerActivatePartitionsMarker.Begin();
        }

        void OnPartitionManagerEndActivatePartitions(object sender, EventArgs e)
        {
            partitionManagerActivatePartitionsMarker.End();
        }

        void OnBeginClassifySceneObjects(object sender, EventArgs e)
        {
            classifySceneObjectsMarker.Begin();
        }

        void OnEndClassifySceneObjects(object sender, EventArgs e)
        {
            classifySceneObjectsMarker.End();
        }

        void OnBeginDrawShadowMap(object sender, EventArgs e)
        {
            drawShadowMapMarker.Begin();
        }

        void OnEndDrawShadowMap(object sender, EventArgs e)
        {
            drawShadowMapMarker.End();
        }

        void OnBeginDrawScene(object sender, EventArgs e)
        {
            drawSceneMarker.Begin();
        }

        void OnEndDrawScene(object sender, EventArgs e)
        {
            drawSceneMarker.End();
        }

        void OnBeginDrawSceneOcclusionQuery(object sender, EventArgs e)
        {
            drawSceneOcclusionQueryMarker.Begin();
        }

        void OnEndDrawSceneOcclusionQuery(object sender, EventArgs e)
        {
            drawSceneOcclusionQueryMarker.End();
        }

        void OnBeginDrawSceneRendering(object sender, EventArgs e)
        {
            drawSceneRenderingMarker.Begin();
        }

        void OnEndDrawSceneRendering(object sender, EventArgs e)
        {
            drawSceneRenderingMarker.End();
        }

        void OnSsaoBeginProcess(object sender, EventArgs e)
        {
            ssaoProcessMarker.Begin();
        }

        void OnSsaoEndProcess(object sender, EventArgs e)
        {
            ssaoProcessMarker.End();
        }

        void OnDofBeginProcess(object sender, EventArgs e)
        {
            dofProcessMarker.Begin();
        }

        void OnDofEndProcess(object sender, EventArgs e)
        {
            dofProcessMarker.End();
        }

        #endregion
    }
}
