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
        public const int DefaultActivationRange = 10;

        public const int DefaultPassivationRange = 12;

        public const int DefaultTaskQueueSlotCount = 20;

        static readonly VectorI3[] nearbyOffsets =
        {
            VectorI3.Top,
            VectorI3.Bottom,
            VectorI3.Front,
            VectorI3.Back,
            VectorI3.Left,
            VectorI3.Right,
        };

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
                // アクティベーション中のパーティションは破棄。
                if (activationTaskQueue.QueueCount != 0)
                    activationTaskQueue.Clear();

                if (activatingPartitions.Count != 0)
                    activatingPartitions.Clear();

                // パッシベーション中のパーティションを処理。
                passivationTaskQueue.Update();
                CheckPassivationCompleted();

                // アクティブなパーティションを全てパッシベート。
                PassivatePartitions();

                // 全パッシベーションが完了していればクローズ完了。
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

                if (partition.IsPassivationFailed)
                {
                    // 一旦アクティブ リストへ戻す。
                    // 次回のパッシベーション試行に委ねる。
                    activePartitions.Enqueue(partition);
                    continue;
                }

                if (!partition.IsPassivationCompleted)
                {
                    // 未完のためキューへ戻す。
                    passivatingPartitions.Enqueue(partition);
                    continue;
                }

                // パッシベーションに成功したのでプールへ戻す。
                partitionPool.Return(partition);
            }
        }

        void CheckActivationCompleted()
        {
            int partitionCount = activatingPartitions.Count;
            for (int i = 0; i < partitionCount; i++)
            {
                var partition = activatingPartitions.Dequeue();

                if (partition.IsActivationFailed)
                {
                    // 失敗したのでプールへ戻す。
                    partitionPool.Return(partition);

                    // 次回のアクティベーション試行に委ねる。
                    continue;
                }

                if (!partition.IsActivationCompleted)
                {
                    // 未完のためキューへ戻す。
                    activatingPartitions.Enqueue(partition);
                    continue;
                }

                // アクティベーションに成功したのでアクティブ リストへ追加。
                activePartitions.Enqueue(partition);

                // アクティブな隣接パーティションへ成功を通知。
                NotifyNeighborActivated(partition);
            }
        }

        void NotifyNeighborActivated(Partition partition)
        {
            var position = partition.Position;
            for (int i = 0; i < nearbyOffsets.Length; i++)
            {
                var nearbyPosition = position + nearbyOffsets[i];
                Partition neighbor;
                if (activePartitions.TryGetItem(ref nearbyPosition, out neighbor))
                {
                    var side = (CubeSides) i;
                    switch (side)
                    {
                        case CubeSides.Top: side = CubeSides.Bottom; break;
                        case CubeSides.Bottom: side = CubeSides.Top; break;
                        case CubeSides.Front: side = CubeSides.Back; break;
                        case CubeSides.Back: side = CubeSides.Front; break;
                        case CubeSides.Left: side = CubeSides.Right; break;
                        case CubeSides.Right: side = CubeSides.Left; break;
                        default: throw new InvalidOperationException();
                    }

                    neighbor.OnNeighborActivated(partition, side);
                }
            }
        }

        void PassivatePartitions()
        {
            // パッシベーション不要領域 (アクティブ状態維持領域) を算出。
            BoundingBoxI bounds;
            CalculateBounds(ref eyePosition, PassivationRange, out bounds);

            int partitionCount = activePartitions.Count;
            for (int i = 0; i < partitionCount; i++)
            {
                var partition = activePartitions.Dequeue();

                if (!Closing)
                {
                    var position = partition.Position;
                    ContainmentType containmentType;
                    bounds.Contains(ref position, out containmentType);

                    if (containmentType == ContainmentType.Contains)
                    {
                        // パッシベーション不要領域内ならばアクティブ リストへ戻す。
                        activePartitions.Enqueue(partition);
                        continue;
                    }
                }

                // パッシベーション キューへ追加。
                passivatingPartitions.Enqueue(partition);

                // 非同期パッシベーションを要求。
                passivationTaskQueue.Enqueue(partition.PassivateAction);
            }
        }

        void ActivatePartitions()
        {
            // アクティベーション領域を算出。
            BoundingBoxI bounds;
            CalculateBounds(ref eyePosition, ActivationRange, out bounds);

            var position = new VectorI3();
            for (position.Z = bounds.Min.Z; position.Z < bounds.Max.Z; position.Z++)
            {
                for (position.Y = bounds.Min.Y; position.Y < bounds.Max.Y; position.Y++)
                {
                    for (position.X = bounds.Min.X; position.X < bounds.Max.X; position.X++)
                    {
                        // 同時アクティベーション許容数を越えるならばスキップ。
                        if (activationCapacity <= activatingPartitions.Count) continue;

                        // アクティベーション中あるいはパッシベーション中かどうか。
                        if (activatingPartitions.Contains(position) ||
                            passivatingPartitions.Contains(position))
                            continue;

                        // 既にアクティブであるかどうか。
                        if (activePartitions.Contains(position)) continue;

                        // アクティベーション可能であるかどうか。
                        if (!CanActivatePartition(ref position)) continue;

                        // プールからパーティション インスタンスを取得。
                        // プール枯渇ならばアクティベーションは一時取消。
                        var partition = partitionPool.Borrow();
                        if (partition == null) return;

                        // パーティションを初期化。
                        InitializePartition(partition, ref position);

                        // アクティベーション キューへ追加。
                        activatingPartitions.Enqueue(partition);

                        // 非同期アクティベーションを要求。
                        activationTaskQueue.Enqueue(partition.ActivateAction);
                    }
                }
            }
        }

        void InitializePartition(Partition partition, ref VectorI3 position)
        {
            partition.Position = position;
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
