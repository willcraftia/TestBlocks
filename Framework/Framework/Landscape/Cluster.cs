#region Using

using System;
using System.Collections.Generic;

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
        public VectorI3 Position;

        /// <summary>
        /// 領域幅。
        /// </summary>
        int extent;

        /// <summary>
        /// パーティションの位置をキーとするパーティションのディクショナリ。
        /// </summary>
        Dictionary<VectorI3, Partition> partitionMap;

        /// <summary>
        /// パーティション数を取得します。
        /// </summary>
        public int Count
        {
            get { return partitionMap.Count; }
        }

        /// <summary>
        /// 指定の領域幅でクラスタを生成します。
        /// </summary>
        /// <param name="extent">領域幅。</param>
        public Cluster(int extent)
        {
            this.extent = extent;

            partitionMap = new Dictionary<VectorI3, Partition>(extent * extent * extent);
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
            return partitionMap.ContainsKey(position);
        }

        /// <summary>
        /// パーティションを取得します。
        /// </summary>
        /// <param name="position">パーティションの位置。</param>
        /// <returns>パーティション。</returns>
        public Partition GetPartition(ref VectorI3 position)
        {
            return partitionMap[position];
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
            return partitionMap.TryGetValue(position, out result);
        }

        /// <summary>
        /// クラスタへパーティションを追加します。
        /// </summary>
        /// <param name="partition">パーティション。</param>
        public void AddPartition(Partition partition)
        {
            partitionMap[partition.Position] = partition;
        }

        /// <summary>
        /// クラスタからパーティションを削除します。
        /// </summary>
        /// <param name="position">パーティションの位置。</param>
        public void RemovePartition(ref VectorI3 position)
        {
            partitionMap.Remove(position);
        }
    }
}
