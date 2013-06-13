#region Using

using System;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Content;
using Willcraftia.Xna.Framework.Landscape;
using Willcraftia.Xna.Framework.IO;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class ChunkSettings : IAsset
    {
        IntVector3 chunkSize;

        int meshUpdateSearchCapacity;

        int verticesBuilderCount;

        int minActiveRange = 10;

        int maxActiveRange = 11;

        // I/F
        public IResource Resource { get; set; }

        public IntVector3 ChunkSize
        {
            get { return chunkSize; }
            set
            {
                if (value.X < 1 || value.Y < 1 || value.Z < 1 ||
                    value.X % ChunkManager.MeshSize.X != 0 ||
                    value.Y % ChunkManager.MeshSize.Y != 0 ||
                    value.Z % ChunkManager.MeshSize.Z != 0)
                    throw new ArgumentOutOfRangeException("value");

                // 最大配置で ushort の限界を越えるようなサイズは拒否。
                var maxVertices = Chunk.CalculateMaxVertexCount(chunkSize);
                var maxIndices = Chunk.CalculateIndexCount(maxVertices);
                if (ushort.MaxValue < maxIndices)
                    throw new ArgumentException("The indices over the limit of ushort needed.", "value");

                chunkSize = value;
            }
        }

        public int MeshUpdateSearchCapacity
        {
            get { return meshUpdateSearchCapacity; }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException("value");

                meshUpdateSearchCapacity = value;
            }
        }

        public int VerticesBuilderCount
        {
            get { return verticesBuilderCount; }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException("value");

                verticesBuilderCount = value;
            }
        }

        public int MinActiveVolume
        {
            get { return minActiveRange; }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException("value");

                minActiveRange = value;
            }
        }

        public int MaxActiveVolume
        {
            get { return maxActiveRange; }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException("value");

                maxActiveRange = value;
            }
        }

        public ChunkStoreType ChunkStoreType { get; set; }

        public PartitionManager.Settings PartitionManager { get; private set; }

        public ChunkSettings()
        {
            PartitionManager = new PartitionManager.Settings();
        }
    }
}
