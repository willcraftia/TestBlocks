#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Landscape
{
    public sealed class PartitionManagerMonitor
    {
        public event EventHandler BeginUpdate = delegate { };

        public event EventHandler EndUpdate = delegate { };

        public event EventHandler BeginCheckPassivationCompleted = delegate { };

        public event EventHandler EndCheckPassivationCompleted = delegate { };

        public event EventHandler BeginCheckActivationCompleted = delegate { };

        public event EventHandler EndCheckActivationCompleted = delegate { };

        public event EventHandler BeginPassivatePartitions = delegate { };

        public event EventHandler EndPassivatePartitions = delegate { };

        public event EventHandler BeginActivatePartitions = delegate { };

        public event EventHandler EndActivatePartitions = delegate { };

        PartitionManager partitionManager;

        public int ActiveClusterCount { get; set; }

        public int ActivePartitionCount { get; set; }

        public int ActivatingPartitionCount { get; set; }

        public int PassivatingPartitionCount { get; set; }

        public PartitionManagerMonitor(PartitionManager partitionManager)
        {
            if (partitionManager == null) throw new ArgumentNullException("partitionManager");

            this.partitionManager = partitionManager;
        }

        internal void OnBeginUpdate()
        {
            BeginUpdate(partitionManager, EventArgs.Empty);
        }

        internal void OnEndUpdate()
        {
            EndUpdate(partitionManager, EventArgs.Empty);
        }

        internal void OnBeginCheckPassivationCompleted()
        {
            BeginCheckPassivationCompleted(partitionManager, EventArgs.Empty);
        }

        internal void OnEndCheckPassivationCompleted()
        {
            EndCheckPassivationCompleted(partitionManager, EventArgs.Empty);
        }

        internal void OnBeginCheckActivationCompleted()
        {
            BeginCheckActivationCompleted(partitionManager, EventArgs.Empty);
        }

        internal void OnEndCheckActivationCompleted()
        {
            EndCheckActivationCompleted(partitionManager, EventArgs.Empty);
        }

        internal void OnBeginPassivatePartitions()
        {
            BeginPassivatePartitions(partitionManager, EventArgs.Empty);
        }

        internal void OnEndPassivatePartitions()
        {
            EndPassivatePartitions(partitionManager, EventArgs.Empty);
        }

        internal void OnBeginActivatePartitions()
        {
            BeginActivatePartitions(partitionManager, EventArgs.Empty);
        }

        internal void OnEndActivatePartitions()
        {
            EndActivatePartitions(partitionManager, EventArgs.Empty);
        }
    }
}
