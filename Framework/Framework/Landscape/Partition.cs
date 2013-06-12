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
    public abstract class Partition : IDisposable
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
        protected internal volatile bool ActivationCompleted;

        /// <summary>
        /// 非アクティブ化が完了しているか否かを示す値。
        /// </summary>
        /// <value>
        /// true (非同期なアクティブ化処理が終了している場合)、false (それ以外の場合)。
        /// </value>
        protected internal volatile bool PassivationCompleted;

        /// <summary>
        /// 非アクティブ化開始状態であるか否かを示す値。
        /// </summary>
        /// <value>
        /// true (非アクティブ化開始状態である場合)、false (それ以外の場合)。
        /// </value>
        volatile bool passivating;

        /// <summary>
        /// 更新開始状態であるか否かを示す値。
        /// </summary>
        /// <value>
        /// true (更新開始状態である場合)、false (それ以外の場合)。
        /// </value>
        volatile bool updating;

        /// <summary>
        /// 非同期な Activate() あるいは Passivate() の呼び出しが終わるまで、
        /// Dispose() の実行を待機するためのシグナルを管理します。
        /// </summary>
        ManualResetEvent asyncCallEvent = new ManualResetEvent(true);

        /// <summary>
        /// 非アクティブ化開始状態であるか否かを示す値を取得します。
        /// </summary>
        /// <value>
        /// true (非アクティブ化開始状態である場合)、false (それ以外の場合)。
        /// </value>
        public bool Passivating
        {
            get { return passivating; }
        }

        /// <summary>
        /// 更新開始状態であるか否かを示す値を取得します。
        /// </summary>
        /// <value>
        /// true (更新開始状態である場合)、false (それ以外の場合)。
        /// </value>
        public bool Updating
        {
            get { return updating; }
        }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        protected Partition() { }

        /// <summary>
        /// 非アクティブ化開始状態にします。
        /// 更新開始状態の場合、非アクティブ化を開始できません。
        /// </summary>
        /// <returns>
        /// true (非アクティブ化を開始できない場合)、false (それ以外の場合)。
        /// </returns>
        public virtual bool BeginPassivation()
        {
            if (updating)
                return false;

            passivating = true;
            return true;
        }

        /// <summary>
        /// 非アクティブ化開始状態を解除します。
        /// </summary>
        public virtual void EndPassivation()
        {
            if (!passivating)
                throw new InvalidOperationException("BeginPassivation() must be called before EndPassivation().");

            passivating = false;
        }

        /// <summary>
        /// 更新開始状態にします。
        /// 非アクティブ化開始状態の場合、更新を開始できません。
        /// </summary>
        /// <returns>
        /// true (更新を開始できない場合)、false (それ以外の場合)。
        /// </returns>
        public virtual bool BeginUpdate()
        {
            if (passivating)
                return false;

            updating = true;
            return true;
        }

        /// <summary>
        /// 更新開始状態を解除します。
        /// </summary>
        public virtual void EndUpdate()
        {
            if (!updating)
                throw new InvalidOperationException("BeginUpdate() must be called before EndUpdate().");

            updating = false;
        }

        /// <summary>
        /// パーティションのアクティブ化を試行します。
        /// このメソッドは非同期に呼び出されます。
        /// </summary>
        internal void ActivateAsync()
        {
            Debug.Assert(!ActivationCompleted);
            Debug.Assert(!Passivating);

            asyncCallEvent.Reset();

            ActivateOverride();
            ActivationCompleted = true;
            
            asyncCallEvent.Set();
        }

        /// <summary>
        /// パーティションの非アクティブ化を試行します。
        /// このメソッドは非同期に呼び出されます。
        /// </summary>
        internal void PassivateAsync()
        {
            Debug.Assert(ActivationCompleted);
            Debug.Assert(!PassivationCompleted);
            Debug.Assert(Passivating);

            asyncCallEvent.Reset();

            PassivateOverride();
            PassivationCompleted = true;

            asyncCallEvent.Set();
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
        /// アクティブ化の開始直前で呼び出されます。
        /// </summary>
        protected internal virtual void OnActivating() { }

        /// <summary>
        /// アクティブ化の完了直後で呼び出されます。
        /// </summary>
        protected internal virtual void OnActivated() { }

        /// <summary>
        /// 非アクティブ化の開始直前で呼び出されます。
        /// </summary>
        protected internal virtual void OnPassivating() { }

        /// <summary>
        /// 非アクティブ化の完了直後で呼び出されます。
        /// </summary>
        protected internal virtual void OnPassivated() { }

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

        /// <summary>
        /// Dispose() メソッドが呼び出される際に呼び出されます。
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void DisposeOverride(bool disposing) { }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed;

        ~Partition()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposed) return;

            // アクティブ化あるいは非アクティブ化の実行完了を待機。
            asyncCallEvent.WaitOne();

            // ※注意
            // Dispose 中にパッシベートするのは危険。
            // パッシベートでファイルへの永続化を行うなどの場合、
            // 永続化を担うクラス、例えば、StorageContainer などが、
            // その時点で既に Dispose されている可能性がある。
            DisposeOverride(disposing);

            asyncCallEvent.Dispose();

            disposed = true;
        }

        #endregion
    }
}
