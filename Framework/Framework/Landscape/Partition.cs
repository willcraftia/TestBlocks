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

        public bool IsActivationFailed { get; private set; }

        public bool IsPassivationCompleted { get; private set; }

        public bool IsPassivationFailed { get; private set; }

        protected Partition() { }

        // 同期呼び出し。
        // Initialize() 呼び出しの前に、Size、GridPosition は設定済み。
        public void Initialize()
        {
            IsActivationCompleted = false;
            IsActivationFailed = false;
            IsPassivationCompleted = false;
            IsPassivationFailed = false;

            InitializeOverride();
        }

        // 非同期に呼び出される。
        public void Activate()
        {
            asyncCallEvent.Reset();

            if (ActivateOverride())
            {
                IsActivationFailed = false;
                IsActivationCompleted = true;
            }
            else
            {
                IsActivationFailed = true;
            }
            
            asyncCallEvent.Set();
        }

        // 非同期に呼び出される。
        public void Passivate()
        {
            asyncCallEvent.Reset();

            if (IsActivationCompleted)
            {
                if (PassivateOverride())
                {
                    IsPassivationFailed = false;
                    IsPassivationCompleted = true;
                    IsActivationCompleted = false;
                }
                else
                {
                    // リトライさせる。
                    IsPassivationFailed = true;
                }
            }

            asyncCallEvent.Set();
        }

        public virtual void OnNeighborActivated(Partition neighbor) { }

        protected virtual void InitializeOverride() { }

        protected virtual bool ActivateOverride() { return true; }

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
