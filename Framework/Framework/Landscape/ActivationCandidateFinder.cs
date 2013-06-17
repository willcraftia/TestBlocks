#region Using

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Collections;

#endregion

namespace Willcraftia.Xna.Framework.Landscape
{
    internal sealed class ActivationCandidateFinder
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
        /// アクティブ化スレッド。
        /// </summary>
        Thread thread;

        /// <summary>
        /// アクティブ化スレッド停止イベント。
        /// </summary>
        ManualResetEvent stopEvent = new ManualResetEvent(true);

        /// <summary>
        /// パーティション マネージャ。
        /// </summary>
        PartitionManager manager;

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
        Action<IntVector3> collectAction;

        /// <summary>
        /// アクティブ化スレッドが開始しているか否かを示す値。
        /// </summary>
        volatile bool running;

        /// <summary>
        /// カメラ状態フィールドに対するロック オブジェクト。
        /// </summary>
        object cameraLock = new object();

        /// <summary>
        /// スレッドが次のループで使用するビュー行列。
        /// </summary>
        Matrix nextView;

        /// <summary>
        /// スレッドが次のループで使用する射影行列。
        /// </summary>
        Matrix nextProjection;

        /// <summary>
        /// スレッドが次のループで使用する視点位置 (ワールド空間)。
        /// </summary>
        Vector3 nextEyePositionWorld;

        /// <summary>
        /// スレッドが次のループで使用する視点位置 (パーティション空間)。
        /// </summary>
        IntVector3 nextEyePositionPartition;

        /// <summary>
        /// スレッドが現在のループで使用するビュー行列。
        /// </summary>
        Matrix view;

        /// <summary>
        /// スレッドが現在のループで使用する射影行列。
        /// </summary>
        Matrix projection;

        /// <summary>
        /// スレッドが現在のループで使用する視点位置 (ワールド空間)。
        /// </summary>
        Vector3 eyePositionWorld;

        /// <summary>
        /// スレッドが現在のループで使用する視点位置 (パーティション空間)。
        /// </summary>
        IntVector3 eyePositionPartition;

        /// <summary>
        /// アクティブ化スレッドが開始しているか否かを示す値を取得します。
        /// </summary>
        /// <value>
        /// true (アクティブ化スレッドが開始している場合)、false (それ以外の場合)。
        /// </value>
        public bool Running
        {
            get { return running; }
        }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        /// <param name="manager">パーティション マネージャ。</param>
        /// <param name="volume">アクティブ領域。</param>
        /// <param name="priorDistance">アクティブ化優先距離。</param>
        public ActivationCandidateFinder(PartitionManager manager, IActiveVolume volume, float priorDistance)
        {
            this.manager = manager;
            this.volume = volume;

            collectAction = new Action<IntVector3>(Collect);
            comparer = new CandidateComparer(priorDistance);
            candidates = new PriorityQueue<Candidate>(candidateQueueCapacity, comparer);

            thread = new Thread(Run);
        }

        /// <summary>
        /// アクティブ化スレッドを開始します。
        /// </summary>
        public void Start()
        {
            if (running)
                return;

            // 停止イベントを非シグナル状態に。
            stopEvent.Reset();

            // スレッドの開始。
            running = true;
            thread.Start();
        }

        /// <summary>
        /// アクティブ化スレッドを停止します。
        /// </summary>
        public void Stop()
        {
            // スレッド ループの停止。
            running = false;
        }

        /// <summary>
        /// アクティブ化スレッドの停止を待機します。
        /// </summary>
        public void WaitStop()
        {
            stopEvent.WaitOne();
        }

        /// <summary>
        /// カメラ状態を更新します。
        /// </summary>
        /// <param name="view">ビュー行列。</param>
        /// <param name="projection">射影行列。</param>
        /// <param name="eyePositionWorld">視点位置 (ワールド空間)。</param>
        /// <param name="eyePositionPartition">視点位置 (パーティション空間)。</param>
        public void UpdateCamera(Matrix view, Matrix projection, Vector3 eyePositionWorld, IntVector3 eyePositionPartition)
        {
            // 次のループで使用するカメラを更新。
            lock (cameraLock)
            {
                this.nextView = view;
                this.nextProjection = projection;
                this.nextEyePositionWorld = eyePositionWorld;
                this.nextEyePositionPartition = eyePositionPartition;
            }
        }

        /// <summary>
        /// スレッド内処理を実行します。
        /// </summary>
        void Run()
        {
            // スレッド ループ。
            while (running)
            {
                // カメラの反映。
                lock (cameraLock)
                {
                    view = nextView;
                    projection = nextProjection;
                    eyePositionWorld = nextEyePositionWorld;
                    eyePositionPartition = nextEyePositionPartition;
                }

                // 比較オブジェクトの視錐台を更新。
                Matrix viewProjection;
                Matrix.Multiply(ref view, ref projection, out viewProjection);
                comparer.Frustum.Matrix = viewProjection;

                // 比較オブジェクトの視点位置を更新。
                comparer.EyePositionWorld = eyePositionWorld;

                // 候補を探索。
                volume.ForEach(collectAction);

                // 候補をマネージャへ通知。
                while (0 < candidates.Count)
                {
                    // 候補を取得。
                    var candidate = candidates.Dequeue();

                    // 非同期アクティブ化を実行。
                    // マネージャが実行許容量を超えた場合は通知を停止。
                    if (!manager.RequestActivatePartition(candidate.Position))
                        break;
                }

                // 候補キューをリセット。
                candidates.Clear();
            }

            // 停止イベントをシグナル状態に。
            stopEvent.Set();
        }

        /// <summary>
        /// アクティブ領域に含まれるパーティションを探索して候補キューへ入れます。
        /// </summary>
        /// <param name="offset"></param>
        void Collect(IntVector3 offset)
        {
            var position = eyePositionPartition + offset;

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
    }
}
