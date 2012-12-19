#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Collections;
using Willcraftia.Xna.Framework.Threading;

#endregion

namespace Willcraftia.Xna.Framework.Landscape
{
    //
    // このクラスは、Partition のアクティベーションとパッシベーションに専念する。
    // このクラスも Partition クラスも、Partition で管理されるクラスの状態更新には関与しない。
    //
    public abstract class PartitionManager : IDisposable
    {
        //
        // Partition は描画に関与せず、Partition 内で管理するクラスにて行う。
        //

        public const int DefaultActivationRange = 5;

        public const int DefaultPassivationRange = 6;

        static readonly VectorI3[] nearbyPartitionOffsets =
        {
            // Top
            new VectorI3(0, 1, 0),
            // Bottom
            new VectorI3(0, -1, 0),
            // Front
            new VectorI3(0, 0, 1),
            // Back
            new VectorI3(0, 0, -1),
            // Left
            new VectorI3(-1, 0, 0),
            // Right
            new VectorI3(1, 0, 0)
        };

        Vector3 partitionSize;

        Vector3 inversePartitionSize;

        Pool<Partition> partitionPool;

        // TODO
        //
        // 初期容量。

        PartitionCollection activePartitions = new PartitionCollection();

        PartitionCollection activatingPartitions = new PartitionCollection();

        PartitionCollection passivatingPartitions = new PartitionCollection();

        TaskQueue taskQueue = new TaskQueue();

        int activationRange = DefaultActivationRange;
        
        int passivationRange = DefaultPassivationRange;

        VectorI3 eyeGridPosition;

        public bool Closing { get; private set; }

        public bool Closed { get; private set; }

        public int TaskQueueSlotCount
        {
            get { return taskQueue.SlotCount; }
            set { taskQueue.SlotCount = value; }
        }

        public int PoolMaxCapacity
        {
            get { return partitionPool.MaxCapacity; }
            set { partitionPool.MaxCapacity = value; }
        }

        public int ActivationRange
        {
            get { return activationRange; }
            set
            {
                if (passivationRange < value) throw new ArgumentOutOfRangeException("passivationRange < value");
                activationRange = value;
            }
        }

        public int PassivationRange
        {
            get { return passivationRange; }
            set
            {
                if (value < activationRange) throw new ArgumentOutOfRangeException("value < activationRange");
                passivationRange = value;
            }
        }

        public PartitionManager(Vector3 partitionSize, int initialPoolCapacity)
        {
            this.partitionSize = partitionSize;

            inversePartitionSize.X = 1 / partitionSize.X;
            inversePartitionSize.Y = 1 / partitionSize.Y;
            inversePartitionSize.Z = 1 / partitionSize.Z;

            partitionPool = new Pool<Partition>(CreatePartition, initialPoolCapacity);
        }

        public void Update(ref Vector3 eyeWorldPosition)
        {
            if (Closed) return;

            eyeGridPosition.X = Floor(eyeWorldPosition.X * inversePartitionSize.X);
            eyeGridPosition.Y = Floor(eyeWorldPosition.Y * inversePartitionSize.Y);
            eyeGridPosition.Z = Floor(eyeWorldPosition.Z * inversePartitionSize.Z);

            // Update the task queue.
            taskQueue.Update();

            // Try to activate/passivate partitions.
            CheckPassivationCompleted();
            CheckActivationCompleted();

            if (!Closing)
            {
                PassivatePartitions();
                ActivatePartitions();
            }
            else
            {
                // Complete ?

                // Passivating some partitions yet.
                if (passivatingPartitions.Count != 0) return;

                // Activating some partitions yet.
                if (activatingPartitions.Count != 0) return;

                // Some partitions still remain in active ones.
                if (activePartitions.Count != 0)
                {
                    ForcePassivatePartitions();
                    return;
                }

                // Passivated all partitions.
                Closing = false;
                Closed = true;
            }
        }

        public void Close()
        {
            if (Closing || Closed) return;

            Closing = true;
        }

        protected abstract Partition CreatePartition();

        protected virtual bool CanActivatePartition(ref VectorI3 gridPosition)
        {
            return true;
        }

        void CheckPassivationCompleted()
        {
            int index = 0;
            while (index < passivatingPartitions.Count)
            {
                var partition = passivatingPartitions[index];
                if (!partition.IsPassivationCompleted)
                {
                    index++;
                    continue;
                }

                // Deregister.
                passivatingPartitions.RemoveAt(index);

                // Return.
                partitionPool.Return(partition);
            }
        }

