#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework;
using Willcraftia.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework.Landscape
{
    /// <summary>
    /// パーティションを表すクラスです。
    /// </summary>
    public abstract class Partition
    {
        /// <summary>
        /// パーティション空間におけるパーティションの位置。
        /// </summary>
        public IntVector3 Position;

        /// <summary>
        /// アクティブ化が完了しているか否かを示す値。
        /// </summary>
        /// <value>
        /// true (非同期な非アクティブ化処理が終了している場合)、false (それ以外の場合)。
        /// </value>
        volatile bool activationCompleted;

        /// <summary>
        /// パーティションがアクティブであるか否かを示す値を取得します。
        /// パーティションのアクティブ化が完了してから非アクティブ化が開始するまでの間、
        /// このプロパティは true を返します。
        /// </summary>
        /// <value>
        /// true (パーティションがアクティブである場合)、false (それ以外の場合)。
        /// </value>
        public bool Active { get; private set; }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        protected Partition() { }

        /// <summary>
        /// パーティションのアクティブ化を試行します。
        /// このメソッドは非同期に呼び出されます。
        /// </summary>
        internal void ActivateAsync()
        {
            ActivateOverride();
            activationCompleted = true;
        }

        /// <summary>
        /// パーティションの非アクティブ化を試行します。
        /// このメソッドは非同期に呼び出されます。
        /// </summary>
        internal void PassivateAsync()
        {
            Debug.Assert(activationCompleted);

            PassivateOverride();
        }

        /// <summary>
        /// 隣接パーティションがアクティブになった時に呼び出されます。
        /// </summary>
        /// <param name="neighbor">アクティブになった隣接パーティション。</param>
        /// <param name="side">隣接パーティションの方向。</param>
        protected internal virtual void OnNeighborActivated(Partition neighbor, Side side) { }

        /// <summary>
        /// 隣接パーティションが非アクティブになった時に呼び出されます。
        /// </summary>
        /// <param name="neighbor">非アクティブになった隣接パーティション。</param>
        /// <param name="side">隣接パーティションの方向。</param>
        protected internal virtual void OnNeighborPassivated(Partition neighbor, Side side) { }

        /// <summary>
        /// アクティブ化の完了直後で呼び出されます。
        /// </summary>
        protected internal virtual void OnActivated()
        {
            Active = true;
        }

        /// <summary>
        /// 非アクティブ化の開始直前で呼び出されます。
        /// </summary>
        protected internal virtual void OnPassivating()
        {
            Active = false;
        }

        /// <summary>
        /// アクティブ化を試行する際に呼び出されます。
        /// このメソッドをオーバライドしてアクティブ化の詳細を実装します。
        /// 戻り値では、アクティブ化を完了した場合に true、
        /// 途中で取り消した場合に false を返すようにします。
        /// </summary>
        /// <returns>
        /// true (アクティブ化を完了した場合)、false (アクティブ化を取り消した場合)。
        /// </returns>
        protected virtual void ActivateOverride() { }

        /// <summary>
        /// 非アクティブ化を試行する際に呼び出されます。
        /// このメソッドをオーバライドして非アクティブ化の詳細を実装します。
        /// 戻り値では、非アクティブ化を完了した場合に true、
        /// 途中で取り消した場合に false を返すようにします。
        /// </summary>
        /// <returns>
        /// true (非アクティブ化を完了した場合)、false (非アクティブ化を取り消した場合)。
        /// </returns>
        protected virtual void PassivateOverride() { }
    }
}
