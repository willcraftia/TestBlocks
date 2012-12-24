#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Collections;
using Willcraftia.Xna.Framework.Threading;

#endregion

namespace Willcraftia.Xna.Framework.Landscape
{
    /// <summary>
    /// パーティションのアクティブ状態を管理するクラスです。
    /// このクラスは、必要に応じてパーティションをアクティブ化し、また、非アクティブ化します。
    /// このクラスは、パーティションの描画に関与しません。
    /// </summary>
    public abstract class PartitionManager : IDisposable
    {
        public const int DefaultActivationRange = 8;

        public const int DefaultPassivationRange = 10;

        public const int DefaultTaskQueueSlotCount = 20;

        Vector3 partitionSize;

        Vector3 inversePartitionSize;

        Pool<Partition> partitionPool;

        // 効率のためにキュー構造を採用。
        // 全件対象の処理が大半であり、リストでは削除のたびに配列コピーが発生して無駄。

        // TODO
        // 初期容量。

        PartitionQueue activePartitions = new PartitionQueue(5000);

        PartitionQueue activatingPartitions = new PartitionQueue(100);

        PartitionQueue passivatingPartitions = new PartitionQueue(100);

        // activatingPartitions のキュー内配列のサイズの拡大を抑制するための制限。
        int activationCapacity = 100;

        TaskQueue activationTaskQueue = new TaskQueue
        {
            SlotCount = DefaultTaskQueueSlotCount
        };

        TaskQueue passivationTaskQueue = new TaskQueue
        {
            SlotCount = DefaultTaskQueueSlotCount
        };

        int activationRange = DefaultActivationRange;
        
        int passivationRange = DefaultPassivationRange;

        VectorI3 eyePosition;

        public bool Closing { get; private set; }

        public bool Closed { get; private set; }

        public int ActivationTaskQueueSlotCount
        {
            get { return activationTaskQueue.SlotCount; }
            set { activationTaskQueue.SlotCount = value; }
        }