        void CheckActivationCompleted()
        {
            int index = 0;
            while (index < activatingPartitions.Count)
            {
                var partition = activatingPartitions[index];
                if (!partition.IsActivationCompleted)
                {
                    index++;
                    continue;
                }

                // Add this partition into the list of active partitions.
                activatingPartitions.RemoveAt(index);
                activePartitions.Add(partition);

                // Notify that a neighbor is activated.
                for (int i = 0; i < 6; i++)
                {
                    var nearbyPosition = nearbyPartitionOffsets[i];
                    Partition neighbor;
                    if (activePartitions.TryGetItem(ref nearbyPosition, out neighbor))
                        neighbor.OnNeighborActivated(partition);
                }
            }
        }

        void ForcePassivatePartitions()
        {
            int index = 0;
            while (index < activePartitions.Count)
            {
                var partition = activePartitions[index];

                activePartitions.Remove(partition);
                passivatingPartitions.Add(partition);

                taskQueue.Enqueue(partition.Passivate);
            }
        }

        void PassivatePartitions()
        {
            //
            // try to passivate partitions out of the passivation bounds.
            //
            BoundingBoxI bounds;
            CalculateBounds(ref eyeGridPosition, PassivationRange, out bounds);

            int index = 0;
            while (index < activePartitions.Count)
            {
                var partition = activePartitions[index];

                if (!Closing && bounds.Contains(partition.GridPosition) == ContainmentType.Contains)
                {
                    index++;
                    continue;
                }

                // Remove this partition from collections of active partitions.
                activePartitions.Remove(partition);

                // Add this partition into the list of partions waiting to be unload.
                passivatingPartitions.Add(partition);

                // Unload this partition asynchronously.
                taskQueue.Enqueue(partition.Passivate);
            }
        }

        void ActivatePartitions()
        {
            //
            // try to activate partitions in the activation bounds.
            //
            BoundingBoxI bounds;
            CalculateBounds(ref eyeGridPosition, ActivationRange, out bounds);

            var gridPosition = new VectorI3();
            for (gridPosition.Z = bounds.Min.Z; gridPosition.Z < bounds.Max.Z; gridPosition.Z++)
            {
                for (gridPosition.Y = bounds.Min.Y; gridPosition.Y < bounds.Max.Y; gridPosition.Y++)
                {
                    for (gridPosition.X = bounds.Min.X; gridPosition.X < bounds.Max.X; gridPosition.X++)
                    {
                        // This partition is already activated.
                        if (activePartitions.Contains(gridPosition)) continue;

                        // An activation thread is handling this partition now.
                        if (activatingPartitions.Contains(gridPosition)) continue;

                        // An passivation thread is handling this partition now.
                        if (passivatingPartitions.Contains(gridPosition)) continue;

                        // No partition is needed.
                        if (!CanActivatePartition(ref gridPosition)) continue;

                        // A new partition.
                        var partition = partitionPool.Borrow();

                        // Skip if can borrow no object from the pool.
                        if (partition == null) return;

                        // Initialize this partition.
                        InitializePartition(partition, ref gridPosition);

                        // Add.
                        activatingPartitions.Add(partition);
                        taskQueue.Enqueue(partition.Activate);
                    }
                }
            }
        }

        void InitializePartition(Partition partition, ref VectorI3 gridPosition)
        {
            partition.GridPosition = gridPosition;
            partition.Initialize();
        }

        void CalculateBounds(ref VectorI3 center, int range, out BoundingBoxI result)
        {
            result = new BoundingBoxI
            {
                Min = new VectorI3
                {
                    X = center.X - range,
                    Y = center.Y - range,
                    Z = center.Z - range
                },
                Max = new VectorI3
                {
                    X = center.X + range,
                    Y = center.Y + range,
                    Z = center.Z + range
                }
            };
        }

        int Floor(float v)
        {
            // Faster than using (int) Math.Floor(x).
            return 0 < v ? (int) v : (int) v - 1;
        }

        protected virtual void DisposeOverride(bool disposing) { }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed;

        ~PartitionManager()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposed) return;

            //================================================================
            // Subclass

            DisposeOverride(disposing);

            //================================================================
            // Dispose all partitions.

            DisposePartitions(partitionPool);
            partitionPool.Clear();

            DisposePartitions(activePartitions);
            activePartitions.Clear();

            DisposePartitions(activatingPartitions);
            activatingPartitions.Clear();

            DisposePartitions(passivatingPartitions);
            passivatingPartitions.Clear();

            //================================================================
            // Clear all tasks just in case.

            taskQueue.Clear();

            disposed = true;
        }

        void DisposePartitions(Pool<Partition> partitions)
        {
            while (0 < partitions.Count)
            {
                var partition = partitions.Borrow();
                partition.Dispose();
            }
        }

        void DisposePartitions(IEnumerable<Partition> partitions)
        {
            foreach (var partition in partitions)
                partition.Dispose();
        }

        #endregion
    }
}
