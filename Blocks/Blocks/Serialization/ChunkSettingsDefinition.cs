#region Using

using System;
using Willcraftia.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Blocks.Serialization
{
    public struct ChunkSettingsDefinition
    {
        public IntVector3 ChunkSize;

        public int MeshUpdateSearchCapacity;

        public int VerticesBuilderCount;

        public int MinActiveRange;

        public int MaxActiveRange;

        public int ChunkPoolMaxCapacity;

        public IntVector3 ClusterSize;

        public int ActivationCapacity;

        public int PassivationCapacity;

        public int PassivationSearchCapacity;

        public float PriorActiveDistance;
    }
}
