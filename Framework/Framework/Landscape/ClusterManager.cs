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
        internal readonly VectorI3 Size;

        /// <summary>
        /// ワールド空間におけるクラスタのサイズ。
        /// </summary>
        internal readonly Vector3 SizeWorld;

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
        /// <param name="size">パーティション空間におけるクラスタのサイズ。</param>
        /// <param name="partitionSize">ワールド空間におけるパーティションのサイズ。</param>
        /// <param name="capacity">クラスタの初期容量。</param>
        internal ClusterManager(VectorI3 size, Vector3 partitionSize, int capacity)
        {
            if (size.X < 1 || size.Y < 1 || size.X < 1) throw new ArgumentOutOfRangeException("size");
            if (partitionSize.X < 0 || partitionSize.Y < 0 || partitionSize.X < 0)
                throw new ArgumentOutOfRangeException("size");
            if (capacity < 0) throw new ArgumentOutOfRangeException("capacity");

            Size = size;

            SizeWorld = new Vector3
            {
                X = size.X * partitionSize.X,
                Y = size.Y * partitionSize.Y,
                Z = size.Z * partitionSize.Z,
            };

            clusterPool = new Pool<Cluster>(CreateCluster);
            clusters = new ClusterCollection(capacity);
        }

        /// <summary>
        /// 境界錐台と交差するパーティションを収集します。
        /// </summary>
        /// <param name="frustum">境界錐台。</param>
        /// <param name="collector">収集先パーティションのコレクション。</param>
        internal void Collect<T>(BoundingFrustum frustum, ICollection<T> collector) where T : Partition
        {
            foreach (var cluster in clusters)
            {
                bool intersected;
                frustum.Intersects(ref cluster.BoundingBox, out intersected);

                // クラスタが境界錐台と交差するなら、クラスタに含まれるパーティションを収集。
                if (intersected) cluster.CollectPartitions(frustum, collector);
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
        /// 指定の位置にあるパーティションを削除します。
        /// </summary>
        /// <param name="position">パーティション空間におけるパーティションの位置。</param>
        internal void RemovePartition(VectorI3 position)
        {
            var cluster = GetCluster(position);
            if (cluster == null) return;

            cluster.Remove(position);

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
                X = MathExtension.Floor(position.X / (float) Size.X),
                Y = MathExtension.Floor(position.Y / (float) Size.Y),
                Z = MathExtension.Floor(position.Z / (float) Size.Z)
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
