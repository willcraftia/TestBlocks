#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Willcraftia.Xna.Framework.Collections;

#endregion

namespace Willcraftia.Xna.Framework.Landscape
{
    /// <summary>
    /// クラスタを管理するクラスです。
    /// </summary>
    internal sealed class ClusterManager
    {
        /// <summary>
        /// パーティション空間におけるクラスタのサイズ。
        /// 八分木の寸法であり 2^n でなければならない。
        /// </summary>
        internal readonly int Dimension;

        /// <summary>
        /// ワールド空間におけるパーティションのサイズ。
        /// </summary>
        internal readonly Vector3 PartitionSize;

        /// <summary>
        /// クラスタのプール。
        /// </summary>
        Pool<Cluster> clusterPool;

        /// <summary>
        /// クラスタのコレクション。
        /// </summary>
        ClusterCollection clusters;

        /// <summary>
        /// クラスタ数を取得します。
        /// </summary>
        internal int Count
        {
            get { return clusters.Count; }
        }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        /// <param name="dimension">
        /// パーティション空間におけるクラスタのサイズ。
        /// 八分木の寸法であり 2^n でなければならない。
        /// </param>
        /// <param name="partitionSize">ワールド空間におけるパーティションのサイズ。</param>
        /// <param name="capacity">クラスタの初期容量。</param>
        internal ClusterManager(int dimension, Vector3 partitionSize, int capacity)
        {
            if (((dimension - 1) & dimension) != 0) throw new ArgumentException("dimension must be a power of 2.");
            if (partitionSize.X < 0 || partitionSize.Y < 0 || partitionSize.X < 0)
                throw new ArgumentOutOfRangeException("size");
            if (capacity < 0) throw new ArgumentOutOfRangeException("capacity");

            Dimension = dimension;
            PartitionSize = partitionSize;
        
            clusterPool = new Pool<Cluster>(CreateCluster);
            clusters = new ClusterCollection(capacity);
        }

        /// <summary>
        /// 境界錐台と交差するパーティションを収集します。
        /// </summary>
        /// <param name="frustum">境界錐台。</param>
        /// <param name="result">収集先パーティションのコレクション。</param>
        internal void Collect(BoundingFrustum frustum, ICollection<Partition> result)
        {
            foreach (var cluster in clusters)
            {
                bool intersected;
                frustum.Intersects(ref cluster.Box, out intersected);

                // クラスタが境界錐台と交差するなら、クラスタに含まれるパーティションを収集。
                if (intersected) cluster.CollectPartitions(frustum, result);
            }
        }

        /// <summary>
        /// 指定の位置にパーティションが存在するか否かを検査します。
        /// </summary>
        /// <param name="position">パーティション空間におけるパーティションの位置。</param>
        /// <returns>
        /// true (パーティションが存在する場合)、false (それ以外の場合)。
        /// </returns>
        internal bool ContainsPartition(VectorI3 position)
        {
            var cluster = GetCluster(position);
            if (cluster == null) return false;

            return cluster.Contains(position);
        }

        /// <summary>
        /// 指定の位置にあるパーティションを取得します。
        /// </summary>
        /// <param name="position">パーティション空間におけるパーティションの位置。</param>
        /// <returns>
        /// パーティション、あるいは、指定の位置にパーティションが存在しない場合は null。
        /// </returns>
        internal Partition GetPartition(VectorI3 position)
        {
            var cluster = GetCluster(position);
            if (cluster == null) return null;

            return cluster.GetPartition(position);
        }

        /// <summary>
        /// パーティションを追加します。
        /// </summary>
        /// <param name="partition">パーティション。</param>
        internal void AddPartition(Partition partition)
        {
            VectorI3 clusterPosition;
            CalculateClusterPosition(ref partition.Position, out clusterPosition);

            Cluster cluster;
            if (!clusters.TryGet(clusterPosition, out cluster))
            {
                cluster = clusterPool.Borrow();
                cluster.Initialize(clusterPosition);
                clusters.Add(cluster);
            }

            cluster.Add(partition);
        }

        /// <summary>
        /// パーティションを削除します。
        /// </summary>
        /// <param name="partition">パーティション。</param>
        internal void RemovePartition(Partition partition)
        {
            var cluster = GetCluster(partition.Position);
            if (cluster == null) return;

            cluster.Remove(partition);

            if (cluster.Count == 0)
            {
                clusters.Remove(cluster.Position);
                clusterPool.Return(cluster);
            }
        }

        /// <summary>
        /// 全てのパーティションおよび全てのクラスタを削除します。
        /// </summary>
        internal void Clear()
        {
            clusters.Clear();
            clusterPool.Clear();
        }

        /// <summary>
        /// 指定の位置にあるパーティションを含むクラスタを取得します。
        /// </summary>
        /// <param name="position">パーティション空間におけるパーティションの位置。</param>
        /// <returns>
        /// クラスタ、あるいは、該当するクラスタが存在しない場合は null。
        /// </returns>
        Cluster GetCluster(VectorI3 position)
        {
            VectorI3 clusterPosition;
            CalculateClusterPosition(ref position, out clusterPosition);

            Cluster result;
            clusters.TryGet(clusterPosition, out result);
            return result;
        }

        /// <summary>
        /// パーティションが属するクラスタの位置を算出します。
        /// </summary>
        /// <param name="partition">パーティション空間におけるパーティションの位置。</param>
        /// <param name="result">クラスタ空間におけるクラスタの位置。</param>
        void CalculateClusterPosition(ref VectorI3 position, out VectorI3 result)
        {
            result = new VectorI3
            {
                X = (int) Math.Floor(position.X / (float) Dimension),
                Y = (int) Math.Floor(position.Y / (float) Dimension),
                Z = (int) Math.Floor(position.Z / (float) Dimension)
            };
        }

        /// <summary>
        /// クラスタを生成します。
        /// このメソッドは、クラスタ プールがクラスタを生成する際に呼び出されます。
        /// </summary>
        /// <returns>クラスタ。</returns>
        Cluster CreateCluster()
        {
            return new Cluster(this);
        }
    }
}
