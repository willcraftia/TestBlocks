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
using Willcraftia.Xna.Blocks.Models;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class ChunkManager : PartitionManager
    {
        // TODO
        //
        // 実行で最適と思われる値を調べて決定するが、
        // 最終的には定義ファイルのようなもので定義を変更できるようにする。
        //

        // 更新の最大試行数。
        public const int UpdateCapacity = 100;

        public const int InitialActiveChunkCapacity = 5000;

        public const int InterChunkCapacity = 10;

        static readonly VectorI3 chunkSize = Chunk.Size;

        static readonly Vector3 chunkMeshOffset = Chunk.HalfSize.ToVector3();

        GraphicsDevice graphicsDevice;

        RegionManager regionManager;

        SceneManager sceneManager;

        Vector3 inverseChunkSize;

        // 中間チャンクを取得できなければメッシュ更新は行えないため、
        // 容量は中間チャンクの総数で良い。
        Queue<Chunk> updatingChunks = new Queue<Chunk>(InterChunkCapacity);

        Pool<InterChunk> interChunkPool;

        ChunkMeshUpdateManager chunkMeshUpdateManager;

        Queue<ChunkMesh> disposingChunkMeshes = new Queue<ChunkMesh>();

        public int ChunkMeshCount { get; private set; }

        public int TotalInterChunkCount
        {
            get { return interChunkPool.TotalObjectCount; }
        }

        public int PassiveInterChunkCount
        {
            get { return interChunkPool.Count; }
        }

        public int ActiveInterChunkCount
        {
            get { return TotalInterChunkCount - PassiveInterChunkCount; }
        }

        public int TotalVertexCount { get; private set; }

        public int TotalIndexCount { get; private set; }

        // ゲームを通しての最大を記録する。
        public int MaxVertexCount { get; private set; }

        // ゲームを通しての最大を記録する。
        public int MaxIndexCount { get; private set; }

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

            interChunkPool = new Pool<InterChunk>(CreateInterChunk)
            {
                MaxCapacity = InterChunkCapacity
            };
            chunkMeshUpdateManager = new ChunkMeshUpdateManager(this);
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
        protected override void UpdatePartitionsOverride()
        {
            // 破棄要求を受けたチャンク メッシュを処理。
            CheckDisposingChunkMeshes();

            // メッシュ更新が必要なチャンクを探索して更新要求を追加。
            // ただし、クローズが開始したら行わない。
            if (!Closing) CheckDirtyChunkMeshes();

            // メッシュ更新完了を監視。
            // メッシュ更新中はチャンクの更新ロックを取得したままであるため、
            // クローズ中も完了を監視して更新ロックの解放を試みなければならない。
            CheckChunkMeshesUpdated();

            base.UpdatePartitionsOverride();
        }

        /// <summary>
        /// メッシュ更新が必要なチャンクを探索し、その更新要求を追加します。
        /// </summary>
        void CheckDirtyChunkMeshes()
        {
            // TODO
            // パーティション マネージャではアクティブ パーティションをキューで管理している。
            // これはパーティション更新には不都合である。

            // メッシュ更新が必要なチャンクを探索。
            int activePartitionCount = ActivePartitions.Count;
            int trials = 0;
            while (0 < activePartitionCount && trials < UpdateCapacity && trials < activePartitionCount)
            {
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
                        if (!updatingChunks.Contains(chunk))
                        {
                            chunk.InterChunk = interChunkPool.Borrow();

                            if (chunk.InterChunk != null)
                            {
                                chunk.InterChunk.Completed = false;
                                chunkMeshUpdateManager.EnqueueChunk(chunk);
                                updatingChunks.Enqueue(chunk);
                            }
                            else
                            {
                                // 中間チャンク枯渇の場合は次フレーム以降の再更新判定に期待。
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
        /// チャンク メッシュ更新の完了を監視し、
        /// 完了しているならば頂点バッファへの反映を試みます。
        /// </summary>
        void CheckChunkMeshesUpdated()
        {
            int count = updatingChunks.Count;
            for (int i = 0; i < count; i++)
            {
                var chunk = updatingChunks.Dequeue();

                if (chunk.InterChunk.Completed)
                {
                    // 中間チャンク更新完了ならば頂点バッファを更新。
                    if (!Closing) UpdateChunkMesh(chunk);

                    // 中間チャンクは不要となるためプールへ返却。
                    ReturnInterChunk(chunk.InterChunk);
                    chunk.InterChunk = null;

                    chunk.MeshDirty = false;
                    chunk.ExitUpdate();
                }
                else
                {
                    // 未完ならば更新キューへ戻す。
                    updatingChunks.Enqueue(chunk);
                }
            }

            chunkMeshUpdateManager.Update();

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
        public bool TryGetChunk(ref VectorI3 position, out Chunk result)
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
        /// 中間チャンク プールにおける中間チャンク生成で呼び出されます。
        /// </summary>
        /// <returns>中間チャンク。</returns>
        InterChunk CreateInterChunk()
        {
            return new InterChunk();
        }

        /// <summary>
        /// 中間チャンクをプールへ戻します。
        /// </summary>
        /// <param name="interChunk"></param>
        internal void ReturnInterChunk(InterChunk interChunk)
        {
            if (interChunk == null) throw new ArgumentNullException("interChunk");

            interChunk.Opaque.Clear();
            interChunk.Translucent.Clear();
            interChunkPool.Return(interChunk);
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
            lock (disposingChunkMeshes)
                disposingChunkMeshes.Enqueue(chunkMesh);
        }

        /// <summary>
        /// 破棄要求の出されたチャンク メッシュを破棄します。
        /// </summary>
        void CheckDisposingChunkMeshes()
        {
            lock (disposingChunkMeshes)
            {
                var count = disposingChunkMeshes.Count;
                for (int i = 0; i < count; i++)
                {
                    var chunkMesh = disposingChunkMeshes.Dequeue();

                    TotalVertexCount -= chunkMesh.VertexCount;
                    TotalIndexCount -= chunkMesh.IndexCount;

                    chunkMesh.Dispose();

                    ChunkMeshCount--;
                }
            }
        }

        /// <summary>
        /// チャンク メッシュの頂点バッファを更新します。
        /// </summary>
        /// <param name="chunk"></param>
        void UpdateChunkMesh(Chunk chunk)
        {
            var interMesh = chunk.InterChunk;

            // メッシュに設定するワールド座標。
            // チャンクの中心をメッシュの位置とする。
            var position = chunk.WorldPosition + chunkMeshOffset;

            // メッシュに設定するワールド行列。
            Matrix world;
            Matrix.CreateTranslation(ref position, out world);

            //----------------------------------------------------------------
            // 不透明メッシュ

            if (interMesh.Opaque.VertexCount == 0 || interMesh.Opaque.IndexCount == 0)
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
                interMesh.Opaque.Populate(chunk.OpaqueMesh);

                TotalVertexCount += chunk.OpaqueMesh.VertexCount;
                TotalIndexCount += chunk.OpaqueMesh.IndexCount;
                MaxVertexCount = Math.Max(MaxVertexCount, chunk.OpaqueMesh.VertexCount);
                MaxIndexCount = Math.Max(MaxIndexCount, chunk.OpaqueMesh.IndexCount);
            }

            //----------------------------------------------------------------
            // 半透明メッシュ

            if (interMesh.Translucent.VertexCount == 0 || interMesh.Translucent.IndexCount == 0)
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
                interMesh.Translucent.Populate(chunk.TranslucentMesh);

                TotalVertexCount += chunk.TranslucentMesh.VertexCount;
                TotalIndexCount += chunk.TranslucentMesh.IndexCount;
                MaxVertexCount = Math.Max(MaxVertexCount, chunk.TranslucentMesh.VertexCount);
                MaxIndexCount = Math.Max(MaxIndexCount, chunk.TranslucentMesh.IndexCount);
            }
        }
    }
}
