#region Using

using System;
using System.Collections.Generic;
using Willcraftia.Xna.Framework.Collections;

#endregion

namespace Willcraftia.Xna.Framework.Landscape
{
    public sealed class PartitionBundleManager
    {
        int bundleExtent;

        Pool<PartitionBundle> bundlePool;

        Dictionary<VectorI3, PartitionBundle> bundleMap = new Dictionary<VectorI3, PartitionBundle>();

        public PartitionBundleManager(int bundleExtent)
        {
            this.bundleExtent = bundleExtent;

            bundlePool = new Pool<PartitionBundle>(CreateNode);
        }

        public bool ContainsPartition(Partition partition)
        {
            var position = partition.Position;
            return ContainsPartition(ref position);
        }

        public bool ContainsPartition(ref VectorI3 position)
        {
            VectorI3 bundlePosition;
            CalculateBundlePosition(ref position, out bundlePosition);

            PartitionBundle bundle;
            if (!bundleMap.TryGetValue(bundlePosition, out bundle))
                return false;

            return bundle.ContainsPartition(ref position);
        }

        public Partition GetPartition(ref VectorI3 position)
        {
            var bundle = GetBundle(ref position);
            return bundle.GetPartition(ref position);
        }

        public bool TryGetPartition(ref VectorI3 position, out Partition result)
        {
            PartitionBundle bundle;
            if (!TryGetBundle(ref position, out bundle))
            {
                result = null;
                return false;
            }

            return bundle.TryGetPartition(ref position, out result);
        }

        public void AddPartition(Partition partition)
        {
            VectorI3 bundlePosition;
            CalculateBundlePosition(partition, out bundlePosition);

            PartitionBundle bundle;
            if (!bundleMap.TryGetValue(bundlePosition, out bundle))
            {
                bundle = bundlePool.Borrow();
                bundleMap[bundlePosition] = bundle;
            }

            bundle.AddPartition(partition);
        }

        public void RemovePartition(ref VectorI3 position)
        {
            VectorI3 bundlePosition;
            CalculateBundlePosition(ref position, out bundlePosition);

            PartitionBundle bundle;
            if (!bundleMap.TryGetValue(bundlePosition, out bundle))
                return;

            bundle.RemovePartition(ref position);

            if (bundle.Count == 0)
            {
                bundleMap.Remove(bundlePosition);
                bundlePool.Return(bundle);
            }
        }

        public void RemovePartition(Partition partition)
        {
            var position = partition.Position;
            RemovePartition(ref position);
        }

        public void Clear()
        {
            bundleMap.Clear();
            bundlePool.Clear();
        }

        PartitionBundle GetBundle(ref VectorI3 position)
        {
            VectorI3 bundlePosition;
            CalculateBundlePosition(ref position, out bundlePosition);

            return bundleMap[bundlePosition];
        }

        bool TryGetBundle(ref VectorI3 position, out PartitionBundle result)
        {
            VectorI3 bundlePosition;
            CalculateBundlePosition(ref position, out bundlePosition);

            return bundleMap.TryGetValue(position, out result);
        }

        void CalculateBundlePosition(ref VectorI3 position, out VectorI3 result)
        {
            result = position;
            result.X /= bundleExtent;
            result.Y /= bundleExtent;
            result.Z /= bundleExtent;
        }

        void CalculateBundlePosition(Partition partition, out VectorI3 result)
        {
            var position = partition.Position;
            CalculateBundlePosition(ref position, out result);
        }

        PartitionBundle CreateNode()
        {
            return new PartitionBundle(bundleExtent);
        }
    }
}
