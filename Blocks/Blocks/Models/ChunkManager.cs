﻿#region Using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Collections;
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
            public VectorI3 Position;

            public ChunkTaskTypes Type;

            public ChunkTaskPriorities Priority;
        }

        #endregion

        /// <summary>
        /// メッシュ サイズ。
        /// </summary>
        public static readonly VectorI3 MeshSize = new VectorI3(16);

        /// <summary>
        /// チャンク サイズ。
        /// </summary>
        /// <remarks>
        /// チャンク サイズは、メッシュ サイズの等倍でなければなりません。
        /// </remarks>
        public readonly VectorI3 ChunkSize;

        /// <summary>
        /// セグメント サイズ。
        /// </summary>
        /// <remarks>
        /// 指定されたセグメント サイズに従い、
        /// チャンクのメッシュは複数のメッシュへ分割されます。
        /// </remarks>
        public readonly VectorI3 MeshSegments;

        /// <summary>
        /// 半チャンク サイズ。
        /// </summary>
        public readonly VectorI3 HalfChunkSize;

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

        /// <summary>
        /// 通常優先度のメッシュ更新要求のキュー。
        /// </summary>
        ConcurrentQueue<VectorI3> normalUpdateMeshRequests = new ConcurrentQueue<VectorI3>();

        /// <summary>
        /// 高優先度のメッシュ更新要求のキュー。
        /// </summary>
        ConcurrentQueue<VectorI3> highUpdateMeshRequests = new ConcurrentQueue<VectorI3>();

        /// <summary>
        /// 頂点構築中チャンクのキュー。
        /// </summary>
        Queue<Chunk> updateMeshChunkQueue;

        /// <summary>
        /// 頂点ビルダのプール。
        /// </summary>
        Pool<ChunkVerticesBuilder> verticesBuilderPool;

        /// <summary>
        /// 頂点ビルダの処理を非同期に実行するためのタスク キュー。
        /// </summary>
        TaskQueue verticesBuilderTaskQueue;

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
        /// チャンク タスクのキュー。
        /// </summary>
        TaskQueue chunkTaskQueue;

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
        public ChunkManager(ChunkSettings settings, GraphicsDevice graphicsDevice, RegionManager regionManager, SceneManager sceneManager)
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

            MeshSegments = new VectorI3
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

            updateMeshChunkQueue = new Queue<Chunk>(verticesBuilderCount);
            verticesBuilderPool = new Pool<ChunkVerticesBuilder>(() => { return new ChunkVerticesBuilder(this); })
            {
                MaxCapacity = verticesBuilderCount
            };
            verticesBuilderTaskQueue = new TaskQueue
            {
                SlotCount = verticesBuilderCount
            };

            chunkTaskQueue = new TaskQueue
            {
                // TODO
                SlotCount = 4
            };

            BaseNode = sceneManager.CreateSceneNode("ChunkRoot");
            sceneManager.RootNode.Children.Add(BaseNode);
        }

        public void GetChunkPositionByBlockPosition(ref VectorI3 blockPosition, out VectorI3 result)
        {
            result = new VectorI3
            {
                X = (int) Math.Floor(blockPosition.X / (double) ChunkSize.X),
                Y = (int) Math.Floor(blockPosition.Y / (double) ChunkSize.Y),
                Z = (int) Math.Floor(blockPosition.Z / (double) ChunkSize.Z),
            };
        }

        public Chunk GetChunkByBlockPosition(ref VectorI3 blockPosition)
        {
            VectorI3 chunkPosition;
            GetChunkPositionByBlockPosition(ref blockPosition, out chunkPosition);

            return GetChunk(ref chunkPosition);
        }

        public Chunk GetChunk(ref VectorI3 chunkPosition)
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
        public void RequestUpdateMesh(ref VectorI3 position, ChunkMeshUpdatePriorities priority)
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
        public void RequestChunkTask(ref VectorI3 position, ChunkTaskTypes type, ChunkTaskPriorities priority)
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

        /// <summary>
        /// このクラスの実装では、以下の処理を行います。
        /// 
        /// ・指定の位置を含むリージョンがある場合、アクティブ化可能であると判定。
        /// 
        /// </summary>
        protected override bool CanActivate(ref VectorI3 position)
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
        protected override Partition Create(ref VectorI3 position)
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
        /// <param name="gameTime">ゲーム時間。</param>
        protected override void UpdateOverride(GameTime gameTime)
        {
            // 更新が必要なチャンクを探索して更新要求を追加。
            // ただし、クローズが開始したら行わない。
            if (!Closing)
            {
                ProcessUpdateMeshRequests(gameTime);

                ProcessChunkTaskRequests();
            }

            // ビルダのタスク キューを更新。
            verticesBuilderTaskQueue.Update();

            // 更新完了を監視。
            // 更新中はチャンクの更新ロックを取得したままであるため、
            // クローズ中も完了を監視して更新ロックの解放を試みなければならない。
            UpdateMeshes(gameTime);

            base.UpdateOverride(gameTime);
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
            if (0 < chunk.SolidCount)
            {
                // 光レベル構築を待たずにメッシュ更新を要求。
                RequestUpdateMesh(ref chunk.Position, ChunkMeshUpdatePriorities.Normal);
            }

            // 光レベル構築を要求。
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
        /// <param name="gameTime">ゲーム時間。</param>
        void ProcessUpdateMeshRequests(GameTime gameTime)
        {
            ProcessUpdateMeshRequests(gameTime, highUpdateMeshRequests);
            ProcessUpdateMeshRequests(gameTime, normalUpdateMeshRequests);
        }

        void ProcessUpdateMeshRequests(GameTime gameTime, ConcurrentQueue<VectorI3> requestQueue)
        {
            for (int i = 0; i < meshUpdateSearchCapacity; i++)
            {
                VectorI3 position;
                if (!requestQueue.TryDequeue(out position)) break;

                // アクティブ チャンクを取得。
                var chunk = GetChunk(ref position);
                if (chunk == null)
                {
                    // 存在しない場合は要求を無視。
                    continue;
                }

                // チャンクがメッシュ更新中ならば待機キューへ戻す。
                if (updateMeshChunkQueue.Contains(chunk))
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

                // 頂点ビルダを登録。
                verticesBuilderTaskQueue.Enqueue(verticesBuilder.ExecuteAction);

                updateMeshChunkQueue.Enqueue(chunk);
            }
        }

        /// <summary>
        /// チャンク メッシュ更新の完了を監視し、
        /// 完了しているならば頂点バッファへの反映を試みます。
        /// </summary>
        /// <param name="gameTime">ゲーム時間。</param>
        void UpdateMeshes(GameTime gameTime)
        {
            // 頂点ビルダの監視。
            int count = updateMeshChunkQueue.Count;
            for (int i = 0; i < count; i++)
            {
                var chunk = updateMeshChunkQueue.Dequeue();

                if (!chunk.VerticesBuilder.Completed)
                {
                    // 未完ならば更新キューへ戻す。
                    updateMeshChunkQueue.Enqueue(chunk);
                    continue;
                }

                // クローズ中は頂点バッファ反映をスキップ。
                if (!Closing) UpdateMesh(chunk);

                // 頂点ビルダを解放。
                ReleaseVerticesBuilder(chunk.VerticesBuilder);

                // 更新開始で取得したロックを解放。
                chunk.ExitLock();

                // 非アクティブ化の抑制を解除。
                chunk.SuppressPassivation = false;
            }
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
            int capacity = 100;
            ChunkTaskRequest request;

            while (0 < capacity && highTaskRequestQueue.TryDequeue(out request))
            {
                var chunk = GetChunk(ref request.Position);
                if (chunk == null) return;

                // TODO
                // 非アクティブ化の抑制。
                var task = chunk.GetTask(request.Type, request.Priority);
                chunkTaskQueue.Enqueue(task);

                capacity--;
            }
            
            while (0 < capacity && normalTaskRequestQueue.TryDequeue(out request))
            {
                var chunk = GetChunk(ref request.Position);
                if (chunk == null) return;

                // TODO
                // 非アクティブ化の抑制。
                var task = chunk.GetTask(request.Type, request.Priority);
                chunkTaskQueue.Enqueue(task);

                capacity--;
            }

            chunkTaskQueue.Update();
        }
    }
}
