#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Collections;
using Willcraftia.Xna.Framework.Diagnostics;
using Willcraftia.Xna.Framework.Threading;

#endregion

namespace Willcraftia.Xna.Framework.Landscape
{
    /// <summary>
    /// パーティションを管理するクラスです。
    /// このクラスは、必要に応じてパーティションをアクティブ化し、また、非アクティブ化します。
    /// </summary>
    public abstract class PartitionManager : IDisposable
    {
        #region Settings

        public sealed class Settings
        {
            Vector3 partitionSize;

            int partitionPoolMaxCapacity = 0;

            VectorI3 clusterSize = new VectorI3(8);

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

            public VectorI3 ClusterSize
            {
                get { return clusterSize; }
                set
                {
                    if (value.X < 1 || value.Y < 1 || value.Z < 1) throw new ArgumentOutOfRangeException("value");

                    clusterSize = value;
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

        #region VectorI3LengthComparer

        sealed class VectorI3LengthComparer : IComparer<VectorI3>
        {
            internal static readonly VectorI3LengthComparer Instace = new VectorI3LengthComparer();

            VectorI3LengthComparer() { }

            // I/F
            public int Compare(VectorI3 x, VectorI3 y)
            {
                var d1 = x.LengthSquared();
                var d2 = y.LengthSquared();

                if (d1 == d2) return 0;

                return d1 < d2 ? -1 : 1;
            }
        }

        #endregion

        #region PartitionDistanceAscComparer

        sealed class PartitionDistanceAscComparer : IComparer<Partition>
        {
            internal static readonly PartitionDistanceAscComparer Instance = new PartitionDistanceAscComparer();

            internal Vector3 EyePosition;

            PartitionDistanceAscComparer() { }

            // I/F
            public int Compare(Partition x, Partition y)
            {
                var dx = (x.Center - EyePosition).LengthSquared();
                var dy = (y.Center - EyePosition).LengthSquared();

                if (dx == dy) return 0;
                return dx < dy ? -1 : 1;
            }
        }

        #endregion

        #region PartitionDistanceDescComparer

        sealed class PartitionDistanceDescComparer : IComparer<Partition>
        {
            internal static readonly PartitionDistanceDescComparer Instance = new PartitionDistanceDescComparer();

            internal Vector3 EyePosition;

            PartitionDistanceDescComparer() { }

            // I/F
            public int Compare(Partition x, Partition y)
            {
                var dx = (x.Center - EyePosition).LengthSquared();
                var dy = (y.Center - EyePosition).LengthSquared();
                
                if (dx == dy) return 0;
                return dx < dy ? 1 : -1;
            }
        }

        #endregion

        public const string MonitorUpdate = "PartitionManager.Update";

        /// <summary>
        /// ワールド空間におけるパーティションのサイズ。
        /// </summary>
        Vector3 partitionSize;

        /// <summary>
        /// 1 / partitionSize。
        /// </summary>
        Vector3 inversePartitionSize;

        /// <summary>
        /// パーティションのプール。
        /// </summary>
        Pool<Partition> partitionPool;

        /// <summary>
        /// アクティブ化待機リスト。
        /// </summary>
        PartitionCollection waitActivations;

        /// <summary>
        /// 非アクティブ化待機リスト。
        /// </summary>
        PartitionCollection waitPassivations;

        // 効率のためにキュー構造を採用。
        // 全件対象の処理が大半であり、リストでは削除のたびに配列コピーが発生して無駄。

        /// <summary>
        /// アクティブ化実行キュー。
        /// </summary>
        PartitionQueue activations;

        /// <summary>
        /// 非アクティブ化実行キュー。
        /// </summary>
        PartitionQueue passivations;

        // TODO

        /// <summary>
        /// 同時アクティブ化待機許容量。
        /// </summary>
        int waitActivationCapacity = 100;
        
        /// <summary>
        /// 同時非アクティブ化待機許容量。
        /// </summary>
        int waitPassivationCapacity = 100;

        /// <summary>
        /// 同時アクティブ化実行許容量。
        /// </summary>
        int activationCapacity;

        /// <summary>
        /// 同時非アクティブ化実行許容量。
        /// </summary>
        int passivationCapacity;

        /// <summary>
        /// アクティブ化判定の最大試行数。
        /// </summary>
        int activationSearchCapacity;

        /// <summary>
        /// 非アクティブ化判定の最大試行数。
        /// </summary>
        int passivationSearchCapacity;

        /// <summary>
        /// アクティブ化判定の開始インデックス。
        /// </summary>
        int activationSearchOffset = 0;

        /// <summary>
        /// アクティブ化タスク キュー。
        /// </summary>
        TaskQueue activationTaskQueue = new TaskQueue();

        /// <summary>
        /// 非アクティブ化タスク キュー。
        /// </summary>
        TaskQueue passivationTaskQueue = new TaskQueue();

        /// <summary>
        /// 最小アクティブ領域。
        /// </summary>
        ILandscapeVolume minLandscapeVolume;

        /// <summary>
        /// 最大アクティブ領域。
        /// </summary>
        ILandscapeVolume maxLandscapeVolume;

        /// <summary>
        /// 最小アクティブ領域に含まれる座標の配列。
        /// </summary>
        VectorI3[] minActivePointOffsets;

        /// <summary>
        /// パーティション空間における視点の位置。
        /// </summary>
        VectorI3 eyePosition;

        /// <summary>
        /// ワールド空間における視点の位置。
        /// </summary>
        Vector3 eyePositionWorld;

        /// <summary>
        /// クローズ処理中であるか否かを示す値を取得します。
        /// </summary>
        /// <value>
        /// true (クローズ処理中である場合)、false (それ以外の場合)。
        /// </value>
        public bool Closing { get; private set; }

        /// <summary>
        /// クローズ処理が完了しているか否かを示す値を取得します。
        /// </summary>
        /// <value>
        /// true (クローズ処理が完了している場合)、false (それ以外の場合)。
        /// </value>
        public bool Closed { get; private set; }

        /// <summary>
        /// アクティブ クラスタ数を取得します。
        /// </summary>
        public int ActiveClusterCount
        {
            get { return ActivePartitions.ClusterCount; }
        }

        /// <summary>
        /// アクティブ パーティション数を取得します。
        /// </summary>
        public int ActivePartitionCount
        {
            get { return ActivePartitions.Count; }
        }

        /// <summary>
        /// アクティブ化中パーティション数を取得します。
        /// </summary>
        public int ActivatingPartitionCount
        {
            get { return activations.Count; }
        }

        /// <summary>
        /// 非アクティブ化中パーティション数を取得します。
        /// </summary>
        public int PassivatingPartitionCount
        {
            get { return passivations.Count; }
        }

        /// <summary>
        /// アクティブなパーティションのキュー。
        /// </summary>
        protected ClusteredPartitionQueue ActivePartitions { get; private set; }

        /// <summary>
        /// インスタンスを生成します。
        /// 指定する設定は、パーティション マネージャのインスタンス化後に外部から変更しても反映されません。
        /// </summary>
        /// <param name="settings">パーティション マネージャ設定。</param>
        public PartitionManager(Settings settings)
        {
            if (settings == null) throw new ArgumentNullException("settings");

            partitionSize = settings.PartitionSize;

            inversePartitionSize.X = 1 / partitionSize.X;
            inversePartitionSize.Y = 1 / partitionSize.Y;
            inversePartitionSize.Z = 1 / partitionSize.Z;

            partitionPool = new Pool<Partition>(CreatePartition);
            partitionPool.MaxCapacity = settings.PartitionPoolMaxCapacity;

            ActivePartitions = new ClusteredPartitionQueue(
                settings.ClusterSize,
                settings.PartitionSize,
                settings.InitialActiveClusterCapacity,
                settings.InitialActivePartitionCapacity);

            activationCapacity = settings.InitialActivationCapacity;
            passivationCapacity = settings.InitialPassivationCapacity;

            // TODO
            waitActivations = new PartitionCollection(waitActivationCapacity);
            waitPassivations = new PartitionCollection(waitPassivationCapacity);

            activations = new PartitionQueue(activationCapacity);
            passivations = new PartitionQueue(passivationCapacity);

            activationTaskQueue.SlotCount = settings.ActivationTaskQueueSlotCount;
            passivationTaskQueue.SlotCount = settings.PassivationTaskQueueSlotCount;

            // null の場合はデフォルト実装を与える。
            minLandscapeVolume = settings.MinLandscapeVolume ?? new DefaultLandscapeVolume(VectorI3.Zero, 1000);
            minActivePointOffsets = minLandscapeVolume.GetPoints();

            // 中心から近い位置からアクティブ化したいため中心に近い位置でソート。
            // なんらかのアクティブ化中ならば意味は無いが、初回、あるいは、
            // 待機している全てのアクティブ化を終えた後からの新たなアクティブ化では意味がある。
            Array.Sort(minActivePointOffsets, VectorI3LengthComparer.Instace);

            maxLandscapeVolume = settings.MaxLandscapeVolume ?? new DefaultLandscapeVolume(VectorI3.Zero, 1000);

            activationSearchCapacity = settings.ActivationSearchCapacity;
            passivationSearchCapacity = settings.PassivationSearchCapacity;
        }

        /// <summary>
        /// パーティションのアクティブ化と非アクティブ化を行います。
        /// </summary>
        /// <param name="gameTime">ゲーム時間。</param>
        /// <param name="eyePositionWorld">ワールド空間における視点の位置。</param>
        public void Update(GameTime gameTime, Vector3 eyePositionWorld)
        {
            if (Closed) return;

            Monitor.Begin(MonitorUpdate);

            this.eyePositionWorld = eyePositionWorld;

            eyePosition.X = MathExtension.Floor(eyePositionWorld.X * inversePartitionSize.X);
            eyePosition.Y = MathExtension.Floor(eyePositionWorld.Y * inversePartitionSize.Y);
            eyePosition.Z = MathExtension.Floor(eyePositionWorld.Z * inversePartitionSize.Z);

            // アクティブ領域を現在の視点位置を中心に設定。
            maxLandscapeVolume.Center = eyePosition;

            if (!Closing)
            {
                activationTaskQueue.Update();
                passivationTaskQueue.Update();

                CheckPassivations(gameTime);
                CheckActivations(gameTime);

                CheckWaitPassivations(gameTime);
                CheckWaitActivations(gameTime);

                PassivatePartitions(gameTime);
                ActivatePartitions(gameTime);

                UpdatePartitions(gameTime);
            }
            else
            {
                // アクティブ化関連の処理は破棄。
                if (activationTaskQueue.QueueCount != 0) activationTaskQueue.Clear();
                if (activations.Count != 0) activations.Clear();
                if (waitActivations.Count != 0) waitActivations.Clear();

                // 非アクティブ化関連を処理。
                passivationTaskQueue.Update();
                CheckPassivations(gameTime);
                CheckWaitPassivations(gameTime);
                PassivatePartitions(gameTime);

                // パーティション内部でも必要に応じてクローズのための更新を行う。
                UpdatePartitions(gameTime);

                // 全ての非アクティブ化が完了していればクローズ完了。
                if (passivations.Count == 0 && ActivePartitions.Count == 0)
                {
                    Closing = false;
                    Closed = true;
                    OnClosed();
                }
            }

            Monitor.End(MonitorUpdate);
        }

        /// <summary>
        /// パーティション マネージャをクローズします。
        /// クローズ処理が開始すると、パーティションのアクティブ化は完全に停止し、
        /// アクティブなパーティションは全て非アクティブ化されます。
        /// なお、非アクティブ化は非同期に行われるため、
        /// クローズ処理の完了は Closed プロパティで確認する必要があります。
        /// </summary>
        public void Close()
        {
            if (Closing || Closed) return;

            Closing = true;
            OnClosing();
        }

        /// <summary>
        /// 境界錐台と交差するパーティションを収集します。
        /// </summary>
        /// <param name="frustum">境界錐台。</param>
        /// <param name="collector">収集先パーティションのコレクション。</param>
        public void CollectPartitions<T>(BoundingFrustum frustum, ICollection<T> collector) where T : Partition
        {
            foreach (var cluster in ActivePartitions.Clusters)
            {
                bool intersected;
                frustum.Intersects(ref cluster.BoundingBox, out intersected);

                // クラスタが境界錐台と交差するなら、クラスタに含まれるパーティションを収集。
                if (intersected) cluster.CollectPartitions(frustum, collector);
            }
        }

        /// <summary>
        /// 事前にパーティション プールにインスタンスを生成する場合に、
        /// サブクラスのコンストラクタで呼び出します。
        /// </summary>
        /// <param name="initialCapacity">パーティション プールの初期容量。</param>
        protected void PrepareInitialPartitions(int initialCapacity)
        {
            partitionPool.Prepare(initialCapacity);
        }

        /// <summary>
        /// クローズ処理が開始する時に呼び出されます。
        /// </summary>
        protected virtual void OnClosing() { }

        /// <summary>
        /// クローズ処理が完了した時に呼び出されます。
        /// </summary>
        protected virtual void OnClosed() { }

        /// <summary>
        /// パーティション プールでパーティションの新規生成が必要となる際に呼び出されます。
        /// </summary>
        /// <returns>パーティション。</returns>
        protected abstract Partition CreatePartition();

        /// <summary>
        /// パーティションがアクティブ化可能であるか否かを検査します。
        /// パーティション マネージャは、アクティブ領域の観点からアクティブ化すべきパーティションの位置を決定しますが、
        /// 実際にその位置にパーティションが存在するか否かは実装によります。
        /// </summary>
        /// <param name="position">アクティブ化しようとするパーティションの位置。</param>
        /// <returns>
        /// true (指定の位置でパーティションをアクティブ化できる場合)、false (それ以外の場合)。
        /// </returns>
        protected virtual bool CanActivatePartition(ref VectorI3 position)
        {
            return true;
        }

        /// <summary>
        /// パーティション更新処理で呼び出されます。
        /// </summary>
        /// <param name="gameTime">ゲーム時間。</param>
        protected virtual void UpdatePartitionsOverride(GameTime gameTime) { }

        /// <summary>
        /// 非アクティブ化の完了を検査します。
        /// </summary>
        /// <param name="gameTime">ゲーム時間。</param>
        void CheckPassivations(GameTime gameTime)
        {
            int partitionCount = passivations.Count;
            for (int i = 0; i < partitionCount; i++)
            {
                var partition = passivations.Dequeue();

                if (!partition.PassivationCompleted)
                {
                    // 未完ならばキューへ戻す。
                    passivations.Enqueue(partition);
                    continue;
                }
                
                // 非アクティブ化の開始で取得したロックを解放。
                partition.ExitLock();

                // 隣接パーティションへ通知。
                NortifyNeighborPassivated(partition);

                // 完了を通知。
                partition.OnPassivated();

                // 解放。
                partition.Release();

                // プールへ戻す。
                partitionPool.Return(partition);
            }
        }

        /// <summary>
        /// アクティブ化の完了を検査します。
        /// </summary>
        /// <param name="gameTime">ゲーム時間。</param>
        void CheckActivations(GameTime gameTime)
        {
            int partitionCount = activations.Count;
            for (int i = 0; i < partitionCount; i++)
            {
                var partition = activations.Dequeue();

                if (!partition.ActivationCompleted)
                {
                    // 未完ならばキューへ戻す。
                    activations.Enqueue(partition);
                    continue;
                }

                // アクティブ リストへ追加。
                ActivePartitions.Enqueue(partition);

                // 完了を通知。
                partition.OnActivated();

                // 隣接パーティションへ通知。
                NotifyNeighborActivated(partition);
            }
        }

        /// <summary>
        /// パーティションが非アクティブ化された事を隣接パーティションへ通知します。
        /// </summary>
        /// <param name="partition">非アクティブ化されたパーティション。</param>
        void NortifyNeighborPassivated(Partition partition)
        {
            foreach (var side in CubicSide.Items)
            {
                var nearbyPosition = partition.Position + side.Direction;

                Partition neighbor;
                if (!ActivePartitions.TryGetPartition(ref nearbyPosition, out neighbor))
                {
                    // passivatingPartitions にあるパーティションは、
                    // その非アクティブ化が取り消されて activePartitions に戻され、
                    // 次の非アクティブ化判定の試行では非アクティブ化対象ではなくなる場合がある。
                    // このため、passivatingPartitions にあるパーティションについても探索する。
                    if (!passivations.TryGetItem(ref nearbyPosition, out neighbor))
                        continue;
                }

                var reverseSide = side.Reverse();
                neighbor.OnNeighborPassivated(partition, reverseSide);
            }
        }

        /// <summary>
        /// パーティションがアクティブ化された事を隣接パーティションへ通知します。
        /// </summary>
        /// <param name="partition">アクティブ化されたパーティション。</param>
        void NotifyNeighborActivated(Partition partition)
        {
            foreach (var side in CubicSide.Items)
            {
                var nearbyPosition = partition.Position + side.Direction;

                Partition neighbor;
                if (!ActivePartitions.TryGetPartition(ref nearbyPosition, out neighbor))
                {
                    // passivatingPartitions にあるパーティションは、
                    // その非アクティブ化が取り消されて activePartitions に戻され、
                    // 次の非アクティブ化判定の試行では非アクティブ化対象ではなくなる場合がある。
                    // このため、passivatingPartitions にあるパーティションについても探索する。
                    if (!passivations.TryGetItem(ref nearbyPosition, out neighbor))
                        continue;
                }

                // partition へアクティブな隣接パーティションを通知。
                partition.OnNeighborActivated(neighbor, side);

                // アクティブな隣接パーティションへ partition を通知。
                var reverseSide = side.Reverse();
                neighbor.OnNeighborActivated(partition, reverseSide);
            }
        }

        /// <summary>
        /// 非アクティブ化待機中のパーティションを検査します。
        /// </summary>
        /// <param name="gameTime">ゲーム時間。</param>
        void CheckWaitPassivations(GameTime gameTime)
        {
            // 視点から近い順に並び替え。
            // ループ内で待機リストを末尾から処理する際、
            // 要素削除で発生する配列コピーを回避する目的。
            PartitionDistanceAscComparer.Instance.EyePosition = eyePositionWorld;
            waitPassivations.Sort(PartitionDistanceAscComparer.Instance);

            // 視点から遠いパーティションを優先してアクティブ化。
            for (int i = waitPassivations.Count - 1; 0 <= i; i--)
            {
                if (passivationCapacity <= passivations.Count) break;

                var partition = waitPassivations[i];
                waitPassivations.RemoveAt(i);

                // ロックの取得を試行。
                // ロックを取得できない場合は非アクティブ化を断念。
                if (!partition.EnterLock()) continue;

                // 実行キューへ追加。
                passivations.Enqueue(partition);

                // 開始を通知。
                partition.OnPassivating();

                // 非同期処理を要求。
                passivationTaskQueue.Enqueue(partition.PassivateAction);
            }
        }

        /// <summary>
        /// パーティションの非アクティブ化を試行します。
        /// 非アクティブ化すべきと判定されたパーティションは、
        /// 非アクティブ化中リストへ追加され、
        /// 非同期に非アクティブ化を実行するためのタスク キューへ追加されます。
        /// なお、同時に非アクティブ化可能なパーティションの数には上限があり、
        /// 上限に到達した場合には、このフレームでの非アクティブ化は保留されます。
        /// </summary>
        /// <param name="gameTime">ゲーム時間。</param>
        void PassivatePartitions(GameTime gameTime)
        {
            int count = Math.Min(ActivePartitions.Count, passivationSearchCapacity);
            for (int i = 0; i < count; i++)
            {
                // 同時待機許容数を越えるならば終了。
                if (waitPassivationCapacity <= waitPassivations.Count) break;

                var partition = ActivePartitions.Dequeue();

                if (!Closing)
                {
                    if (partition.IsInLandscapeVolume(maxLandscapeVolume))
                    {
                        // アクティブ状態維持領域内ならばアクティブ リストへ戻す。
                        ActivePartitions.Enqueue(partition);
                        continue;
                    }
                }

                // 既に待機中ならスキップ。
                if (waitPassivations.Contains(partition)) continue;

                // アクティブ化待機中ならばそれを破棄。
                if (waitActivations.Contains(partition))
                    waitActivations.Remove(partition);

                // 待機リストへ追加。
                waitPassivations.Add(partition);
            }
        }

        /// <summary>
        /// アクティブ化待機中のパーティションを検査します。
        /// </summary>
        /// <param name="gameTime">ゲーム時間。</param>
        void CheckWaitActivations(GameTime gameTime)
        {
            // 視点から遠い順に並べ替える。
            // ループ内で待機リストを末尾から処理する際、
            // 要素削除で発生する配列コピーを回避する目的。
            PartitionDistanceDescComparer.Instance.EyePosition = eyePositionWorld;
            waitActivations.Sort(PartitionDistanceDescComparer.Instance);

            // 視点から近いパーティションを優先してアクティブ化。
            for (int i = waitActivations.Count - 1; 0 <= i; i--)
            {
                // 同時実行許容量を越えるならば終了。
                if (activationCapacity <= activations.Count) break;

                var partition = waitActivations[i];
                waitActivations.RemoveAt(i);

                // アクティブ化キューへ追加。
                activations.Enqueue(partition);

                // アクティブ化の開始を通知。
                partition.OnActivating();

                // 非同期処理を要求。
                activationTaskQueue.Enqueue(partition.ActivateAction);
            }
        }

        /// <summary>
        /// パーティションのアクティブ化を試行します。
        /// アクティブ化すべきと判定されたパーティションは、
        /// アクティブ化中リストへ追加され、
        /// 非同期にアクティブ化を実行するためのタスク キューへ追加されます。
        /// なお、同時にアクティブ化可能なパーティションの数には上限があり、
        /// 上限に到達した場合には、このフレームでのアクティブ化は保留されます。
        /// </summary>
        /// <param name="gameTime">ゲーム時間。</param>
        void ActivatePartitions(GameTime gameTime)
        {
            int index = activationSearchOffset;
            bool cycled = false;
            for (int i = 0; i < activationSearchCapacity; i++)
            {
                // 同時待機許容数を越えるならば処理終了。
                if (waitActivationCapacity <= waitActivations.Count) break;

                // index が末尾に到達したら先頭へ戻し、循環したとしてマーク。
                if (minActivePointOffsets.Length <= index)
                {
                    index = 0;
                    cycled = true;
                }

                // 循環かつ最初のオフセットに到達しているならば処理終了。
                if (cycled && activationSearchOffset <= index) break;

                var position = eyePosition + minActivePointOffsets[index++];

                // 既にアクティブならばスキップ。
                if (ActivePartitions.Contains(ref position)) continue;

                // 既に待機中ならばスキップ。
                if (waitActivations.Contains(position)) continue;

                // 非アクティブ化待機中ならばそれを破棄。
                if (waitPassivations.Contains(position))
                    waitPassivations.Remove(position);

                // アクティブ化可能であるかどうか。
                if (!CanActivatePartition(ref position)) continue;

                // プールからパーティションを取得。
                // プール枯渇ならば終了。
                var partition = partitionPool.Borrow();
                if (partition == null) break;

                // パーティションを初期化。
                if (partition.Initialize(position, partitionSize))
                {
                    // 待機リストへ追加。
                    waitActivations.Add(partition);
                }
                else
                {
                    // 初期化失敗ならば取消。
                    partitionPool.Return(partition);
                }
            }

            activationSearchOffset = index;
        }

        /// <summary>
        /// パーティションを更新します。
        /// </summary>
        /// <param name="gameTime">ゲーム時間。</param>
        void UpdatePartitions(GameTime gameTime)
        {
            // TODO
            // モニタ

            UpdatePartitionsOverride(gameTime);
        }

        /// <summary>
        /// インスタンスの破棄で呼び出されます。
        /// </summary>
        /// <param name="disposing">
        /// true (明示的な破棄である場合)、false (それ以外の場合)。
        /// </param>
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

            DisposePartitions(ActivePartitions);
            ActivePartitions.Clear();

            DisposePartitions(activations);
            activations.Clear();

            DisposePartitions(passivations);
            passivations.Clear();

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
