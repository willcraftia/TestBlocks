#region Using

using System;
using System.Collections.Generic;

#endregion

namespace Willcraftia.Xna.Framework.Landscape
{
    public sealed class Cluster
    {
        public VectorI3 Position;

        int extent;

        Dictionary<VectorI3, Partition> partitionMap;

        public int Count
        {
            get { return partitionMap.Count; }
        }

        public Cluster(int extent)
        {
            this.extent = extent;

            partitionMap = new Dictionary<VectorI3, Partition>(extent * extent * extent);
        }

        public bool ContainsPartition(ref VectorI3 position)
        {
            return partitionMap.ContainsKey(position);
        }

        public Partition GetPartition(ref VectorI3 position)
        {
            return partitionMap[position];
        }

        public bool TryGetPartition(ref VectorI3 position, out Partition result)
        {
            return partitionMap.TryGetValue(position, out result);
        }

        public void AddPartition(Partition partition)
        {
            partitionMap[partition.Position] = partition;
        }

        public void RemovePartition(ref VectorI3 position)
        {
            partitionMap.Remove(position);
        }
    }
}
