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
        WorldManager worldManager;

        CommandManager commandManager;

        WorldCommandFactory worldCommandFactory;

        ResourceManager resourceManager;

        AssetManager assetManager;

        Brush activeBrush;

        public GraphicsDevice GraphicsDevice { get; private set; }

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
            this.worldManager = worldManager;
            this.commandManager = commandManager;

            worldCommandFactory = new WorldCommandFactory(worldManager);
            
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

        public void Update()
        {
            if (ActiveBrush != null)
            {
                ActiveBrush.Update(worldManager.SceneManager.ActiveCamera);

                // ペイント可能: ブラシ表示
                // ペイント不能: ブラシ非表示
                ActiveBrush.Node.SetVisible(ActiveBrush.CanPaint);
            }
        }

        public void Paint()
        {
            if (ActiveBrush == null || !ActiveBrush.CanPaint) return;

            var command = worldCommandFactory.Create<SetBlockCommand>();

            command.BlockPosition = ActiveBrush.PaintPosition;
            command.BlockIndex = SelectedBlockIndex;

            commandManager.RequestCommand(command);
        }

        public void Erase()
        {
            if (ActiveBrush == null || !ActiveBrush.CanPaint) return;

            var command = worldCommandFactory.Create<SetBlockCommand>();

            command.BlockPosition = ActiveBrush.ErasePosition;
            command.BlockIndex = Block.EmptyIndex;

            commandManager.RequestCommand(command);
        }

        internal T LoadAsset<T>(string uri)
        {
            var resource = resourceManager.Load(uri);
            return assetManager.Load<T>(resource);
        }

        internal byte? GetBlockIndex(ref VectorI3 absoluteBlockPosition)
        {
            var chunk = worldManager.ChunkManager.GetChunkByBlockPosition(ref absoluteBlockPosition);
            if (chunk == null) return null;

            VectorI3 relativePosition;
            chunk.GetRelativeBlockPosition(ref absoluteBlockPosition, out relativePosition);

            return chunk.GetBlockIndex(ref relativePosition);
        }
    }
}
