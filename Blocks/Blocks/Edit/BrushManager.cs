#region Using

using System;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Collections;
using Willcraftia.Xna.Framework.Content;
using Willcraftia.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Blocks.Content;
using Willcraftia.Xna.Blocks.Models;

#endregion

namespace Willcraftia.Xna.Blocks.Edit
{
    public sealed class BrushManager
    {
        /// <summary>
        /// 繰り返しブロック配置における遅延フレーム数。
        /// </summary>
        const int paintRepeatDelay = 30;

        CommandManager commandManager;

        WorldCommandFactory worldCommandFactory;

        BrushCommandFactory brushCommandFactory;

        ResourceManager resourceManager;

        AssetManager assetManager;

        Brush activeBrush;

        bool paintModeStarted;

        int paintRepeatCount;

        public GraphicsDevice GraphicsDevice { get; private set; }

        public WorldManager WorldManager { get; private set; }

        public byte SelectedBlockIndex { get; set; }

        public Brush ActiveBrush
        {
            get { return activeBrush; }
            set
            {
                if (activeBrush == value) return;

                if (activeBrush != null) activeBrush.Active = false;
                activeBrush = value;
                if (activeBrush != null) activeBrush.Active = true;
            }
        }

        public StickyBrush StickyBrush { get; private set; }

        public FreeBrush FreeBrush { get; private set; }

        internal SceneNode BrushNodeBase { get; private set; }

        // TODO
        //
        // WorldManager をワールド単位として明確にし、
        // ワールド独立な情報をエンジン クラスなどで分離し、
        // ブラシはワールドとの独立性を保つように修正する。

        public BrushManager(IServiceProvider serviceProvider, GraphicsDevice graphicsDevice, WorldManager worldManager, CommandManager commandManager)
        {
            if (serviceProvider == null) throw new ArgumentNullException("serviceProvider");
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");
            if (worldManager == null) throw new ArgumentNullException("worldManager");
            if (commandManager == null) throw new ArgumentNullException("commandManager");

            GraphicsDevice = graphicsDevice;
            this.WorldManager = worldManager;
            this.commandManager = commandManager;

            worldCommandFactory = new WorldCommandFactory(worldManager);
            brushCommandFactory = new BrushCommandFactory(this);
            
            resourceManager = new ResourceManager();
            
            assetManager = new AssetManager(serviceProvider);
            assetManager.RegisterLoader(typeof(Mesh), new MeshLoader());

            BrushNodeBase = worldManager.SceneManager.CreateSceneNode("BrushBase");
            worldManager.SceneManager.RootNode.Children.Add(BrushNodeBase);

            SelectedBlockIndex = 1;

            //----------------------------------------------------------------
            // 粘着ブラシ

            var stickyBrushNode = worldManager.SceneManager.CreateSceneNode("StickyBrush");
            BrushNodeBase.Children.Add(stickyBrushNode);
            worldManager.SceneManager.UpdateOctreeSceneNode(stickyBrushNode);

            StickyBrush = new StickyBrush(this, stickyBrushNode);

            //----------------------------------------------------------------
            // 自由ブラシ

            var freeBrushNode = worldManager.SceneManager.CreateSceneNode("FreeBrush");
            BrushNodeBase.Children.Add(freeBrushNode);
            worldManager.SceneManager.UpdateOctreeSceneNode(freeBrushNode);

            FreeBrush = new FreeBrush(this, freeBrushNode);

            //----------------------------------------------------------------
            // デフォルトのアクティブ ブラシを設定

            ActiveBrush = StickyBrush;
        }

        public void StartPaintMode()
        {
            if (paintModeStarted) return;

            paintModeStarted = true;
            paintRepeatCount = 0;

            // ブラシのペイント モードを開始。
            activeBrush.StartPaint();
        }

        public void EndPaintMode()
        {
            if (!paintModeStarted) return;

            paintModeStarted = false;

            activeBrush.EndPaint();
        }

        public void Update()
        {
            if (activeBrush == null) return;

            activeBrush.Update(WorldManager.SceneManager.ActiveCamera);

            // ペイント可能: ブラシ表示
            // ペイント不能: ブラシ非表示
            activeBrush.Node.SetVisible(activeBrush.CanPaint);
        }

        public void Paint()
        {
            if (activeBrush == null || !activeBrush.CanPaint) return;

            // 繰り返しペイントのフレーム遅延数未満ならばスキップ。
            if (paintRepeatCount++ < paintRepeatDelay) return;

            // 繰り返しフレーム数をリセット。
            paintRepeatCount = 0;

            var command = worldCommandFactory.CreateSetBlockCommand();

            command.BlockPosition = activeBrush.PaintPosition;
            command.BlockIndex = SelectedBlockIndex;

            commandManager.RequestCommand(command);
        }

        public void Erase()
        {
            if (activeBrush == null || !activeBrush.CanPaint) return;

            var command = worldCommandFactory.CreateSetBlockCommand();

            command.BlockPosition = activeBrush.ErasePosition;
            command.BlockIndex = Block.EmptyIndex;

            commandManager.RequestCommand(command);
        }

        public void Pick()
        {
            if (activeBrush == null || !activeBrush.CanPaint) return;

            var command = brushCommandFactory.CreatePickBlockCommand();
            commandManager.RequestCommand(command);
        }

        public void SetBrush(Brush brush)
        {
            if (brush == null) throw new ArgumentNullException("brush");

            if (activeBrush == brush) return;

            var command = brushCommandFactory.CreateSetBrushCommand();
            command.Brush = brush;
            commandManager.RequestCommand(command);
        }

        internal T LoadAsset<T>(string uri)
        {
            var resource = resourceManager.Load(uri);
            return assetManager.Load<T>(resource);
        }

        internal byte? GetBlockIndex(ref IntVector3 absoluteBlockPosition)
        {
            var chunk = WorldManager.ChunkManager.GetChunkByBlockPosition(ref absoluteBlockPosition);
            if (chunk == null) return null;

            IntVector3 relativePosition;
            chunk.GetRelativeBlockPosition(ref absoluteBlockPosition, out relativePosition);

            return chunk.GetBlockIndex(ref relativePosition);
        }
    }
}
