#region Using

using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework;
using Willcraftia.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework.Landscape
{
    public abstract class Partition : IDisposable
    {
        /// <summary>
        /// 非同期な Activate() あるいは Passivate() の呼び出しが終わるまで、
        /// Dispose() の実行を待機するためのシグナルを管理します。
        /// </summary>
        ManualResetEvent asyncCallEvent = new ManualResetEvent(true);

        VectorI3 position;

        volatile bool activationCompleted;

        volatile bool activationCanceled;

        volatile bool passivationCompleted;

        volatile bool passivationCanceled;

        /// <summary>
        /// 座標を取得します。
        /// </summary>
        /// <value>
        /// パーティション空間における座標。
        /// </value>
        public VectorI3 Position
        {
            get { return position; }
        }

        /// <summary>
        /// 非同期なアクティブ化処理が終了しているかどうかを示す値を取得または設定します。
        /// アクティブ化の成功可否は ActivationCanceled プロパティで判定する必要があります。
        /// </summary>
        /// <value>
        /// true (非同期なアクティブ化処理が終了している場合)、false (それ以外の場合)。
        /// </value>
        internal bool ActivationCompleted
        {
            get { return activationCompleted; }
            set { activationCompleted = value; }
        }

        /// <summary>
        /// アクティブ化処理中に処理が取り消されているかどうかを示す値を取得または設定します。
        /// このプロパティは ActivationCompleted が true の場合にのみ有効です。
        /// </summary>
        /// <value>
        /// true (アクティブ化処理中に処理が取り消されている場合)、false (それ以外の場合)。
        /// </value>
        internal bool ActivationCanceled
        {
            get { return activationCanceled; }
            set { activationCanceled = value; }
        }

        /// <summary>
        /// 非同期な非アクティブ化処理が終了しているかどうかを示す値を取得または設定します。
        /// 非アクティブ化の成功可否は PassivationCanceled プロパティで判定する必要があります。
        /// </summary>
        /// <value>
        /// true (非同期な非アクティブ化処理が終了している場合)、false (それ以外の場合)。
        /// </value>
        internal bool PassivationCompleted
        {
            get { return passivationCompleted; }
            set { passivationCompleted = value; }
        }

        /// <summary>
        /// 非アクティブ化処理中に処理が取り消されているかどうかを示す値を取得または設定します。
        /// このプロパティは PassivationCompleted が true の場合にのみ有効です。
        /// </summary>
        /// <value>
        /// true (非アクティブ化処理中に処理が取り消されている場合)、false (それ以外の場合)。
        /// </value>
        internal bool PassivationCanceled
        {
            get { return passivationCanceled; }
            set { passivationCanceled = value; }
        }

        /// <summary>
        /// Activate() メソッドのデリゲートです。
        /// 非同期処理の要求毎にデリゲート インスタンスが生成されることを回避するために用います。
        /// </summary>
        internal Action ActivateAction { get; private set; }

        /// <summary>
        /// Passivate() メソッドのデリゲートです。
        /// 非同期処理の要求毎にデリゲート インスタンスが生成されることを回避するために用います。
        /// </summary>
        internal Action PassivateAction { get; private set; }

        protected Partition()
        {
            ActivateAction = new Action(Activate);
            PassivateAction = new Action(Passivate);
        }

        internal bool IsInBounds(ref PartitionSpaceBounds volume)
        {
            bool result;
            volume.Contains(ref position, out result);
            return result;
        }

        /// <summary>
        /// パーティションを初期化します。
        /// このメソッドは、パーティション プールから取り出され、
        /// アクティブ化が要求されるまえに呼び出されます。
        /// </summary>
        /// <param name="position">パーティションの座標。</param>
        internal void Initialize(ref VectorI3 position)
        {
            this.position = position;

            activationCompleted = false;
            activationCanceled = false;
            passivationCompleted = false;
            passivationCanceled = false;

            InitializeOverride();
        }

        /// <summary>
        /// パーティションを開放します。
        /// このメソッドは、非アクティブ化が成功し、
        /// パーティションがプールへ戻される直前に呼び出されます。
        /// </summary>
        internal void Release()
        {
            position = VectorI3.Zero;

            ReleaseOverride();
        }

        /// <summary>
        /// パーティションのアクティブ化を試行します。
        /// このメソッドは非同期に呼び出されます。
        /// </summary>
        internal void Activate()
        {
            Debug.Assert(!activationCompleted);
            Debug.Assert(!activationCanceled);

            asyncCallEvent.Reset();

            activationCanceled = !ActivateOverride();
            activationCompleted = true;
            
            asyncCallEvent.Set();
        }

        /// <summary>
        /// パーティションの非アクティブ化を試行します。
        /// このメソッドは非同期に呼び出されます。
        /// </summary>
        internal void Passivate()
        {
            Debug.Assert(activationCompleted && !activationCanceled);
            Debug.Assert(!passivationCompleted);

            asyncCallEvent.Reset();

            passivationCanceled = !PassivateOverride();
            passivationCompleted = true;

            asyncCallEvent.Set();
        }

        /// <summary>
        /// 隣接パーティションがアクティブになった時に呼び出されます。
        /// </summary>
        /// <param name="neighbor">アクティブになった隣接パーティション。</param>
        /// <param name="side">隣接パーティションの方向。</param>
        public virtual void OnNeighborActivated(Partition neighbor, CubicSide side) { }

        /// <summary>
        /// 隣接パーティションが非アクティブになった時に呼び出されます。
        /// </summary>
        /// <param name="neighbor">非アクティブになった隣接パーティション。</param>
        /// <param name="side">隣接パーティションの方向。</param>
        public virtual void OnNeighborPassivated(Partition neighbor, CubicSide side) { }

        /// <summary>
        /// パーティションの初期化で呼び出されます。
        /// </summary>
        protected virtual void InitializeOverride() { }

        /// <summary>
        /// アクティブ化を試行する際に呼び出されます。
        /// このメソッドをオーバライドしてアクティブ化の詳細を実装します。
        /// 戻り値では、アクティブ化を完了した場合に true、
        /// 途中で取り消した場合に false を返すようにします。
        /// </summary>
        /// <returns>
        /// true (アクティブ化を完了した場合)、false (アクティブ化を取り消した場合)。
        /// </returns>
        protected virtual bool ActivateOverride() { return true; }

        /// <summary>
        /// 非アクティブ化を試行する際に呼び出されます。
        /// このメソッドをオーバライドして非アクティブ化の詳細を実装します。
        /// 戻り値では、非アクティブ化を完了した場合に true、
        /// 途中で取り消した場合に false を返すようにします。
        /// </summary>
        /// <returns>
        /// true (非アクティブ化を完了した場合)、false (非アクティブ化を取り消した場合)。
        /// </returns>
        protected virtual bool PassivateOverride() { return true; }

        /// <summary>
        /// パーティションの解放で呼び出されます。
        /// </summary>
        protected virtual void ReleaseOverride() { }

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
