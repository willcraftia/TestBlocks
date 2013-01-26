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
            internal VectorI3 Position;

            /// <summary>
            /// ワールド空間におけるクラスタの位置。
            /// </summary>
            internal Vector3 PositionWorld;

            /// <summary>
            /// ワールド空間におけるクラスタの境界ボックス。
            /// </summary>
            internal BoundingBox BoundingBox;

            /// <summary>
            /// クラスタ マネージャ。
            /// </summary>
            ClusterManager manager;

            /// <summary>
            /// パーティションの位置をキーとするパーティションのディクショナリ。
            /// </summary>
            Dictionary<VectorI3, Partition> partitionsByPosition;

            /// <summary>
            /// パーティション数を取得します。
            /// </summary>
            internal int Count
            {
                get { return partitionsByPosition.Count; }
            }

            /// <summary>
            /// 指定の位置についてパーティションを取得または設定します。
            /// </summary>
            /// <param name="position">パーティションの位置。</param>
            /// <returns>
            /// パーティション、あるいは、指定の位置にパーティションが存在しないならば null。
            /// </returns>
            internal Partition this[VectorI3 position]
            {
                get
                {
                    Partition result;
                    partitionsByPosition.TryGetValue(position, out result);
                    return result;
                }
                set
                {
                    partitionsByPosition[position] = value;
                }
            }

            /// <summary>
            /// インスタンスを生成します。
            /// </summary>
            /// <param name="manager">クラスタ マネージャ。</param>
            internal Cluster(ClusterManager manager)
            {
                if (manager == null) throw new ArgumentNullException("manager");

                this.manager = manager;

                var size = manager.size;
                partitionsByPosition = new Dictionary<VectorI3, Partition>(size.X * size.Y * size.Z);
            }

            /// <summary>
            /// 指定の位置にあるクラスタとして初期化します。
            /// </summary>
            /// <param name="position">クラスタ空間におけるクラスタの位置。</param>
            internal void Initialize(VectorI3 position)
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
            internal void Release()
            {
                Position = VectorI3.Zero;
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
            internal bool Contains(VectorI3 position)
            {
                return partitionsByPosition.ContainsKey(position);
            }

            /// <summary>
            /// 指定の位置にあるパーティションを削除します。
            /// </summary>
            /// <param name="position">パーティションの位置。</param>
            internal bool Remove(VectorI3 position)
            {
                return partitionsByPosition.Remove(position);
            }

            /// <summary>
            /// 全てのパーティションを削除します。
            /// </summary>
            internal void Clear()
            {
                partitionsByPosition.Clear();
            }
        }

        #endregion

        /// <summary>
        /// パーティション空間におけるクラスタのサイズ。
        /// </summary>
        readonly VectorI3 size;

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
        Dictionary<VectorI3, Cluster> clustersByPosition;

        /// <summary>
        /// クラスタ数を取得します。
        /// </summary>
        internal int Count
        {
            get { return clustersByPosition.Count; }
        }

        /// <summary>
        /// 指定の位置についてパーティションを取得または設定します。
        /// </summary>
        /// <param name="position">パーティションの位置。</param>
        /// <returns>
        /// パーティション、あるいは、指定の位置にパーティションが存在しない場合は null。
        /// </returns>
        internal Partition this[VectorI3 position]
        {
            get
            {
                var cluster = GetCluster(position);
                if (cluster == null) return null;

                return cluster[position];
            }
            set
            {
                VectorI3 clusterPosition;
                CalculateClusterPosition(ref position, out clusterPosition);

                Cluster cluster;
                if (!clustersByPosition.TryGetValue(clusterPosition, out cluster))
                {
                    cluster = clusterPool.Borrow();
                    cluster.Initialize(clusterPosition);
                    clustersByPosition[clusterPosition] = cluster;
                }

                cluster[position] = value;
            }
        }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        /// <param name="size">パーティション空間におけるクラスタのサイズ。</param>
        /// <param name="partitionSize">ワールド空間におけるパーティションのサイズ。</param>
        internal ClusterManager(VectorI3 size, Vector3 partitionSize)
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
            clustersByPosition = new Dictionary<VectorI3, Cluster>();
        }

        /// <summary>
        /// 指定の位置にパーティションが存在するか否かを検査します。
        /// </summary>
        /// <param name="position">パーティション空間におけるパーティションの位置。</param>
        /// <returns>
        /// true (パーティションが存在する場合)、false (それ以外の場合)。
        /// </returns>
        internal bool Contains(VectorI3 position)
        {
            var cluster = GetCluster(position);
            if (cluster == null) return false;

            return cluster.Contains(position);
        }

        /// <summary>
        /// 指定の位置にあるパーティションを削除します。
        /// パーティションの削除により、それを管理していたクラスタにパーティションがゼロになる場合、
        /// クラスタを削除してプールへ戻します。
        /// </summary>
        /// <param name="position">パーティションの位置。</param>
        internal bool Remove(VectorI3 position)
        {
            var cluster = GetCluster(position);
            if (cluster == null) return false;

            var result = cluster.Remove(position);

            if (cluster.Count == 0)
            {
                clustersByPosition.Remove(cluster.Position);
                clusterPool.Return(cluster);
            }

            return result;
        }

        /// <summary>
        /// 全てのパーティションおよび全てのクラスタを削除します。
        /// </summary>
        internal void Clear()
        {
            foreach (var cluster in clustersByPosition.Values)
                cluster.Clear();

            clustersByPosition.Clear();
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
            clustersByPosition.TryGetValue(clusterPosition, out result);
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
