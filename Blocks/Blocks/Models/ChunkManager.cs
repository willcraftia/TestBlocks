#region Using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Collections;
using Willcraftia.Xna.Framework.Diagnostics;
using Willcraftia.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Landscape;
using Willcraftia.Xna.Framework.Threading;
using Willcraftia.Xna.Blocks.Models;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    /// <summary>
    /// チャンクをパーティションとして管理するパーティション マネージャの実装です。
    /// </summary>
    public sealed class ChunkManager : PartitionManager
    {
        #region ChunkTaskRequest

        struct ChunkTaskRequest
        {
            public Chunk Chunk;

            public ChunkTaskType Type;

            public ChunkTaskPriority Priority;
        }

        #endregion

        #region ChunkTask

        class ChunkTask
        {
            public readonly Action ExecuteAction;

            ChunkManager manager;

            Action<Chunk> task;

            public Chunk Chunk { get; private set; }

            public ChunkTaskPriority Priority { get; private set; }

            public ChunkTask(ChunkManager manager)
            {
                if (manager == null) throw new ArgumentNullException("manager");

                this.manager = manager;
                ExecuteAction = new Action(Execute);
            }

            public void Initialize(Chunk chunk, Action<Chunk> task, ChunkTaskPriority priority)
            {
                if (chunk == null) throw new ArgumentNullException("chunk");
                if (task == null) throw new ArgumentNullException("task");

                Chunk = chunk;
                this.task = task;
                Priority = priority;
            }

            public void Clear()
            {
                Chunk = null;
                task = null;
            }

            void Execute()
            {
                task(Chunk);
                manager.OnChunkTaskFinished(this);
            }
        }

        #endregion

        #region ChunkMeshBufferRequest

        struct ChunkMeshBufferRequest
        {
            public ChunkVerticesBuilder VerticesBuilder;

            public IntVector3 Segment;

            public bool Translucece;
        }

        #endregion

        public const string MonitorProcessProcessBuildVerticesRequests = "ChunkManager.ProcessBuildVerticesRequests";

        public const string MonitorProcessChunkTaskRequests = "ChunkManager.ProcessChunkTaskRequests";

        public const string MonitorUpdateMeshBuffers = "ChunkManager.UpdateMeshBuffers";

        /// <summary>
        /// メッシュ サイズ。
        /// </summary>
        public static readonly IntVector3 MeshSize = new IntVector3(16);

        /// <summary>
        /// チャンク サイズ。
        /// </summary>
        /// <remarks>
        /// チャンク サイズは、メッシュ サイズの等倍でなければなりません。
        /// </remarks>
        public readonly IntVector3 ChunkSize;

        /// <summary>
        /// セグメント サイズ。
        /// </summary>
        /// <remarks>
        /// 指定されたセグメント サイズに従い、
        /// チャンクのメッシュは複数のメッシュへ分割されます。
        /// </remarks>
        public readonly IntVector3 MeshSegments;

        /// <summary>
        /// 半チャンク サイズ。
        /// </summary>
        public readonly IntVector3 HalfChunkSize;

        /// <summary>
        /// チャンク メッシュのオフセット。
        /// </summary>
        public readonly Vector3 MeshOffset;

        /// <summary>
        /// メッシュ更新の最大試行数。
        /// </summary>
        int meshUpdateSearchCapacity;

        /// <summary>
        /// 頂点ビルダの総数。
        /// </summary>
        int verticesBuilderCount;

        /// <summary>
        /// グラフィックス デバイス。
        /// </summary>
        GraphicsDevice graphicsDevice;

        /// <summary>
        /// リージョン マネージャ。
        /// </summary>
        RegionManager regionManager;

        /// <summary>
        /// チャンクのノード名の自動決定で利用する連番の記録。
        /// </summary>
        int nodeIdSequence;

        ConcurrentPool<ChunkTask> chunkTaskPool;

        ConcurrentPool<ChunkLightUpdater> lightUpdaterPool;

        /// <summary>
        /// 通常優先度のメッシュ更新要求のキュー。
        /// </summary>
        ConcurrentQueue<Chunk> normalBuildVerticesRequests = new ConcurrentQueue<Chunk>();

        /// <summary>
        /// 高優先度のメッシュ更新要求のキュー。
        /// </summary>
        ConcurrentQueue<Chunk> highBuildVerticesRequests = new ConcurrentQueue<Chunk>();

        Dictionary<Chunk, ChunkVerticesBuilder> activeVerticesBuilder;

        /// <summary>
        /// 頂点ビルダのプール。
        /// </summary>
        Pool<ChunkVerticesBuilder> verticesBuilderPool;

        /// <summary>
        /// メッシュ名の構築で利用する文字列ビルダ。
        /// </summary>
        StringBuilder meshNameBuilder = new StringBuilder(18);

        /// <summary>
        /// 通常優先度のチャンク タスク要求のキュー。
        /// </summary>
        ConcurrentQueue<ChunkTaskRequest> normalChunkTaskRequests = new ConcurrentQueue<ChunkTaskRequest>();

        /// <summary>
        /// 高優先度のチャンク タスク要求のキュー。
        /// </summary>
        ConcurrentQueue<ChunkTaskRequest> highChunkTaskRequests = new ConcurrentQueue<ChunkTaskRequest>();

        ConcurrentQueue<ChunkMeshBufferRequest> chunkMeshBufferRequests = new ConcurrentQueue<ChunkMeshBufferRequest>();

        int updateMeshBufferPerFrame = 1;

        int updateMeshBufferFrameDelay = 0;

        int currentUpdateMeshBufferFrame;

        /// <summary>
        /// シーン マネージャ。
        /// </summary>
        public SceneManager SceneManager { get; private set; }

        /// <summary>
        /// チャンク ノードを関連付けるためのノード。
        /// </summary>
        public SceneNode BaseNode { get; private set; }

        /// <summary>
        /// チャンク メッシュの数を取得します。
        /// </summary>
        public int MeshCount { get; private set; }

        /// <summary>
        /// 頂点ビルダの総数を取得します。
        /// </summary>
        public int TotalVerticesBuilderCount
        {
            get { return verticesBuilderPool.TotalObjectCount; }
        }

        /// <summary>
        /// 未使用の頂点ビルダの数を取得します。
        /// </summary>
        public int PassiveVerticesBuilderCount
        {
            get { return verticesBuilderPool.Count; }
        }

        /// <summary>
        /// 使用中の頂点ビルダの数を取得します。
        /// </summary>
        public int ActiveVerticesBuilderCount
        {
            get { return TotalVerticesBuilderCount - PassiveVerticesBuilderCount; }
        }

        /// <summary>
        /// 頂点の総数を取得します。
        /// </summary>
        public int TotalVertexCount { get; private set; }

        /// <summary>
        /// インデックスの総数を取得します。
        /// </summary>
        public int TotalIndexCount { get; private set; }

        /// <summary>
        /// 処理全体を通じて最も大きな頂点バッファのサイズを取得します。
        /// </summary>
        public int MaxVertexCount { get; private set; }

        /// <summary>
        /// 処理全体を通じて最も大きなインデックス バッファのサイズを取得します。
        /// </summary>
        public int MaxIndexCount { get; private set; }

        public IChunkStore ChunkStore { get; private set; }

        /// <summary>
        /// 空データを取得します。
        /// </summary>
        internal ChunkData EmptyData { get; private set; }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        /// <param name="settings">チャンク設定。</param>
        /// <param name="graphicsDevice">グラフィックス デバイス。</param>
        /// <param name="regionManager">リージョン マネージャ。</param>
        /// <param name="sceneManager">シーン マネージャ。</param>
        public ChunkManager(
            ChunkSettings settings,
            GraphicsDevice graphicsDevice,
            RegionManager regionManager,
            SceneManager sceneManager)
            : base(settings.PartitionManager)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");
            if (regionManager == null) throw new ArgumentNullException("regionManager");

            ChunkSize = settings.ChunkSize;
            this.graphicsDevice = graphicsDevice;
            this.regionManager = regionManager;
            SceneManager = sceneManager;
            meshUpdateSearchCapacity = settings.MeshUpdateSearchCapacity;
            verticesBuilderCount = settings.VerticesBuilderCount;

            switch (settings.ChunkStoreType)
            {
                case ChunkStoreType.Storage:
                    ChunkStore = StorageChunkStore.Instance;
                    break;
                default:
                    ChunkStore = NullChunkStore.Instance;
                    break;
            }

            MeshSegments = new IntVector3
            {
                X = ChunkSize.X / MeshSize.X,
                Y = ChunkSize.Y / MeshSize.Y,
                Z = ChunkSize.Z / MeshSize.Z
            };

            HalfChunkSize = ChunkSize;
            HalfChunkSize.X /= 2;
            HalfChunkSize.Y /= 2;
            HalfChunkSize.Z /= 2;

            var halfMeshSize = MeshSize;
            halfMeshSize.X /= 2;
            halfMeshSize.Y /= 2;
            halfMeshSize.Z /= 2;
            MeshOffset = halfMeshSize.ToVector3();

            EmptyData = new ChunkData(this);

            activeVerticesBuilder = new Dictionary<Chunk, ChunkVerticesBuilder>(verticesBuilderCount);

            verticesBuilderPool = new Pool<ChunkVerticesBuilder>(() => { return new ChunkVerticesBuilder(this); })
            {
                MaxCapacity = verticesBuilderCount
            };

            chunkTaskPool = new ConcurrentPool<ChunkTask>(() => { return new ChunkTask(this); });
            lightUpdaterPool = new ConcurrentPool<ChunkLightUpdater>(() => { return new ChunkLightUpdater(this); });

            BaseNode = sceneManager.CreateSceneNode("ChunkRoot");
            sceneManager.RootNode.Children.Add(BaseNode);
        }

        public IntVector3 GetChunkPositionByBlockPosition(IntVector3 blockPosition)
        {
            return new IntVector3
            {
                X = (int) Math.Floor(blockPosition.X / (double) ChunkSize.X),
                Y = (int) Math.Floor(blockPosition.Y / (double) ChunkSize.Y),
                Z = (int) Math.Floor(blockPosition.Z / (double) ChunkSize.Z),
            };
        }

        public Chunk GetChunkByBlockPosition(IntVector3 blockPosition)
        {
            var chunkPosition = GetChunkPositionByBlockPosition(blockPosition);

            return GetChunk(chunkPosition);
        }

        public Chunk GetChunk(IntVector3 chunkPosition)
        {
            return GetPartition(chunkPosition) as Chunk;
        }

        /// <summary>
        /// チャンクのメッシュ更新を要求します。
        /// </summary>
        /// <remarks>
        /// 要求はキューで管理され、非同期に順次実行されます。
        /// </remarks>
        /// <param name="chunk">チャンク。</param>
        /// <param name="priority">優先度。</param>
        public void RequestBuildVertices(Chunk chunk, ChunkMeshUpdatePriority priority)
        {
            switch (priority)
            {
                case ChunkMeshUpdatePriority.Normal:
                    normalBuildVerticesRequests.Enqueue(chunk);
                    break;
                case ChunkMeshUpdatePriority.High:
                    highBuildVerticesRequests.Enqueue(chunk);
                    break;
            }
        }

        /// <summary>
        /// チャンクの内部状態構築タスクを要求します。
        /// </summary>
        /// <remarks>
        /// 要求はキューで管理され、非同期に順次実行されます。
        /// </remarks>
        /// <param name="chunk">チャンク。</param>
        /// <param name="type">タスク種別。</param>
        /// <param name="priority">優先度。</param>
        public void RequestChunkTask(Chunk chunk, ChunkTaskType type, ChunkTaskPriority priority)
        {
            var request = new ChunkTaskRequest
            {
                Chunk = chunk,
                Type = type,
                Priority = priority
            };

            switch (priority)
            {
                case ChunkTaskPriority.Normal:
                    normalChunkTaskRequests.Enqueue(request);
                    break;
                case ChunkTaskPriority.High:
                    highChunkTaskRequests.Enqueue(request);
                    break;
            }
        }

        public ChunkLightUpdater BorrowLightUpdater()
        {
            return lightUpdaterPool.Borrow();
        }

        public void ReturnLightUpdater(ChunkLightUpdater lightUpdater)
        {
            lightUpdaterPool.Return(lightUpdater);
        }

        /// <summary>
        /// このクラスの実装では、以下の処理を行います。
        /// 
        /// ・指定の位置を含むリージョンがある場合、アクティブ化可能であると判定。
        /// 
        /// </summary>
        protected override bool CanActivate(IntVector3 position)
        {
            if (regionManager.GetRegionByChunkPosition(position) == null) return false;

            return base.CanActivate(position);
        }

        /// <summary>
        /// 指定の位置にあるチャンクのインスタンスを生成します。
        /// </summary>
        protected override Partition Create(IntVector3 position)
        {
            // 対象リージョンの取得。
            var region = regionManager.GetRegionByChunkPosition(position);
            if (region == null)
                throw new InvalidOperationException(string.Format("No region exists for a chunk '{0}'.", position));

            return new Chunk(this, region, position);
        }

        /// <summary>
        /// このクラスの実装では、以下の処理を行います。
        /// 
        /// ・新たなメッシュ更新の開始。
        /// ・チャンク タスクの実行。
        /// ・メッシュ更新完了の監視。
        /// 
        /// </summary>
        protected override void UpdateOverride()
        {
            // 更新が必要なチャンクを探索して更新要求を追加。
            // ただし、クローズが開始したら行わない。
            if (!Closing)
            {
                ProcessBuildVerticesRequests();

                ProcessChunkTaskRequests();
            }

            // 更新完了を監視。
            // 更新中はチャンクの更新ロックを取得したままであるため、
            // クローズ中も完了を監視して更新ロックの解放を試みなければならない。
            UpdateMeshBuffers();

            base.UpdateOverride();
        }

        /// <summary>
        /// このクラスの実装では、以下の処理を行います。
        ///
        /// ・チャンクのメッシュ更新を要求。
        /// ・チャンクのノードをノード グラフへ追加。
        /// 
        /// </summary>
        /// <param name="partition"></param>
        protected override void OnActivated(Partition partition)
        {
            var chunk = partition as Chunk;

            // 光レベル構築を要求。
            RequestSubsequentTask(chunk, ChunkTaskPriority.Normal);

            // ノードを追加。
            BaseNode.Children.Add(chunk.Node);

            base.OnActivated(partition);
        }

        internal SceneNode CreateNode()
        {
            nodeIdSequence++;
            return new SceneNode(SceneManager, "Chunk" + nodeIdSequence);
        }

        /// <summary>
        /// チャンク メッシュを破棄します。
        /// </summary>
        /// <param name="chunkMesh">チャンク メッシュ。</param>
        internal void DisposeMesh(ChunkMesh chunkMesh)
        {
            TotalVertexCount -= chunkMesh.VertexCount;
            TotalIndexCount -= chunkMesh.IndexCount;

            chunkMesh.Dispose();

            MeshCount--;
        }

        /// <summary>
        /// メッシュ更新が必要なチャンクを探索し、その更新要求を追加します。
        /// </summary>
        void ProcessBuildVerticesRequests()
        {
            Monitor.Begin(MonitorProcessProcessBuildVerticesRequests);

            ProcessBuildVerticesRequests(highBuildVerticesRequests);
            ProcessBuildVerticesRequests(normalBuildVerticesRequests);

            Monitor.End(MonitorProcessProcessBuildVerticesRequests);
        }

        void ProcessBuildVerticesRequests(ConcurrentQueue<Chunk> requestQueue)
        {
            for (int i = 0; i < meshUpdateSearchCapacity; i++)
            {
                Chunk chunk;
                if (!requestQueue.TryDequeue(out chunk)) break;

                if (!chunk.Active)
                {
                    // 非アクティブならば無視。
                    continue;
                }

                // TODO
                // 同一チャンクでもメッシュ更新は独立しているため、
                // 更新中であっても更新処理を実行しても良いのでは？

                // チャンクがメッシュ更新中ならば待機キューへ戻す。
                if (activeVerticesBuilder.ContainsKey(chunk))
                {
                    requestQueue.Enqueue(chunk);
                    continue;
                }

                var verticesBuilder = verticesBuilderPool.Borrow();
                if (verticesBuilder == null)
                {
                    // プール枯渇の場合は待機キューへ戻す。
                    // かつ、以降全ての待機を処理しない。
                    requestQueue.Enqueue(chunk);
                    break;
                }

                if (!verticesBuilder.Initialize(chunk))
                {
                    // 構築可能な状態ではないならば待機キューへ戻す。
                    requestQueue.Enqueue(chunk);
                    verticesBuilderPool.Return(verticesBuilder);
                    continue;
                }

                // 実行中としてマーク。
                activeVerticesBuilder[chunk] = verticesBuilder;

                // タスク実行。
                Task.Factory.StartNew(verticesBuilder.ExecuteAction);
            }
        }

        internal void RequestUpdateMeshBuffers(ChunkVerticesBuilder verticesBuilder)
        {
            for (int z = 0; z < MeshSegments.Z; z++)
            {
                for (int y = 0; y < MeshSegments.Y; y++)
                {
                    for (int x = 0; x < MeshSegments.X; x++)
                    {
                        var opaqueRequest = new ChunkMeshBufferRequest
                        {
                            VerticesBuilder = verticesBuilder,
                            Segment = new IntVector3(x, y, z),
                            Translucece = false
                        };
                        chunkMeshBufferRequests.Enqueue(opaqueRequest);

                        var translucentRequest = new ChunkMeshBufferRequest
                        {
                            VerticesBuilder = verticesBuilder,
                            Segment = new IntVector3(x, y, z),
                            Translucece = true
                        };
                        chunkMeshBufferRequests.Enqueue(translucentRequest);
                    }
                }
            }

            activeVerticesBuilder.Remove(verticesBuilder.Chunk);
        }

        /// <summary>
        /// チャンク メッシュ更新の完了を監視し、
        /// 完了しているならば頂点バッファへの反映を試みます。
        /// </summary>
        void UpdateMeshBuffers()
        {
            Monitor.Begin(MonitorUpdateMeshBuffers);

            // TODO
            // ここが負荷の原因。

            // TODO
            // 試しに更新数を制限してみたが、どうやらこれは問題ではない模様。
            // 更新間隔を置くと多少はマシになるが、それでも瞬間的な負荷は発生する。
            // 原因は、複数スレッドによるロックの奪い合いか、
            // 巨大な頂点バッファの連続作成か？
            //
            // バッファ更新頻度を下げても負荷があるが、
            // この負荷はチャンクのアクティブ化と合わさった場合に負荷となるようにみえる。
            // となると、スレッド間の関係に問題があるのかもしれない。

            if (0 < currentUpdateMeshBufferFrame)
            {
                currentUpdateMeshBufferFrame--;
                Monitor.End(MonitorUpdateMeshBuffers);
                return;
            }

            int updateCount = 0;

            ChunkMeshBufferRequest request;
            while (chunkMeshBufferRequests.TryDequeue(out request))
            {
                var verticesBuilder = request.VerticesBuilder;

                // チャンクの非アクティブ化は、このメソッドと同じスレッドで処理される。
                // このため、Active プロパティが示す値は、このメソッドで保証される。
                if (!verticesBuilder.Chunk.Active)
                    continue;

                var chunk = verticesBuilder.Chunk;
                var segment = request.Segment;
                var translucence = request.Translucece;

                // クローズ中は頂点バッファ反映をスキップ。
                if (!Closing)
                {
                    if (UpdateMeshSegmentBuffer(verticesBuilder, segment.X, segment.Y, segment.Z, translucence))
                    {
                        // バッファに変更があったならばチャンクのノードを更新。
                        chunk.Node.Update(false);

                        updateCount++;
                    }
                }

                // バッファを反映済みとしてマーク。
                var vertices = verticesBuilder.GetVertices(segment.X, segment.Y, segment.Z, translucence);
                vertices.Consumed = true;

                // 全てのバッファへ反映したならば頂点ビルダを解放。
                if (verticesBuilder.ConsumedAll())
                {
                    verticesBuilder.Clear();
                    verticesBuilderPool.Return(verticesBuilder);
                }

                if (updateMeshBufferPerFrame <= updateCount)
                    break;
            }

            if (0 < updateCount)
            {
                currentUpdateMeshBufferFrame = updateMeshBufferFrameDelay;
            }

            Monitor.End(MonitorUpdateMeshBuffers);
        }

        bool UpdateMeshSegmentBuffer(ChunkVerticesBuilder verticesBuilder, int segmentX, int segmentY, int segmentZ, bool translucence)
        {
            var chunk = verticesBuilder.Chunk;

            // メッシュに設定するワールド座標。
            // メッシュの中心をメッシュの位置とする。
            var position = new Vector3
            {
                X = chunk.Position.X * PartitionSize.X + segmentX * MeshSize.X + MeshOffset.X,
                Y = chunk.Position.Y * PartitionSize.Y + segmentY * MeshSize.Y + MeshOffset.Y,
                Z = chunk.Position.Z * PartitionSize.Z + segmentZ * MeshSize.Z + MeshOffset.Z
            };

            // メッシュに設定するワールド行列。
            Matrix world;
            Matrix.CreateTranslation(ref position, out world);

            // メッシュに設定するエフェクト。
            var effect = chunk.Region.ChunkEffect;

            var vertices = verticesBuilder.GetVertices(segmentX, segmentY, segmentZ, translucence);

            var mesh = chunk.GetMesh(segmentX, segmentY, segmentZ, translucence);
            if (mesh != null)
            {
                TotalVertexCount -= mesh.VertexCount;
                TotalIndexCount -= mesh.IndexCount;
            }

            bool updated;
            if (vertices.VertexCount == 0 || vertices.IndexCount == 0)
            {
                if (mesh != null)
                {
                    chunk.SetMesh(segmentX, segmentY, segmentZ, translucence, null);
                    updated = true;
                }
                else
                {
                    // 更新不要。
                    updated = false;
                }
            }
            else
            {
                if (mesh == null)
                {
                    mesh = CreateMesh(effect, false, segmentX, segmentY, segmentZ);
                    chunk.SetMesh(segmentX, segmentY, segmentZ, translucence, mesh);
                }

                mesh.PositionWorld = position;
                mesh.World = world;
                vertices.Populate(mesh);

                TotalVertexCount += mesh.VertexCount;
                TotalIndexCount += mesh.IndexCount;
                MaxVertexCount = Math.Max(MaxVertexCount, mesh.VertexCount);
                MaxIndexCount = Math.Max(MaxIndexCount, mesh.IndexCount);

                updated = true;
            }

            return updated;
        }

        /// <summary>
        /// チャンク メッシュを生成します。
        /// </summary>
        /// <param name="effect">チャンクのエフェクト。</param>
        /// <param name="translucent">
        /// true (半透明の場合)、false (それ以外の場合)。
        /// </param>
        /// <returns>チャンク メッシュ。</returns>
        ChunkMesh CreateMesh(ChunkEffect effect, bool translucent, int segmentX, int segmentY, int segmentZ)
        {
            meshNameBuilder.Length = 0;
            meshNameBuilder.Append((translucent) ? "TranslucentMesh" : "OpaqueMesh");
            meshNameBuilder.Append('_').AppendNumber(segmentX);
            meshNameBuilder.Append('_').AppendNumber(segmentY);
            meshNameBuilder.Append('_').AppendNumber(segmentZ);

            var name = meshNameBuilder.ToString();

            var mesh = new ChunkMesh(name, effect);
            mesh.Translucent = translucent;

            MeshCount++;

            return mesh;
        }

        void ProcessChunkTaskRequests()
        {
            // TODO
            // どうやら、ここも瞬間的に重くなるように見える。

            Monitor.Begin(MonitorProcessChunkTaskRequests);

            // TODO
            int capacity = 10;
            ChunkTaskRequest request;

            while (0 < capacity && highChunkTaskRequests.TryDequeue(out request))
            {
                ProcessChunkTask(ref request);

                capacity--;
            }
            
            while (0 < capacity && normalChunkTaskRequests.TryDequeue(out request))
            {
                ProcessChunkTask(ref request);

                capacity--;
            }

            Monitor.End(MonitorProcessChunkTaskRequests);
        }

        void ProcessChunkTask(ref ChunkTaskRequest request)
        {
            var chunk = request.Chunk;
            if (!chunk.Active)
                return;

            var task = chunkTaskPool.Borrow();

            switch (request.Type)
            {
                case ChunkTaskType.BuildLocalLights:
                    task.Initialize(chunk, ChunkLightBuilder.BuildLocalLightsAction, request.Priority);
                    break;
                case ChunkTaskType.PropagateLights:
                    task.Initialize(chunk, ChunkLightBuilder.PropagateLightsAction, request.Priority);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            Task.Factory.StartNew(task.ExecuteAction);
        }

        void RequestSubsequentTask(Chunk chunk, ChunkTaskPriority priority)
        {
            switch (chunk.LightState)
            {
                case ChunkLightState.WaitBuildLocal:
                    RequestChunkTask(chunk, ChunkTaskType.BuildLocalLights, priority);
                    break;
                case ChunkLightState.WaitPropagate:
                    RequestChunkTask(chunk, ChunkTaskType.PropagateLights, priority);
                    break;
                case ChunkLightState.Complete:
                    if (0 < chunk.SolidCount)
                        RequestBuildVertices(chunk, ChunkMeshUpdatePriority.Normal);
                    break;
            }
        }

        void OnChunkTaskFinished(ChunkTask task)
        {
            RequestSubsequentTask(task.Chunk, task.Priority);
            task.Clear();
            chunkTaskPool.Return(task);
        }
    }
}
