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
        BoundingBoxI bounds;

        // I/F
        public IResource Resource { get; set; }

        public SceneManager SceneManager { get; private set; }

        public SceneSettings SceneSettings { get; private set; }

        public AssetManager AssetManager { get; private set; }

        /// <summary>
        /// このリージョンに属するチャンクのためのエフェクトを取得します。
        /// タイル カタログはリージョン毎に定義され、
        /// チャンクは自身が属するリージョンのタイル カタログを参照する必要があります。
        /// このため、各リージョンは、自身に属するチャンクのための固有のチャンク エフェクトを管理します。
        /// </summary>
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

        public void Initialize(SceneManager sceneManager, SceneSettings sceneSettings, AssetManager assetManager, Effect chunkEffect)
        {
            if (sceneManager == null) throw new ArgumentNullException("sceneManager");
            if (sceneSettings == null) throw new ArgumentNullException("sceneSettings");
            if (assetManager == null) throw new ArgumentNullException("assetManager");
            if (chunkEffect == null) throw new ArgumentNullException("chunkEffect");

            SceneManager = sceneManager;
            SceneSettings = sceneSettings;
            AssetManager = assetManager;

            // リージョン毎にタイル カタログが異なるため、エフェクトを複製して利用。
            ChunkEffect = new ChunkEffect(chunkEffect);

            // タイル カタログのテクスチャをチャンク エフェクトへ設定。
            ChunkEffect.TileMap = TileCatalog.TileMap;
            ChunkEffect.DiffuseMap = TileCatalog.DiffuseColorMap;
            ChunkEffect.EmissiveMap = TileCatalog.EmissiveColorMap;
            ChunkEffect.SpecularMap = TileCatalog.SpecularColorMap;
        }

        public bool ContainsChunkPosition(VectorI3 chunkPosition)
        {
            // BoundingBoxI.Contains では Max 境界も含めてしまうため、
            // それを用いずに判定する。
            if (chunkPosition.X < bounds.Min.X || chunkPosition.Y < bounds.Min.Y || chunkPosition.Z < bounds.Min.Z ||
                bounds.Max.X <= chunkPosition.X || bounds.Max.Y <= chunkPosition.Y || bounds.Max.Z <= chunkPosition.Z)
                return false;

            return true;
        }

        public void Update()
        {
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
