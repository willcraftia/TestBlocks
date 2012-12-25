#region Using

using System;
using System.Diagnostics;
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

        public VectorI3 ChunkSize
        {
            get { return RegionManager.ChunkSize; }
        }

#if DEBUG

        public RegionMonitor Monitor { get; private set; }

#endif

        public void Initialize(GraphicsDevice graphicsDevice, AssetManager assetManager, IChunkStore chunkStore)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");
            if (assetManager == null) throw new ArgumentNullException("assetManager");
            if (chunkStore == null) throw new ArgumentNullException("chunkStore");

            GraphicsDevice = graphicsDevice;
            AssetManager = assetManager;

            chunkManager = new ChunkManager(this, chunkStore, RegionManager.ChunkSize);

            DebugInitialize();
        }

        public void Update()
        {
            DebugUpdate();

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
            // TODO
            var lightDirection = new Vector3(1, -1, -1);
            lightDirection.Normalize();
            ChunkEffect.LightDirection = lightDirection;
            ChunkEffect.LightDiffuseColor = Vector3.One;
            ChunkEffect.LightSpecularColor = Vector3.Zero;
            ChunkEffect.FogEnabled = true;
            // TODO
            ChunkEffect.FogStart = Math.Max(50, (projection as PerspectiveFov).FarPlaneDistance - 50);
            ChunkEffect.FogEnd = (projection as PerspectiveFov).FarPlaneDistance;
            ChunkEffect.FogColor = Color.CornflowerBlue.ToVector3();
            ChunkEffect.TileMap = TileCatalog.TileMap;
            ChunkEffect.DiffuseMap = TileCatalog.DiffuseColorMap;
            ChunkEffect.EmissiveMap = TileCatalog.EmissiveColorMap;
            ChunkEffect.SpecularMap = TileCatalog.SpecularColorMap;

            //ChunkEffect.BackingEffect.CurrentTechnique = ChunkEffect.DefaultTequnique;
            ChunkEffect.BackingEffect.CurrentTechnique = ChunkEffect.WireframeTequnique;

            chunkManager.Draw(view, projection);
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

        [Conditional("DEBUG")]
        void DebugInitialize()
        {
            if (Monitor == null) Monitor = new RegionMonitor();
        }

        [Conditional("DEBUG")]
        void DebugUpdate()
        {
            Monitor.Clear();
        }

        #region ToString

        public override string ToString()
        {
            return "[Uri:" + ((Resource != null) ? Resource.AbsoluteUri : string.Empty) + "]";
        }

        #endregion
    }
}
