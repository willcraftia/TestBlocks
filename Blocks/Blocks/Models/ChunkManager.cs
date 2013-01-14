#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Collections;
using Willcraftia.Xna.Framework.Diagnostics;
using Willcraftia.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Framework.Threading;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    using DiagnosticsMonitor = Willcraftia.Xna.Framework.Diagnostics.Monitor;

    public sealed class ChunkManager
    {
        #region DisposingChunkMesh

        struct DisposingChunkMesh
        {
            public int Age;

            public ChunkMesh ChunkMesh;
        }

        #endregion

        public const string MonitorUpdate = "ChunkManager.Update";

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

        SceneManager sceneManager;

        Vector3 inverseChunkSize;

        ChunkCollection activeChunks = new ChunkCollection(InitialActiveChunkCapacity);

        Queue<Chunk> updatingChunks = new Queue<Chunk>(UpdateCapacity);

        // 更新の開始インデックス。
        int updateOffset = 0;

        // チャンク数はパーティション数に等しい。
        // このため、ここでは最大チャンク数を決定できない。

        ConcurrentPool<Chunk> chunkPool;

        Pool<InterChunk> interChunkPool;

        ChunkMeshUpdateManager chunkMeshUpdateManager;

        Queue<DisposingChunkMesh> disposingChunkMeshes = new Queue<DisposingChunkMesh>();

        bool closing;

        bool closed;

        public int TotalChunkCount
        {
            get { return chunkPool.TotalObjectCount; }
        }

        public int ActiveChunkCount
        {
            get { lock (activeChunks) return activeChunks.Count; }
        }

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

        public ChunkManager(GraphicsDevice graphicsDevice, SceneManager sceneManager)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");
            if (sceneManager == null) throw new ArgumentNullException("sceneManager");

            this.graphicsDevice = graphicsDevice;
            this.sceneManager = sceneManager;

            inverseChunkSize.X = 1 / (float) chunkSize.X;
            inverseChunkSize.Y = 1 / (float) chunkSize.Y;
            inverseChunkSize.Z = 1 / (float) chunkSize.Z;

            chunkPool = new ConcurrentPool<Chunk>(CreateChunk);
            interChunkPool = new Pool<InterChunk>(CreateInterChunk)
            {
                MaxCapacity = InterChunkCapacity
            };
            chunkMeshUpdateManager = new ChunkMeshUpdateManager(this);
        }

        public void Update()
        {
            if (closed) return;

            if (closing)
            {
                lock (activeChunks)
                {
                    if (activeChunks.Count == 0)
                    {
                        closing = false;
                        closed = true;
                        return;
                    }
                }
            }

            DiagnosticsMonitor.Begin(MonitorUpdate);

            // チャンク メッシュ破棄キューを処理。
            TryDisposeChunkMeshes();

            // 長時間のロックを避けるために、一時的に作業リストへコピー。
            lock (activeChunks)
            {
                // アクティブ チャンクが無いならば更新処理終了。
                if (activeChunks.Count == 0)
                {
                    DiagnosticsMonitor.End(MonitorUpdate);
                    return;
                }

                int index = updateOffset;
                bool cycled = false;
                while (updatingChunks.Count < UpdateCapacity)
                {
                    if (activeChunks.Count <= index)
                    {
                        index = 0;
                        cycled = true;
                    }

                    if (cycled && updateOffset <= index) break;

                    var chunk = activeChunks[index++];
                    updatingChunks.Enqueue(chunk);
                }

                updateOffset = index;
            }

            Debug.Assert(updatingChunks.Count <= UpdateCapacity);

            int count = updatingChunks.Count;
            for (int i = 0; i < count; i++)
            {
                var chunk = updatingChunks.Dequeue();

                if (!chunk.EnterUpdate()) continue;

                // 現在の隣接チャンクのアクティブ状態が前回のメッシュ更新時のアクティブ状態と異なるならば、
                // 新たにアクティブ化された隣接チャンクを考慮してメッシュを更新するために、
                // 強制的にチャンクを Dirty とする。
                if (chunk.ActiveNeighbors != chunk.NeighborsReferencedOnUpdate)
                    chunk.MeshDirty = true;

                if (!chunk.MeshDirty)
                {
                    chunk.ExitUpdate();
                    continue;
                }

                if (chunk.InterChunk == null)
                {
                    // 中間チャンク未設定ならば設定を試行。

                    if (closing)
                    {
                        // クローズが開始したならば新規の更新要求は破棄。
                        chunk.ExitUpdate();
                    }
                    else if (BorrowInterChunk(chunk))
                    {
                        chunk.InterChunk.Completed = false;
                        chunkMeshUpdateManager.EnqueueChunk(chunk);
                    }
                    else
                    {
                        // 中間チャンクが枯渇しているため更新を次回の試行に委ねる。
                        chunk.ExitUpdate();
                    }
                }
                else if (chunk.InterChunk.Completed)
                {
                    // 中間チャンクが更新完了ならば Mesh を更新。
                    UpdateChunkMesh(chunk);

                    // 中間チャンクは不要となるためプールへ返却。
                    ReturnInterChunk(chunk.InterChunk);
                    chunk.InterChunk = null;

                    chunk.MeshDirty = false;
                    chunk.ExitUpdate();
                }
            }

            Debug.Assert(updatingChunks.Count == 0);

            chunkMeshUpdateManager.Update();

            DiagnosticsMonitor.End(MonitorUpdate);
        }

        // 非同期呼び出し。
        public Chunk ActivateChunk(Region region, ref VectorI3 position)
        {
            var chunk = chunkPool.Borrow();
            if (chunk == null) return null;

            Debug.Assert(!chunk.Active);

            if (!region.ChunkStore.GetChunk(ref position, chunk))
            {
                chunk.Position = position;

                foreach (var procedure in region.ChunkProcesures)
                    procedure.Generate(chunk);
            }

            chunk.OnActivated(region);

            lock (activeChunks) activeChunks.Add(chunk);

            return chunk;
        }

        // 非同期呼び出し。
        public bool PassivateChunk(Chunk chunk)
        {
            if (chunk == null) throw new ArgumentNullException("chunk");

            Debug.Assert(chunk.Active);

            if (!chunk.EnterPassivate()) return false;

            lock (activeChunks) activeChunks.Remove(chunk);

            if (chunk.OpaqueMesh != null)
            {
                DisposeChunkMesh(chunk.OpaqueMesh);
                chunk.OpaqueMesh = null;
            }
            if (chunk.TranslucentMesh != null)
            {
                DisposeChunkMesh(chunk.TranslucentMesh);
                chunk.TranslucentMesh = null;
            }
            if (chunk.InterChunk != null)
            {
                ReturnInterChunk(chunk.InterChunk);
                chunk.InterChunk = null;
            }

            // 定義に変更があるならば永続化領域を更新。
            if (chunk.DefinitionDirty) chunk.Region.ChunkStore.AddChunk(chunk);

            chunk.OnPassivated();
            chunk.ExitPassivate();

            chunkPool.Return(chunk);

            return true;
        }

        public void Close()
        {
            if (closing || closed) return;

            closing = true;
        }

        public bool TryGetChunk(ref VectorI3 position, out Chunk result)
        {
            lock (activeChunks)
                return activeChunks.TryGetItem(ref position, out result);
        }

        Chunk CreateChunk()
        {
            return new Chunk();
        }

        InterChunk CreateInterChunk()
        {
            return new InterChunk();
        }

        bool BorrowInterChunk(Chunk chunk)
        {
            if (chunk.InterChunk == null)
                chunk.InterChunk = interChunkPool.Borrow();

            return chunk.InterChunk != null;
        }

        void ReturnInterChunk(InterChunk interChunk)
        {
            interChunk.Opaque.Clear();
            interChunk.Translucent.Clear();
            interChunkPool.Return(interChunk);
        }

        ChunkMesh CreateChunkMesh(bool translucent)
        {
            var chunkMesh = new ChunkMesh(graphicsDevice);
            chunkMesh.Translucent = translucent;

            sceneManager.AddSceneObject(chunkMesh);

            ChunkMeshCount++;

            return chunkMesh;
        }

        void DisposeChunkMesh(ChunkMesh chunkMesh)
        {
            TotalVertexCount -= chunkMesh.VertexCount;
            TotalIndexCount -= chunkMesh.IndexCount;

            sceneManager.RemoveSceneObject(chunkMesh);

            // GPU で描画中の可能性があるため、破棄キューへ入れて待機させる。
            var disposingChunkMesh = new DisposingChunkMesh
            {
                ChunkMesh = chunkMesh
            };

            lock (disposingChunkMeshes)
                disposingChunkMeshes.Enqueue(disposingChunkMesh);

            ChunkMeshCount--;
        }

        void TryDisposeChunkMeshes()
        {
            lock (disposingChunkMeshes)
            {
                var count = disposingChunkMeshes.Count;
                for (int i = 0; i < count; i++)
                {
                    var disposingChunkMesh = disposingChunkMeshes.Dequeue();
                    // 3 フレーム程待機。
                    if (disposingChunkMesh.Age < 3)
                    {
                        disposingChunkMesh.Age++;
                        disposingChunkMeshes.Enqueue(disposingChunkMesh);
                    }
                    else
                    {
                        disposingChunkMesh.ChunkMesh.Dispose();
                    }
                }
            }
        }

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
