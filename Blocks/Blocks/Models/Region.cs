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

        BoundingBoxI bounds;

        // I/F
        public IResource Resource { get; set; }

        public GraphicsDevice GraphicsDevice { get; private set; }

        public AssetManager AssetManager { get; private set; }

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

        public ChunkEffect ChunkEffect { get; set; }

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

        public void Draw(View view, Projection projection)
        {
            Vector3 eyePosition;
            View.GetEyePosition(ref view.Matrix, out eyePosition);

            Matrix viewProjection;
            Matrix.Multiply(ref view.Matrix, ref projection.Matrix, out viewProjection);

            ChunkEffect.EyePosition = eyePosition;
            ChunkEffect.ViewProjection = viewProjection;
            ChunkEffect.AmbientLightColor = Vector3.One;
            ChunkEffect.LightDirection = Vector3.Down;
            ChunkEffect.LightDiffuseColor = Vector3.One;
            ChunkEffect.LightSpecularColor = Vector3.Zero;
            ChunkEffect.FogEnabled = true;
            ChunkEffect.FogStart = 50;
            ChunkEffect.FogEnd = 150;
            ChunkEffect.FogColor = Color.CornflowerBlue.ToVector3();
            ChunkEffect.TileMap = TileCatalog.TileMap;
            ChunkEffect.DiffuseMap = TileCatalog.DiffuseColorMap;
            ChunkEffect.EmissiveMap = TileCatalog.EmissiveColorMap;
            ChunkEffect.SpecularMap = TileCatalog.SpecularColorMap;

            ChunkEffect.BackingEffect.CurrentTechnique = ChunkEffect.DefaultTequnique;

            chunkManager.Draw(view, projection);
        }

        public bool ContainsGridPosition(ref VectorI3 gridPosition)
        {
            // BoundingBoxI.Contains では Max 境界も含めてしまうため、
            // それを用いずに判定する。
            if (gridPosition.X < bounds.Min.X || gridPosition.Y < bounds.Min.Y || gridPosition.Z < bounds.Min.Z ||
                bounds.Max.X <= gridPosition.X || bounds.Max.Y <= gridPosition.Y || bounds.Max.Z <= gridPosition.Z)
                return false;

            return true;
        }

        // 非同期呼び出し。
        public bool ActivateChunk(ref VectorI3 position)
        {
            return chunkManager.ActivateChunk(ref position);
        }

        // 非同期呼び出し。
        public bool PassivateChunk(ref VectorI3 position)
        {
            return chunkManager.PassivateChunk(ref position);
        }

        #region ToString

        public override string ToString()
        {
            return "[Uri:" + ((Resource != null) ? Resource.AbsoluteUri : string.Empty) + "]";
        }

        #endregion
    }
}
