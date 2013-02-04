﻿#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        #region UpdateMeshPriority

        /// <summary>
        /// メッシュ更新の優先度を示す列挙型です。
        /// </summary>
        public enum UpdateMeshPriority
        {
            /// <summary>
            /// 通常優先度。
            /// </summary>
            Normal,

            /// <summary>
            /// 高優先度。
            /// </summary>
            High
        }

        #endregion

        public static readonly VectorI3 MeshSize = new VectorI3(16);

        /// <summary>
        /// チャンク サイズ。
        /// </summary>
        public readonly VectorI3 ChunkSize;

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
        Queue<VectorI3> normalUpdateMeshRequests;

        /// <summary>
        /// 高優先度のメッシュ更新要求のキュー。
        /// </summary>
        Queue<VectorI3> highUpdateMeshRequests;

        /// <summary>
        /// 頂点ビルダ実行中チャンクのキュー。
        /// </summary>
        Queue<Chunk> buildVerticesQueue;

        /// <summary>
        /// 頂点ビルダのプール。
        /// </summary>
        Pool<ChunkVerticesBuilder> verticesBuilderPool;

        /// <summary>
        /// 頂点ビルダの処理を非同期に実行するためのタスク キュー。
        /// </summary>
        TaskQueue verticesBuilderTaskQueue;

        Queue<VectorI3> buildLocalLightRequests;

        Queue<Chunk> buildLocalLightQueue;

        Pool<ChunkLocalLightBuilder> localLightBuilderPool;

        TaskQueue localLightBuilderTaskQueue;

        Queue<VectorI3> propagateLightRequests;

        Queue<Chunk> propagateLightQueue;

        Pool<ChunkLightPropagator> lightPropagatorPool;

        TaskQueue lightPropagatorTaskQueue;

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

            normalUpdateMeshRequests = new Queue<VectorI3>();
            highUpdateMeshRequests = new Queue<VectorI3>();
            buildVerticesQueue = new Queue<Chunk>(verticesBuilderCount);
            verticesBuilderPool = new Pool<ChunkVerticesBuilder>(() => { return new ChunkVerticesBuilder(this); })
            {
                MaxCapacity = verticesBuilderCount
            };
            verticesBuilderTaskQueue = new TaskQueue
            {
                SlotCount = verticesBuilderCount
            };

            buildLocalLightRequests = new Queue<VectorI3>();
            buildLocalLightQueue = new Queue<Chunk>();
            localLightBuilderPool = new Pool<ChunkLocalLightBuilder>(() => { return new ChunkLocalLightBuilder(this); })
            {
                // TODO
                MaxCapacity = 2
            };
            localLightBuilderTaskQueue = new TaskQueue
            {
                // TODO
                SlotCount = 2
            };

            propagateLightRequests = new Queue<VectorI3>();
            propagateLightQueue = new Queue<Chunk>();
            lightPropagatorPool = new Pool<ChunkLightPropagator>(() => { return new ChunkLightPropagator(this); })
            {
                // TODO
                MaxCapacity = 2
            };
            lightPropagatorTaskQueue = new TaskQueue
            {
                // TODO
                SlotCount = 2
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
                CheckUpdateMesheRequests(gameTime);
                CheckBuildLocalLightRequests(gameTime);
            }

            // ビルダのタスク キューを更新。
            verticesBuilderTaskQueue.Update();
            localLightBuilderTaskQueue.Update();

            // 更新完了を監視。
            // 更新中はチャンクの更新ロックを取得したままであるため、
            // クローズ中も完了を監視して更新ロックの解放を試みなければならない。
            CheckMeshesUpdated(gameTime);
            CheckBuildLocalLightCompleted(gameTime);

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
                var bounds = BoundingBoxI.CreateFromCenterExtents(partition.Position, new VectorI3(1));
                RequestUpdateMesh(ref bounds, UpdateMeshPriority.Normal);
                
                RequestBuildLocalLight(ref chunk.Position);
            }

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

        /// <summary>
        /// チャンクのメッシュ更新要求を追加します。
        /// </summary>
        /// <param name="position">チャンクの位置。</param>
        /// <param name="priority">優先度。</param>
        internal void RequestUpdateMesh(ref VectorI3 position, UpdateMeshPriority priority)
        {
            switch (priority)
            {
                case UpdateMeshPriority.Normal:
                    if (!normalUpdateMeshRequests.Contains(position))
                        normalUpdateMeshRequests.Enqueue(position);
                    break;
                case UpdateMeshPriority.High:
                    if (!highUpdateMeshRequests.Contains(position))
                        highUpdateMeshRequests.Enqueue(position);
                    break;
            }
        }

        internal void RequestUpdateMesh(ref BoundingBoxI bounds, UpdateMeshPriority priority)
        {
            for (int z = bounds.Min.Z; z < (bounds.Min.Z + bounds.Size.Z); z++)
            {
                for (int y = bounds.Min.Y; y < (bounds.Min.Y + bounds.Size.Y); y++)
                {
                    for (int x = bounds.Min.X; x < (bounds.Min.X + bounds.Size.X); x++)
                    {
                        var position = new VectorI3(x, y, z);
                        RequestUpdateMesh(ref position, priority);
                    }
                }
            }
        }

        internal void RequestBuildLocalLight(ref VectorI3 position)
        {
            if (!buildLocalLightRequests.Contains(position))
                buildLocalLightRequests.Enqueue(position);
        }

        internal void RequestPropagateLight(ref VectorI3 position)
        {
            if (!propagateLightRequests.Contains(position))
                propagateLightRequests.Enqueue(position);
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

        internal void ReleaseLocalLightBuilder(ChunkLocalLightBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException("builder");

            builder.Chunk.LocalLightBuilder = null;
            builder.Chunk = null;
            localLightBuilderPool.Return(builder);
        }

        internal void ReleaseLightPropagator(ChunkLightPropagator propagator)
        {
            if (propagator == null) throw new ArgumentNullException("propagator");

            propagator.Chunk.LightPropagator = null;
            propagator.Chunk = null;
            lightPropagatorPool.Return(propagator);
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

        void CheckBuildLocalLightRequests(GameTime gameTime)
        {
            // TODO
            var searchCapacity = 10;

            int count = buildLocalLightRequests.Count;
            for (int i = 0; i < count && i < searchCapacity; i++)
            {
                var position = buildLocalLightRequests.Peek();

                // アクティブ チャンクを取得。
                var chunk = GetChunk(ref position);
                if (chunk == null)
                {
                    // 存在しない場合は要求を取り消す。
                    buildLocalLightRequests.Dequeue();
                    continue;
                }

                if (chunk.SolidCount == 0)
                {
                    // 空チャンクならば光レベルの更新は不要。
                    buildLocalLightRequests.Dequeue();
                    continue;
                }

                // チャンクが更新中ならば待機キューへ戻す。
                if (buildLocalLightQueue.Contains(chunk))
                {
                    buildLocalLightRequests.Dequeue();
                    buildLocalLightRequests.Enqueue(position);
                    continue;
                }

                var lightBuilder = localLightBuilderPool.Borrow();
                if (lightBuilder == null)
                {
                    // プール枯渇の場合は次フレームでの再更新判定に期待。
                    break;
                }

                // 更新が終わるまで非アクティブ化を抑制。
                // パーティション マネージャは非アクティブ化前にロック取得を試みるが、
                // チャンク マネージャはそのサブクラスでロックを取得しているため、
                // ロックだけでは非アクティブ化を抑制できない事に注意。
                chunk.SuppressPassivation = true;

                // チャンクのロックを試行。
                // 更新が完了するまでロックを維持。
                if (!chunk.EnterLock())
                {
                    // ロックを取得できない場合は待機キューへ戻す。
                    buildLocalLightRequests.Dequeue();
                    buildLocalLightRequests.Enqueue(position);
                    
                    localLightBuilderPool.Return(lightBuilder);

                    continue;
                }

                // ビルダを初期化。
                lightBuilder.Chunk = chunk;
                chunk.LocalLightBuilder = lightBuilder;
                lightBuilder.Completed = false;

                // ビルダを登録。
                localLightBuilderTaskQueue.Enqueue(lightBuilder.ExecuteAction);

                buildLocalLightQueue.Enqueue(chunk);
                buildLocalLightRequests.Dequeue();
            }
        }

        void CheckPropagateLightRequests(GameTime gameTime)
        {
            // TODO
            var searchCapacity = 10;

            int count = propagateLightRequests.Count;
            for (int i = 0; i < count && i < searchCapacity; i++)
            {
                var position = propagateLightRequests.Peek();

                // アクティブ チャンクを取得。
                var chunk = GetChunk(ref position);
                if (chunk == null)
                {
                    // 存在しない場合は要求を取り消す。
                    propagateLightRequests.Dequeue();
                    continue;
                }

                if (chunk.SolidCount == 0)
                {
                    // 空チャンクならば光レベルの更新は不要。
                    propagateLightRequests.Dequeue();
                    continue;
                }

                // チャンクが更新中ならば待機キューへ戻す。
                if (propagateLightQueue.Contains(chunk))
                {
                    propagateLightRequests.Dequeue();
                    propagateLightRequests.Enqueue(position);
                    continue;
                }

                var propagator = lightPropagatorPool.Borrow();
                if (propagator == null)
                {
                    // プール枯渇の場合は次フレームでの再更新判定に期待。
                    break;
                }

                // 更新が終わるまで非アクティブ化を抑制。
                // パーティション マネージャは非アクティブ化前にロック取得を試みるが、
                // チャンク マネージャはそのサブクラスでロックを取得しているため、
                // ロックだけでは非アクティブ化を抑制できない事に注意。
                chunk.SuppressPassivation = true;

                // チャンクのロックを試行。
                // 更新が完了するまでロックを維持。
                if (!chunk.EnterLock())
                {
                    // ロックを取得できない場合は待機キューへ戻す。
                    propagateLightRequests.Dequeue();
                    propagateLightRequests.Enqueue(position);

                    lightPropagatorPool.Return(propagator);

                    continue;
                }

                // ビルダを初期化。
                propagator.Chunk = chunk;
                chunk.LightPropagator = propagator;
                propagator.Completed = false;

                // ビルダを登録。
                lightPropagatorTaskQueue.Enqueue(propagator.ExecuteAction);

                propagateLightQueue.Enqueue(chunk);
                propagateLightRequests.Dequeue();
            }
        }

        /// <summary>
        /// メッシュ更新が必要なチャンクを探索し、その更新要求を追加します。
        /// </summary>
        /// <param name="gameTime">ゲーム時間。</param>
        void CheckUpdateMesheRequests(GameTime gameTime)
        {
            CheckUpdateMesheRequests(gameTime, highUpdateMeshRequests);
            CheckUpdateMesheRequests(gameTime, normalUpdateMeshRequests);
        }

        void CheckUpdateMesheRequests(GameTime gameTime, Queue<VectorI3> requestQueue)
        {
            int count = requestQueue.Count;
            for (int i = 0; i < count && i < meshUpdateSearchCapacity; i++)
            {
                var position = requestQueue.Peek();

                // アクティブ チャンクを取得。
                var chunk = GetChunk(ref position);
                if (chunk == null)
                {
                    // 存在しない場合はメッシュ更新要求を取り消す。
                    requestQueue.Dequeue();
                    continue;
                }

                // チャンクがメッシュ更新中ならば待機キューへ戻す。
                if (buildVerticesQueue.Contains(chunk))
                {
                    requestQueue.Dequeue();
                    requestQueue.Enqueue(position);
                    continue;
                }

                var verticesBuilder = verticesBuilderPool.Borrow();
                if (verticesBuilder == null)
                {
                    // プール枯渇の場合は次フレームでの再更新判定に期待。
                    break;
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
                    requestQueue.Dequeue();
                    requestQueue.Enqueue(position);

                    verticesBuilderPool.Return(verticesBuilder);

                    continue;
                }

                // 頂点ビルダを初期化。
                InitializeInterVerticesBuilder(verticesBuilder, chunk);

                // 頂点ビルダを登録。
                verticesBuilderTaskQueue.Enqueue(verticesBuilder.ExecuteAction);

                buildVerticesQueue.Enqueue(chunk);
                requestQueue.Dequeue();
            }
        }

        /// <summary>
        /// 頂点ビルダを初期化します。
        /// </summary>
        /// <param name="verticesBuilder">頂点ビルダ。</param>
        /// <param name="chunk">チャンク。</param>
        void InitializeInterVerticesBuilder(ChunkVerticesBuilder verticesBuilder, Chunk chunk)
        {
            verticesBuilder.Chunk = chunk;
            chunk.VerticesBuilder = verticesBuilder;

            // 完了フラグを初期化。
            verticesBuilder.Completed = false;
        }

        void CheckBuildLocalLightCompleted(GameTime gameTime)
        {
            // ビルダの監視。
            int count = buildLocalLightQueue.Count;
            for (int i = 0; i < count; i++)
            {
                var chunk = buildLocalLightQueue.Dequeue();

                if (!chunk.LocalLightBuilder.Completed)
                {
                    // 未完ならば更新キューへ戻す。
                    buildLocalLightQueue.Enqueue(chunk);
                    continue;
                }

                // 完了フラグ ON。
                chunk.LocalLightingCompleted = true;

                // ビルダを解放。
                ReleaseLocalLightBuilder(chunk.LocalLightBuilder);

                // 更新開始で取得したロックを解放。
                chunk.ExitLock();

                // 非アクティブ化の抑制を解除。
                chunk.SuppressPassivation = false;

                if (!Closing)
                {
                    // 非クローズ中ならばメッシュ更新を要求。
                    RequestUpdateMesh(ref chunk.Position, UpdateMeshPriority.High);

                    var bottomNeighbor = chunk.Position + CubicSide.Bottom.Direction;
                    RequestBuildLocalLight(ref bottomNeighbor);
                }
            }
        }

        void CheckPropagateLightCompleted(GameTime gameTime)
        {
            // ビルダの監視。
            int count = propagateLightQueue.Count;
            for (int i = 0; i < count; i++)
            {
                var chunk = propagateLightQueue.Dequeue();

                if (!chunk.LightPropagator.Completed)
                {
                    // 未完ならば更新キューへ戻す。
                    propagateLightQueue.Enqueue(chunk);
                    continue;
                }

                // ビルダを解放。
                ReleaseLightPropagator(chunk.LightPropagator);

                // 更新開始で取得したロックを解放。
                chunk.ExitLock();

                // 非アクティブ化の抑制を解除。
                chunk.SuppressPassivation = false;

                if (!Closing)
                {
                    // 非クローズ中ならばメッシュ更新を要求。
                    //RequestUpdateMesh(ref chunk.Position, UpdateMeshPriority.High);

                    //var bottomNeighbor = chunk.Position + CubicSide.Bottom.Direction;
                    //RequestBuildLocalLight(ref bottomNeighbor);
                }
            }
        }

        /// <summary>
        /// チャンク メッシュ更新の完了を監視し、
        /// 完了しているならば頂点バッファへの反映を試みます。
        /// </summary>
        /// <param name="gameTime">ゲーム時間。</param>
        void CheckMeshesUpdated(GameTime gameTime)
        {
            // 頂点ビルダの監視。
            int count = buildVerticesQueue.Count;
            for (int i = 0; i < count; i++)
            {
                var chunk = buildVerticesQueue.Dequeue();

                if (!chunk.VerticesBuilder.Completed)
                {
                    // 未完ならば更新キューへ戻す。
                    buildVerticesQueue.Enqueue(chunk);
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
                    mesh = CreateMesh(effect, false);
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
                    mesh = CreateMesh(effect, true);
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
        ChunkMesh CreateMesh(ChunkEffect effect, bool translucent)
        {
            var name = (translucent) ? "TranslucentMesh" : "OpaqueMesh";

            var mesh = new ChunkMesh(name, effect);
            mesh.Translucent = translucent;

            MeshCount++;

            return mesh;
        }
    }
}
