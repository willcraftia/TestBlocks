#region Using

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
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

            IntVector3 clusterSize = new IntVector3(8);

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

            public IntVector3 ClusterSize
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
                public IntVector3 Position;

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
                public BoundingFrustum Frustum = new BoundingFrustum(Matrix.Identity);

                public Vector3 EyePositionWorld;

                float priorDistanceSquared;

                public CandidateComparer(float priorDistance)
                {
                    priorDistanceSquared = priorDistance * priorDistance;
                }

                public int Compare(Candidate candidate1, Candidate candidate2)
                {
                    float distance1;
                    Vector3.DistanceSquared(ref candidate1.CenterWorld, ref EyePositionWorld, out distance1);

                    float distance2;
                    Vector3.DistanceSquared(ref candidate2.CenterWorld, ref EyePositionWorld, out distance2);

                    // 優先領域にある物をより優先。
                    if (distance1 <= priorDistanceSquared && priorDistanceSquared < distance2) return -1;
                    if (priorDistanceSquared < distance1 && distance2 <= priorDistanceSquared) return 1;

                    // 視錐台に含まれる物は、含まれない物より優先。
                    bool intersected1;
                    Frustum.Intersects(ref candidate1.BoxWorld, out intersected1);
                    bool intersected2;
                    Frustum.Intersects(ref candidate2.BoxWorld, out intersected2);

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
            /// 視点位置 (パーティション空間)。
            /// </summary>
            IntVector3 eyePositionPartition;

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
            CandidateComparer comparer;

            /// <summary>
            /// Collect メソッドのデリゲート。
            /// </summary>
            RefAction<IntVector3> collectAction;

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
            /// <param name="volume">アクティブ領域。</param>
            /// <param name="priorDistance">アクティブ化優先距離。</param>
            public Activator(PartitionManager manager, IActiveVolume volume, float priorDistance)
            {
                this.manager = manager;
                this.volume = volume;
                
                collectAction = new RefAction<IntVector3>(Collect);
                comparer = new CandidateComparer(priorDistance);
                candidates = new PriorityQueue<Candidate>(candidateQueueCapacity, comparer);
            }

            /// <summary>
            /// 非同期処理を開始します。
            /// </summary>
            public void Start()
            {
                if (active) return;

                // 別スレッドで実行するため、現在のカメラ位置などを複製。
                eyePositionPartition = manager.eyePosition;

                // 比較オブジェクトの視錐台を更新。
                Matrix viewProjection;
                Matrix.Multiply(ref manager.view, ref manager.projection, out viewProjection);
                comparer.Frustum.Matrix = viewProjection;

                // 比較オブジェクトの視点位置を更新。
                comparer.EyePositionWorld = manager.eyePositionWorld;

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
            void Collect(ref IntVector3 offset)
            {
                var position = eyePositionPartition + offset;

                // 既にアクティブならばスキップ。
                if (manager.ContainsPartition(position)) return;

                // 既に実行中ならばスキップ。
                if (manager.activations.ContainsKey(position)) return;

                // アクティブ化可能であるかどうか。
                if (!manager.CanActivate(position)) return;

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
                    // 候補を取得。
                    var candidate = candidates.Dequeue();

                    // 非同期アクティブ化を実行。
                    manager.RequestActivatePartitionAsync(candidate.Position);
                }

                // 実行キューへ追加しなかった全てを取消。
                candidates.Clear();
            }
        }

        #endregion

        public const string MonitorUpdate = "PartitionManager.Update";

        public const string MonitorCheckPassivations = "PartitionManager.CheckPassivations";

        public const string MonitorCheckActivations = "PartitionManager.CheckActivations";

        public const string MonitorPassivate = "PartitionManager.Passivate";

        public const string MonitorActivate = "PartitionManager.Activate";

        /// <summary>
        /// ワールド空間におけるパーティションのサイズ。
        /// </summary>
        protected readonly Vector3 PartitionSize;

        readonly Action<object> activatePartitionAsyncAction;

        readonly Action<object> passivatePartitionAsyncAction;

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

        ConcurrentDictionary<IntVector3, Partition> activations;

        ConcurrentQueue<Partition> finishedActivationTasks = new ConcurrentQueue<Partition>();

        ConcurrentDictionary<IntVector3, Partition> passivations;

        ConcurrentQueue<Partition> finishedPassivationTasks = new ConcurrentQueue<Partition>();

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

        /// <summary>
        /// 最大アクティブ領域。
        /// </summary>
        IActiveVolume maxActiveVolume;

        /// <summary>
        /// 視点位置 (パーティション空間)。
        /// </summary>
        IntVector3 eyePosition;

        /// <summary>
        /// 視点位置 (ワールド空間)。
        /// </summary>
        Vector3 eyePositionWorld;

        /// <summary>
        /// ビュー行列。
        /// </summary>
        Matrix view;

        /// <summary>
        /// 射影行列。
        /// </summary>
        Matrix projection;

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
        public int ClusterCount
        {
            get { return clusterManager.ClusterCount; }
        }

        /// <summary>
        /// パーティション数を取得します。
        /// この数には、アクティブ化が完了していないパーティション、および、
        /// 非アクティブ化の開始したパーティションは含まれません。
        /// </summary>
        public int Count
        {
            get { return partitions.Count; }
        }

        /// <summary>
        /// アクティブ化中パーティション数を取得します。
        /// </summary>
        public int ActivationCount
        {
            get { return activations.Count; }
        }

        /// <summary>
        /// 非アクティブ化中パーティション数を取得します。
        /// </summary>
        public int PassivationCount
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
            passivationSearchCapacity = settings.PassivationSearchCapacity;

            activations = new ConcurrentDictionary<IntVector3, Partition>(activationCapacity, activationCapacity);
            passivations = new ConcurrentDictionary<IntVector3, Partition>(passivationCapacity, passivationCapacity);

            maxActiveVolume = settings.MaxActiveVolume ?? new DefaultActiveVolume(8);

            var minActiveVolume = settings.MinActiveVolume ?? new DefaultActiveVolume(4);
            activator = new Activator(this, minActiveVolume, settings.PriorActiveDistance);

            activatePartitionAsyncAction = new Action<object>(ActivatePartitionAsync);
            passivatePartitionAsyncAction = new Action<object>(PassivatePartitionAsync);
        }

        /// <summary>
        /// パーティションのアクティブ化と非アクティブ化を行います。
        /// </summary>
        /// <param name="eyePositionWorld">ワールド空間における視点の位置。</param>
        public void Update(Matrix view, Matrix projection)
        {
            if (Closed) return;

            Monitor.Begin(MonitorUpdate);

            this.view = view;
            this.projection = projection;

            eyePositionWorld = View.GetPosition(view);

            eyePosition.X = (int) Math.Floor(eyePositionWorld.X / PartitionSize.X);
            eyePosition.Y = (int) Math.Floor(eyePositionWorld.Y / PartitionSize.Y);
            eyePosition.Z = (int) Math.Floor(eyePositionWorld.Z / PartitionSize.Z);

            if (!Closing)
            {
                CheckPassivations();
                CheckActivations();

                Passivate();
                Activate();

                // サブクラスにおける追加の更新処理。
                UpdateOverride();
            }
            else
            {
                CheckPassivations();
                CheckActivations();

                // クローズが開始したら新規アクティブ化を停止。
                // 非アクティブ化のみを実行。

                Passivate();

                // サブクラスにおける追加の更新処理。
                // クローズ中に行うべきこともある。
                UpdateOverride();

                // 全ての非アクティブ化が完了していればクローズ完了。
                if (activations.Count == 0 && passivations.Count == 0 && partitions.Count == 0 &&
                    finishedActivationTasks.Count == 0 && finishedPassivationTasks.Count == 0)
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
        public bool ContainsPartition(IntVector3 position)
        {
            return clusterManager.ContainsPartition(position);
        }

        /// <summary>
        /// 指定の位置にあるアクティブなパーティションを取得します。
        /// </summary>
        /// <param name="position">パーティションの位置。</param>
        /// <returns>
        /// パーティション、あるいは、指定の位置にパーティションが存在しない場合は null。
        /// </returns>
        public Partition GetPartition(IntVector3 position)
        {
            return clusterManager.GetPartition(position);
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
        protected abstract Partition Create(IntVector3 position);

        /// <summary>
        /// 非アクティブ化でパーティションを解放する際に呼び出されます。
        /// </summary>
        /// <param name="partition">パーティション。</param>
        protected abstract void Release(Partition partition);

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
        protected virtual bool CanActivate(IntVector3 position)
        {
            return true;
        }

        /// <summary>
        /// パーティション更新処理で呼び出されます。
        /// </summary>
        protected virtual void UpdateOverride() { }

        protected virtual void OnActivated(Partition partition) { }

        protected virtual void OnPassivating(Partition partition) { }

        protected virtual void OnPassivated(Partition partition) { }

        /// <summary>
        /// 非アクティブ化の完了を検査します。
        /// </summary>
        void CheckPassivations()
        {
            Monitor.Begin(MonitorCheckPassivations);

            while (!finishedPassivationTasks.IsEmpty)
            {
                Partition partition;
                if (!finishedPassivationTasks.TryDequeue(out partition))
                    break;

                Partition removedPartition;
                if (!passivations.TryRemove(partition.Position, out removedPartition))
                    continue;

                // 完了を通知。
                partition.OnPassivated();

                // 隣接パーティションへ通知。
                NortifyNeighborPassivated(partition);

                // サブクラスへも通知。
                OnPassivated(partition);

                // 解放。
                Release(partition);
            }

            Monitor.End(MonitorCheckPassivations);
        }

        /// <summary>
        /// アクティブ化の完了を検査します。
        /// </summary>
        void CheckActivations()
        {
            Monitor.Begin(MonitorCheckActivations);

            while (!finishedActivationTasks.IsEmpty)
            {
                Partition partition;
                if (!finishedActivationTasks.TryDequeue(out partition))
                    break;

                Partition removedPartition;
                if (!activations.TryRemove(partition.Position, out removedPartition))
                    continue;

                // アクティブ リストへ追加。
                clusterManager.AddPartition(partition);
                partitions.Enqueue(partition);

                // 完了を通知。
                partition.OnActivated();

                // 隣接パーティションへ通知。
                NotifyNeighborActivated(partition);

                // サブクラスへも通知。
                OnActivated(partition);
            }

            Monitor.End(MonitorCheckActivations);
        }

        /// <summary>
        /// パーティションが非アクティブ化された事を隣接パーティションへ通知します。
        /// </summary>
        /// <param name="partition">非アクティブ化されたパーティション。</param>
        void NortifyNeighborPassivated(Partition partition)
        {
            for (int i = 0; i < Side.Items.Count; i++)
            {
                var side = Side.Items[i];

                var nearbyPosition = partition.Position + side.Direction;

                var neighbor = clusterManager.GetPartition(nearbyPosition);
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
            for (int i = 0; i < Side.Items.Count; i++)
            {
                var side = Side.Items[i];

                var nearbyPosition = partition.Position + side.Direction;

                var neighbor = clusterManager.GetPartition(nearbyPosition);
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
        void Passivate()
        {
            Monitor.Begin(MonitorPassivate);

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

                // 非アクティブ化中としてマーク。
                passivations[partition.Position] = partition;

                // アクティブではない状態にする。
                clusterManager.RemovePartition(partition.Position);

                // 開始を通知。
                //
                // 注意！
                // このメソッドのスレッドと同じであることを前提に、
                // パーティションのアクティブ状態を監視するクラスもあるため、
                // 処理を変更する場合には注意すること。
                partition.OnPassivating();

                // サブクラスへも通知。
                OnPassivating(partition);

                // タスク実行。
                Task.Factory.StartNew(passivatePartitionAsyncAction, partition);
            }

            Monitor.End(MonitorPassivate);
        }

        /// <summary>
        /// 非アクティブ化タスク処理です。
        /// </summary>
        /// <param name="state">タスク状態としての非アクティブ化対象パーティション。</param>
        void PassivatePartitionAsync(object state)
        {
            var partition = state as Partition;

            // パーティションの非アクティブ化を実行。
            partition.PassivateAsync();

            // タスク終了を記録。
            finishedPassivationTasks.Enqueue(partition);
        }

        /// <summary>
        /// パーティションのアクティブ化を試行します。
        /// この試行は非同期に実行されます。
        /// なお、同時にアクティブ化できるパーティションの数には上限があり、
        /// 上限に到達した場合には、このフレームでのアクティブ化は保留されます。
        /// </summary>
        /// <param name="gameTime">ゲーム時間。</param>
        void Activate()
        {
            Monitor.Begin(MonitorActivate);

            if (!activator.Active)
            {
                // 非同期処理中ではないならば、アクティベータの実行を開始。
                activator.Start();
            }

            Monitor.End(MonitorActivate);
        }

        /// <summary>
        /// インスタンスの破棄で呼び出されます。
        /// </summary>
        /// <param name="disposing">
        /// true (明示的な破棄である場合)、false (それ以外の場合)。
        /// </param>
        protected virtual void DisposeOverride(bool disposing) { }

        /// <summary>
        /// 指定位置のパーティションをアクティブ化するように要求します。
        /// </summary>
        /// <param name="position">パーティション位置。</param>
        /// <returns>
        /// true (アクティブ化要求を許可した場合)、false (それ以外の場合)。
        /// </returns>
        internal bool RequestActivatePartitionAsync(IntVector3 position)
        {
            // 同時実行許容量を越えるならば終了。
            if (activationCapacity <= activations.Count)
                return false;

            // インスタンス生成。
            var partition = Create(position);

            // 生成が拒否されたら終了。
            if (partition == null)
                return false;

            // アクティブ化中としてマーク。
            activations[partition.Position] = partition;

            // アクティブ化の開始を通知。
            partition.OnActivating();

            // タスク実行。
            Task.Factory.StartNew(activatePartitionAsyncAction, partition);

            return true;
        }

        /// <summary>
        /// アクティブ化タスク処理です。
        /// </summary>
        /// <param name="state">タスク状態としてのアクティブ化対象パーティション。</param>
        void ActivatePartitionAsync(object state)
        {
            var partition = state as Partition;

            // パーティションへアクティブ化を要求。
            partition.ActivateAsync();
            
            // タスク完了を記録。
            finishedActivationTasks.Enqueue(partition);
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

            //----------------------------------------------------------------
            // サブクラスにおける破棄。

            DisposeOverride(disposing);

            //----------------------------------------------------------------
            // 全パーティションの破棄。

            if (disposing)
            {
                DisposeQueue(partitions);
                // TODO
                // 整理
                foreach (var partition in activations.Values)
                    partition.Dispose();
                foreach (var partition in passivations.Values)
                    partition.Dispose();
                foreach (var partition in finishedActivationTasks)
                    partition.Dispose();
                foreach (var partition in finishedPassivationTasks)
                    partition.Dispose();
            }

            disposed = true;
        }

        void DisposeQueue(Queue<Partition> partitions)
        {
            while (0 < partitions.Count)
                partitions.Dequeue().Dispose();
        }

        void DisposeConcurrentKeyedQueue(ConcurrentKeyedQueue<IntVector3, Partition> partitions)
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
