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
        public const int ClusterExtent = 8;

        public const int InitialActivePartitionCapacity = 5000;

        public const int InitialActiveClusterPartitionCapacity = 50;

        public const int DefaultTaskQueueSlotCount = 50;

        public const int DefaultActivationCapacity = 100;

        public const int DefaultPassivationCapacity = 1000;

        public const int DefaultPassivationSearchCapacity = 200;

        public const int DefaultActivationSearchCapacity = 1000;

        Vector3 partitionSize;

        Vector3 inversePartitionSize;

        Pool<Partition> partitionPool;

        // 効率のためにキュー構造を採用。
        // 全件対象の処理が大半であり、リストでは削除のたびに配列コピーが発生して無駄。

        // TODO
        // 初期容量。

        ClusteredPartitionQueue activePartitions = new ClusteredPartitionQueue(
            ClusterExtent,
            InitialActiveClusterPartitionCapacity,
            InitialActivePartitionCapacity);

        PartitionQueue activatingPartitions = new PartitionQueue(DefaultActivationCapacity);

        PartitionQueue passivatingPartitions = new PartitionQueue(DefaultPassivationCapacity);

        // 同時アクティブ化許容量。
        int activationCapacity = DefaultActivationCapacity;

        // 同時非アクティブ化許容量。
        int passivationCapacity = DefaultPassivationCapacity;

        // 非アクティブ化可能パーティション検索の最大試行数。
        int passivationSearchCapacity = DefaultPassivationSearchCapacity;

        // アクティブ化可能パーティション検索の最大試行数。
        int activationSearchCapacity = DefaultActivationSearchCapacity;

        // アクティブ化可能パーティション検索の開始インデックス。
        int activationSearchOffset = 0;

        TaskQueue activationTaskQueue = new TaskQueue
        {
            SlotCount = DefaultTaskQueueSlotCount
        };

        TaskQueue passivationTaskQueue = new TaskQueue
        {
            SlotCount = DefaultTaskQueueSlotCount
        };

        // 最大アクティブ化領域。
        PartitionSpaceBounds maxActiveBounds;

        // 最小アクティブ化領域に含まれる座標の配列。
        VectorI3[] minActivePointOffsets;

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

        // activatingPartitions のキュー内配列のサイズの拡大を抑制するための制限。
        public int ActivationCapacity
        {
            get { return activationCapacity; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");
                activationCapacity = value;
            }
        }

        public int PassivationCapacity
        {
            get { return passivationCapacity; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");
                passivationCapacity = value;
            }
        }

#if DEBUG

        public PartitionManagerMonitor Monitor { get; private set;}

#endif

        public PartitionManager(Vector3 partitionSize)
        {
            this.partitionSize = partitionSize;

            inversePartitionSize.X = 1 / partitionSize.X;
            inversePartitionSize.Y = 1 / partitionSize.Y;
            inversePartitionSize.Z = 1 / partitionSize.Z;

            partitionPool = new Pool<Partition>(CreatePartition);

            DebugInitialize();
        }

        public void Initialize(int minActiveRange, int maxActiveRange)
        {
            if (minActiveRange < 0) throw new ArgumentOutOfRangeException("minActiveRange");
            if (maxActiveRange < 0 || maxActiveRange <= minActiveRange)
                throw new ArgumentOutOfRangeException("maxActiveRange");

            maxActiveBounds = new PartitionSpaceBounds { Radius = maxActiveRange };

            var dummyBounds = new PartitionSpaceBounds { Radius = minActiveRange };
            minActivePointOffsets = dummyBounds.GetPoints();
        }

        public void Update(ref Vector3 eyeWorldPosition)
        {
            if (Closed) return;

            DebugUpdateMonitor();

            eyePosition.X = MathExtension.Floor(eyeWorldPosition.X * inversePartitionSize.X);
            eyePosition.Y = MathExtension.Floor(eyeWorldPosition.Y * inversePartitionSize.Y);
            eyePosition.Z = MathExtension.Floor(eyeWorldPosition.Z * inversePartitionSize.Z);

            // アクティブ領域を現在の視点位置を中心に設定。
            maxActiveBounds.Center = eyePosition;

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
                if (passivatingPartitions.Count == 0 && activePartitions.Count == 0)
                {
                    Closing = false;
                    Closed = true;
                    OnClosed();
                }
            }
        }

        public void Close()
        {
            if (Closing || Closed) return;

            Closing = true;
            OnClosing();
        }

        protected void PrepareInitialPartitions(int initialCapacity)
        {
            partitionPool.Prepare(initialCapacity);
        }

        protected virtual void OnClosing() { }

        protected virtual void OnClosed() { }

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
                if (activePartitions.TryGetPartition(ref nearbyPosition, out neighbor))
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
                if (activePartitions.TryGetPartition(ref nearbyPosition, out neighbor))
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
            int count = Math.Min(activePartitions.Count, passivationSearchCapacity);
            for (int i = 0; i < count; i++)
            {
                // 同時非アクティブ化許容数を越えるならば、以降の非アクティブ化を全てスキップ。
                if (0 < passivationCapacity && passivationCapacity <= passivatingPartitions.Count)
                    return;

                var partition = activePartitions.Dequeue();

                if (!Closing)
                {
                    if (partition.IsInBounds(ref maxActiveBounds))
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
            int index = activationSearchOffset;
            bool cycled = false;
            int count = 0;
            while (count < activationSearchCapacity)
            {
                if (minActivePointOffsets.Length <= index)
                {
                    index = 0;
                    cycled = true;
                }

                if (cycled && activationSearchOffset <= index) break;

                var position = eyePosition + minActivePointOffsets[index++];

                // 同時アクティブ化許容数を越えるならば、以降のアクティブ化を全てスキップ。
                if (0 < activationCapacity && activationCapacity <= activatingPartitions.Count)
                    return;

                // アクティブ化中あるいは非アクティブ化中かどうか。
                if (activatingPartitions.Contains(position) ||
                    passivatingPartitions.Contains(position))
                    continue;

                // 既にアクティブであるかどうか。
                if (activePartitions.Contains(ref position)) continue;

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

                count++;
            }

            activationSearchOffset = index;
        }

        protected virtual void DisposeOverride(bool disposing) { }

        [Conditional("DEBUG")]
        void DebugInitialize()
        {
            Monitor = new PartitionManagerMonitor();
        }

        [Conditional("DEBUG")]
        void DebugUpdateMonitor()
        {
            Monitor.ActiveClusterCount = activePartitions.ClusterCount;
            Monitor.ActivePartitionCount = activePartitions.Count;
            Monitor.ActivatingPartitionCount = activatingPartitions.Count;
            Monitor.PassivatingPartitionCount = passivatingPartitions.Count;
        }

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

        void DisposePartitions(ClusteredPartitionQueue partitions)
        {
            while (0 < partitions.Count)
            {
                var partition = partitions.Dequeue();
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
