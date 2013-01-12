#region Using

using System;

#endregion

namespace Willcraftia.Xna.Blocks.Serialization
{
    public struct LandscapeSettingsDefinition
    {
        public int MinActiveRange;

        public int MaxActiveRange;

        public int PartitionPoolMaxCapacity;

        public int ClusterExtent;

        public int InitialActivePartitionCapacity;

        public int InitialActiveClusterCapacity;

        public int InitialActivationCapacity;

        public int InitialPassivationCapacity;

        public int ActivationTaskQueueSlotCount;

        public int PassivationTaskQueueSlotCount;

        public int ActivationSearchCapacity;

        public int PassivationSearchCapacity;
    }
}
