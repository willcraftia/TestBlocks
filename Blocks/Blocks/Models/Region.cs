#region Using

using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Content;
using Willcraftia.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.IO;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class Region : IAsset
    {
        ChunkManager chunkManager;

        BoundingBoxI bounds;

        // I/F
        public IResource Resource { get; set; }

        public SceneManager SceneManager { get; private set; }

        public GraphicsDevice GraphicsDevice { get; private set; }

        public BlocksSceneSettings SceneSettings { get; private set; }

        public AssetManager AssetManager { get; private set; }

        public ChunkEffect ChunkEffect { get; private set; }

        public string Name { get; set; }

        public BoundingBoxI Bounds
        {
            get { return bounds; }
            set { bounds = value; }
        }

        public TileCatalog TileCatalog { get; set; }

        public BlockCatalog BlockCatalog { get; set; }

        public IBiomeManager BiomeManager { get; set; }

        public IResource ChunkBundleResource { get; set; }

        public List<IChunkProcedure> ChunkProcesures { get; set; }

        public IChunkStore ChunkStore { get; set; }

        public RegionMonitor Monitor { get; private set; }

        public void Initialize(SceneManager sceneManager, BlocksSceneSettings sceneSettings, AssetManager assetManager, ChunkEffect chunkEffect)
        {
            if (sceneManager == null) throw new ArgumentNullException("sceneManager");
            if (sceneSettings == null) throw new ArgumentNullException("sceneSettings");
            if (assetManager == null) throw new ArgumentNullException("assetManager");

            SceneManager = sceneManager;
            GraphicsDevice = sceneManager.GraphicsDevice;
            SceneSettings = sceneSettings;
            AssetManager = assetManager;
            ChunkEffect = chunkEffect;

            chunkManager = new ChunkManager(this);

            if (Monitor == null) Monitor = new RegionMonitor();
        }

        public void Update()
        {
            chunkManager.Update();
        }

        public bool ContainsPosition(ref VectorI3 position)
        {
            // BoundingBoxI.Contains では Max 境界も含めてしまうため、
            // それを用いずに判定する。
            if (position.X < bounds.Min.X || position.Y < bounds.Min.Y || position.Z < bounds.Min.Z ||
                bounds.Max.X <= position.X || bounds.Max.Y <= position.Y || bounds.Max.Z <= position.Z)
                return false;

            return true;
        }

        // 非同期呼び出し。
        public Chunk ActivateChunk(ref VectorI3 position)
        {
            return chunkManager.ActivateChunk(ref position);
        }

        // 非同期呼び出し。
        public bool PassivateChunk(Chunk chunk)
        {
            return chunkManager.PassivateChunk(chunk);
        }

        public void Close()
        {
            // チャンク マネージャにクローズ処理を要求。
            // チャンク マネージャは即座に更新を終えるのではなく、
            // 更新のために占有しているチャンクの解放を全て待ってから更新を終える。
            chunkManager.Close();
        }

        #region ToString

        public override string ToString()
        {
            return "[Uri:" + ((Resource != null) ? Resource.AbsoluteUri : string.Empty) + "]";
        }

        #endregion
    }
}
