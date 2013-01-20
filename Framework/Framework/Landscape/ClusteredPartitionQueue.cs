#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework.Landscape
{
    /// <summary>
    /// クラスタ化されたパーティションを管理するキューです。
    /// </summary>
    public sealed class ClusteredPartitionQueue
    {
        /// <summary>
        /// クラスタ マネージャ。
        /// </summary>
        ClusterManager clusterManager;

        /// <summary>
        /// パーティションのキュー。
        /// </summary>
        Queue<Partition> partitionQueue;

        /// <summary>
        /// パーティション数を取得します。
        /// </summary>
        internal int Count
        {
            get { return partitionQueue.Count; }
        }

        /// <summary>
        /// クラスタ数を取得します。
        /// </summary>
        internal int ClusterCount
        {
            get { return clusterManager.Count; }
        }

        internal ICollection<Cluster> Clusters
        {
            get { return clusterManager.Clusters; }
        }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        /// <param name="clusterSize">パーティション空間におけるクラスタのサイズ。</param>
        /// <param name="partitionSize">ワールド空間におけるパーティションのサイズ。</param>
        /// <param name="clusterCapacity">クラスタの初期容量。</param>
        /// <param name="partitionCapacity">パーティションの初期容量。</param>
        internal ClusteredPartitionQueue(VectorI3 clusterSize, Vector3 partitionSize, int clusterCapacity, int partitionCapacity)
        {
            clusterManager = new ClusterManager(clusterSize, partitionSize, clusterCapacity);
            partitionQueue = new Queue<Partition>(partitionCapacity);
        }

        /// <summary>
        /// 指定の位置にあるパーティションを取得します。
        /// </summary>
        /// <param name="position">パーティション空間におけるパーティションの位置。</param>
        /// <returns>
        /// パーティション、あるいは、指定の位置にパーティションが存在しない場合は null。
        /// </returns>
        public Partition GetPartition(ref VectorI3 position)
        {
            return clusterManager.GetPartition(ref position);
        }

        /// <summary>
        /// パーティションをキューへ追加します。
        /// </summary>
        /// <param name="partition">パーティション。</param>
        internal void Enqueue(Partition partition)
        {
            clusterManager.AddPartition(partition);
            partitionQueue.Enqueue(partition);
        }

        /// <summary>
        /// キューからパーティションを取り出します。
        /// </summary>
        /// <returns>パーティション。</returns>
        internal Partition Dequeue()
        {
            var partition = partitionQueue.Dequeue();
            clusterManager.RemovePartition(partition);
            return partition;
        }

        /// <summary>
        /// パーティションが存在するか否かを検査します。
        /// </summary>
        /// <param name="partition">パーティション。</param>
        /// <returns>
        /// true (パーティションが存在する場合)、false (それ以外の場合)。
        /// </returns>
        internal bool Contains(Partition partition)
        {
            return clusterManager.ContainsPartition(partition);
        }

        /// <summary>
        /// パーティションが存在するか否かを検査します。
        /// </summary>
        /// <param name="position">パーティションの位置。</param>
        /// <returns>
        /// true (パーティションが存在する場合)、false (それ以外の場合)。
        /// </returns>
        internal bool Contains(ref VectorI3 position)
        {
            return clusterManager.ContainsPartition(ref position);
        }

        /// <summary>
        /// キューにある全てのパーティションを除去します。
        /// </summary>
        internal void Clear()
        {
            clusterManager.Clear();
            partitionQueue.Clear();
        }
    }
}
