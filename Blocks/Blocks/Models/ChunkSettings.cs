﻿#region Using

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
        VectorI3 chunkSize;

        int meshUpdateSearchCapacity;

        int verticesBuilderCount;

        int minActiveRange = 10;

        int maxActiveRange = 11;

        // I/F
        public IResource Resource { get; set; }

        public VectorI3 ChunkSize
        {
            get { return chunkSize; }
            set
            {
                if (value.X < 1 || value.Y < 1 || value.Z < 1 ||
                    value.X % 2 != 0 || value.Y % 2 != 0 || value.Z % 2 != 0)
                    throw new ArgumentOutOfRangeException("value");

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

        public int MinActiveRange
        {
            get { return minActiveRange; }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException("value");

                minActiveRange = value;
            }
        }

        public int MaxActiveRange
        {
            get { return maxActiveRange; }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException("value");

                maxActiveRange = value;
            }
        }

        public PartitionManager.Settings PartitionManager { get; private set; }

        public ChunkSettings()
        {
            PartitionManager = new PartitionManager.Settings();
        }
    }
}
