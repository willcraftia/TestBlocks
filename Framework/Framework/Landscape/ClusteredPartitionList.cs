#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework.Landscape
{
    public sealed class ClusteredPartitionList : ICollection<Partition>
    {
        /// <summary>
        /// クラスタ マネージャ。
        /// </summary>
        ClusterManager clusterManager;

        /// <summary>
        /// パーティションの連結リスト。
        /// </summary>
        LinkedList<Partition> partitions;

        /// <summary>
        /// クラスタ数を取得します。
        /// </summary>
        public int ClusterCount
        {
            get { return clusterManager.Count; }
        }

        public LinkedListNode<Partition> First
        {
            get { return partitions.First; }
        }

        public LinkedListNode<Partition> Last
        {
            get { return partitions.Last; }
        }

        /// <summary>
        /// 指定の位置にあるパーティションを取得します。
        /// </summary>
        /// <param name="position">パーティション空間におけるパーティションの位置。</param>
        /// <returns>
        /// パーティション、あるいは、指定の位置にパーティションが存在しない場合は null。
        /// </returns>
        public Partition this[VectorI3 position]
        {
            get { return clusterManager.GetPartition(ref position); }
        }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        /// <param name="clusterSize">パーティション空間におけるクラスタのサイズ。</param>
        /// <param name="partitionSize">ワールド空間におけるパーティションのサイズ。</param>
        /// <param name="clusterCapacity">クラスタの初期容量。</param>
        /// <param name="partitionCapacity">パーティションの初期容量。</param>
        internal ClusteredPartitionList(VectorI3 clusterSize, Vector3 partitionSize, int clusterCapacity, int partitionCapacity)
        {
            clusterManager = new ClusterManager(clusterSize, partitionSize, clusterCapacity);
            partitions = new LinkedList<Partition>();
        }

        // I/F
        public void Add(Partition item)
        {
            partitions.AddLast(item.ListNode);
            clusterManager.AddPartition(item);
        }

        // I/F
        public void Clear()
        {
            clusterManager.Clear();
            partitions.Clear();
        }

        // I/F
        public bool Contains(Partition item)
        {
            return clusterManager.ContainsPartition(ref item.Position);
        }

        // I/F
        public void CopyTo(Partition[] array, int arrayIndex)
        {
            partitions.CopyTo(array, arrayIndex);
        }

        // I/F
        public int Count
        {
            get { return partitions.Count; }
        }

        // I/F
        public bool IsReadOnly
        {
            get { return false; }
        }

        // I/F
        public bool Remove(Partition item)
        {
            var result = (item.ListNode.List == partitions);

            partitions.Remove(item.ListNode);
            clusterManager.RemovePartition(ref item.Position);

            return result;
        }

        // I/F
        public IEnumerator<Partition> GetEnumerator()
        {
            return partitions.GetEnumerator();
        }

        // I/F
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// 指定の位置にパーティションが存在するか否かを検査します。
        /// </summary>
        /// <param name="position">パーティションの位置。</param>
        /// <returns>
        /// true (パーティションが存在する場合)、false (それ以外の場合)。
        /// </returns>
        public bool Contains(VectorI3 position)
        {
            return clusterManager.ContainsPartition(ref position);
        }

        public void AddFirst(LinkedListNode<Partition> node)
        {
            partitions.AddFirst(node);
            clusterManager.AddPartition(node.Value);
        }

        public void AddLast(LinkedListNode<Partition> node)
        {
            partitions.AddLast(node);
            clusterManager.AddPartition(node.Value);
        }

        public void RemoveFirst()
        {
            var node = partitions.First;
            partitions.RemoveFirst();
            clusterManager.RemovePartition(ref node.Value.Position);
        }

        public void RemoveLast()
        {
            var node = partitions.Last;
            partitions.RemoveLast();
            clusterManager.RemovePartition(ref node.Value.Position);
        }

        /// <summary>
        /// 境界錐台と交差するパーティションを収集します。
        /// </summary>
        /// <param name="frustum">境界錐台。</param>
        /// <param name="collector">収集先パーティションのコレクション。</param>
        public void Collect<T>(BoundingFrustum frustum, ICollection<T> collector) where T : Partition
        {
            foreach (var cluster in clusterManager.Clusters)
            {
                bool intersected;
                frustum.Intersects(ref cluster.BoundingBox, out intersected);

                // クラスタが境界錐台と交差するなら、クラスタに含まれるパーティションを収集。
                if (intersected) cluster.CollectPartitions(frustum, collector);
            }
        }
    }
}
