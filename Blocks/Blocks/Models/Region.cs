#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Assets;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class Region
    {
        //====================================================================
        // Efficiency

        public VectorI3 ChunkSize;

        public BoundingBoxI Bounds;

        //
        //====================================================================

        ChunkManager chunkManager;

        public UriManager UriManager { get; set; }

        public AssetManager AssetManager { get; set; }

        public IChunkStore ChunkStore { get; set; }

        public IUri Uri { get; set; }

        public string Name { get; set; }

        public TileCatalog TileCatalog { get; set; }

        public BlockCatalog BlockCatalog { get; set; }

        public IUri ChunkBundleUri { get; set; }

        public List<IProcedure<Chunk>> ChunkProcesures { get; set; }

        public void Initialize()
        {
            chunkManager = new ChunkManager(this, ChunkStore);
        }

        public void Update()
        {
            chunkManager.Update();
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
    }
}
