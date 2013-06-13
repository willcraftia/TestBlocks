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
        #region Cluster

        /// <summary>
        /// ある領域に含まれるパーティションを一つの集合として管理するクラスです。
        /// パーティションのアクティブ化および非アクティブ化では、
        /// 対象とするパーティションの位置からパーティションを検索しますが、
        /// 検索対象はパーティション数に比例するため、
        /// パーティション数が増加する程に検索負荷が増加する問題があります。
        /// この問題を解決するために、パーティションをクラスタで纏めて管理し、
        /// 検索の際には、クラスタを検索した後、クラスタからパーティションを検索するという方法を採ります。
        /// </summary>
        sealed class Cluster
        {
            /// <summary>
            /// クラスタ空間におけるクラスタの位置。
            /// </summary>
            public IntVector3 Position;

            /// <summary>
            /// ワールド空間におけるクラスタの位置。
            /// </summary>
            public Vector3 PositionWorld;

            /// <summary>
            /// ワールド空間におけるクラスタの境界ボックス。
            /// </summary>
            public BoundingBox BoundingBox;

            /// <summary>
            /// クラスタ マネージャ。
            /// </summary>
            ClusterManager manager;

            /// <summary>
            /// パーティションの位置をキーとするパーティションのディクショナリ。
            /// </summary>
            Dictionary<IntVector3, Partition> partitionsByPosition;

            /// <summary>
            /// パーティション数を取得します。
            /// </summary>
            public int PartitionCount
            {
                get
                {
                    lock (partitionsByPosition)
                    {
                        return partitionsByPosition.Count;
                    }
                }
            }

            /// <summary>
            /// インスタンスを生成します。
            /// </summary>
            /// <param name="manager">クラスタ マネージャ。</param>
            public Cluster(ClusterManager manager)
            {
                if (manager == null) throw new ArgumentNullException("manager");

                this.manager = manager;

                var size = manager.size;
                partitionsByPosition = new Dictionary<IntVector3, Partition>(size.X * size.Y * size.Z);
            }

            /// <summary>
            /// 指定の位置にあるクラスタとして初期化します。
            /// </summary>
            /// <param name="position">クラスタ空間におけるクラスタの位置。</param>
            public void Initialize(IntVector3 position)
            {
                Position = position;

                var sizeWorld = manager.sizeWorld;
                PositionWorld.X = Position.X * sizeWorld.X;
                PositionWorld.Y = Position.Y * sizeWorld.Y;
                PositionWorld.Z = Position.Z * sizeWorld.Z;

                BoundingBox.Min = PositionWorld;
                BoundingBox.Max = PositionWorld + sizeWorld;
            }

            /// <summary>
            /// クラスタを開放します。
            /// </summary>
            public void Release()
            {
                Position = IntVector3.Zero;
                PositionWorld = Vector3.Zero;
                BoundingBox = BoundingBoxHelper.Empty;
            }

            /// <summary>
            /// パーティションが存在するか否かを検査します。
            /// </summary>
            /// <param name="position">パーティションの位置。</param>
            /// <returns>
            /// true (パーティションが存在する場合)、false (それ以外の場合)。
            /// </returns>
            public bool ContainsPartition(ref IntVector3 position)
            {
                lock (partitionsByPosition)
                {
                    return partitionsByPosition.ContainsKey(position);
                }
            }

            /// <summary>
            /// 指定の位置にあるパーティションを取得します。
            /// </summary>
            /// <param name="position">パーティションの位置。</param>
            /// <returns>
            /// パーティション、あるいは、指定の位置にパーティションが存在しないならば null。
            /// </returns>
            public Partition GetPartition(ref IntVector3 position)
            {
                lock (partitionsByPosition)
                {
                    Partition result;
                    partitionsByPosition.TryGetValue(position, out result);
                    return result;
                }
            }

            /// <summary>
            /// パーティションを追加します。
            /// </summary>
            /// <param name="partition">パーティション。</param>
            public void AddPartition(Partition partition)
            {
                lock (partitionsByPosition)
                {
                    partitionsByPosition[partition.Position] = partition;
                }
            }

            /// <summary>
            /// 指定の位置にあるパーティションを削除します。
            /// </summary>
            /// <param name="position">パーティションの位置。</param>
            public bool RemovePartition(IntVector3 position)
            {
                lock (partitionsByPosition)
                {
                    return partitionsByPosition.Remove(position);
                }
            }

            /// <summary>
            /// 全てのパーティションを削除します。
            /// </summary>
            public void ClearPartitions()
            {
                lock (partitionsByPosition)
                {
                    partitionsByPosition.Clear();
                }
            }
        }

        #endregion

        /// <summary>
        /// パーティション空間におけるクラスタのサイズ。
        /// </summary>
        readonly IntVector3 size;

        /// <summary>
        /// ワールド空間におけるクラスタのサイズ。
        /// </summary>
        readonly Vector3 sizeWorld;

        /// <summary>
        /// クラスタのプール。
        /// </summary>
        Pool<Cluster> clusterPool;

        /// <summary>
        /// クラスタのコレクション。
        /// </summary>
        Dictionary<IntVector3, Cluster> clustersByPosition;

        /// <summary>
        /// クラスタ数を取得します。
        /// </summary>
        public int ClusterCount
        {
            get
            {
                lock (clustersByPosition)
                {
                    return clustersByPosition.Count;
                }
            }
        }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        /// <param name="size">パーティション空間におけるクラスタのサイズ。</param>
        /// <param name="partitionSize">ワールド空間におけるパーティションのサイズ。</param>
        public ClusterManager(IntVector3 size, Vector3 partitionSize)
        {
            if (size.X < 1 || size.Y < 1 || size.X < 1) throw new ArgumentOutOfRangeException("size");
            if (partitionSize.X < 0 || partitionSize.Y < 0 || partitionSize.X < 0)
                throw new ArgumentOutOfRangeException("size");

            this.size = size;

            sizeWorld = new Vector3
            {
                X = size.X * partitionSize.X,
                Y = size.Y * partitionSize.Y,
                Z = size.Z * partitionSize.Z,
            };

            clusterPool = new Pool<Cluster>(CreateCluster);
            clustersByPosition = new Dictionary<IntVector3, Cluster>();
        }

        /// <summary>
        /// 指定の位置にパーティションが存在するか否かを検査します。
        /// </summary>
        /// <param name="position">パーティション空間におけるパーティションの位置。</param>
        /// <returns>
        /// true (パーティションが存在する場合)、false (それ以外の場合)。
        /// </returns>
        public bool ContainsPartition(IntVector3 position)
        {
            var cluster = GetCluster(ref position);
            if (cluster == null) return false;

            return cluster.ContainsPartition(ref position);
        }

        /// <summary>
        /// 指定の位置にあるパーティションを取得します。
        /// </summary>
        /// <param name="position">パーティションの位置。</param>
        /// <returns>
        /// パーティション、あるいは、指定の位置にパーティションが存在しない場合は null。
        /// </returns>
        public Partition GetPartition(IntVector3 position)
        {
            var cluster = GetCluster(ref position);
            if (cluster == null) return null;

            return cluster.GetPartition(ref position);
        }

        /// <summary>
        /// パーティションを追加します。
        /// </summary>
        /// <param name="partition">パーティション。</param>
        public void AddPartition(Partition partition)
        {
            IntVector3 clusterPosition;
            CalculateClusterPosition(ref partition.Position, out clusterPosition);

            Cluster cluster;

            lock (clustersByPosition)
            {
                if (!clustersByPosition.TryGetValue(clusterPosition, out cluster))
                {
                    cluster = clusterPool.Borrow();
                    cluster.Initialize(clusterPosition);
                    clustersByPosition[clusterPosition] = cluster;
                }
            }

            cluster.AddPartition(partition);
        }

        /// <summary>
        /// 指定の位置にあるパーティションを削除します。
        /// パーティションの削除により、それを管理していたクラスタにパーティションがゼロになる場合、
        /// クラスタを削除してプールへ戻します。
        /// </summary>
        /// <param name="position">パーティションの位置。</param>
        public bool RemovePartition(IntVector3 position)
        {
            var cluster = GetCluster(ref position);
            if (cluster == null) return false;

            var result = cluster.RemovePartition(position);

            if (cluster.PartitionCount == 0)
            {
                lock (clustersByPosition)
                {
                    clustersByPosition.Remove(cluster.Position);
                }

                clusterPool.Return(cluster);
            }

            return result;
        }

        /// <summary>
        /// 全てのパーティションおよび全てのクラスタを削除します。
        /// </summary>
        public void ClearPartitions()
        {
            lock (clustersByPosition)
            {
                foreach (var cluster in clustersByPosition.Values)
                    cluster.ClearPartitions();

                clustersByPosition.Clear();
            }

            clusterPool.Clear();
        }

        /// <summary>
        /// 指定の位置にあるパーティションを含むクラスタを取得します。
        /// </summary>
        /// <param name="position">パーティション空間におけるパーティションの位置。</param>
        /// <returns>
        /// クラスタ、あるいは、該当するクラスタが存在しない場合は null。
        /// </returns>
        Cluster GetCluster(ref IntVector3 position)
        {
            IntVector3 clusterPosition;
            CalculateClusterPosition(ref position, out clusterPosition);

            lock (clustersByPosition)
            {
                Cluster result;
                clustersByPosition.TryGetValue(clusterPosition, out result);
                return result;
            }
        }

        /// <summary>
        /// パーティションが属するクラスタの位置を算出します。
        /// </summary>
        /// <param name="partition">パーティション空間におけるパーティションの位置。</param>
        /// <param name="result">クラスタ空間におけるクラスタの位置。</param>
        void CalculateClusterPosition(ref IntVector3 position, out IntVector3 result)
        {
            result = new IntVector3
            {
                X = MathExtension.Floor(position.X / (float) size.X),
                Y = MathExtension.Floor(position.Y / (float) size.Y),
                Z = MathExtension.Floor(position.Z / (float) size.Z)
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
