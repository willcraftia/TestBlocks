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
    public sealed class Region : IAsset, IDisposable
    {
        // I/F
        public IResource Resource { get; set; }

        public AssetManager AssetManager { get; private set; }

        /// <summary>
        /// このリージョンに属するチャンクのためのエフェクトを取得します。
        /// タイル カタログはリージョン毎に定義され、
        /// チャンクは自身が属するリージョンのタイル カタログを参照する必要があります。
        /// このため、各リージョンは、自身に属するチャンクのための固有のチャンク エフェクトを管理します。
        /// </summary>
        public ChunkEffect ChunkEffect { get; private set; }

        public string Name { get; set; }

        public BoundingBoxI Box;

        public TileCatalog TileCatalog { get; set; }

        public BlockCatalog BlockCatalog { get; set; }

        public IBiomeManager BiomeManager { get; set; }

        public IResource ChunkBundleResource { get; set; }

        public List<IChunkProcedure> ChunkProcesures { get; set; }

        public IChunkStore ChunkStore { get; set; }

        public void Initialize(AssetManager assetManager, Effect chunkEffect)
        {
            if (assetManager == null) throw new ArgumentNullException("assetManager");
            if (chunkEffect == null) throw new ArgumentNullException("chunkEffect");

            AssetManager = assetManager;

            // リージョン毎にタイル カタログが異なるため、エフェクトを複製して利用。
            ChunkEffect = new ChunkEffect(chunkEffect);

            // タイル カタログのテクスチャをチャンク エフェクトへ設定。
            ChunkEffect.TileMap = TileCatalog.TileMap;
            ChunkEffect.EmissiveMap = TileCatalog.EmissiveColorMap;
        }

        #region ToString

        public override string ToString()
        {
            return "[Uri:" + ((Resource != null) ? Resource.AbsoluteUri : string.Empty) + "]";
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed;

        ~Region()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                ChunkEffect.Dispose();
            }

            disposed = true;
        }

        #endregion
    }
}
