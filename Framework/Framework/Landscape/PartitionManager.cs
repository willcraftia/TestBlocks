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

            VectorI3 clusterSize = new VectorI3(8);

            int activationCapacity = 3;

            int passivationCapacity = 10;

            int passivationSearchCapacity = 200;

            float priorActiveDistance = 6 * 16;

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

            public VectorI3 ClusterSize
            {
                get { return clusterSize; }
                set
                {
                    if (value.X < 1 || value.Y < 1 || value.Z < 1) throw new ArgumentOutOfRangeException("value");

                    clusterSize = value;
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

            public int PassivationSearchCapacity
            {
                get { return passivationSearchCapacity; }
                set
                {
                    if (value < 1) throw new ArgumentOutOfRangeException("value");

                    passivationSearchCapacity = value;
                }
            }

            public float PriorActiveDistance
            {
                get { return priorActiveDistance; }
                set
                {
                    if (value < 0) throw new ArgumentOutOfRangeException("value");

                    priorActiveDistance = value;
                }
            }

            public IActiveVolume MinActiveVolume { get; set; }

            public IActiveVolume MaxActiveVolume { get; set; }
        }

        #endregion

        #region Activator

        /// <summary>
        /// 非同期にアクティブ化判定とアクティブ化要求を実行するクラスです。
        /// </summary>
        sealed class Activator
        {
            #region Candidate

            /// <summary>
            /// アクティブ化候補の構造体です。
            /// </summary>
            struct Candidate
            {
                /// <summary>
                /// パーティション空間におけるパーティションの位置。
                /// </summary>
                public VectorI3 Position;

                /// <summary>
                /// ワールド空間におけるパーティションの原点位置。
                /// </summary>
                public Vector3 PositionWorld;

                /// <summary>
                /// ワールド空間におけるパーティションの境界ボックス。
                /// </summary>
                public BoundingBox BoxWorld;

                /// <summary>
                /// ワールド空間におけるパーティションの中心位置。
                /// </summary>
                public Vector3 CenterWorld;
            }

            #endregion

            #region CandidateComparer

            /// <summary>
            /// アクティブ化候補の優先度を決定するための比較クラスです。
            /// </summary>
            sealed class CandidateComparer : IComparer<Candidate>
            {
                BoundingFrustum frustum = new BoundingFrustum(Matrix.Identity);

                float priorDistanceSquared;

                Vector3 eyePositionWorld;

                public void Initialize(Matrix view, Matrix projection, float priorDistance)
                {
                    frustum.Matrix = view * projection;
                    priorDistanceSquared = priorDistance * priorDistance;

                    View.GetPosition(ref view, out eyePositionWorld);
                }

                // I/F
                public int Compare(Candidate candidate1, Candidate candidate2)
                {
                    float distance1;
                    Vector3.DistanceSquared(ref candidate1.CenterWorld, ref eyePositionWorld, out distance1);

                    float distance2;
                    Vector3.DistanceSquared(ref candidate2.CenterWorld, ref eyePositionWorld, out distance2);

                    // 優先領域にある物をより優先。
                    if (distance1 <= priorDistanceSquared && priorDistanceSquared < distance2) return -1;
                    if (priorDistanceSquared < distance1 && distance2 <= priorDistanceSquared) return 1;

                    // 視錐台に含まれる物は、含まれない物より優先。
                    bool intersected1;
                    frustum.Intersects(ref candidate1.BoxWorld, out intersected1);
                    bool intersected2;
                    frustum.Intersects(ref candidate2.BoxWorld, out intersected2);

                    if (intersected1 && !intersected2) return -1;
                    if (!intersected1 && intersected2) return 1;

                    // 互いに視錐台に含まれる、あるいは、含まれない場合、
                    // より視点に近い物を優先。

                    if (distance1 == distance2) return 0;
                    return distance1 < distance2 ? -1 : 1;
                }
            }

            #endregion

            // 初期容量を事前に定めることが難しい。
            // ひとまず、最大距離であろう 16 を基準に考える。
            const int candidateQueueCapacity = 16 * 16 * 16;

            /// <summary>
            /// パーティション マネージャ。
            /// </summary>
            PartitionManager manager;

            /// <summary>
            /// ビュー行列。
            /// </summary>
            Matrix view;

            /// <summary>
            /// 射影行列。
            /// </summary>
            Matrix projection;

            /// <summary>
            /// 視点位置。
            /// </summary>
            VectorI3 eyePosition;

            /// <summary>
            /// アクティブ領域。
            /// </summary>
            IActiveVolume volume;

            /// <summary>
            /// 優先度付き候補キュー。
            /// </summary>
            PriorityQueue<Candidate> candidates;

            /// <summary>
            /// 候補キューにおける優先度を判定するための比較オブジェクト。
            /// </summary>
            CandidateComparer comparer = new CandidateComparer();

            /// <summary>
            /// Collect メソッドのデリゲート。
            /// </summary>
            Action<VectorI3> collectAction;

            /// <summary>
            /// 非同期処理中であるか否かを示す値。
            /// </summary>
            volatile bool active;

            /// <summary>
            /// 非同期処理中であるか否かを示す値を取得します。
            /// </summary>
            /// <value>
            /// true (非同期処理中である場合)、false (それ以外の場合)。
            /// </value>
            public bool Active
            {
                get { return active; }
            }

            /// <summary>
            /// インスタンスを生成します。
            /// </summary>
            /// <param name="manager">パーティション マネージャ。</param>
            public Activator(PartitionManager manager)
            {
                this.manager = manager;
                collectAction = new Action<VectorI3>(Collect);

                candidates = new PriorityQueue<Candidate>(candidateQueueCapacity, comparer);
            }

            /// <summary>
            /// 非同期処理を開始します。
            /// </summary>
            /// <param name="view">ビュー行列。</param>
            /// <param name="projection">射影行列。</param>
            /// <param name="eyePosition">視点位置。</param>
            /// <param name="volume">アクティブ領域。</param>
            /// <param name="priorDistance">アクティブ化優先距離。</param>
            public void Start(Matrix view, Matrix projection, VectorI3 eyePosition, IActiveVolume volume, float priorDistance)
            {
                if (active) return;

                this.view = view;
                this.projection = projection;
                this.eyePosition = eyePosition;
                this.volume = volume;

                comparer.Initialize(view, projection, priorDistance);

                active = true;

                System.Threading.ThreadPool.QueueUserWorkItem(WaitCallback, null);
            }

            /// <summary>
            /// スレッド プールのコールバック メソッドです。
            /// </summary>
            /// <param name="state"></param>
            void WaitCallback(object state)
            {
                // 候補を探索。
                volume.ForEach(collectAction);

                // 候補をアクティブ化。
                Activate();

                active = false;
            }

            /// <summary>
            /// 指定のオフセット位置にあるパーティションが候補となるか否かを検査し、
            /// 候補となる場合には候補キューへ入れます。
            /// </summary>
            /// <param name="offset"></param>
            void Collect(VectorI3 offset)
            {
                var position = eyePosition + offset;

                // 既にアクティブならばスキップ。
                if (manager.ContainsPartition(position)) return;

                // 既に実行中ならばスキップ。
                if (manager.activations.Contains(position)) return;

                // アクティブ化可能であるかどうか。
                if (!manager.CanActivatePartition(position)) return;

                var candidate = new Candidate();
                candidate.Position = position;
                candidate.PositionWorld = new Vector3
                {
                    X = position.X * manager.PartitionSize.X,
                    Y = position.Y * manager.PartitionSize.Y,
                    Z = position.Z * manager.PartitionSize.Z,
                };
                candidate.BoxWorld.Min = candidate.PositionWorld;
                candidate.BoxWorld.Max = candidate.PositionWorld + manager.PartitionSize;
                candidate.CenterWorld = candidate.BoxWorld.GetCenter();

                candidates.Enqueue(candidate);
            }

            /// <summary>
            /// 候補を順にアクティブ化します。
            /// </summary>
            void Activate()
            {
                while (0 < candidates.Count)
                {
                    // 同時実行許容量を越えるならば終了。
                    if (manager.activationCapacity <= manager.activations.Count) break;

                    // 候補を取得。
                    var candidate = candidates.Dequeue();

                    // インスタンス生成。
                    var partition = manager.CreatePartition(candidate.Position);

                    // 生成が拒否されたら終了。
                    if (partition == null) break;

                    // 実行中キューへ追加。
                    manager.activations.Enqueue(partition);

                    // アクティブ化の開始を通知。
                    partition.OnActivating();

                    // 非同期処理を要求。
                    manager.activationTaskQueue.Enqueue(partition.ActivateAction);
                }

                // 実行キューへ追加しなかった全てを取消。
                candidates.Clear();
            }
        }

        #endregion

        public const string MonitorUpdate = "PartitionManager.Update";

        /// <summary>
        /// ワールド空間におけるパーティションのサイズ。
        /// </summary>
        protected readonly Vector3 PartitionSize;

        /// <summary>
        /// クラスタ マネージャ。
        /// アクティブなパーティションを参照する場合は、
        /// 基本的にはクラスタ マネージャから参照します。
        /// </summary>
        ClusterManager clusterManager;

        /// <summary>
        /// アクティブ パーティションのキュー。
        /// 非アクティブ化調査では、このキューの先頭から順にパーティションを取り出し、
        /// 非アクティブ化可能であるか否かを判定します。
        /// 非アクティブ化不能であった場合、このキューの末尾へパーティションを戻します。
        /// </summary>
        Queue<Partition> partitions;

        /// <summary>
        /// アクティベータ。
        /// </summary>
        Activator activator;

        /// <summary>
        /// アクティブ化実行キュー。
        /// </summary>
        ConcurrentKeyedQueue<VectorI3, Partition> activations;

        /// <summary>
        /// 非アクティブ化実行キュー。
        /// </summary>
        Queue<Partition> passivations;

        /// <summary>
        /// 同時アクティブ化実行許容量。
        /// </summary>
        int activationCapacity;

        /// <summary>
        /// 同時非アクティブ化実行許容量。
        /// </summary>
        int passivationCapacity;

        /// <summary>
        /// 非アクティブ化判定の最大試行数。
        /// </summary>
        int passivationSearchCapacity;

        float priorActiveDistance;

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
        IActiveVolume minActiveVolume;

        /// <summary>
        /// 最大アクティブ領域。
        /// </summary>
        IActiveVolume maxActiveVolume;

        /// <summary>
        /// パーティション空間における視点の位置。
        /// </summary>
        VectorI3 eyePosition;

        /// <summary>
        /// ビュー行列。
        /// </summary>
        Matrix view;

        /// <summary>
        /// 射影行列。
        /// </summary>
        Matrix projection;

        /// <summary>
        /// 指定の位置にあるアクティブなパーティションを取得します。
        /// </summary>
        /// <param name="position">パーティションの位置。</param>
        /// <returns>
        /// パーティション、あるいは、指定の位置にパーティションが存在しない場合は null。
        /// </returns>
        public Partition this[VectorI3 position]
        {
            get { return clusterManager[position]; }
        }

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
            get { return clusterManager.ClusterCount; }
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

            PartitionSize = settings.PartitionSize;

            clusterManager = new ClusterManager(settings.ClusterSize, settings.PartitionSize);
            partitions = new Queue<Partition>();

            activationCapacity = settings.ActivationCapacity;
            passivationCapacity = settings.PassivationCapacity;

            activator = new Activator(this);

            activations = new ConcurrentKeyedQueue<VectorI3, Partition>(
                (i) => { return i.Position; }, activationCapacity);
            passivations = new Queue<Partition>(passivationCapacity);

            activationTaskQueue.SlotCount = activationCapacity;
            passivationTaskQueue.SlotCount = passivationCapacity;

            // null の場合はデフォルト実装を与える。
            minActiveVolume = settings.MinActiveVolume ?? new DefaultActiveVolume(4);
            maxActiveVolume = settings.MaxActiveVolume ?? new DefaultActiveVolume(8);
            priorActiveDistance = settings.PriorActiveDistance;

            passivationSearchCapacity = settings.PassivationSearchCapacity;
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

            var eyePositionWorld = View.GetPosition(view);

            eyePosition.X = (int) Math.Floor(eyePositionWorld.X / PartitionSize.X);
            eyePosition.Y = (int) Math.Floor(eyePositionWorld.Y / PartitionSize.Y);
            eyePosition.Z = (int) Math.Floor(eyePositionWorld.Z / PartitionSize.Z);

            if (!Closing)
            {
                activationTaskQueue.Update();
                passivationTaskQueue.Update();

                CheckPassivations(gameTime);
                CheckActivations(gameTime);

                PassivatePartitions(gameTime);
                ActivatePartitions(gameTime);

                // サブクラスにおける追加の更新処理。
                UpdateOverride(gameTime);
            }
            else
            {
                // アクティブ化関連の処理は破棄。
                if (activationTaskQueue.QueueCount != 0)
                    activationTaskQueue.Clear();
                
                if (activations.Count != 0)
                    DisposePartitions(activations);

                // 非アクティブ化関連を処理。
                passivationTaskQueue.Update();
                CheckPassivations(gameTime);
                PassivatePartitions(gameTime);

                // サブクラスにおける追加の更新処理。
                // クローズ中に行うべきこともある。
                UpdateOverride(gameTime);

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
        /// 指定の位置にパーティションが存在するか否かを検査します。
        /// </summary>
        /// <param name="position">パーティション空間におけるパーティションの位置。</param>
        /// <returns>
        /// true (パーティションが存在する場合)、false (それ以外の場合)。
        /// </returns>
        public bool ContainsPartition(VectorI3 position)
        {
            return clusterManager.Contains(position);
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
        /// アクティブ化でパーティションを生成する際に呼び出されます。
        /// null を戻り値とした場合、その時点では、それ以上のアクティブ化が抑制されます。
        /// </summary>
        /// <param name="position">パーティションの位置。</param>
        /// <returns>
        /// パーティション、あるいは、アクティブ化を抑制する場合には null。
        /// </returns>
        protected abstract Partition CreatePartition(VectorI3 position);

        /// <summary>
        /// 非アクティブ化でパーティションを解放する際に呼び出されます。
        /// </summary>
        /// <param name="partition">パーティション。</param>
        protected abstract void ReleasePartition(Partition partition);

        /// <summary>
        /// パーティションがアクティブ化可能であるか否かを検査します。
        /// パーティション マネージャは、
        /// アクティブ領域からアクティブ化対象のパーティションの位置を決定しますが、
        /// 実際にその位置でパーティションをアクティブ化すべきであるか否かは実装によります。
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
        protected virtual void UpdateOverride(GameTime gameTime) { }

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
                ReleasePartition(partition);
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
                Partition partition;
                if (!activations.TryDequeue(out partition)) break;

                if (!partition.ActivationCompleted)
                {
                    // 未完ならばキューへ戻す。
                    activations.Enqueue(partition);
                    continue;
                }

                // アクティブ リストへ追加。
                clusterManager.Add(partition);
                partitions.Enqueue(partition);

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

                var neighbor = clusterManager[nearbyPosition];
                if (neighbor == null) continue;

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

                var neighbor = clusterManager[nearbyPosition];
                if (neighbor == null) continue;

                // 自身へアクティブな隣接パーティションを通知。
                partition.OnNeighborActivated(neighbor, side);

                // アクティブな隣接パーティションへ自身を通知。
                var reverseSide = side.Reverse();
                neighbor.OnNeighborActivated(partition, reverseSide);
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
            // メモ
            //
            // 非アクティブ化はパーティションのロックを伴う。
            // このため、非アクティブ化判定そのものを非同期化することは難しい。

            int count = Math.Min(partitions.Count, passivationSearchCapacity);

            for (int i = 0; i < count; i++)
            {
                // 同時実行数を越えるならば終了。
                if (passivationCapacity <= passivations.Count) break;

                var partition = partitions.Dequeue();

                if (!Closing)
                {
                    if (maxActiveVolume.Contains(eyePosition, partition.Position))
                    {
                        // アクティブ状態維持領域内ならばリストへ戻す。
                        partitions.Enqueue(partition);
                        continue;
                    }
                }

                // 非アクティブ化が抑制されている場合は断念。
                if (partition.SuppressPassivation) continue;

                // ロックの取得を試行。
                // ロックを取得できない場合は断念。
                if (!partition.EnterLock()) continue;

                // アクティブではない状態にする。
                clusterManager.Remove(partition.Position);

                // 実行キューへ追加。
                passivations.Enqueue(partition);

                // 開始を通知。
                partition.OnPassivating();

                // 非同期処理を要求。
                passivationTaskQueue.Enqueue(partition.PassivateAction);
            }
        }

        /// <summary>
        /// パーティションのアクティブ化を試行します。
        /// この試行は非同期に実行されます。
        /// なお、同時にアクティブ化できるパーティションの数には上限があり、
        /// 上限に到達した場合には、このフレームでのアクティブ化は保留されます。
        /// </summary>
        /// <param name="gameTime">ゲーム時間。</param>
        void ActivatePartitions(GameTime gameTime)
        {
            if (!activator.Active)
            {
                // 非同期処理中ではないならば、アクティベータの実行を開始。
                activator.Start(view, projection, eyePosition, minActiveVolume, priorActiveDistance);
            }
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

            //----------------------------------------------------------------
            // サブクラスにおける破棄。

            DisposeOverride(disposing);

            //----------------------------------------------------------------
            // 全パーティションの破棄。

            if (disposing)
            {
                DisposePartitions(partitions);
                DisposePartitions(activations);
                DisposePartitions(passivations);
            }

            disposed = true;
        }

        void DisposePartitions(Queue<Partition> partitions)
        {
            while (0 < partitions.Count)
                partitions.Dequeue().Dispose();
        }

        void DisposePartitions(KeyedQueue<VectorI3, Partition> partitions)
        {
            while (0 < partitions.Count)
                partitions.Dequeue().Dispose();
        }

        void DisposePartitions(ConcurrentKeyedQueue<VectorI3, Partition> partitions)
        {
            while (0 < partitions.Count)
            {
                Partition partition;
                if (partitions.TryDequeue(out partition))
                    partition.Dispose();
            }
        }

        #endregion
    }
}
