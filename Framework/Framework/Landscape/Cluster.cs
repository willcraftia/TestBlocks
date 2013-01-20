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
    internal sealed class Cluster
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
        Dictionary<VectorI3, Partition> dictionary;

        /// <summary>
        /// パーティション数を取得します。
        /// </summary>
        internal int Count
        {
            get { return dictionary.Count; }
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
            dictionary = new Dictionary<VectorI3, Partition>(size.X * size.Y * size.Z);
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
        internal void CollectPartitions<T>(BoundingFrustum frustum, ICollection<T> collector) where T : Partition
        {
            foreach (var partition in dictionary.Values)
            {
                bool intersected;
                frustum.Intersects(ref partition.BoundingBox, out intersected);

                if (intersected) collector.Add(partition as T);
            }
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
            return dictionary.ContainsKey(position);
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
            Partition result;
            dictionary.TryGetValue(position, out result);
            return result;
        }

        /// <summary>
        /// クラスタへパーティションを追加します。
        /// </summary>
        /// <param name="partition">パーティション。</param>
        internal void Add(Partition partition)
        {
            dictionary[partition.Position] = partition;
        }

        /// <summary>
        /// クラスタからパーティションを削除します。
        /// </summary>
        /// <param name="position">パーティションの位置。</param>
        internal void Remove(VectorI3 position)
        {
            dictionary.Remove(position);
        }
    }
}
