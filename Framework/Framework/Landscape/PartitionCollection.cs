#region Using

using System;
using Willcraftia.Xna.Framework.Collections;

#endregion

namespace Willcraftia.Xna.Framework.Landscape
{
    /// <summary>
    /// パーティションのコレクションです。
    /// コレクション内のパーティションは、パーティションの位置をキーとしても管理されます。
    /// </summary>
    internal sealed class PartitionCollection : KeyedList<VectorI3, Partition>
    {
        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        /// <param name="capacity">初期容量。</param>
        internal PartitionCollection(int capacity)
            : base(capacity)
        {
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
