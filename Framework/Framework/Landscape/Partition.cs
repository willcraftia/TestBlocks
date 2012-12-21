#region Using

using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Willcraftia.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework.Landscape
{
    public abstract class Partition : IDisposable
    {
        // MEMO
        //
        // ロードの取消は考えないこととする。
        // ロードの取消を考える程に、頻繁なパッシベーションの発生を事前に抑制するようにコーディングする。
        // 明らかに通常ではない高負荷を掛ける場合は、頻繁なパッシベーションの発生をやむなしとする。

        ManualResetEvent asyncCallEvent = new ManualResetEvent(true);

        //====================================================================
        // Efficiency

        // A position in the partition space.
        public VectorI3 GridPosition;

        //
        //====================================================================

        public bool IsActivationCompleted { get; private set; }

        public bool IsPassivationCompleted { get; private set; }

        public bool IsPassivationFailed { get; private set; }

        protected Partition() { }

        // 同期呼び出し。
        // Initialize() 呼び出しの前に、Size、GridPosition は設定済み。
        public void Initialize()
        {
            IsActivationCompleted = false;
            IsPassivationCompleted = false;
            IsPassivationFailed = false;

            InitializeOverride();
        }

        // 非同期に呼び出される。
        // Activate() が呼び出される Partition は、PartitionManager の activatingParitions リスト内にある。
        // activatingParitions リスト内の Partition は、Passivate() を呼び出されることはない。
        public void Activate()
        {
            asyncCallEvent.Reset();

            ActivateOverride();

            IsActivationCompleted = true;
            
            asyncCallEvent.Set();
        }

        // 非同期に呼び出される。
        // UnloadContent() が呼び出される Partiton は、PartitionManager の passivatingPartitions リスト内にある。
        // passivatingPartitions リスト内の Partition は、LoadContent() を呼び出されることはない。
        //
        // passivatingPartitions リスト内の Partition に対して、非同期な状態変更を行なってはならない。
        // 
        public void Passivate()
        {
            asyncCallEvent.Reset();

            if (IsActivationCompleted)
            {
                IsPassivationFailed = false;

                if (PassivateOverride())
                {
                    IsPassivationCompleted = true;
                }
                else
                {
                    IsPassivationFailed = true;
                }
            }

            asyncCallEvent.Set();
        }

        public virtual void OnNeighborActivated(Partition neighbor) { }

        protected virtual void InitializeOverride() { }

        protected virtual void ActivateOverride() { }

        protected virtual bool PassivateOverride() { return true; }

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

            // Wait.
            asyncCallEvent.WaitOne();

            // Dispose 中にパッシベートするのは危険。
            // パッシベートでファイルへの永続化を行うなどの場合、
            // 永続化を担うクラス、例えば、StorageContainer などが、
            // その時点で既に Dispose されている可能性がある。
            //Passivate();
            DisposeOverride(disposing);

            asyncCallEvent.Dispose();

            disposed = true;
        }

        #endregion
    }
}
