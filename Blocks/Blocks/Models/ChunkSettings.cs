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

        int vertexBuildConcurrencyLevel;

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
                    value.X % ChunkMeshManager.MeshSize.X != 0 ||
                    value.Y % ChunkMeshManager.MeshSize.Y != 0 ||
                    value.Z % ChunkMeshManager.MeshSize.Z != 0)
                    throw new ArgumentOutOfRangeException("value");

                chunkSize = value;
            }
        }

        public int VertexBuildConcurrencyLevel
        {
            get { return vertexBuildConcurrencyLevel; }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException("value");

                vertexBuildConcurrencyLevel = value;
            }
        }

        public int UpdateBufferCountPerFrame
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
