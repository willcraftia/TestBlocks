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
        #region Settings

        public sealed class Settings
        {
            Vector3 partitionSize;

            int partitionPoolMaxCapacity = 0;

            int clusterExtent = 8;

            int initialActivePartitionCapacity = 5000;

            int initialActiveClusterCapacity = 50;

            int initialActivationCapacity = 100;

            int initialPassivationCapacity = 1000;

            int activationSearchCapacity = 100;

            int passivationSearchCapacity = 200;

            int activationTaskQueueSlotCount = 50;

            int passivationTaskQueueSlotCount = 50;

            public Vector3 PartitionSize
            {
                get { return partitionSize; }
                set
                {
                    if (value.X < 0 || value.Y < 0 || value.Z < 0)
                        throw new ArgumentOutOfRangeException("value");

                    partitionSize = value;
                }
            }

            public int PartitionPoolMaxCapacity
            {
                get { return partitionPoolMaxCapacity; }
                set
                {
                    if (value < 0) throw new ArgumentOutOfRangeException("value");

                    partitionPoolMaxCapacity = value;
                }
            }

            public int ClusterExtent
            {
                get { return clusterExtent; }
                set
                {
                    if (value < 1) throw new ArgumentOutOfRangeException("value");

                    clusterExtent = value;
                }
            }

            public int InitialActivePartitionCapacity
            {
                get { return initialActivePartitionCapacity; }
                set
                {
                    if (value < 0) throw new ArgumentOutOfRangeException("value");

                    initialActivePartitionCapacity = value;
                }
            }

            public int InitialActiveClusterCapacity
            {
                get { return initialActiveClusterCapacity; }
                set
                {
                    if (value < 0) throw new ArgumentOutOfRangeException("value");

                    initialActiveClusterCapacity = value;
                }
            }

            public int InitialActivationCapacity
            {
                get { return initialActivationCapacity; }
                set
                {
                    if (value < 1) throw new ArgumentOutOfRangeException("value");

                    initialActivationCapacity = value;
                }
            }

            public int InitialPassivationCapacity
            {
                get { return initialPassivationCapacity; }
                set
                {
                    if (value < 1) throw new ArgumentOutOfRangeException("value");

                    initialPassivationCapacity = value;
                }
            }

            public int ActivationTaskQueueSlotCount
            {
                get { return activationTaskQueueSlotCount; }
                set
                {
                    if (value < 1) throw new ArgumentOutOfRangeException("value");

                    activationTaskQueueSlotCount = value;
                }
            }

            public int PassivationTaskQueueSlotCount
            {
                get { return passivationTaskQueueSlotCount; }
                set
                {
                    if (value < 1) throw new ArgumentOutOfRangeException("value");

                    passivationTaskQueueSlotCount = value;
                }
            }

            public ILandscapeVolume MinLandscapeVolume { get; set; }

            public ILandscapeVolume MaxLandscapeVolume { get; set; }

            public int ActivationSearchCapacity
            {
                get { return activationSearchCapacity; }
                set
                {
                    if (value < 1) throw new ArgumentOutOfRangeException("value");

                    activationSearchCapacity = value;
                }
            }

            public int PassivationSearchCapacity
            {
                get { return passivationSearchCapacity; }
                set
                {
                    if (value < 1) throw new ArgumentOutOfRangeException("value");

                    passivationSearchCapacity = value;
                }
            }
        }

        #endregion

        Vector3 partitionSize;

        Vector3 inversePartitionSize;

        Pool<Partition> partitionPool;

        // 効率のためにキュー構造を採用。
        // 全件対象の処理が大半であり、リストでは削除のたびに配列コピーが発生して無駄。

        ClusteredPartitionQueue activePartitions;

        PartitionQueue activatingPartitions;

        PartitionQueue passivatingPartitions;

        // 同時アクティブ化許容量。
        int activationCapacity;

        // 同時非アクティブ化許容量。
        int passivationCapacity;

        // アクティブ化可能パーティション検索の最大試行数。
        int activationSearchCapacity;

        // 非アクティブ化可能パーティション検索の最大試行数。
        int passivationSearchCapacity;

        // アクティブ化可能パーティション検索の開始インデックス。
        int activationSearchOffset = 0;

        TaskQueue activationTaskQueue = new TaskQueue();

        TaskQueue passivationTaskQueue = new TaskQueue();

        // 最小アクティブ領域。
        ILandscapeVolume minLandscapeVolume;

        // 最大アクティブ領域。
        ILandscapeVolume maxLandscapeVolume;

        // 最小アクティブ領域に含まれる座標の配列。
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

        public PartitionManagerMonitor Monitor { get; private set; }

        public PartitionManager(Settings settings)
        {
            if (settings == null) throw new ArgumentNullException("settings");

            partitionSize = settings.PartitionSize;

            inversePartitionSize.X = 1 / partitionSize.X;
            inversePartitionSize.Y = 1 / partitionSize.Y;
            inversePartitionSize.Z = 1 / partitionSize.Z;

            partitionPool = new Pool<Partition>(CreatePartition);
            partitionPool.MaxCapacity = settings.PartitionPoolMaxCapacity;

            activePartitions = new ClusteredPartitionQueue(
                settings.ClusterExtent,
                settings.InitialActiveClusterCapacity,
                settings.InitialActivePartitionCapacity);

            activationCapacity = settings.InitialActivationCapacity;
            passivationCapacity = settings.InitialPassivationCapacity;

            activatingPartitions = new PartitionQueue(activationCapacity);
            passivatingPartitions = new PartitionQueue(passivationCapacity);

            activationTaskQueue.SlotCount = settings.ActivationTaskQueueSlotCount;
            passivationTaskQueue.SlotCount = settings.PassivationTaskQueueSlotCount;

            // null の場合はデフォルト実装を与える。
            minLandscapeVolume = settings.MinLandscapeVolume ?? new DefaultLandscapeVolume(VectorI3.Zero, 1000);
            minActivePointOffsets = minLandscapeVolume.GetPoints();
            maxLandscapeVolume = settings.MaxLandscapeVolume ?? new DefaultLandscapeVolume(VectorI3.Zero, 1000);

            activationSearchCapacity = settings.ActivationSearchCapacity;
            passivationSearchCapacity = settings.PassivationSearchCapacity;

            Monitor = new PartitionManagerMonitor(this);
        }

        public void Update(ref Vector3 eyeWorldPosition)
        {
            if (Closed) return;

            Monitor.OnBeginUpdate();

            Monitor.ActiveClusterCount = activePartitions.ClusterCount;
            Monitor.ActivePartitionCount = activePartitions.Count;
            Monitor.ActivatingPartitionCount = activatingPartitions.Count;
            Monitor.PassivatingPartitionCount = passivatingPartitions.Count;

            eyePosition.X = MathExtension.Floor(eyeWorldPosition.X * inversePartitionSize.X);
            eyePosition.Y = MathExtension.Floor(eyeWorldPosition.Y * inversePartitionSize.Y);
            eyePosition.Z = MathExtension.Floor(eyeWorldPosition.Z * inversePartitionSize.Z);

            // アクティブ領域を現在の視点位置を中心に設定。
            maxLandscapeVolume.Center = eyePosition;

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

            Monitor.OnEndUpdate();
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
            Monitor.OnBeginCheckPassivationCompleted();

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
                    partition.PassivationCanceled = false;
                    partition.PassivationCompleted = false;
                    activePartitions.Enqueue(partition);
                    continue;
                }
                
                // アクティブな隣接パーティションへ非アクティブ化を通知。
                NortifyNeighborPassivated(partition);

                // 解放処理を呼び出す。
                partition.Release();

                // 非アクティブ化に成功したのでプールへ戻す。
                partitionPool.Return(partition);
            }

            Monitor.OnEndCheckPassivationCompleted();
        }

        void CheckActivationCompleted()
        {
            Monitor.OnBeginCheckActivationCompleted();

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
                    partition.PassivationCanceled = false;
                    partition.ActivationCompleted = false;
                    partitionPool.Return(partition);
                    continue;
                }

                // アクティブ化に成功したのでアクティブ リストへ追加。
                activePartitions.Enqueue(partition);

                // アクティブな隣接パーティションへ通知。
                NotifyNeighborActivated(partition);
            }

            Monitor.OnEndCheckActivationCompleted();
        }

        void NortifyNeighborPassivated(Partition partition)
        {
            var position = partition.Position;

            foreach (var side in CubicSide.Items)
            {
                var nearbyPosition = position + side.Direction;

                Partition neighbor;
                if (!activePartitions.TryGetPartition(ref nearbyPosition, out neighbor))
                {
                    // passivatingPartitions にあるパーティションは、
                    // その非アクティブ化が取り消されて activePartitions に戻され、
                    // 次の非アクティブ化判定の試行では非アクティブ化対象ではなくなる場合がある。
                    // このため、passivatingPartitions にあるパーティションについても探索する。
                    if (!passivatingPartitions.TryGetItem(ref nearbyPosition, out neighbor))
                        continue;
                }

                var reverseSide = side.Reverse();
                neighbor.OnNeighborPassivated(partition, reverseSide);
            }
        }

        void NotifyNeighborActivated(Partition partition)
        {
            var position = partition.Position;

            foreach (var side in CubicSide.Items)
            {
                var nearbyPosition = position + side.Direction;

                Partition neighbor;
                if (!activePartitions.TryGetPartition(ref nearbyPosition, out neighbor))
                {
                    // passivatingPartitions にあるパーティションは、
                    // その非アクティブ化が取り消されて activePartitions に戻され、
                    // 次の非アクティブ化判定の試行では非アクティブ化対象ではなくなる場合がある。
                    // このため、passivatingPartitions にあるパーティションについても探索する。
                    if (!passivatingPartitions.TryGetItem(ref nearbyPosition, out neighbor))
                        continue;
                }

                // partition へアクティブな隣接パーティションを通知。
                partition.OnNeighborActivated(neighbor, side);

                // アクティブな隣接パーティションへ partition を通知。
                var reverseSide = side.Reverse();
                neighbor.OnNeighborActivated(partition, reverseSide);
            }
        }

        void PassivatePartitions()
        {
            Monitor.OnBeginPassivatePartitions();

            int count = Math.Min(activePartitions.Count, passivationSearchCapacity);
            for (int i = 0; i < count; i++)
            {
                // 同時非アクティブ化許容数を越えるならば処理終了。
                if (0 < passivationCapacity && passivationCapacity <= passivatingPartitions.Count)
                    break;

                var partition = activePartitions.Dequeue();

                if (!Closing)
                {
                    if (partition.IsInLandscapeVolume(maxLandscapeVolume))
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

            Monitor.OnEndPassivatePartitions();
        }

        void ActivatePartitions()
        {
            Monitor.OnBeginActivatePartitions();

            int index = activationSearchOffset;
            bool cycled = false;
            for (int i = 0; i < activationSearchCapacity; i++)
            {
                // 同時アクティブ化許容数を越えるならば処理終了。
                if (0 < activationCapacity && activationCapacity <= activatingPartitions.Count)
                    break;

                // index が末尾に到達したら先頭へ戻し、循環したとしてマーク。
                if (minActivePointOffsets.Length <= index)
                {
                    index = 0;
                    cycled = true;
                }

                // 循環かつ最初のオフセットに到達しているならば処理終了。
                if (cycled && activationSearchOffset <= index) break;

                var position = eyePosition + minActivePointOffsets[index++];

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
                if (partition == null) break;

                // パーティションを初期化。
                partition.Initialize(ref position);

                // アクティブ化キューへ追加。
                activatingPartitions.Enqueue(partition);

                // 非同期処理を要求。
                activationTaskQueue.Enqueue(partition.ActivateAction);
            }

            activationSearchOffset = index;

            Monitor.OnEndActivatePartitions();
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
