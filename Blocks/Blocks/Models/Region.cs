#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Content;
using Willcraftia.Xna.Framework.IO;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class Region : IAsset
    {
        ChunkManager chunkManager;

        // I/F
        public IResource Resource { get; set; }

        public GraphicsDevice GraphicsDevice { get; private set; }

        public AssetManager AssetManager { get; private set; }

        public string Name { get; set; }

        public BoundingBoxI Bounds { get; set; }

        public TileCatalog TileCatalog { get; set; }

        public BlockCatalog BlockCatalog { get; set; }

        public IBiomeManager BiomeManager { get; set; }

        public IResource ChunkBundleResource { get; set; }

        public List<IChunkProcedure> ChunkProcesures { get; set; }

        public void Initialize(GraphicsDevice graphicsDevice, AssetManager assetManager, IChunkStore chunkStore)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");
            if (assetManager == null) throw new ArgumentNullException("assetManager");
            if (chunkStore == null) throw new ArgumentNullException("chunkStore");

            GraphicsDevice = graphicsDevice;
            AssetManager = assetManager;

            chunkManager = new ChunkManager(this, chunkStore, RegionManager.ChunkSize);
        }

        public void Update()
        {
            chunkManager.Update();
        }

        public void Draw()
        {
            chunkManager.Draw();
        }

        public bool ContainsGridPosition(ref VectorI3 gridPosition)
        {
            ContainmentType result;
            Bounds.Contains(ref gridPosition, out result);
            return ContainmentType.Contains == result;
        }

        // 非同期呼び出し。
        public void ActivateChunk(ref VectorI3 position)
        {
            chunkManager.ActivateChunk(ref position);
        }

        // 非同期呼び出し。
        public void PassivateChunk(ref VectorI3 position)
        {
            chunkManager.PassivateChunk(ref position);
        }

        #region ToString

        public override string ToString()
        {
            return "[Uri:" + ((Resource != null) ? Resource.AbsoluteUri : string.Empty) + "]";
        }

        #endregion
    }
}
