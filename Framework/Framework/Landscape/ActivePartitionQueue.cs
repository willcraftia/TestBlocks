#region Using

using System;
using System.Collections.Generic;

#endregion

namespace Willcraftia.Xna.Framework.Landscape
{
    public sealed class ActivePartitionQueue
    {
        PartitionBundleManager bundleManager;

        Queue<Partition> queue;

        public int Count
        {
            get { return queue.Count; }
        }

        public ActivePartitionQueue(int bundleExtent, int capacity)
        {
            bundleManager = new PartitionBundleManager(bundleExtent);
            queue = new Queue<Partition>(capacity);
        }

        public void Enqueue(Partition partition)
        {
            bundleManager.AddPartition(partition);
            queue.Enqueue(partition);
        }

        public Partition Dequeue()
        {
            var partition = queue.Dequeue();
            bundleManager.RemovePartition(partition);
            return partition;
        }

        public bool Contains(Partition partition)
        {
            return bundleManager.ContainsPartition(partition);
        }

        public bool Contains(ref VectorI3 position)
        {
            return bundleManager.ContainsPartition(ref position);
        }

        public bool TryGetPartition(ref VectorI3 position, out Partition result)
        {
            return bundleManager.TryGetPartition(ref position, out result);
        }

        public void Clear()
        {
            bundleManager.Clear();
            queue.Clear();
        }
    }
}
