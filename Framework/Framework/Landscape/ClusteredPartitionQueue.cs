#region Using

using System;
using System.Collections.Generic;

#endregion

namespace Willcraftia.Xna.Framework.Landscape
{
    public sealed class ClusteredPartitionQueue
    {
        ClusterManager clusterManager;

        Queue<Partition> partitionQueue;

        public int Count
        {
            get { return partitionQueue.Count; }
        }

        public int ClusterCount
        {
            get { return clusterManager.Count; }
        }

        public ClusteredPartitionQueue(int clusterExtent, int clusterCapacity, int partitionCapacity)
        {
            clusterManager = new ClusterManager(clusterExtent, clusterCapacity);
            partitionQueue = new Queue<Partition>(partitionCapacity);
        }

        public void Enqueue(Partition partition)
        {
            clusterManager.AddPartition(partition);
            partitionQueue.Enqueue(partition);
        }

        public Partition Dequeue()
        {
            var partition = partitionQueue.Dequeue();
            clusterManager.RemovePartition(partition);
            return partition;
        }

        public bool Contains(Partition partition)
        {
            return clusterManager.ContainsPartition(partition);
        }

        public bool Contains(ref VectorI3 position)
        {
            return clusterManager.ContainsPartition(ref position);
        }

        public bool TryGetPartition(ref VectorI3 position, out Partition result)
        {
            return clusterManager.TryGetPartition(ref position, out result);
        }

        public void Clear()
        {
            clusterManager.Clear();
            partitionQueue.Clear();
        }
    }
}
