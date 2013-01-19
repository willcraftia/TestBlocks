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
using Willcraftia.Xna.Framework.Landscape;
using Willcraftia.Xna.Blocks.Models;

#endregion

namespace Willcraftia.Xna.Blocks.Content.Demo
{
    using DiagnosticsMonitor = Willcraftia.Xna.Framework.Diagnostics.Monitor;

    public class MainGame : Game
    {
        public const string MonitorUpdate = "MainGame.Update";

        public const string MonitorDraw = "MainGame.Draw";

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

        TimeRulerMonitorListener monitorListener;

        TextureDisplay textureDisplay;

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
            //----------------------------------------------------------------
            // GraphicsDeviceManager

            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            //graphics.PreferMultiSampling = true;

            //----------------------------------------------------------------
            // ロギング

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
            #region ロギング

            logger.Info("Initialize");

            EnvironmentLog.Info();
            GraphicsAdapterLog.Info();

            #endregion

            #region FPS カウンタ

            var fpsCounter = new FpsCounter(this);
            fpsCounter.Content.RootDirectory = "Content";
            fpsCounter.HorizontalAlignment = DebugHorizontalAlignment.Right;
            fpsCounter.SampleSpan = TimeSpan.FromSeconds(2);
            //fpsCounter.Enabled = false;
            //fpsCounter.Visible = false;
            Components.Add(fpsCounter);

            #endregion

            #region タイム ルーラ

            timeRuler = new TimeRuler(this);
            timeRuler.BackgroundColor = Color.Black;
            //timeRuler.Enabled = false;
            //timeRuler.Visible = false;
            Components.Add(timeRuler);

            #endregion

            #region モニタ
            
            monitorListener = new TimeRulerMonitorListener(timeRuler);
            DiagnosticsMonitor.Listeners.Add(monitorListener);

            monitorListener.CreateMarker(MonitorUpdate, 0, Color.White);

            monitorListener.CreateMarker(PartitionManager.MonitorUpdate, 1, Color.Cyan);
            monitorListener.CreateMarker(ChunkManager.MonitorUpdate, 1, Color.Orange);
            monitorListener.CreateMarker(RegionManager.MonitorUpdate, 1, Color.Green);
            
            monitorListener.CreateMarker(MonitorDraw, 2, Color.White);

            monitorListener.CreateMarker(SceneManager.MonitorDrawShadowMap, 3, Color.Cyan);
            monitorListener.CreateMarker(SceneManager.MonitorDrawScene, 3, Color.Orange);
            monitorListener.CreateMarker(SceneManager.MonitorOcclusionQuery, 3, Color.Green);
            monitorListener.CreateMarker(SceneManager.MonitorDrawSceneObjects, 3, Color.Red);
            monitorListener.CreateMarker(SceneManager.MonitorDrawParticles, 3, Color.Yellow);
            monitorListener.CreateMarker(SceneManager.MonitorPostProcess, 3, Color.Magenta);

            #endregion

            #region テクスチャ ディスプレイ

            textureDisplay = new TextureDisplay(this);
            textureDisplay.Visible = false;
            Components.Add(textureDisplay);

            #endregion

            base.Initialize();
        }

        protected override void LoadContent()
        {
            logger.Info("LoadContent");

            //----------------------------------------------------------------
            // ストレージ マネージャ

            StorageManager.SelectStorageContainer("Blocks.Demo.MainGame");

            //----------------------------------------------------------------
            // リソース ローダ

            ResourceLoader.Register(ContentResourceLoader.Instance);
            ResourceLoader.Register(TitleResourceLoader.Instance);
            ResourceLoader.Register(StorageResourceLoader.Instance);
            ResourceLoader.Register(FileResourceLoader.Instance);

            //----------------------------------------------------------------
            // ビュー コントローラ

            var viewport = GraphicsDevice.Viewport;
            viewInput.InitialMousePositionX = viewport.Width / 2;
            viewInput.InitialMousePositionY = viewport.Height / 2;
            viewInput.MoveVelocity = moveVelocity;
            viewInput.DashFactor = dashFactor;
            viewInput.Yaw(MathHelper.Pi);

            //----------------------------------------------------------------
            // ワールド マネージャ

            worldManager = new WorldManager(Services, GraphicsDevice);
            worldManager.Initialize();

            //----------------------------------------------------------------
            // リージョン

            // TODO
            region = worldManager.Load("dummy");

            //----------------------------------------------------------------
            // その他

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

            monitorListener.Close();

            spriteBatch.Dispose();
            fillTexture.Dispose();
        }

