#region Using

using System;
using System.Collections.Generic;

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
        public int Count
        {
            get { return partitionQueue.Count; }
        }

        /// <summary>
        /// クラスタ数を取得します。
        /// </summary>
        public int ClusterCount
        {
            get { return clusterManager.Count; }
        }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        /// <param name="clusterExtent">クラスタの領域幅。</param>
        /// <param name="clusterCapacity">クラスタの初期容量。</param>
        /// <param name="partitionCapacity">パーティションの初期容量。</param>
        public ClusteredPartitionQueue(int clusterExtent, int clusterCapacity, int partitionCapacity)
        {
            clusterManager = new ClusterManager(clusterExtent, clusterCapacity);
            partitionQueue = new Queue<Partition>(partitionCapacity);
        }

        /// <summary>
        /// パーティションをキューへ追加します。
        /// </summary>
        /// <param name="partition">パーティション。</param>
        public void Enqueue(Partition partition)
        {
            clusterManager.AddPartition(partition);
            partitionQueue.Enqueue(partition);
        }

        /// <summary>
        /// キューからパーティションを取り出します。
        /// </summary>
        /// <returns>パーティション。</returns>
        public Partition Dequeue()
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
        public bool Contains(Partition partition)
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
        public bool Contains(ref VectorI3 position)
        {
            return clusterManager.ContainsPartition(ref position);
        }

        /// <summary>
        /// パーティションの取得を試行します。
        /// </summary>
        /// <param name="position">パーティションの位置。</param>
        /// <param name="result">
        /// パーティション、あるいは、パーティションが存在しない場合は null。
        /// </param>
        /// <returns>
        /// true (パーティションが存在する場合)、false (それ以外の場合)。
        /// </returns>
        public bool TryGetPartition(ref VectorI3 position, out Partition result)
        {
            return clusterManager.TryGetPartition(ref position, out result);
        }

        /// <summary>
        /// キューにある全てのパーティションを除去します。
        /// </summary>
        public void Clear()
        {
            clusterManager.Clear();
            partitionQueue.Clear();
        }
    }
}
