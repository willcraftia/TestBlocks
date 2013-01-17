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

        public int InitialActiveChunkCapacity;

        public int InitialActiveClusterCapacity;

        public int InitialActivationCapacity;

        public int InitialPassivationCapacity;

        public int ActivationTaskQueueSlotCount;

        public int PassivationTaskQueueSlotCount;

        public int ActivationSearchCapacity;

        public int PassivationSearchCapacity;
    }
}
