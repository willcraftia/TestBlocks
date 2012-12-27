#region Using

using System;
using System.Collections.Generic;
using Willcraftia.Xna.Framework.Collections;

#endregion

namespace Willcraftia.Xna.Framework.Landscape
{
    public sealed class ClusterManager
    {
        int extent;

        Pool<Cluster> clusterPool;

        Dictionary<VectorI3, Cluster> clusterMap;

        public int Count
        {
            get { return clusterMap.Count; }
        }

        public ClusterManager(int extent, int capacity)
        {
            if (extent < 1) throw new ArgumentOutOfRangeException("extent");

            this.extent = extent;

            clusterPool = new Pool<Cluster>(CreateNode);
            clusterMap = new Dictionary<VectorI3, Cluster>(capacity);
        }

        public bool ContainsPartition(Partition partition)
        {
            var position = partition.Position;
            return ContainsPartition(ref position);
        }

        public bool ContainsPartition(ref VectorI3 position)
        {
            VectorI3 clusterPosition;
            CalculateClusterPosition(ref position, out clusterPosition);

            Cluster cluster;
            if (!clusterMap.TryGetValue(clusterPosition, out cluster))
                return false;

            return cluster.ContainsPartition(ref position);
        }

        public Partition GetPartition(ref VectorI3 position)
        {
            var cluster = GetCluster(ref position);
            return cluster.GetPartition(ref position);
        }

        public bool TryGetPartition(ref VectorI3 position, out Partition result)
        {
            Cluster cluster;
            if (!TryGetCluster(ref position, out cluster))
            {
                result = null;
                return false;
            }

            return cluster.TryGetPartition(ref position, out result);
        }

        public void AddPartition(Partition partition)
        {
            VectorI3 clusterPosition;
            CalculateClusterPosition(partition, out clusterPosition);

            Cluster cluster;
            if (!clusterMap.TryGetValue(clusterPosition, out cluster))
            {
                cluster = clusterPool.Borrow();
                clusterMap[clusterPosition] = cluster;
            }

            cluster.AddPartition(partition);
        }

        public void RemovePartition(ref VectorI3 position)
        {
            VectorI3 clusterPosition;
            CalculateClusterPosition(ref position, out clusterPosition);

            Cluster cluster;
            if (!clusterMap.TryGetValue(clusterPosition, out cluster))
                return;

            cluster.RemovePartition(ref position);

            if (cluster.Count == 0)
            {
                clusterMap.Remove(clusterPosition);
                clusterPool.Return(cluster);
            }
        }

        public void RemovePartition(Partition partition)
        {
            var position = partition.Position;
            RemovePartition(ref position);
        }

        public void Clear()
        {
            clusterMap.Clear();
            clusterPool.Clear();
        }

        Cluster GetCluster(ref VectorI3 position)
        {
            VectorI3 clusterPosition;
            CalculateClusterPosition(ref position, out clusterPosition);

            return clusterMap[clusterPosition];
        }

        bool TryGetCluster(ref VectorI3 position, out Cluster result)
        {
            VectorI3 clusterPosition;
            CalculateClusterPosition(ref position, out clusterPosition);

            return clusterMap.TryGetValue(position, out result);
        }

        void CalculateClusterPosition(ref VectorI3 position, out VectorI3 result)
        {
            result = position;
            result.X /= extent;
            result.Y /= extent;
            result.Z /= extent;
        }

        void CalculateClusterPosition(Partition partition, out VectorI3 result)
        {
            var position = partition.Position;
            CalculateClusterPosition(ref position, out result);
        }

        Cluster CreateNode()
        {
            return new Cluster(extent);
        }
    }
}
