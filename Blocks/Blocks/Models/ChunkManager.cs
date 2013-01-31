#region Using

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
        /// <summary>
        /// チャンク サイズ。
        /// </summary>
        public readonly VectorI3 ChunkSize;

        /// <summary>
        /// 半チャンク サイズ。
        /// </summary>
        public readonly VectorI3 HalfChunkSize;

        /// <summary>
        /// チャンク メッシュのオフセット。
        /// </summary>
        public readonly Vector3 ChunkMeshOffset;

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

        int chunkNodeIdSequence;

        /// <summary>
        /// チャンクのプール。
        /// </summary>
        ConcurrentPool<Chunk> chunkPool;

        /// <summary>
        /// データのプール。
        /// </summary>
        ConcurrentPool<ChunkData> chunkDataPool;

        /// <summary>
        /// 頂点ビルダ実行待ちチャンクのキュー。
        /// このキューではチャンクそのものではなく、その位置を管理する。
        /// 頂点ビルダ実行判定では、管理している位置からアクティブ リストを参照し、
        /// 処理対象のチャンクを取得するという手順を採る。
        /// つまり、非アクティブ化が開始したチャンクは、
        /// その開始によりアクティブ リストから除外されるため、
        /// 頂点ビルダ実行判定からも除外される。
        /// </summary>
        Queue<VectorI3> waitBuildVerticesQueue;

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

        /// <summary>
        /// シーン マネージャ。
        /// </summary>
        public SceneManager SceneManager { get; private set; }

        /// <summary>
        /// チャンク ノードを関連付けるためのノード。
        /// </summary>
        public SceneNode ChunkRootNode { get; private set; }

        /// <summary>
        /// チャンク メッシュの数を取得します。
        /// </summary>
        public int ChunkMeshCount { get; private set; }

        /// <summary>
        /// 頂点ビルダの総数を取得します。
        /// </summary>
        public int TotalChunkVerticesBuilderCount
        {
            get { return verticesBuilderPool.TotalObjectCount; }
        }

        /// <summary>
        /// 未使用の頂点ビルダの数を取得します。
        /// </summary>
        public int PassiveChunkVerticesBuilderCount
        {
            get { return verticesBuilderPool.Count; }
        }

        /// <summary>
        /// 使用中の頂点ビルダの数を取得します。
        /// </summary>
        public int ActiveChunkVerticesBuilderCount
        {
            get { return TotalChunkVerticesBuilderCount - PassiveChunkVerticesBuilderCount; }
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
        internal ChunkData EmptyChunkData { get; private set; }

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

            HalfChunkSize = ChunkSize;
            HalfChunkSize.X /= 2;
            HalfChunkSize.Y /= 2;
            HalfChunkSize.Z /= 2;

            ChunkMeshOffset = HalfChunkSize.ToVector3();

            chunkPool = new ConcurrentPool<Chunk>(() => { return new Chunk(this); });
            chunkPool.MaxCapacity = settings.ChunkPoolMaxCapacity;
            chunkDataPool = new ConcurrentPool<ChunkData>(() => { return new ChunkData(this); });
            EmptyChunkData = new ChunkData(this);

            verticesBuilderPool = new Pool<ChunkVerticesBuilder>(() => { return new ChunkVerticesBuilder(this); })
            {
                MaxCapacity = verticesBuilderCount
            };
            verticesBuilderTaskQueue = new TaskQueue
            {
                SlotCount = verticesBuilderCount
            };

            waitBuildVerticesQueue = new Queue<VectorI3>();
            buildVerticesQueue = new Queue<Chunk>(verticesBuilderCount);

            ChunkRootNode = sceneManager.CreateSceneNode("ChunkRoot");
            sceneManager.RootNode.Children.Add(ChunkRootNode);
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

        public VectorI3 GetChunkPositionByBlockPosition(VectorI3 blockPosition)
        {
            VectorI3 result;
            GetChunkPositionByBlockPosition(ref blockPosition, out result);
            return result;
        }

        public Chunk GetChunkByBlockPosition(VectorI3 blockPosition)
        {
            VectorI3 chunkPosition;
            GetChunkPositionByBlockPosition(ref blockPosition, out chunkPosition);

            return this[chunkPosition] as Chunk;
        }

        /// <summary>
        /// 指定の位置を含むリージョンがある場合、アクティブ化可能であると判定します。
        /// </summary>
        protected override bool CanActivate(VectorI3 position)
        {
            if (regionManager.GetRegionByChunkPosition(position) == null) return false;

            return base.CanActivate(position);
        }

        /// <summary>
        /// プールからチャンクを取得して返します。
        /// プールが枯渇している場合は null を返します。
        /// </summary>
        protected override Partition Create(VectorI3 position)
        {
            // プールから取得。
            var chunk = chunkPool.Borrow();
            if (chunk == null) return null;

            // 対象リージョンの取得。
            var region = regionManager.GetRegionByChunkPosition(position);
            if (region == null) throw new InvalidOperationException("Region not found: ChunkPosition = " + position);

            // 初期化。
            chunk.Initialize(position, region);

            return chunk;
        }

        /// <summary>
        /// プールへチャンクを戻します。
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
        /// 新たなメッシュ更新の開始、メッシュ更新完了の監視を行います。
        /// </summary>
        /// <param name="gameTime">ゲーム時間。</param>
        protected override void UpdateOverride(GameTime gameTime)
        {
            // メッシュ更新が必要なチャンクを探索して更新要求を追加。
            // ただし、クローズが開始したら行わない。
            if (!Closing) CheckDirtyChunkMeshes(gameTime);

            // 頂点ビルダのタスク キューを更新。
            verticesBuilderTaskQueue.Update();

            // メッシュ更新完了を監視。
            // メッシュ更新中はチャンクの更新ロックを取得したままであるため、
            // クローズ中も完了を監視して更新ロックの解放を試みなければならない。
            CheckChunkMeshesUpdated(gameTime);

            base.UpdateOverride(gameTime);
        }

        protected override void DisposeOverride(bool disposing)
        {
            // TODO
            // プール内チャンクの破棄。

            chunkPool.Clear();

            base.DisposeOverride(disposing);
        }

        internal ChunkData BorrowChunkData()
        {
            return chunkDataPool.Borrow();
        }

        internal void ReturnChunkData(ChunkData data)
        {
            data.Clear();
            chunkDataPool.Return(data);
        }

        /// <summary>
        /// チャンクのメッシュ更新要求を追加します。
        /// </summary>
        /// <param name="chunkPosition">チャンクの位置。</param>
        internal void RequestUpdateMesh(VectorI3 chunkPosition)
        {
            if (!waitBuildVerticesQueue.Contains(chunkPosition))
                waitBuildVerticesQueue.Enqueue(chunkPosition);
        }

        internal int CreateChunkNodeId()
        {
            return chunkNodeIdSequence++;
        }

        /// <summary>
        /// 頂点ビルダをプールへ戻します。
        /// </summary>
        /// <param name="builder"></param>
        internal void ReleaseVerticesBuilder(ChunkVerticesBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException("builder");

            builder.Chunk.VerticesBuilder = null;
            builder.Chunk = null;
            builder.Opaque.Clear();
            builder.Translucent.Clear();
            verticesBuilderPool.Return(builder);
        }

        /// <summary>
        /// チャンク メッシュを破棄します。
        /// ここでは破棄要求をキューに入れるのみであり、
        /// Dispose メソッド呼び出しは Update メソッド内で処理されます。
        /// </summary>
        /// <param name="chunkMesh">チャンク メッシュ。</param>
        internal void DisposeChunkMesh(ChunkMesh chunkMesh)
        {
            TotalVertexCount -= chunkMesh.VertexCount;
            TotalIndexCount -= chunkMesh.IndexCount;

            chunkMesh.Dispose();

            ChunkMeshCount--;
        }

        /// <summary>
        /// メッシュ更新が必要なチャンクを探索し、その更新要求を追加します。
        /// </summary>
        /// <param name="gameTime">ゲーム時間。</param>
        void CheckDirtyChunkMeshes(GameTime gameTime)
        {
            // メッシュ更新が必要なチャンクを探索。

            var searchCapacity = meshUpdateSearchCapacity;
            if (gameTime.IsRunningSlowly) searchCapacity /= 2;

            int count = waitBuildVerticesQueue.Count;
            for (int i = 0; i < count && i < meshUpdateSearchCapacity; i++)
            {
                var chunkPosition = waitBuildVerticesQueue.Peek();

                // アクティブ チャンクを取得。
                var chunk = this[chunkPosition] as Chunk;
                if (chunk == null)
                {
                    // 存在しない場合はメッシュ更新要求を取り消す。
                    waitBuildVerticesQueue.Dequeue();
                    continue;
                }

                // チャンクがメッシュ更新中ならば待機キューへ戻す。
                if (buildVerticesQueue.Contains(chunk))
                {
                    waitBuildVerticesQueue.Dequeue();
                    waitBuildVerticesQueue.Enqueue(chunkPosition);
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
                    waitBuildVerticesQueue.Dequeue();
                    waitBuildVerticesQueue.Enqueue(chunkPosition);
                    continue;
                }

                // 頂点ビルダを初期化。
                InitializeInterVerticesBuilder(verticesBuilder, chunk);

                // 頂点ビルダを登録。
                verticesBuilderTaskQueue.Enqueue(verticesBuilder.ExecuteAction);

                buildVerticesQueue.Enqueue(chunk);
                waitBuildVerticesQueue.Dequeue();
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

        /// <summary>
        /// チャンク メッシュ更新の完了を監視し、
        /// 完了しているならば頂点バッファへの反映を試みます。
        /// </summary>
        /// <param name="gameTime">ゲーム時間。</param>
        void CheckChunkMeshesUpdated(GameTime gameTime)
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
                if (!Closing) UpdateChunkMesh(chunk);

                // 頂点ビルダを解放。
                ReleaseVerticesBuilder(chunk.VerticesBuilder);

                // 更新開始で取得したロックを解放。
                chunk.ExitLock();

                // 非アクティブ化の抑制を解除。
                chunk.SuppressPassivation = false;
            }
        }

        /// <summary>
        /// チャンク メッシュを生成します。
        /// </summary>
        /// <param name="chunkEffect">チャンクのエフェクト。</param>
        /// <param name="translucent">
        /// true (半透明の場合)、false (それ以外の場合)。
        /// </param>
        /// <returns>チャンク メッシュ。</returns>
        ChunkMesh CreateChunkMesh(ChunkEffect chunkEffect, bool translucent)
        {
            var name = (translucent) ? "TranslucentMesh" : "OpaqueMesh";

            var chunkMesh = new ChunkMesh(name, chunkEffect);
            chunkMesh.Translucent = translucent;

            ChunkMeshCount++;

            return chunkMesh;
        }

        /// <summary>
        /// チャンクに関連付けられた頂点ビルダの結果で頂点バッファを更新します。
        /// </summary>
        /// <param name="chunk">チャンク。</param>
        void UpdateChunkMesh(Chunk chunk)
        {
            var builder = chunk.VerticesBuilder;

            // メッシュに設定するワールド座標。
            // チャンクの中心をメッシュの位置とする。
            var position = new Vector3
            {
                X = chunk.Position.X * PartitionSize.X + ChunkMeshOffset.X,
                Y = chunk.Position.Y * PartitionSize.Y + ChunkMeshOffset.Y,
                Z = chunk.Position.Z * PartitionSize.Z + ChunkMeshOffset.Z,
            };

            // メッシュに設定するワールド行列。
            Matrix world;
            Matrix.CreateTranslation(ref position, out world);

            // メッシュに設定するエフェクト。
            var chunkEffect = chunk.Region.ChunkEffect;

            //----------------------------------------------------------------
            // 不透明メッシュ

            if (builder.Opaque.VertexCount == 0 || builder.Opaque.IndexCount == 0)
            {
                if (chunk.OpaqueMesh != null)
                    chunk.OpaqueMesh = null;
            }
            else
            {
                if (chunk.OpaqueMesh == null)
                {
                    chunk.OpaqueMesh = CreateChunkMesh(chunkEffect, false);
                }
                else
                {
                    TotalVertexCount -= chunk.OpaqueMesh.VertexCount;
                    TotalIndexCount -= chunk.OpaqueMesh.IndexCount;
                }

                chunk.OpaqueMesh.PositionWorld = position;
                chunk.OpaqueMesh.World = world;
                builder.Opaque.Populate(chunk.OpaqueMesh);

                TotalVertexCount += chunk.OpaqueMesh.VertexCount;
                TotalIndexCount += chunk.OpaqueMesh.IndexCount;
                MaxVertexCount = Math.Max(MaxVertexCount, chunk.OpaqueMesh.VertexCount);
                MaxIndexCount = Math.Max(MaxIndexCount, chunk.OpaqueMesh.IndexCount);
            }

            //----------------------------------------------------------------
            // 半透明メッシュ

            if (builder.Translucent.VertexCount == 0 || builder.Translucent.IndexCount == 0)
            {
                if (chunk.TranslucentMesh != null)
                    chunk.TranslucentMesh = null;
            }
            else
            {
                if (chunk.TranslucentMesh == null)
                {
                    chunk.TranslucentMesh = CreateChunkMesh(chunkEffect, true);
                }
                else
                {
                    TotalVertexCount -= chunk.TranslucentMesh.VertexCount;
                    TotalIndexCount -= chunk.TranslucentMesh.IndexCount;
                }

                chunk.TranslucentMesh.PositionWorld = position;
                chunk.TranslucentMesh.World = world;
                builder.Translucent.Populate(chunk.TranslucentMesh);

                TotalVertexCount += chunk.TranslucentMesh.VertexCount;
                TotalIndexCount += chunk.TranslucentMesh.IndexCount;
                MaxVertexCount = Math.Max(MaxVertexCount, chunk.TranslucentMesh.VertexCount);
                MaxIndexCount = Math.Max(MaxIndexCount, chunk.TranslucentMesh.IndexCount);
            }

            // チャンクのノードを更新。
            chunk.Node.Update(false);
        }
    }
}
