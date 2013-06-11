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
            public IntVector3 Position;

            public ChunkTaskTypes Type;

            public ChunkTaskPriorities Priority;
        }

        #endregion

        #region ChunkTaskCallback

        delegate void ChunkTaskCallback(ChunkTask task);

        #endregion

        #region ChunkTask

        abstract class ChunkTask
        {
            public ChunkTaskPriorities Priority { get; private set; }

            public ChunkManager Manager { get; private set; }

            public Chunk Chunk { get; private set; }

            public ChunkTaskCallback Callback { get; private set; }

            public Action Action { get; private set; }

            protected ChunkTask(ChunkManager manager)
            {
                if (manager == null) throw new ArgumentNullException("manager");

                Manager = manager;
                Action = new Action(Execute);
            }

            public virtual void Initialize(Chunk chunk, ChunkTaskPriorities priority, ChunkTaskCallback callback)
            {
                if (chunk == null) throw new ArgumentNullException("chunk");

                Chunk = chunk;
                Priority = priority;
                Callback = callback;
            }

            public virtual void Clear()
            {
                Chunk = null;
                Callback = null;
            }

            protected virtual void Execute()
            {
                if (Callback != null) Callback(this);
            }
        }

        #endregion

        #region ChunkTaskBuildLocalLights

        sealed class ChunkTaskBuildLocalLights : ChunkTask
        {
            public ChunkTaskBuildLocalLights(ChunkManager manager)
                : base(manager)
            {
            }

            protected override void Execute()
            {
                ChunkLightBuilder.BuildLocalLights(Chunk);

                base.Execute();
            }
        }

        #endregion

        #region ChunkTaskPropagateLights

        sealed class ChunkTaskPropagateLights : ChunkTask
        {
            public ChunkTaskPropagateLights(ChunkManager manager)
                : base(manager)
            {
            }

            protected override void Execute()
            {
                ChunkLightBuilder.PropagateLights(Chunk);

                base.Execute();
            }
        }

        #endregion

        public const string MonitorProcessUpdateMeshRequests = "ChunkManager.ProcessUpdateMeshRequests";

        public const string MonitorProcessChunkTaskRequests = "ChunkManager.ProcessChunkTaskRequests";

        public const string MonitorUpdateMeshes = "ChunkManager.UpdateMeshes";

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

        /// <summary>
        /// チャンクのプール。
        /// </summary>
        ConcurrentPool<Chunk> chunkPool;

        /// <summary>
        /// データのプール。
        /// </summary>
        ConcurrentPool<ChunkData> dataPool;

        ConcurrentPool<ChunkTaskBuildLocalLights> buildLocalLightsPool;

        ConcurrentPool<ChunkTaskPropagateLights> propagateLightsPool;

        ChunkTaskCallback chunkTaskCallback;

        ConcurrentPool<ChunkLightUpdater> lightUpdaterPool;

        /// <summary>
        /// 通常優先度のメッシュ更新要求のキュー。
        /// </summary>
        ConcurrentQueue<IntVector3> normalUpdateMeshRequests = new ConcurrentQueue<IntVector3>();

        /// <summary>
        /// 高優先度のメッシュ更新要求のキュー。
        /// </summary>
        ConcurrentQueue<IntVector3> highUpdateMeshRequests = new ConcurrentQueue<IntVector3>();

        ConcurrentDictionary<IntVector3, Chunk> updatingChunks;

        ConcurrentQueue<Chunk> finishedUpdateMeshTasks = new ConcurrentQueue<Chunk>();

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
        ConcurrentQueue<ChunkTaskRequest> normalTaskRequestQueue = new ConcurrentQueue<ChunkTaskRequest>();

        /// <summary>
        /// 高優先度のチャンク タスク要求のキュー。
        /// </summary>
        ConcurrentQueue<ChunkTaskRequest> highTaskRequestQueue = new ConcurrentQueue<ChunkTaskRequest>();

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
            SceneManager sceneManager,
            IChunkStore chunkStore)
            : base(settings.PartitionManager)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");
            if (regionManager == null) throw new ArgumentNullException("regionManager");

            ChunkSize = settings.ChunkSize;
            this.graphicsDevice = graphicsDevice;
            this.regionManager = regionManager;
            SceneManager = sceneManager;
            ChunkStore = chunkStore ?? NullChunkStore.Instance;

            meshUpdateSearchCapacity = settings.MeshUpdateSearchCapacity;
            verticesBuilderCount = settings.VerticesBuilderCount;

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

            chunkPool = new ConcurrentPool<Chunk>(() => { return new Chunk(this); });
            chunkPool.MaxCapacity = settings.ChunkPoolMaxCapacity;
            dataPool = new ConcurrentPool<ChunkData>(() => { return new ChunkData(this); });
            EmptyData = new ChunkData(this);

            updatingChunks = new ConcurrentDictionary<IntVector3, Chunk>(verticesBuilderCount, verticesBuilderCount);

            verticesBuilderPool = new Pool<ChunkVerticesBuilder>(() => { return new ChunkVerticesBuilder(this); })
            {
                MaxCapacity = verticesBuilderCount
            };

            buildLocalLightsPool = new ConcurrentPool<ChunkTaskBuildLocalLights>(() => { return new ChunkTaskBuildLocalLights(this); });
            propagateLightsPool = new ConcurrentPool<ChunkTaskPropagateLights>(() => { return new ChunkTaskPropagateLights(this); });
            chunkTaskCallback = OnChunkTaskCompleted;

            lightUpdaterPool = new ConcurrentPool<ChunkLightUpdater>(() => { return new ChunkLightUpdater(this); });

            BaseNode = sceneManager.CreateSceneNode("ChunkRoot");
            sceneManager.RootNode.Children.Add(BaseNode);
        }

        public void GetChunkPositionByBlockPosition(ref IntVector3 blockPosition, out IntVector3 result)
        {
            result = new IntVector3
            {
                X = (int) Math.Floor(blockPosition.X / (double) ChunkSize.X),
                Y = (int) Math.Floor(blockPosition.Y / (double) ChunkSize.Y),
                Z = (int) Math.Floor(blockPosition.Z / (double) ChunkSize.Z),
            };
        }

        public Chunk GetChunkByBlockPosition(ref IntVector3 blockPosition)
        {
            IntVector3 chunkPosition;
            GetChunkPositionByBlockPosition(ref blockPosition, out chunkPosition);

            return GetChunk(ref chunkPosition);
        }

        public Chunk GetChunk(ref IntVector3 chunkPosition)
        {
            return GetPartition(ref chunkPosition) as Chunk;
        }

        /// <summary>
        /// チャンクのメッシュ更新を要求します。
        /// </summary>
        /// <remarks>
        /// 要求はキューで管理され、非同期に順次実行されます。
        /// </remarks>
        /// <param name="position">チャンクの位置。</param>
        /// <param name="priority">優先度。</param>
        public void RequestUpdateMesh(ref IntVector3 position, ChunkMeshUpdatePriorities priority)
        {
            switch (priority)
            {
                case ChunkMeshUpdatePriorities.Normal:
                    normalUpdateMeshRequests.Enqueue(position);
                    break;
                case ChunkMeshUpdatePriorities.High:
                    highUpdateMeshRequests.Enqueue(position);
                    break;
            }
        }

        /// <summary>
        /// チャンクの内部状態構築タスクを要求します。
        /// </summary>
        /// <remarks>
        /// 要求はキューで管理され、非同期に順次実行されます。
        /// </remarks>
        /// <param name="position">チャンクの位置。</param>
        /// <param name="type">タスク種別。</param>
        /// <param name="priority">優先度。</param>
        public void RequestChunkTask(ref IntVector3 position, ChunkTaskTypes type, ChunkTaskPriorities priority)
        {
            var request = new ChunkTaskRequest
            {
                Position = position,
                Type = type,
                Priority = priority
            };

            switch (priority)
            {
                case ChunkTaskPriorities.Normal:
                    normalTaskRequestQueue.Enqueue(request);
                    break;
                case ChunkTaskPriorities.High:
                    highTaskRequestQueue.Enqueue(request);
                    break;
            }
        }

        public void RequestChunkTask(ref IntBoundingBox bounds, ChunkTaskTypes type, ChunkTaskPriorities priority)
        {
            for (int z = bounds.Min.Z; z < (bounds.Min.Z + bounds.Size.Z); z++)
            {
                for (int y = bounds.Min.Y; y < (bounds.Min.Y + bounds.Size.Y); y++)
                {
                    for (int x = bounds.Min.X; x < (bounds.Min.X + bounds.Size.X); x++)
                    {
                        var position = new IntVector3(x, y, z);
                        RequestChunkTask(ref position, type, priority);
                    }
                }
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
        protected override bool CanActivate(ref IntVector3 position)
        {
            if (regionManager.GetRegionByChunkPosition(ref position) == null) return false;

            return base.CanActivate(ref position);
        }

        /// <summary>
        /// このクラスの実装では、以下の処理を行います。
        /// 
        /// ・プールからチャンクを取得して返却。ただし、プールが枯渇している場合は null。
        /// 
        /// </summary>
        protected override Partition Create(ref IntVector3 position)
        {
            // プールから取得。
            var chunk = chunkPool.Borrow();
            if (chunk == null) return null;

            // 対象リージョンの取得。
            var region = regionManager.GetRegionByChunkPosition(ref position);
            if (region == null) throw new InvalidOperationException("Region not found: ChunkPosition = " + position);

            // 初期化。
            chunk.Initialize(position, region);

            return chunk;
        }

        /// <summary>
        /// このクラスの実装では、以下の処理を行います。
        /// 
        /// ・チャンクの解放処理の呼び出し。
        /// ・プールへチャンクを返却。
        /// 
        /// </summary>
        protected override void Release(Partition partition)
        {
            var chunk = partition as Chunk;

            // 解放。
            chunk.Release();

            // プールへ戻す。
            chunkPool.Return(chunk);
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
                ProcessUpdateMeshRequests();

                ProcessChunkTaskRequests();
            }

            // 更新完了を監視。
            // 更新中はチャンクの更新ロックを取得したままであるため、
            // クローズ中も完了を監視して更新ロックの解放を試みなければならない。
            UpdateMeshes();

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
            if (chunk.LightState == ChunkLightState.WaitBuildLocal)
                RequestChunkTask(ref chunk.Position, ChunkTaskTypes.BuildLocalLights, ChunkTaskPriorities.Normal);

            // ノードを追加。
            BaseNode.Children.Add(chunk.Node);

            base.OnActivated(partition);
        }

        /// <summary>
        /// このクラスの実装では、以下の処理を行います。
        /// 
        /// ・ノード グラフからチャンクのノードを削除。
        /// 
        /// </summary>
        /// <param name="partition"></param>
        protected override void OnPassivating(Partition partition)
        {
            var chunk = partition as Chunk;
            
            // ノードを削除。
            BaseNode.Children.Remove(chunk.Node);

            base.OnPassivating(partition);
        }

        /// <summary>
        /// このクラスの実装では、以下の処理を行います。
        /// 
        /// ・プールにある全てのチャンクの削除。
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected override void DisposeOverride(bool disposing)
        {
            // TODO
            // プール内チャンクの破棄。

            chunkPool.Clear();

            base.DisposeOverride(disposing);
        }

        internal ChunkData BorrowData()
        {
            return dataPool.Borrow();
        }

        internal void ReturnData(ChunkData data)
        {
            data.Clear();
            dataPool.Return(data);
        }

        internal SceneNode CreateNode()
        {
            nodeIdSequence++;
            return new SceneNode(SceneManager, "Chunk" + nodeIdSequence);
        }

        /// <summary>
        /// 頂点ビルダをプールへ戻します。
        /// </summary>
        /// <param name="builder"></param>
        internal void ReleaseVerticesBuilder(ChunkVerticesBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException("builder");

            builder.Clear();
            verticesBuilderPool.Return(builder);
        }

        /// <summary>
        /// チャンク メッシュを破棄します。
        /// ここでは破棄要求をキューに入れるのみであり、
        /// Dispose メソッド呼び出しは Update メソッド内で処理されます。
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
        void ProcessUpdateMeshRequests()
        {
            Monitor.Begin(MonitorProcessUpdateMeshRequests);

            ProcessUpdateMeshRequests(highUpdateMeshRequests);
            ProcessUpdateMeshRequests(normalUpdateMeshRequests);

            Monitor.End(MonitorProcessUpdateMeshRequests);
        }

        void ProcessUpdateMeshRequests(ConcurrentQueue<IntVector3> requestQueue)
        {
            for (int i = 0; i < meshUpdateSearchCapacity; i++)
            {
                IntVector3 position;
                if (!requestQueue.TryDequeue(out position)) break;

                // アクティブ チャンクを取得。
                var chunk = GetChunk(ref position);
                if (chunk == null)
                {
                    // 存在しない場合は要求を無視。
                    continue;
                }

                // チャンクがメッシュ更新中ならば待機キューへ戻す。
                if (updatingChunks.ContainsKey(chunk.Position))
                {
                    requestQueue.Enqueue(position);
                    continue;
                }

                var verticesBuilder = verticesBuilderPool.Borrow();
                if (verticesBuilder == null)
                {
                    // プール枯渇の場合は待機キューへ戻す。
                    // かつ、以降全ての待機を処理しない。
                    requestQueue.Enqueue(position);
                    break;
                }

                if (!verticesBuilder.Initialize(chunk))
                {
                    // 構築可能な状態ではないならば待機キューへ戻す。
                    requestQueue.Enqueue(position);
                    verticesBuilderPool.Return(verticesBuilder);
                    continue;
                }

                // メッシュ更新が終わるまで非アクティブ化を抑制。
                // パーティション マネージャは非アクティブ化前にロック取得を試みるが、
                // チャンク マネージャはそのサブクラスでロックを取得しているため、
                // ロックだけでは非アクティブ化を抑制できない事に注意。
                chunk.SuppressPassivation = true;

                // チャンクのロックを試行。
                // 頂点ビルダの更新が完了するまでロックを維持。
                if (!chunk.EnterLock())
                {
                    // ロックを取得できない場合は待機キューへ戻す。
                    requestQueue.Enqueue(position);
                    verticesBuilderPool.Return(verticesBuilder);
                    continue;
                }

                // メッシュ更新中としてマーク。
                updatingChunks[chunk.Position] = chunk;

                // タスク実行。
                Task.Factory.StartNew(verticesBuilder.ExecuteAction);
            }
        }

        internal void OnUpdateMeshFinished(Chunk chunk)
        {
            finishedUpdateMeshTasks.Enqueue(chunk);
        }

        /// <summary>
        /// チャンク メッシュ更新の完了を監視し、
        /// 完了しているならば頂点バッファへの反映を試みます。
        /// </summary>
        void UpdateMeshes()
        {
            Monitor.Begin(MonitorUpdateMeshes);

            // TODO
            // ここが負荷の原因。

            // TODO
            // 試しに更新数を制限してみたが、どうやらこれは問題ではない模様。
            // 更新間隔を置くと多少はマシになるが、それでも瞬間的な負荷は発生する。
            // 原因は、複数スレッドによるロックの奪い合いか、
            // 巨大な頂点バッファの連続作成か？

            // 頂点ビルダの監視。
            while (!finishedUpdateMeshTasks.IsEmpty)
            {
                Chunk chunk;
                if (!finishedUpdateMeshTasks.TryDequeue(out chunk))
                    break;

                // 更新中マークを解除。
                Chunk removedChunk;
                updatingChunks.TryRemove(chunk.Position, out removedChunk);

                // クローズ中は頂点バッファ反映をスキップ。
                if (!Closing) UpdateMesh(chunk);

                // 頂点ビルダを解放。
                ReleaseVerticesBuilder(chunk.VerticesBuilder);

                // 更新開始で取得したロックを解放。
                chunk.ExitLock();

                // 非アクティブ化の抑制を解除。
                chunk.SuppressPassivation = false;
            }

            Monitor.End(MonitorUpdateMeshes);
        }

        /// <summary>
        /// チャンクに関連付けられた頂点ビルダの結果で頂点バッファを更新します。
        /// </summary>
        /// <param name="chunk">チャンク。</param>
        void UpdateMesh(Chunk chunk)
        {
            for (int z = 0; z < MeshSegments.Z; z++)
            {
                for (int y = 0; y < MeshSegments.Y; y++)
                {
                    for (int x = 0; x < MeshSegments.X; x++)
                    {
                        UpdateMesh(chunk, x, y, z);
                    }
                }
            }

            // チャンクのノードを更新。
            chunk.Node.Update(false);
        }

        void UpdateMesh(Chunk chunk, int segmentX, int segmentY, int segmentZ)
        {
            var builder = chunk.VerticesBuilder;

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

            var opaque = builder.GetOpaque(segmentX, segmentY, segmentZ);
            var translucence = builder.GetTranslucence(segmentX, segmentY, segmentZ);

            //----------------------------------------------------------------
            // 不透明メッシュ

            if (opaque.VertexCount == 0 || opaque.IndexCount == 0)
            {
                if (chunk.GetOpaqueMesh(segmentX, segmentY, segmentZ) != null)
                    chunk.SetOpaqueMesh(segmentX, segmentY, segmentZ, null);
            }
            else
            {
                var mesh = chunk.GetOpaqueMesh(segmentX, segmentY, segmentZ);
                if (mesh == null)
                {
                    mesh = CreateMesh(effect, false, segmentX, segmentY, segmentZ);
                    chunk.SetOpaqueMesh(segmentX, segmentY, segmentZ, mesh);
                }
                else
                {
                    TotalVertexCount -= mesh.VertexCount;
                    TotalIndexCount -= mesh.IndexCount;
                }

                mesh.PositionWorld = position;
                mesh.World = world;
                opaque.Populate(mesh);

                TotalVertexCount += mesh.VertexCount;
                TotalIndexCount += mesh.IndexCount;
                MaxVertexCount = Math.Max(MaxVertexCount, mesh.VertexCount);
                MaxIndexCount = Math.Max(MaxIndexCount, mesh.IndexCount);
            }

            //----------------------------------------------------------------
            // 半透明メッシュ

            if (translucence.VertexCount == 0 || translucence.IndexCount == 0)
            {
                if (chunk.GetTranslucentMesh(segmentX, segmentY, segmentZ) != null)
                    chunk.SetTranslucentMesh(segmentX, segmentY, segmentZ, null);
            }
            else
            {
                var mesh = chunk.GetTranslucentMesh(segmentX, segmentY, segmentZ);
                if (mesh == null)
                {
                    mesh = CreateMesh(effect, true, segmentX, segmentY, segmentZ);
                    chunk.SetTranslucentMesh(segmentX, segmentY, segmentZ, mesh);
                }
                else
                {
                    TotalVertexCount -= mesh.VertexCount;
                    TotalIndexCount -= mesh.IndexCount;
                }

                mesh.PositionWorld = position;
                mesh.World = world;
                translucence.Populate(mesh);

                TotalVertexCount += mesh.VertexCount;
                TotalIndexCount += mesh.IndexCount;
                MaxVertexCount = Math.Max(MaxVertexCount, mesh.VertexCount);
                MaxIndexCount = Math.Max(MaxIndexCount, mesh.IndexCount);
            }
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
            int capacity = 4;
            ChunkTaskRequest request;

            while (0 < capacity && highTaskRequestQueue.TryDequeue(out request))
            {
                ProcessChunkTask(ref request);

                capacity--;
            }
            
            while (0 < capacity && normalTaskRequestQueue.TryDequeue(out request))
            {
                ProcessChunkTask(ref request);

                capacity--;
            }

            Monitor.End(MonitorProcessChunkTaskRequests);
        }

        void ProcessChunkTask(ref ChunkTaskRequest request)
        {
            var chunk = GetChunk(ref request.Position);
            if (chunk == null) return;

            ChunkTask chunkTask;
            switch (request.Type)
            {
                case ChunkTaskTypes.BuildLocalLights:
                    chunkTask = buildLocalLightsPool.Borrow();
                    break;
                case ChunkTaskTypes.PropagateLights:
                    chunkTask = propagateLightsPool.Borrow();
                    break;
                default:
                    throw new InvalidOperationException();
            }

            chunkTask.Initialize(chunk, request.Priority, chunkTaskCallback);

            // TODO
            // 非アクティブ化の抑制。
            Task.Factory.StartNew(chunkTask.Action);
        }

        void OnChunkTaskCompleted(ChunkTask task)
        {
            var chunk = task.Chunk;

            switch (chunk.LightState)
            {
                case ChunkLightState.WaitBuildLocal:
                    RequestChunkTask(ref chunk.Position, ChunkTaskTypes.BuildLocalLights, task.Priority);
                    break;
                case ChunkLightState.WaitPropagate:
                    RequestChunkTask(ref chunk.Position, ChunkTaskTypes.PropagateLights, task.Priority);
                    break;
                case ChunkLightState.Complete:
                    // TODO
                    if (0 < chunk.SolidCount)
                        RequestUpdateMesh(ref chunk.Position, ChunkMeshUpdatePriorities.Normal);
                    break;
            }

            task.Clear();
        }
    }
}