        public int PassivationTaskQueueSlotCount
        {
            get { return passivationTaskQueue.SlotCount; }
            set { passivationTaskQueue.SlotCount = value; }
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

        public int ActivePartitionCount
        {
            get { return activePartitions.Count; }
        }

        public int ActivatingPartitionCount
        {
            get { return activatingPartitions.Count; }
        }

        public int PassivatingPartitionCount
        {
            get { return passivatingPartitions.Count; }
        }

        public PartitionManager(Vector3 partitionSize)
        {
            this.partitionSize = partitionSize;

            inversePartitionSize.X = 1 / partitionSize.X;
            inversePartitionSize.Y = 1 / partitionSize.Y;
            inversePartitionSize.Z = 1 / partitionSize.Z;

            partitionPool = new Pool<Partition>(CreatePartition);
        }

        protected void PrepareInitialPartitions(int initialCapacity)
        {
            partitionPool.Prepare(initialCapacity);
        }

        public void Update(ref Vector3 eyeWorldPosition)
        {
            if (Closed) return;

            eyePosition.X = MathExtension.Floor(eyeWorldPosition.X * inversePartitionSize.X);
            eyePosition.Y = MathExtension.Floor(eyeWorldPosition.Y * inversePartitionSize.Y);
            eyePosition.Z = MathExtension.Floor(eyeWorldPosition.Z * inversePartitionSize.Z);

            if (!Closing)
            {
                activationTaskQueue.Update();
                passivationTaskQueue.Update();

                CheckPassivationCompleted();
                CheckActivationCompleted();

                PassivatePartitions();
                ActivatePartitions();
            }
            else
            {
                // アクティブ化中のパーティションは破棄。
                if (activationTaskQueue.QueueCount != 0)
                    activationTaskQueue.Clear();

                if (activatingPartitions.Count != 0)
                    activatingPartitions.Clear();

                // 非アクティブ化中のパーティションを処理。
                passivationTaskQueue.Update();
                CheckPassivationCompleted();

                // アクティブなパーティションを全て非アクティブ化。
                PassivatePartitions();

                // 全ての非アクティブ化が完了していればクローズ完了。
                if (passivatingPartitions.Count == 0)
                {
                    Closing = false;
                    Closed = true;
                }
            }
        }

        public void Close()
        {
            if (Closing || Closed) return;

            Closing = true;
        }

        protected abstract Partition CreatePartition();

        protected virtual bool CanActivatePartition(ref VectorI3 position)
        {
            return true;
        }

        void CheckPassivationCompleted()
        {
            int partitionCount = passivatingPartitions.Count;
            for (int i = 0; i < partitionCount; i++)
            {
                var partition = passivatingPartitions.Dequeue();

                if (!partition.PassivationCompleted)
                {
                    // 未完ならばキューへ戻す。
                    passivatingPartitions.Enqueue(partition);
                    continue;
                }

                if (partition.PassivationCanceled)
                {
                    // 取り消されていたならばアクティブ リストへ戻し、次回の非アクティブ化に委ねる。
                    partition.PassivationCompleted = false;
                    activePartitions.Enqueue(partition);
                    continue;
                }
                
                // アクティブな隣接パーティションへ非アクティブ化を通知。
                NortifyNeighborPassivated(partition);

                // 非アクティブ化成功したのでプールへ戻す。
                partitionPool.Return(partition);
            }
        }

        void CheckActivationCompleted()
        {
            int partitionCount = activatingPartitions.Count;
            for (int i = 0; i < partitionCount; i++)
            {
                var partition = activatingPartitions.Dequeue();

                if (!partition.ActivationCompleted)
                {
                    // 未完ならばキューへ戻す。
                    activatingPartitions.Enqueue(partition);
                    continue;
                }

                if (partition.ActivationCanceled)
                {
                    // 取り消されていたならばプールへ戻す。
                    partition.ActivationCompleted = false;
                    partitionPool.Return(partition);
                    continue;
                }

                // アクティブ化に成功したのでアクティブ リストへ追加。
                activePartitions.Enqueue(partition);

                // アクティブな隣接パーティションへ通知。
                NotifyNeighborActivated(partition);
            }
        }

        void NortifyNeighborPassivated(Partition partition)
        {
            var position = partition.Position;

            foreach (var side in CubicSide.Items)
            {
                var nearbyPosition = position + side.Direction;

                Partition neighbor;
                if (activePartitions.TryGetItem(ref nearbyPosition, out neighbor))
                {
                    var reverseSide = side.Reverse();
                    neighbor.OnNeighborPassivated(partition, reverseSide);
                }
            }
        }

        void NotifyNeighborActivated(Partition partition)
        {
            var position = partition.Position;

            foreach (var side in CubicSide.Items)
            {
                var nearbyPosition = position + side.Direction;

                Partition neighbor;
                if (activePartitions.TryGetItem(ref nearbyPosition, out neighbor))
                {
                    // partition へアクティブな隣接パーティションを通知。
                    partition.OnNeighborActivated(neighbor, side);

                    // アクティブな隣接パーティションへ partition を通知。
                    var reverseSide = side.Reverse();
                    neighbor.OnNeighborActivated(partition, reverseSide);
                }
            }
        }

        void PassivatePartitions()
        {
            // アクティブ状態維持領域を算出。
            BoundingBoxI bounds;
            CalculateBounds(ref eyePosition, PassivationRange, out bounds);

            int partitionCount = activePartitions.Count;
            for (int i = 0; i < partitionCount; i++)
            {
                var partition = activePartitions.Dequeue();

                if (!Closing)
                {
                    if (partition.IsInBounds(ref bounds))
                    {
                        // アクティブ状態維持領域内ならばアクティブ リストへ戻す。
                        activePartitions.Enqueue(partition);
                        continue;
                    }
                }

                // 非アクティブ化キューへ追加。
                passivatingPartitions.Enqueue(partition);

                // 非同期処理を要求。
                passivationTaskQueue.Enqueue(partition.PassivateAction);
            }
        }

        void ActivatePartitions()
        {
            // アクティブ化領域を算出。
            BoundingBoxI bounds;
            CalculateBounds(ref eyePosition, ActivationRange, out bounds);

            var position = new VectorI3();
            for (position.Z = bounds.Min.Z; position.Z < bounds.Max.Z; position.Z++)
            {
                for (position.Y = bounds.Min.Y; position.Y < bounds.Max.Y; position.Y++)
                {
                    for (position.X = bounds.Min.X; position.X < bounds.Max.X; position.X++)
                    {
                        // 同時アクティブ化許容数を越えるならばスキップ。
                        if (activationCapacity <= activatingPartitions.Count) continue;

                        // アクティブ化中あるいは非アクティブ化中かどうか。
                        if (activatingPartitions.Contains(position) ||
                            passivatingPartitions.Contains(position))
                            continue;

                        // 既にアクティブであるかどうか。
                        if (activePartitions.Contains(position)) continue;

                        // アクティブ化可能であるかどうか。
                        if (!CanActivatePartition(ref position)) continue;

                        // プールからパーティションを取得。
                        // プール枯渇ならば以降のアクティブ化を全てスキップ。
                        var partition = partitionPool.Borrow();
                        if (partition == null) return;

                        // パーティションを初期化。
                        partition.Initialize(ref position);

                        // アクティブ化キューへ追加。
                        activatingPartitions.Enqueue(partition);

                        // 非同期処理を要求。
                        activationTaskQueue.Enqueue(partition.ActivateAction);
                    }
                }
            }
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
            // Clear all tasks.

            activationTaskQueue.Clear();
            passivationTaskQueue.Clear();

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
