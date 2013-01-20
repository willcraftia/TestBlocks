#region Using

using System;
using Willcraftia.Xna.Framework.Collections;

#endregion

namespace Willcraftia.Xna.Framework.Landscape
{
    /// <summary>
    /// パーティションのキューです。
    /// キュー内のパーティションは、パーティションの位置をキーとしても管理されます。
    /// </summary>
    internal sealed class PartitionQueue : KeyedQueue<VectorI3, Partition>
    {
        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        /// <param name="capacity">初期容量。</param>
        internal PartitionQueue(int capacity)
            : base(capacity)
        {
        }

        /// <summary>
        /// パーティションの取得を試行します。
        /// </summary>
        /// <param name="key">パーティションの位置。</param>
        /// <param name="item">
        /// パーティション、あるいは、パーティションが存在しない場合は null。
        /// </param>
        /// <returns>
        /// true (パーティションが存在する場合)、false (それ以外の場合)。
        /// </returns>
        internal bool TryGetItem(ref VectorI3 key, out Partition item)
        {
            return Dictionary.TryGetValue(key, out item);
        }

        /// <summary>
        /// パーティションの位置をキーとして返します。
        /// </summary>
        /// <param name="item">パーティション。</param>
        /// <returns>パーティションの位置。</returns>
        protected override VectorI3 GetKeyForItem(Partition item)
        {
            return item.Position;
        }
    }
}
