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
using Willcraftia.Xna.Blocks.Models;

#endregion

namespace Willcraftia.Xna.Blocks.Content.Demo
{
    public class MainGame : Game
    {
        static readonly Logger logger = new Logger(typeof(MainGame).Name);

        GraphicsDeviceManager graphics;

        public MainGame()
        {
            //================================================================
            // GraphicsDeviceManager

            graphics = new GraphicsDeviceManager(this);
            graphics.PreferMultiSampling = true;

            //================================================================
            // Logging

            FileTraceListenerManager.Add(@"Logs\App.log", false);
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
            logger.InfoBegin("Initialize");

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
            // Others

            base.Initialize();

            logger.InfoEnd("Initialize");
        }

        class DelegateTarget
        {
            public int TestFunction(int value)
            {
                return value;
            }
        }

        class DelegateOwner
        {
            public Func<int, int> Func { get; set; }
        }

        protected override void LoadContent()
        {
            logger.InfoBegin("LoadContent");

            //================================================================
            // Region

            var regionManager = new RegionManager(Services);
            var region = regionManager.LoadRegion("title:Resources/DefaultRegion.json");

            //================================================================
            // SpriteFont via the asset manager of the region.

            var spriteFontUri = ContentResourceLoader.Instance.LoadResource("content:Fonts/Demo");
            var spriteFont = region.AssetManager.Load<SpriteFont>(spriteFontUri);

            DelegateTarget target = new DelegateTarget();
            DelegateOwner owner = new DelegateOwner
            {
                Func = target.TestFunction
            };

            var properties = owner.GetType().GetProperties();
            foreach (var property in properties)
            {
                if (typeof(Delegate).IsAssignableFrom(property.PropertyType))
                {
                    var d = property.GetValue(owner, null) as Delegate;
                    var t = d.Target;
                    Console.WriteLine("t = " + t);
                }
            }

            var value = Convert.ToString(owner);

            logger.InfoEnd("LoadContent");
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Escape))
                Exit();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

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