        protected override void Update(GameTime gameTime)
        {
            // タイム ルーラの計測開始
            timeRuler.StartFrame();

            // これをしないとデバッグできない。
            if (!IsActive) return;

            // モニタ
            DiagnosticsMonitor.Begin(MonitorUpdate);

            // キーボード状態の取得
            var keyboardState = Keyboard.GetState();

            //----------------------------------------------------------------
            // アプリケーション終了

            // TODO
            if (!worldManager.ChunkManager.Closing && !worldManager.ChunkManager.Closed &&
                keyboardState.IsKeyDown(Keys.Escape))
            {
                worldManager.ChunkManager.Close();

                logger.Info("Wait until all chunks are passivated...");
            }

            if (worldManager.ChunkManager.Closed)
                Exit();

            //----------------------------------------------------------------
            // ビューの操作

            viewInput.Update(gameTime, worldManager.SceneManager.ActiveCamera.View);

            if (keyboardState.IsKeyDown(Keys.PageUp))
                viewInput.MoveVelocity += 10;
            if (keyboardState.IsKeyDown(Keys.PageDown))
            {
                viewInput.MoveVelocity -= 10;
                if (viewInput.MoveVelocity < 10) viewInput.MoveVelocity = 10;
            }

            //----------------------------------------------------------------
            // ワールドの更新

            worldManager.Update(gameTime);

            //----------------------------------------------------------------
            // その他

            // F1
            if (keyboardState.IsKeyUp(Keys.F1) && lastKeyboardState.IsKeyDown(Keys.F1))
                helpVisible = !helpVisible;
            // F2
            if (keyboardState.IsKeyUp(Keys.F2) && lastKeyboardState.IsKeyDown(Keys.F2))
                worldManager.DebugChunkBoundingBoxVisible = !worldManager.DebugChunkBoundingBoxVisible;
            // F3
            if (keyboardState.IsKeyUp(Keys.F3) && lastKeyboardState.IsKeyDown(Keys.F3))
                RegionManager.Wireframe = !RegionManager.Wireframe;
            // F4
            if (keyboardState.IsKeyUp(Keys.F4) && lastKeyboardState.IsKeyDown(Keys.F4))
                worldManager.SceneSettings.FogEnabled = !worldManager.SceneSettings.FogEnabled;
            // F5
            if (keyboardState.IsKeyUp(Keys.F5) && lastKeyboardState.IsKeyDown(Keys.F5))
                textureDisplay.Visible = !textureDisplay.Visible;

            //----------------------------------------------------------------
            // キーボード状態の記録

            lastKeyboardState = keyboardState;

            base.Update(gameTime);

            // モニタ
            DiagnosticsMonitor.End(MonitorUpdate);
        }

        protected override void Draw(GameTime gameTime)
        {
            // モニタ
            DiagnosticsMonitor.Begin(MonitorDraw);

            // ワールドの描画
            worldManager.Draw(gameTime);

            // ヘルプ ウィンドウの描画
            DrawHelp();

            // モニタ
            DiagnosticsMonitor.End(MonitorDraw);

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

            var chunkManager = worldManager.ChunkManager;
            sb.Append("Chunk: ");
            sb.Append("A(").AppendNumber(chunkManager.ActiveClusterCount).Append(":");
            sb.AppendNumber(chunkManager.ActivePartitionCount).Append(") ");
            sb.Append("W(").AppendNumber(chunkManager.ActivatingPartitionCount).Append(") ");
            sb.Append("P(").AppendNumber(chunkManager.PassivatingPartitionCount).Append(")").AppendLine();

            sb.Append("Mesh: ").AppendNumber(chunkManager.ChunkMeshCount).Append(" ");
            sb.Append("Inter: ").AppendNumber(chunkManager.ActiveChunkVerticesBuilderCount).Append("/");
            sb.AppendNumber(chunkManager.TotalChunkVerticesBuilderCount).AppendLine();

            sb.Append("ChunkVertex: ");
            sb.Append("Max(").AppendNumber(chunkManager.MaxVertexCount).Append(") ");
            sb.Append("Total(").AppendNumber(chunkManager.TotalVertexCount).Append(")").AppendLine();

            sb.Append("ChunkIndex: ");
            sb.Append("Max(").AppendNumber(chunkManager.MaxIndexCount).Append(") ");
            sb.Append("Total(").AppendNumber(chunkManager.TotalIndexCount).Append(")").AppendLine();

            var sceneManager = worldManager.SceneManager;
            sb.Append("SceneObejcts: ").AppendNumber(sceneManager.RenderedSceneObjectCount).Append("/");
            sb.AppendNumber(sceneManager.SceneObjectCount).AppendLine();

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
    }
}
