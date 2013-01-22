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

            int activePartitionCapacity = 5000;

            int activeClusterCapacity = 50;

            int waitActivationCapacity = 9;

            int waitPassivationCapacity = 30;

            int activationCapacity = 3;

            int passivationCapacity = 10;

            int activationSearchCapacity = 100;

            int passivationSearchCapacity = 200;

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

            public int ActivePartitionCapacity
            {
                get { return activePartitionCapacity; }
                set
                {
                    if (value < 0) throw new ArgumentOutOfRangeException("value");

                    activePartitionCapacity = value;
                }
            }

            public int ActiveClusterCapacity
            {
                get { return activeClusterCapacity; }
                set
                {
                    if (value < 0) throw new ArgumentOutOfRangeException("value");

                    activeClusterCapacity = value;
                }
            }

            public int WaitActivationCapacity
            {
                get { return waitActivationCapacity; }
                set
                {
                    if (value < 1) throw new ArgumentOutOfRangeException("value");

                    waitActivationCapacity = value;
                }
            }

            public int WaitPassivationCapacity
            {
                get { return waitPassivationCapacity; }
                set
                {
                    if (value < 1) throw new ArgumentOutOfRangeException("value");

                    waitPassivationCapacity = value;
                }
            }

            public int ActivationCapacity
            {
                get { return activationCapacity; }
                set
                {
                    if (value < 1) throw new ArgumentOutOfRangeException("value");

                    activationCapacity = value;
                }
            }

            public int PassivationCapacity
            {
                get { return passivationCapacity; }
                set
                {
                    if (value < 1) throw new ArgumentOutOfRangeException("value");

                    passivationCapacity = value;
                }
            }

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

            public ILandscapeVolume MinLandscapeVolume { get; set; }

            public ILandscapeVolume MaxLandscapeVolume { get; set; }
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

        #region PartitionQueue

        /// <summary>
        /// 位置による参照を可能とするパーティションのキューです。
        /// </summary>
        sealed class PartitionQueue : KeyedQueue<VectorI3, Partition>
        {
            /// <summary>
            /// インスタンスを生成します。
            /// </summary>
            /// <param name="capacity">初期容量。</param>
            internal PartitionQueue(int capacity)
                : base(capacity)
            {
            }

            /// <summary>
            /// パーティションの位置をキーとして返します。
            /// </summary>
            /// <param name="item">パーティション。</param>
            /// <returns>パーティションの位置。</returns>
            protected override VectorI3 GetKeyForItem(Partition item)
            {
                return item.Position;
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
        /// クラスタ マネージャ。
        /// アクティブなパーティションを参照する場合は、
        /// 基本的にはクラスタ マネージャから参照します。
        /// </summary>
        ClusterManager clusterManager;

        /// <summary>
        /// アクティブ パーティションの連結リスト。
        /// 連結リストのノードはパーティションが管理しています。
        /// アクティブ化と非アクティブ化の判定のために用いる連結リストであり、
        /// 要素として登録されているパーティションの順序は、
        /// アクティブ化と非アクティブ化の判定が実行されるごとに変化します。
        /// </summary>
        LinkedList<Partition> partitions;

        /// <summary>
        /// LRU アルゴリズムのための連結リスト。
        /// 描画などでパーティションを利用したら、末尾へノードを設定し直します。
        /// これにより、最も利用されていないパーティションのノードの位置は先頭へ向かうため、
        /// パーティション許容量を越えた場合には、先頭ノードが表すパーティションから順に
        /// 非アクティブ化します。
        /// </summary>
        LinkedList<Partition> lruPartitions;

        /// <summary>
        /// アクティブ化待機リスト。
        /// </summary>
        PartitionCollection waitActivations;

        /// <summary>
        /// 非アクティブ化待機リスト。
        /// </summary>
        PartitionCollection waitPassivations;

        /// <summary>
        /// アクティブ化実行キュー。
        /// </summary>
        PartitionQueue activations;

        /// <summary>
        /// 非アクティブ化実行キュー。
        /// </summary>
        PartitionQueue passivations;

        int activePartitionCapacity;

        /// <summary>
        /// 同時アクティブ化待機許容量。
        /// </summary>
        int waitActivationCapacity;
        
        /// <summary>
        /// 同時非アクティブ化待機許容量。
        /// </summary>
        int waitPassivationCapacity;

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

        Matrix view;

        Matrix projection;

        PartitionOctree octree;

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
            get { return clusterManager.Count; }
        }

        /// <summary>
        /// アクティブ パーティション数を取得します。
        /// </summary>
        public int ActivePartitionCount
        {
            get { return partitions.Count; }
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

            // TODO
            clusterManager = new ClusterManager(16, settings.PartitionSize, settings.ActiveClusterCapacity);
            partitions = new LinkedList<Partition>();
            lruPartitions = new LinkedList<Partition>();

            activePartitionCapacity = settings.ActivePartitionCapacity;
            waitActivationCapacity = settings.WaitActivationCapacity;
            waitPassivationCapacity = settings.WaitPassivationCapacity;
            activationCapacity = settings.ActivationCapacity;
            passivationCapacity = settings.PassivationCapacity;

            waitActivations = new PartitionCollection(waitActivationCapacity);
            waitPassivations = new PartitionCollection(waitPassivationCapacity);

            activations = new PartitionQueue(activationCapacity);
            passivations = new PartitionQueue(passivationCapacity);

            activationTaskQueue.SlotCount = activationCapacity;
            passivationTaskQueue.SlotCount = passivationCapacity;

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

            octree = new PartitionOctree(512, 16);
        }

        /// <summary>
        /// パーティションのアクティブ化と非アクティブ化を行います。
        /// </summary>
        /// <param name="gameTime">ゲーム時間。</param>
        /// <param name="eyePositionWorld">ワールド空間における視点の位置。</param>
        public void Update(GameTime gameTime, Matrix view, Matrix projection)
        {
            if (Closed) return;

            Monitor.Begin(MonitorUpdate);

            this.view = view;
            this.projection = projection;

            this.eyePositionWorld = View.GetPosition(view);

            eyePosition.X = (int) Math.Floor(eyePositionWorld.X * inversePartitionSize.X);
            eyePosition.Y = (int) Math.Floor(eyePositionWorld.Y * inversePartitionSize.Y);
            eyePosition.Z = (int) Math.Floor(eyePositionWorld.Z * inversePartitionSize.Z);

            // アクティブ領域を現在の視点位置を中心に設定。
            maxLandscapeVolume.Center = eyePosition;

            // 八分木の境界錐台を更新。
            octree.Update(view, projection);

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
                if (activationTaskQueue.QueueCount != 0)
                    activationTaskQueue.Clear();

                if (waitActivations.Count != 0)
                {
                    DisposePartitions(waitActivations);
                    waitActivations.Clear();
                }
                
                if (activations.Count != 0)
                {
                    DisposePartitions(activations);
                    activations.Clear();
                }

                // 非アクティブ化関連を処理。
                passivationTaskQueue.Update();
                CheckPassivations(gameTime);
                CheckWaitPassivations(gameTime);
                PassivatePartitions(gameTime);

                // パーティション内部でも必要に応じてクローズのための更新を行う。
                UpdatePartitions(gameTime);

                // 全ての非アクティブ化が完了していればクローズ完了。
                if (passivations.Count == 0 && partitions.Count == 0)
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
        /// <param name="result">収集先パーティションのコレクション。</param>
        public void CollectPartitions(BoundingFrustum frustum, ICollection<Partition> result)
        {
            clusterManager.Collect(frustum, result);
        }

        /// <summary>
        /// 指定のパーティションを利用したものとマークします。
        /// このメソッドを呼び出す事で、パーティションに対応するノードは、
        /// LRU 連結リストの末尾へ移動します。
        /// </summary>
        /// <param name="partition">パーティション。</param>
        public void TouchPartition(Partition partition)
        {
            if (partition == null) throw new ArgumentNullException("partition");

            lruPartitions.Remove(partition.LruNode);
            lruPartitions.AddLast(partition.LruNode);
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
        protected virtual bool CanActivatePartition(VectorI3 position)
        {
            return true;
        }

        /// <summary>
        /// パーティション更新処理で呼び出されます。
        /// </summary>
        /// <param name="gameTime">ゲーム時間。</param>
        protected virtual void UpdatePartitionsOverride(GameTime gameTime) { }

        protected Partition GetActivePartition(VectorI3 position)
        {
            return clusterManager.GetPartition(position);
        }

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
                clusterManager.AddPartition(partition);
                partitions.AddLast(partition.ListNode);
                lruPartitions.AddLast(partition.LruNode);

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

                var neighbor = clusterManager.GetPartition(nearbyPosition);
                if (neighbor == null)
                {
                    // 非アクティブ化待機中パーティションは、待機取消が発生する可能性がある。
                    // このため、非アクティブ化待機中パーティションについても探索。
                    if (!waitPassivations.TryGet(nearbyPosition, out neighbor))
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

                var neighbor = clusterManager.GetPartition(nearbyPosition);
                if (neighbor == null)
                {
                    // 非アクティブ化待機中パーティションは、待機取消が発生する可能性がある。
                    // このため、非アクティブ化待機中パーティションについても探索。
                    if (!waitPassivations.TryGet(nearbyPosition, out neighbor))
                        continue;
                }

                // 自身へアクティブな隣接パーティションを通知。
                partition.OnNeighborActivated(neighbor, side);

                // アクティブな隣接パーティションへ自身を通知。
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

            // 視点から遠いパーティションを優先。
            for (int i = waitPassivations.Count - 1; 0 <= i; i--)
            {
                if (passivationCapacity <= passivations.Count) break;

                var partition = waitPassivations[i];
                waitPassivations.RemoveAt(i);

                // 非アクティブ化が抑制されている場合は断念。
                if (partition.SuppressPassivation) continue;

                // ロックの取得を試行。
                // ロックを取得できない場合は断念。
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
            //while (activePartitionCapacity < lruPartitions.Count)
            //{
            //    // 同時待機許容数を越えるならば終了。
            //    if (waitPassivationCapacity <= waitPassivations.Count) break;

            //    var node = lruPartitions.First;
            //    var partition = node.Value;

            //    if (minLandscapeVolume.Contains(partition.Position))
            //    {
            //        // 最小アクティブ領域内ならば非アクティブ化しない。
            //        // ここで強制的に利用した状態としてマーク。
            //        TouchPartition(partition);
            //        continue;
            //    }

            //    clusterManager.RemovePartition(partition);
            //    partitions.Remove(partition.ListNode);
            //    lruPartitions.RemoveFirst();

            //    // アクティブ化待機リストにあるならばそれを削除。
            //    waitActivations.Remove(partition);

            //    // 待機リストへ追加。
            //    waitPassivations.Add(partition);
            //}

            int count = Math.Min(partitions.Count, passivationSearchCapacity);

            for (int i = 0; i < count; i++)
            {
                // 同時待機許容数を越えるならば終了。
                if (waitPassivationCapacity <= waitPassivations.Count) break;

                var node = partitions.First;
                partitions.RemoveFirst();

                var partition = node.Value;

                if (!Closing)
                {
                    if (octree.Contains(partition.PositionWorld - eyePositionWorld))
                    {
                        partitions.AddLast(node);
                        continue;
                    }

                    //if (maxLandscapeVolume.Contains(partition.Position))
                    ////if (minLandscapeVolume.Contains(partition.Position))
                    //{
                    //    // アクティブ状態維持領域内ならばリストへ戻す。
                    //    partitions.AddLast(node);
                    //    continue;
                    //}
                }

                // アクティブではない状態にする。
                clusterManager.RemovePartition(partition);
                lruPartitions.Remove(partition.LruNode);

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

            // 視点から近いパーティションを優先。
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

        void TryActivate(PartitionOctree.Leaf leaf)
        {
            // ここで制御すると、視点近隣以外のノードで待機リストが占有されてしまう。
            //if (waitActivationCapacity <= waitActivations.Count) return;

            var position = eyePosition + leaf.PartitionOffset;

            // 既にアクティブならばスキップ。
            if (clusterManager.ContainsPartition(position)) return;

            // 既に実行中ならばスキップ。
            if (activations.Contains(position)) return;

            // 既に待機中ならばスキップ。
            if (waitActivations.Contains(position)) return;

            // 非アクティブ化待機中ならばそれを破棄。
            if (waitPassivations.Contains(position))
                waitPassivations.Remove(position);

            // アクティブ化可能であるかどうか。
            if (!CanActivatePartition(position)) return;

            // プールからパーティションを取得。
            // プール枯渇ならば終了。
            var partition = partitionPool.Borrow();
            
            // TODO
            if (partition == null) return;

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
            octree.Execute(TryActivate);

            //int index = activationSearchOffset;
            //bool cycled = false;
            //for (int i = 0; i < activationSearchCapacity; i++)
            //{
            //    // 同時待機許容数を越えるならば処理終了。
            //    if (waitActivationCapacity <= waitActivations.Count) break;

            //    // index が末尾に到達したら先頭へ戻し、循環したとしてマーク。
            //    if (minActivePointOffsets.Length <= index)
            //    {
            //        index = 0;
            //        cycled = true;
            //    }

            //    // 循環かつ最初のオフセットに到達しているならば処理終了。
            //    if (cycled && activationSearchOffset <= index) break;

            //    var position = eyePosition + minActivePointOffsets[index++];

            //    // 既にアクティブならばスキップ。
            //    if (clusterManager.ContainsPartition(position)) continue;

            //    // TODO: 実行中もスキップ。

            //    // 既に待機中ならばスキップ。
            //    if (waitActivations.Contains(position)) continue;

            //    // 非アクティブ化待機中ならばそれを破棄。
            //    if (waitPassivations.Contains(position))
            //        waitPassivations.Remove(position);

            //    // アクティブ化可能であるかどうか。
            //    if (!CanActivatePartition(position)) continue;

            //    // プールからパーティションを取得。
            //    // プール枯渇ならば終了。
            //    var partition = partitionPool.Borrow();
            //    if (partition == null) break;

            //    // パーティションを初期化。
            //    if (partition.Initialize(position, partitionSize))
            //    {
            //        // 待機リストへ追加。
            //        waitActivations.Add(partition);
            //    }
            //    else
            //    {
            //        // 初期化失敗ならば取消。
            //        partitionPool.Return(partition);
            //    }
            //}

            //activationSearchOffset = index;
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

            DisposePartitions(partitions);
            partitions.Clear();

            DisposePartitions(waitActivations);
            waitActivations.Clear();

            DisposePartitions(waitPassivations);
            waitPassivations.Clear();

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

        void DisposePartitions(IEnumerable<Partition> partitions)
        {
            foreach (var partition in partitions)
                partition.Dispose();
        }

        #endregion
    }
}
