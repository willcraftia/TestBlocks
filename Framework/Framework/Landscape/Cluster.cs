#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

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
    public sealed class Cluster
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
        Dictionary<VectorI3, Partition> partitions;

        /// <summary>
        /// パーティション数を取得します。
        /// </summary>
        public int Count
        {
            get { return partitions.Count; }
        }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        /// <param name="manager">クラスタ マネージャ。</param>
        internal Cluster(ClusterManager manager)
        {
            if (manager == null) throw new ArgumentNullException("manager");

            this.manager = manager;

            var size = manager.Size;
            partitions = new Dictionary<VectorI3, Partition>(size.X * size.Y * size.Z);
        }

        /// <summary>
        /// 指定の位置にあるクラスタとして初期化します。
        /// </summary>
        /// <param name="position">クラスタ空間におけるクラスタの位置。</param>
        internal void Initialize(VectorI3 position)
        {
            Position = position;

            var sizeWorld = manager.SizeWorld;
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
        /// 境界錐台と交差するパーティションを収集します。
        /// </summary>
        /// <param name="frustum">境界錐台。</param>
        /// <param name="collector">収集先パーティションのコレクション。</param>
        public void CollectPartitions(BoundingFrustum frustum, ICollection<Partition> collector)
        {
            foreach (var partition in partitions.Values)
            {
                bool intersected;
                frustum.Intersects(ref partition.BoundingBox, out intersected);

                if (intersected) collector.Add(partition);
            }
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
            return partitions.ContainsKey(position);
        }

        /// <summary>
        /// パーティションを取得します。
        /// </summary>
        /// <param name="position">パーティションの位置。</param>
        /// <returns>パーティション。</returns>
        public Partition GetPartition(ref VectorI3 position)
        {
            return partitions[position];
        }

        /// <summary>
        /// パーティションの取得を試行します。
        /// </summary>
        /// <param name="position">パーティションの位置。</param>
        /// <param name="result">
        /// パーティション、あるいは、パーティションが存在しない場合には null。
        /// </param>
        /// <returns>
        /// true (パーティションが存在する場合)、false (それ以外の場合)。
        /// </returns>
        public bool TryGetPartition(ref VectorI3 position, out Partition result)
        {
            return partitions.TryGetValue(position, out result);
        }

        /// <summary>
        /// クラスタへパーティションを追加します。
        /// </summary>
        /// <param name="partition">パーティション。</param>
        internal void AddPartition(Partition partition)
        {
            partitions[partition.Position] = partition;
        }

        /// <summary>
        /// クラスタからパーティションを削除します。
        /// </summary>
        /// <param name="position">パーティションの位置。</param>
        internal void RemovePartition(ref VectorI3 position)
        {
            partitions.Remove(position);
        }
    }
}
