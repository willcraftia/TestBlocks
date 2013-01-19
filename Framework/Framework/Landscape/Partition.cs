﻿#region Using

using System;
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
        public VectorI3 Position;

        /// <summary>
        /// ワールド空間におけるパーティションの原点位置。
        /// </summary>
        public Vector3 PositionWorld;

        /// <summary>
        /// ワールド空間におけるパーティションの境界ボックス。
        /// </summary>
        public BoundingBox BoundingBox;

        /// <summary>
        /// ワールド空間におけるパーティションの中心位置。
        /// </summary>
        public Vector3 Center;

        /// <summary>
        /// 非同期な Activate() あるいは Passivate() の呼び出しが終わるまで、
        /// Dispose() の実行を待機するためのシグナルを管理します。
        /// </summary>
        ManualResetEvent asyncCallEvent = new ManualResetEvent(true);

        /// <summary>
        /// アクティブ化が完了しているか否かを示す値。
        /// </summary>
        volatile bool activationCompleted;

        /// <summary>
        /// 非アクティブ化が完了しているか否かを示す値。
        /// </summary>
        volatile bool passivationCompleted;

        /// <summary>
        /// 非同期なアクティブ化処理が終了しているかどうかを示す値を取得または設定します。
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
        /// 非同期な非アクティブ化処理が終了しているかどうかを示す値を取得または設定します。
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
        /// Activate() メソッドのデリゲートです。
        /// 非同期処理の要求毎にデリゲート インスタンスが生成されることを回避するために用います。
        /// </summary>
        internal Action ActivateAction { get; private set; }

        /// <summary>
        /// Passivate() メソッドのデリゲートです。
        /// 非同期処理の要求毎にデリゲート インスタンスが生成されることを回避するために用います。
        /// </summary>
        internal Action PassivateAction { get; private set; }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        protected Partition()
        {
            ActivateAction = new Action(Activate);
            PassivateAction = new Action(Passivate);
        }

        /// <summary>
        /// パーティションに対するロックの取得を試行します。
        /// 他のスレッドがロック中の場合は、ロックの取得に失敗します。
        /// ロックの取得に成功した場合は、
        /// 必ず ExitLock メソッドでロックを解放しなければなりません。
        /// </summary>
        /// <returns>
        /// true (ロックを取得できた場合)、false (それ以外の場合)。
        /// </returns>
        public bool EnterLock()
        {
            return Monitor.TryEnter(this);
        }

        /// <summary>
        /// EnterLock メソッドで取得した更新ロックを開放します。
        /// </summary>
        public void ExitLock()
        {
            Monitor.Exit(this);
        }

        /// <summary>
        /// アクティブ パーティション領域に含まれるか否かを検査します。
        /// </summary>
        /// <param name="volume">アクティブ パーティション領域。</param>
        /// <returns>
        /// true (アクティブ パーティション領域に含まれる場合)、false (それ以外の場合)。
        /// </returns>
        internal bool IsInLandscapeVolume(ILandscapeVolume volume)
        {
            bool result;
            volume.Contains(ref Position, out result);
            return result;
        }

        /// <summary>
        /// パーティションを初期化します。
        /// このメソッドは、パーティション プールから取り出され、
        /// アクティブ化が要求されるまえに呼び出されます。
        /// 戻り値が false を返す場合、パーティションのアクティブ化は取り消されます。
        /// </summary>
        /// <param name="position">パーティション空間におけるパーティションの位置。</param>
        /// <param name="partitionSize">ワールド空間におけるパーティションのサイズ。</param>
        /// <returns>
        /// true (初期化に成功した場合)、false (それ以外の場合)。
        /// </returns>
        internal bool Initialize(VectorI3 position, Vector3 partitionSize)
        {
            Position = position;

            PositionWorld = new Vector3
            {
                X = position.X * partitionSize.X,
                Y = position.Y * partitionSize.Y,
                Z = position.Z * partitionSize.Z,
            };

            BoundingBox.Min = PositionWorld;
            BoundingBox.Max = PositionWorld + partitionSize;

            Center = (BoundingBox.Max + BoundingBox.Min) / 2;

            activationCompleted = false;
            passivationCompleted = false;

            return InitializeOverride();
        }

        /// <summary>
        /// パーティションを開放します。
        /// このメソッドは、非アクティブ化が成功し、
        /// パーティションがプールへ戻される直前に呼び出されます。
        /// </summary>
        internal void Release()
        {
            Position = VectorI3.Zero;

            activationCompleted = false;
            passivationCompleted = false;

            ReleaseOverride();
        }

        /// <summary>
        /// パーティションのアクティブ化を試行します。
        /// このメソッドは非同期に呼び出されます。
        /// </summary>
        internal void Activate()
        {
            Debug.Assert(!activationCompleted);

            asyncCallEvent.Reset();

            ActivateOverride();
            activationCompleted = true;
            
            asyncCallEvent.Set();
        }

        /// <summary>
        /// パーティションの非アクティブ化を試行します。
        /// このメソッドは非同期に呼び出されます。
        /// </summary>
        internal void Passivate()
        {
            Debug.Assert(activationCompleted);
            Debug.Assert(!passivationCompleted);

            asyncCallEvent.Reset();

            PassivateOverride();
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
        /// <returns>
        /// true (初期化に成功した場合)、false (それ以外の場合)。
        /// </returns>
        protected virtual bool InitializeOverride() { return true; }

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
