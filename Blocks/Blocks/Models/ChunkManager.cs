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
        // TODO
        //
        // 実行で最適と思われる値を調べて決定するが、
        // 最終的には定義ファイルのようなもので定義を変更できるようにする。
        //

        // 更新の最大試行数。
        public const int ChunkMeshUpdateSearchCapacity = 100;

        public const int ChunkVerticesBuilderCapacity = 10;

        static readonly VectorI3 chunkSize = Chunk.Size;

        static readonly Vector3 chunkMeshOffset = Chunk.HalfSize.ToVector3();

        /// <summary>
        /// グラフィックス デバイス。
        /// </summary>
        GraphicsDevice graphicsDevice;

        /// <summary>
        /// リージョン マネージャ。
        /// </summary>
        RegionManager regionManager;

        /// <summary>
        /// シーン マネージャ。
        /// チャンク メッシュをシーン オブジェクトとして登録するために必要。
        /// </summary>
        SceneManager sceneManager;

        /// <summary>
        /// 1 / chunkSize。
        /// </summary>
        Vector3 inverseChunkSize;

        /// <summary>
        /// 頂点ビルダ実行待ちチャンクのキュー。
        /// </summary>
        Queue<Chunk> buildVerticesQueue = new Queue<Chunk>(ChunkVerticesBuilderCapacity);

        /// <summary>
        /// 頂点バッファ反映待ちチャンクのキュー。
        /// </summary>
        Queue<Chunk> updateBufferQueue = new Queue<Chunk>(ChunkVerticesBuilderCapacity);

        /// <summary>
        /// 頂点ビルダのプール。
        /// </summary>
        Pool<ChunkVerticesBuilder> verticesBuilderPool;

        /// <summary>
        /// 頂点ビルダの処理を非同期に実行するためのタスク キュー。
        /// </summary>
        TaskQueue verticesBuilderTaskQueue;

        /// <summary>
        /// 破棄待ちチャンク メッシュのキュー。
        /// </summary>
        Queue<ChunkMesh> disposeMeshQueue = new Queue<ChunkMesh>();

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
        /// インスタンスを生成します。
        /// </summary>
        /// <param name="settings">パーティション設定。</param>
        /// <param name="graphicsDevice">グラフィックス デバイス。</param>
        /// <param name="regionManager">リージョン マネージャ。</param>
        /// <param name="sceneManager">シーン マネージャ。</param>
        public ChunkManager(Settings settings, GraphicsDevice graphicsDevice, RegionManager regionManager, SceneManager sceneManager)
            : base(settings)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");
            if (regionManager == null) throw new ArgumentNullException("regionManager");
            if (sceneManager == null) throw new ArgumentNullException("sceneManager");

            this.graphicsDevice = graphicsDevice;
            this.regionManager = regionManager;
            this.sceneManager = sceneManager;

            inverseChunkSize.X = 1 / (float) chunkSize.X;
            inverseChunkSize.Y = 1 / (float) chunkSize.Y;
            inverseChunkSize.Z = 1 / (float) chunkSize.Z;

            verticesBuilderPool = new Pool<ChunkVerticesBuilder>(CreateInterChunkMeshTask)
            {
                MaxCapacity = ChunkVerticesBuilderCapacity
            };
            verticesBuilderTaskQueue = new TaskQueue
            {
                SlotCount = ChunkVerticesBuilderCapacity
            };
        }

        /// <summary>
        /// チャンクをパーティションとして生成します。
        /// </summary>
        protected override Partition CreatePartition()
        {
            return new Chunk(this, regionManager);
        }

        /// <summary>
        /// 指定の位置を含むリージョンがある場合、アクティブ化可能であると判定します。
        /// </summary>
        protected override bool CanActivatePartition(ref VectorI3 position)
        {
            if (!regionManager.RegionExists(ref position)) return false;

            return base.CanActivatePartition(ref position);
        }

        /// <summary>
        /// チャンク メッシュの破棄、新たなメッシュ更新の開始、メッシュ更新完了の監視を行います。
        /// </summary>
        /// <param name="gameTime">ゲーム時間。</param>
        protected override void UpdatePartitionsOverride(GameTime gameTime)
        {
            // 破棄要求を受けたチャンク メッシュを処理。
            CheckDisposingChunkMeshes(gameTime);

            // メッシュ更新が必要なチャンクを探索して更新要求を追加。
            // ただし、クローズが開始したら行わない。
            if (!Closing) CheckDirtyChunkMeshes(gameTime);

            // 頂点ビルダのタスク キューを更新。
            verticesBuilderTaskQueue.Update();

            // メッシュ更新完了を監視。
            // メッシュ更新中はチャンクの更新ロックを取得したままであるため、
            // クローズ中も完了を監視して更新ロックの解放を試みなければならない。
            CheckChunkMeshesUpdated(gameTime);

            base.UpdatePartitionsOverride(gameTime);
        }

        /// <summary>
        /// メッシュ更新が必要なチャンクを探索し、その更新要求を追加します。
        /// </summary>
        /// <param name="gameTime">ゲーム時間。</param>
        void CheckDirtyChunkMeshes(GameTime gameTime)
        {
            // メッシュ更新が必要なチャンクを探索。
            int activePartitionCount = ActivePartitions.Count;
            int trials = 0;
            while (0 < activePartitionCount && trials < ChunkMeshUpdateSearchCapacity && trials < activePartitionCount)
            {
                // TODO
                // 視点位置の近隣を優先するためのアルゴリズムは無いのだろうか？
                var chunk = ActivePartitions.Dequeue() as Chunk;

                if (chunk.EnterUpdate())
                {
                    // 現在の隣接チャンクのアクティブ状態が前回のメッシュ更新時のアクティブ状態と異なるならば、
                    // 新たにアクティブ化された隣接チャンクを考慮してメッシュを更新するために、
                    // 強制的にチャンクを Dirty とする。
                    if (chunk.ActiveNeighbors != chunk.NeighborsReferencedOnUpdate)
                        chunk.MeshDirty = true;

                    if (chunk.MeshDirty)
                    {
                        if (!buildVerticesQueue.Contains(chunk) && !updateBufferQueue.Contains(chunk))
                        {
                            var verticesBuilder = verticesBuilderPool.Borrow();
                            if (verticesBuilder != null)
                            {
                                // 頂点ビルダを初期化。
                                InitializeInterVerticesBuilder(verticesBuilder, chunk);

                                // 頂点ビルダを登録。
                                verticesBuilderTaskQueue.Enqueue(verticesBuilder.ExecuteAction);

                                buildVerticesQueue.Enqueue(chunk);
                            }
                            else
                            {
                                // プール枯渇の場合は次フレーム以降の再更新判定に期待。
                                chunk.ExitUpdate();
                            }
                        }
                    }
                    else
                    {
                        chunk.ExitUpdate();
                    }
                }

                ActivePartitions.Enqueue(chunk);

                trials++;
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

            // 隣接チャンク集合を初期化。
            var centerPosition = chunk.Position;
            for (int z = -1; z <= 1; z++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        // 8 つの隅は不要。
                        if (x != 0 && y != 0 && z != 0) continue;

                        // 中心には自身を設定。
                        if (x == 0 && y == 0 && z == 0)
                        {
                            verticesBuilder.CloseChunks[0, 0, 0] = chunk;
                            continue;
                        }

                        var closePosition = centerPosition;
                        closePosition.X += x;
                        closePosition.Y += y;
                        closePosition.Z += z;

                        Chunk closeChunk;
                        TryGetChunk(ref closePosition, out closeChunk);

                        verticesBuilder.CloseChunks[x, y, z] = closeChunk;
                    }
                }
            }

            // このフレームで収集できた面隣接チャンクを記録。
            var flags = CubicSide.Flags.None;
            foreach (var side in CubicSide.Items)
            {
                if (chunk.VerticesBuilder.CloseChunks[side] != null)
                    flags |= side.ToFlags();
            }
            chunk.NeighborsReferencedOnUpdate = flags;
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

                if (Closing)
                {
                    // クローズ中ならば頂点バッファ反映をスキップして更新ロックを解放。
                    ReleaseVerticesBuilder(chunk.VerticesBuilder);

                    chunk.MeshDirty = false;
                    chunk.ExitUpdate();
                    continue;
                }

                // 頂点バッファ更新キューへ追加。
                updateBufferQueue.Enqueue(chunk);
            }

            // 頂点バッファへの反映。
            if (0 < updateBufferQueue.Count && !gameTime.IsRunningSlowly)
            {
                // 各フレームでひとつずつバッファへ反映。
                var chunk = updateBufferQueue.Dequeue();

                UpdateChunkMesh(chunk);

                // 頂点ビルダを解放。
                ReleaseVerticesBuilder(chunk.VerticesBuilder);

                // 更新ロックを解放。
                chunk.MeshDirty = false;
                chunk.ExitUpdate();
            }
        }

        /// <summary>
        /// 指定の位置にあるチャンクの取得を試行します。
        /// </summary>
        /// <param name="position">パーティション空間におけるチャンクの位置。</param>
        /// <param name="result">
        /// 指定の位置にあるチャンク、あるいは、そのようなチャンクが無い場合は null。
        /// </param>
        /// <returns>
        /// true (指定の位置にチャンクが存在した場合)、false (それ以外の場合)。
        /// </returns>
        bool TryGetChunk(ref VectorI3 position, out Chunk result)
        {
            Partition partition;
            if (ActivePartitions.TryGetPartition(ref position, out partition))
            {
                result = partition as Chunk;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        /// <summary>
        /// 頂点ビルダ プールのインスタンス生成メソッドです。
        /// </summary>
        /// <returns>頂点ビルダ。</returns>
        ChunkVerticesBuilder CreateInterChunkMeshTask()
        {
            return new ChunkVerticesBuilder();
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
            builder.CloseChunks.Clear();
            builder.Opaque.Clear();
            builder.Translucent.Clear();
            verticesBuilderPool.Return(builder);
        }

        /// <summary>
        /// チャンク メッシュを生成します。
        /// </summary>
        /// <param name="translucent">
        /// true (半透明の場合)、false (それ以外の場合)。
        /// </param>
        /// <returns>チャンク メッシュ。</returns>
        ChunkMesh CreateChunkMesh(bool translucent)
        {
            var chunkMesh = new ChunkMesh(graphicsDevice);
            chunkMesh.Translucent = translucent;

            sceneManager.AddSceneObject(chunkMesh);

            ChunkMeshCount++;

            return chunkMesh;
        }

        /// <summary>
        /// チャンク メッシュを破棄します。
        /// ここでは破棄要求をキューに入れるのみであり、
        /// Dispose メソッド呼び出しは Update メソッド内で処理されます。
        /// </summary>
        /// <param name="chunkMesh">チャンク メッシュ。</param>
        internal void DisposeChunkMesh(ChunkMesh chunkMesh)
        {
            sceneManager.RemoveSceneObject(chunkMesh);

            // 破棄を待機する。
            lock (disposeMeshQueue)
                disposeMeshQueue.Enqueue(chunkMesh);
        }

        /// <summary>
        /// 破棄要求の出されたチャンク メッシュを破棄します。
        /// </summary>
        /// <param name="gameTime">ゲーム時間。</param>
        void CheckDisposingChunkMeshes(GameTime gameTime)
        {
            lock (disposeMeshQueue)
            {
                var count = disposeMeshQueue.Count;
                for (int i = 0; i < count; i++)
                {
                    var chunkMesh = disposeMeshQueue.Dequeue();

                    TotalVertexCount -= chunkMesh.VertexCount;
                    TotalIndexCount -= chunkMesh.IndexCount;

                    chunkMesh.Dispose();

                    ChunkMeshCount--;
                }
            }
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
            var position = chunk.WorldPosition + chunkMeshOffset;

            // メッシュに設定するワールド行列。
            Matrix world;
            Matrix.CreateTranslation(ref position, out world);

            //----------------------------------------------------------------
            // 不透明メッシュ

            if (builder.Opaque.VertexCount == 0 || builder.Opaque.IndexCount == 0)
            {
                if (chunk.OpaqueMesh != null)
                {
                    DisposeChunkMesh(chunk.OpaqueMesh);
                    chunk.OpaqueMesh = null;
                }
            }
            else
            {
                if (chunk.OpaqueMesh == null)
                {
                    chunk.OpaqueMesh = CreateChunkMesh(false);
                }
                else
                {
                    TotalVertexCount -= chunk.OpaqueMesh.VertexCount;
                    TotalIndexCount -= chunk.OpaqueMesh.IndexCount;
                }

                chunk.OpaqueMesh.Position = position;
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
                {
                    DisposeChunkMesh(chunk.TranslucentMesh);
                    chunk.TranslucentMesh = null;
                }
            }
            else
            {
                if (chunk.TranslucentMesh == null)
                {
                    chunk.TranslucentMesh = CreateChunkMesh(true);
                }
                else
                {
                    TotalVertexCount -= chunk.TranslucentMesh.VertexCount;
                    TotalIndexCount -= chunk.TranslucentMesh.IndexCount;
                }

                chunk.TranslucentMesh.Position = position;
                chunk.TranslucentMesh.World = world;
                builder.Translucent.Populate(chunk.TranslucentMesh);

                TotalVertexCount += chunk.TranslucentMesh.VertexCount;
                TotalIndexCount += chunk.TranslucentMesh.IndexCount;
                MaxVertexCount = Math.Max(MaxVertexCount, chunk.TranslucentMesh.VertexCount);
                MaxIndexCount = Math.Max(MaxIndexCount, chunk.TranslucentMesh.IndexCount);
            }
        }
    }
}
