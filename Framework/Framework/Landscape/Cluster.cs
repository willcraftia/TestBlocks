#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Willcraftia.Xna.Framework.Collections;

#endregion

namespace Willcraftia.Xna.Framework.Landscape
{
    /// <summary>
    /// ある領域に含まれるパーティションを一つの集合として管理するクラスです。
    /// パーティションのアクティブ化および非アクティブ化では、
    /// 対象とするパーティションの位置からパーティションを検索しますが、
    /// 検索対象はパーティション数に比例するため、
    /// パーティション数が増加する程に検索負荷が増加する問題があります。
    /// この問題を解決するために、パーティションをクラスタで纏めて管理し、
    /// 検索の際には、クラスタを検索した後、クラスタからパーティションを検索するという方法を採ります。
    /// </summary>
    internal sealed class Cluster
    {
        #region Collector

        struct Collector
        {
            public Vector3 PartitionSize;

            public Vector3 ClusterPositionWorld;

            public BoundingFrustum Frustum;

            public ICollection<Partition> List;

            public bool IntersectFrustum(Octree<Partition>.Node node)
            {
                var relativePosition = new Vector3
                {
                    X = node.Origin.X * PartitionSize.X,
                    Y = node.Origin.Y * PartitionSize.Y,
                    Z = node.Origin.Z * PartitionSize.Z
                };

                var sizeWorld = PartitionSize * node.Size;
                var positionWorld = ClusterPositionWorld + relativePosition;

                var nodeBox = new BoundingBox
                {
                    Min = positionWorld,
                    Max = positionWorld + sizeWorld
                };

                bool intersected;
                Frustum.Intersects(ref nodeBox, out intersected);

                return intersected;
            }

            public void Add(Octree<Partition>.Leaf leaf)
            {
                if (leaf.Item != null)
                {
                    List.Add(leaf.Item);
                }
            }
        }

        #endregion

        /// <summary>
        /// クラスタ空間におけるクラスタの位置。
        /// </summary>
        internal VectorI3 Position;

        /// <summary>
        /// ワールド空間におけるクラスタの境界ボックス。
        /// </summary>
        internal BoundingBox Box;

        /// <summary>
        /// クラスタ マネージャ。
        /// </summary>
        ClusterManager manager;

        /// <summary>
        /// パーティションを管理する八分木。
        /// </summary>
        Octree<Partition> octree;

        /// <summary>
        /// パーティション数を取得します。
        /// </summary>
        internal int Count { get; private set; }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        /// <param name="manager">クラスタ マネージャ。</param>
        internal Cluster(ClusterManager manager)
        {
            if (manager == null) throw new ArgumentNullException("manager");

            this.manager = manager;

            octree = new Octree<Partition>(manager.Dimension);
        }

        /// <summary>
        /// 指定の位置にあるクラスタとして初期化します。
        /// </summary>
        /// <param name="position">クラスタ空間におけるクラスタの位置。</param>
        internal void Initialize(VectorI3 position)
        {
            Position = position;

            var sizeWorld = manager.PartitionSize * manager.Dimension;
            var positionWorld = new Vector3
            {
                X = Position.X * sizeWorld.X,
                Y = Position.Y * sizeWorld.Y,
                Z = Position.Z * sizeWorld.Z
            };

            Box.Min = positionWorld;
            Box.Max = positionWorld + sizeWorld;
        }

        /// <summary>
        /// クラスタを開放します。
        /// </summary>
        internal void Release()
        {
            Position = VectorI3.Zero;
            Box = BoundingBoxHelper.Empty;
        }

        /// <summary>
        /// 境界錐台と交差するパーティションを収集します。
        /// </summary>
        /// <param name="frustum">境界錐台。</param>
        /// <param name="result">収集先パーティションのコレクション。</param>
        internal void CollectPartitions(BoundingFrustum frustum, ICollection<Partition> result)
        {
            var collector = new Collector
            {
                ClusterPositionWorld = Box.Min,
                PartitionSize = manager.PartitionSize,
                Frustum = frustum,
                List = result,
            };

            octree.Execute(collector.Add, collector.IntersectFrustum);
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
            var relativePosition = position - Position * manager.Dimension;
            return octree[relativePosition] != null;
        }

        /// <summary>
        /// 指定の位置にあるパーティションを取得します。
        /// </summary>
        /// <param name="position">パーティションの位置。</param>
        /// <returns>
        /// パーティション、あるいは、指定の位置にパーティションが存在しないならば null。
        /// </returns>
        internal Partition GetPartition(VectorI3 position)
        {
            var relativePosition = position - Position * manager.Dimension;
            return octree[relativePosition];
        }

        /// <summary>
        /// パーティションを追加します。
        /// </summary>
        /// <param name="partition">パーティション。</param>
        internal void Add(Partition partition)
        {
            var relativePosition = partition.Position - Position * manager.Dimension;
            octree[relativePosition] = partition;

            Count++;
        }

        /// <summary>
        /// パーティションを削除します。
        /// </summary>
        /// <param name="partition">パーティション。</param>
        internal void Remove(Partition partition)
        {
            var relativePosition = partition.Position - Position * manager.Dimension;
            octree.RemoveItem(relativePosition);

            Count--;
        }
    }
}
