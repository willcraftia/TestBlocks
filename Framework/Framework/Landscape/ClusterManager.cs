#region Using

using System;
using System.Collections.Generic;
using Willcraftia.Xna.Framework.Collections;

#endregion

namespace Willcraftia.Xna.Framework.Landscape
{
    /// <summary>
    /// クラスタを管理するクラスです。
    /// </summary>
    public sealed class ClusterManager
    {
        /// <summary>
        /// クラスタの領域幅。
        /// </summary>
        int extent;

        /// <summary>
        /// 1 / extent。
        /// </summary>
        float inverseExtent;

        /// <summary>
        /// クラスタのプール。
        /// </summary>
        Pool<Cluster> clusterPool;

        /// <summary>
        /// クラスタの位置をキーとするクラスタのディクショナリ。
        /// </summary>
        Dictionary<VectorI3, Cluster> clusterMap;

        /// <summary>
        /// クラスタ数を取得します。
        /// </summary>
        public int Count
        {
            get { return clusterMap.Count; }
        }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        /// <param name="extent">領域幅。</param>
        /// <param name="capacity">クラスタの初期容量。</param>
        public ClusterManager(int extent, int capacity)
        {
            if (extent < 1) throw new ArgumentOutOfRangeException("extent");

            this.extent = extent;

            inverseExtent = 1 / (float) extent;

            clusterPool = new Pool<Cluster>(CreateCluster);
            clusterMap = new Dictionary<VectorI3, Cluster>(capacity);
        }

        /// <summary>
        /// パーティションが存在するか否かを検査します。
        /// </summary>
        /// <param name="partition">パーティション。</param>
        /// <returns>
        /// true (パーティションが存在する場合)、false (それ以外の場合)。
        /// </returns>
        public bool ContainsPartition(Partition partition)
        {
            var position = partition.Position;
            return ContainsPartition(ref position);
        }

        /// <summary>
        /// パーティションが存在するか否かを検査します。
        /// </summary>
        /// <param name="position">パーティションの位置。</param>
        /// <returns>
        /// true (パーティションが存在する場合)、false (それ以外の場合)。
        /// </returns>
        public bool ContainsPartition(ref VectorI3 position)
        {
            VectorI3 clusterPosition;
            CalculateClusterPosition(ref position, out clusterPosition);

            Cluster cluster;
            if (!clusterMap.TryGetValue(clusterPosition, out cluster))
                return false;

            return cluster.ContainsPartition(ref position);
        }

        /// <summary>
        /// パーティションを取得します。
        /// </summary>
        /// <param name="position">パーティションの位置。</param>
        /// <returns>パーティション。</returns>
        public Partition GetPartition(ref VectorI3 position)
        {
            var cluster = GetCluster(ref position);
            return cluster.GetPartition(ref position);
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
            Cluster cluster;
            if (!TryGetCluster(ref position, out cluster))
            {
                result = null;
                return false;
            }

            return cluster.TryGetPartition(ref position, out result);
        }

        /// <summary>
        /// パーティションを追加します。
        /// </summary>
        /// <param name="partition">パーティション。</param>
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

        /// <summary>
        /// パーティションを削除します。
        /// </summary>
        /// <param name="position">パーティションの位置。</param>
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

        /// <summary>
        /// パーティションを削除します。
        /// </summary>
        /// <param name="partition">パーティション。</param>
        public void RemovePartition(Partition partition)
        {
            var position = partition.Position;
            RemovePartition(ref position);
        }

        /// <summary>
        /// 全てのパーティションおよび全てのクラスタを削除します。
        /// </summary>
        public void Clear()
        {
            clusterMap.Clear();
            clusterPool.Clear();
        }

        /// <summary>
        /// パーティションの位置からクラスタを取得します。
        /// </summary>
        /// <param name="position">パーティションの位置。</param>
        /// <returns>クラスタ。</returns>
        Cluster GetCluster(ref VectorI3 position)
        {
            VectorI3 clusterPosition;
            CalculateClusterPosition(ref position, out clusterPosition);

            return clusterMap[clusterPosition];
        }

        /// <summary>
        /// パーティションの位置からクラスタの取得を試行します。
        /// </summary>
        /// <param name="position">パーティションの位置。</param>
        /// <param name="result">
        /// クラスタ、あるいは、クラスタが存在しない場合は null。
        /// </param>
        /// <returns>
        /// true (クラスタが存在する場合)、false (それ以外の場合)。
        /// </returns>
        bool TryGetCluster(ref VectorI3 position, out Cluster result)
        {
            VectorI3 clusterPosition;
            CalculateClusterPosition(ref position, out clusterPosition);

            return clusterMap.TryGetValue(clusterPosition, out result);
        }

        /// <summary>
        /// パーティションが属するクラスタの位置を算出します。
        /// </summary>
        /// <param name="partition">パーティションの位置。</param>
        /// <param name="result">クラスタの位置。</param>
        void CalculateClusterPosition(ref VectorI3 position, out VectorI3 result)
        {
            result = new VectorI3
            {
                X = MathExtension.Floor(position.X * inverseExtent),
                Y = MathExtension.Floor(position.Y * inverseExtent),
                Z = MathExtension.Floor(position.Z * inverseExtent)
            };
        }

        /// <summary>
        /// パーティションが属するクラスタの位置を算出します。
        /// </summary>
        /// <param name="partition">パーティション。</param>
        /// <param name="result">クラスタの位置。</param>
        void CalculateClusterPosition(Partition partition, out VectorI3 result)
        {
            var position = partition.Position;
            CalculateClusterPosition(ref position, out result);
        }

        /// <summary>
        /// クラスタを生成します。
        /// このメソッドは、クラスタ プールがクラスタを生成する際に呼び出されます。
        /// </summary>
        /// <returns>クラスタ。</returns>
        Cluster CreateCluster()
        {
            return new Cluster(extent);
        }
    }
}
