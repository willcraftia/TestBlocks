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
        /// </summary>
        VectorI3 size;

        /// <summary>
        /// ワールド空間におけるクラスタのサイズ。
        /// </summary>
        Vector3 sizeWorld;

        /// <summary>
        /// クラスタのプール。
        /// </summary>
        Pool<Cluster> clusterPool;

        /// <summary>
        /// クラスタの位置をキーとするクラスタのディクショナリ。
        /// </summary>
        Dictionary<VectorI3, Cluster> clusters;

        /// <summary>
        /// パーティション空間におけるクラスタのサイズを取得します。
        /// </summary>
        internal VectorI3 Size
        {
            get { return size; }
        }

        /// <summary>
        /// ワールド空間におけるクラスタのサイズを取得します。
        /// </summary>
        internal Vector3 SizeWorld
        {
            get { return sizeWorld; }
        }

        /// <summary>
        /// クラスタ数を取得します。
        /// </summary>
        internal int Count
        {
            get { return clusters.Count; }
        }

        internal ICollection<Cluster> Clusters
        {
            get { return clusters.Values; }
        }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        /// <param name="size">パーティション空間におけるクラスタのサイズ。</param>
        /// <param name="partitionSize">ワールド空間におけるパーティションのサイズ。</param>
        /// <param name="capacity">クラスタの初期容量。</param>
        internal ClusterManager(VectorI3 size, Vector3 partitionSize, int capacity)
        {
            if (size.X < 1 || size.Y < 1 || size.X < 1) throw new ArgumentOutOfRangeException("size");
            if (partitionSize.X < 0 || partitionSize.Y < 0 || partitionSize.X < 0)
                throw new ArgumentOutOfRangeException("size");
            if (capacity < 0) throw new ArgumentOutOfRangeException("capacity");

            this.size = size;

            sizeWorld = new Vector3
            {
                X = size.X * partitionSize.X,
                Y = size.Y * partitionSize.Y,
                Z = size.Z * partitionSize.Z,
            };

            clusterPool = new Pool<Cluster>(CreateCluster);
            clusters = new Dictionary<VectorI3, Cluster>(capacity);
        }

        /// <summary>
        /// パーティションが存在するか否かを検査します。
        /// </summary>
        /// <param name="partition">パーティション。</param>
        /// <returns>
        /// true (パーティションが存在する場合)、false (それ以外の場合)。
        /// </returns>
        internal bool ContainsPartition(Partition partition)
        {
            return ContainsPartition(ref partition.Position);
        }

        /// <summary>
        /// パーティションが存在するか否かを検査します。
        /// </summary>
        /// <param name="position">パーティション空間におけるパーティションの位置。</param>
        /// <returns>
        /// true (パーティションが存在する場合)、false (それ以外の場合)。
        /// </returns>
        internal bool ContainsPartition(ref VectorI3 position)
        {
            VectorI3 clusterPosition;
            CalculateClusterPosition(ref position, out clusterPosition);

            Cluster cluster;
            if (!clusters.TryGetValue(clusterPosition, out cluster))
                return false;

            return cluster.ContainsPartition(ref position);
        }

        /// <summary>
        /// パーティションを取得します。
        /// </summary>
        /// <param name="position">パーティション空間におけるパーティションの位置。</param>
        /// <returns>パーティション。</returns>
        internal Partition GetPartition(ref VectorI3 position)
        {
            var cluster = GetCluster(ref position);
            return cluster.GetPartition(ref position);
        }

        /// <summary>
        /// パーティションの取得を試行します。
        /// </summary>
        /// <param name="position">パーティション空間におけるパーティションの位置。</param>
        /// <param name="result">
        /// パーティション、あるいは、パーティションが存在しない場合は null。
        /// </param>
        /// <returns>
        /// true (パーティションが存在する場合)、false (それ以外の場合)。
        /// </returns>
        internal bool TryGetPartition(ref VectorI3 position, out Partition result)
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
        internal void AddPartition(Partition partition)
        {
            VectorI3 clusterPosition;
            CalculateClusterPosition(partition, out clusterPosition);

            Cluster cluster;
            if (!clusters.TryGetValue(clusterPosition, out cluster))
            {
                cluster = clusterPool.Borrow();
                cluster.Initialize(clusterPosition);
                clusters[clusterPosition] = cluster;
            }

            cluster.AddPartition(partition);
        }

        /// <summary>
        /// パーティションを削除します。
        /// </summary>
        /// <param name="position">パーティション空間におけるパーティションの位置。</param>
        internal void RemovePartition(ref VectorI3 position)
        {
            VectorI3 clusterPosition;
            CalculateClusterPosition(ref position, out clusterPosition);

            Cluster cluster;
            if (!clusters.TryGetValue(clusterPosition, out cluster))
                return;

            cluster.RemovePartition(ref position);

            if (cluster.Count == 0)
            {
                clusters.Remove(clusterPosition);
                clusterPool.Return(cluster);
            }
        }

        /// <summary>
        /// パーティションを削除します。
        /// </summary>
        /// <param name="partition">パーティション。</param>
        internal void RemovePartition(Partition partition)
        {
            RemovePartition(ref partition.Position);
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
        /// パーティションの位置からクラスタを取得します。
        /// </summary>
        /// <param name="position">パーティション空間におけるパーティションの位置。</param>
        /// <returns>クラスタ。</returns>
        Cluster GetCluster(ref VectorI3 position)
        {
            VectorI3 clusterPosition;
            CalculateClusterPosition(ref position, out clusterPosition);

            return clusters[clusterPosition];
        }

        /// <summary>
        /// パーティションの位置からクラスタの取得を試行します。
        /// </summary>
        /// <param name="position">パーティション空間におけるパーティションの位置。</param>
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

            return clusters.TryGetValue(clusterPosition, out result);
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
                X = MathExtension.Floor(position.X / (float) size.X),
                Y = MathExtension.Floor(position.Y / (float) size.Y),
                Z = MathExtension.Floor(position.Z / (float) size.Z)
            };
        }

        /// <summary>
        /// パーティションが属するクラスタの位置を算出します。
        /// </summary>
        /// <param name="partition">パーティション。</param>
        /// <param name="result">クラスタ空間におけるクラスタの位置。</param>
        void CalculateClusterPosition(Partition partition, out VectorI3 result)
        {
            CalculateClusterPosition(ref partition.Position, out result);
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
