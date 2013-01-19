#region Using

using System;
using Willcraftia.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Blocks.Serialization
{
    public struct ChunkSettingsDefinition
    {
        public VectorI3 ChunkSize;

        public int MeshUpdateSearchCapacity;

        public int VerticesBuilderCount;

        public int MinActiveRange;

        public int MaxActiveRange;

        public int ChunkPoolMaxCapacity;

        public VectorI3 ClusterSize;

        public int ActiveChunkCapacity;

        public int ActiveClusterCapacity;

        public int WaitActivationCapacity;

        public int WaitPassivationCapacity;

        public int ActivationCapacity;

        public int PassivationCapacity;

        public int ActivationSearchCapacity;

        public int PassivationSearchCapacity;
    }
}
