#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Landscape
{
    /// <summary>
    /// パーティション マネージャの動作をモニタリングするためのクラスです。
    /// </summary>
    public sealed class PartitionManagerMonitor
    {
        /// <summary>
        /// Update メソッド開始イベント。
        /// </summary>
        public event EventHandler BeginUpdate = delegate { };

        /// <summary>
        /// Update メソッド終了イベント。
        /// </summary>
        public event EventHandler EndUpdate = delegate { };

        /// <summary>
        /// CheckPassivationCompleted メソッド開始イベント。
        /// </summary>
        public event EventHandler BeginCheckPassivationCompleted = delegate { };

        /// <summary>
        /// CheckPassivationCompleted メソッド終了イベント。
        /// </summary>
        public event EventHandler EndCheckPassivationCompleted = delegate { };

        /// <summary>
        /// CheckActivationCompleted メソッド開始イベント。
        /// </summary>
        public event EventHandler BeginCheckActivationCompleted = delegate { };

        /// <summary>
        /// CheckActivationCompleted メソッド終了イベント。
        /// </summary>
        public event EventHandler EndCheckActivationCompleted = delegate { };

        /// <summary>
        /// PassivatePartitions メソッド開始イベント。
        /// </summary>
        public event EventHandler BeginPassivatePartitions = delegate { };

        /// <summary>
        /// PassivatePartitions メソッド終了イベント。
        /// </summary>
        public event EventHandler EndPassivatePartitions = delegate { };

        /// <summary>
        /// ActivatePartitions メソッド開始イベント。
        /// </summary>
        public event EventHandler BeginActivatePartitions = delegate { };

        /// <summary>
        /// ActivatePartitions メソッド終了イベント。
        /// </summary>
        public event EventHandler EndActivatePartitions = delegate { };

        PartitionManager partitionManager;

        /// <summary>
        /// アクティブ クラスタ数を取得します。
        /// </summary>
        public int ActiveClusterCount { get; internal set; }

        /// <summary>
        /// アクティブ パーティション数を取得します。
        /// </summary>
        public int ActivePartitionCount { get; internal set; }

        /// <summary>
        /// アクティブ化中パーティション数を取得します。
        /// </summary>
        public int ActivatingPartitionCount { get; internal set; }

        /// <summary>
        /// 非アクティブ化中パーティション数を取得します。
        /// </summary>
        public int PassivatingPartitionCount { get; internal set; }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        /// <param name="partitionManager">パーティション マネージャ。</param>
        internal PartitionManagerMonitor(PartitionManager partitionManager)
        {
            if (partitionManager == null) throw new ArgumentNullException("partitionManager");

            this.partitionManager = partitionManager;
        }

        /// <summary>
        /// BeginUpdate イベントを発生させます。
        /// </summary>
        internal void OnBeginUpdate()
        {
            BeginUpdate(partitionManager, EventArgs.Empty);
        }

        /// <summary>
        /// EndUpdate イベントを発生させます。
        /// </summary>
        internal void OnEndUpdate()
        {
            EndUpdate(partitionManager, EventArgs.Empty);
        }

        /// <summary>
        /// BeginCheckPassivationCompleted イベントを発生させます。
        /// </summary>
        internal void OnBeginCheckPassivationCompleted()
        {
            BeginCheckPassivationCompleted(partitionManager, EventArgs.Empty);
        }

        /// <summary>
        /// EndCheckPassivationCompleted イベントを発生させます。
        /// </summary>
        internal void OnEndCheckPassivationCompleted()
        {
            EndCheckPassivationCompleted(partitionManager, EventArgs.Empty);
        }

        /// <summary>
        /// BeginCheckActivationCompleted イベントを発生させます。
        /// </summary>
        internal void OnBeginCheckActivationCompleted()
        {
            BeginCheckActivationCompleted(partitionManager, EventArgs.Empty);
        }

        /// <summary>
        /// EndCheckActivationCompleted イベントを発生させます。
        /// </summary>
        internal void OnEndCheckActivationCompleted()
        {
            EndCheckActivationCompleted(partitionManager, EventArgs.Empty);
        }

        /// <summary>
        /// BeginPassivatePartitions イベントを発生させます。
        /// </summary>
        internal void OnBeginPassivatePartitions()
        {
            BeginPassivatePartitions(partitionManager, EventArgs.Empty);
        }

        /// <summary>
        /// EndPassivatePartitions イベントを発生させます。
        /// </summary>
        internal void OnEndPassivatePartitions()
        {
            EndPassivatePartitions(partitionManager, EventArgs.Empty);
        }

        /// <summary>
        /// BeginActivatePartitions イベントを発生させます。
        /// </summary>
        internal void OnBeginActivatePartitions()
        {
            BeginActivatePartitions(partitionManager, EventArgs.Empty);
        }

        /// <summary>
        /// EndActivatePartitions イベントを発生させます。
        /// </summary>
        internal void OnEndActivatePartitions()
        {
            EndActivatePartitions(partitionManager, EventArgs.Empty);
        }
    }
}
