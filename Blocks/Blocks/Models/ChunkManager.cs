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
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Framework.Threading;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class ChunkManager
    {
        #region ChunkDistanceComparer

        class ChunkDistanceComparer : IComparer<Chunk>
        {
            VectorI3 chunkSize;

            public Vector3 EyePosition;

            public ChunkDistanceComparer(VectorI3 chunkSize)
            {
                this.chunkSize = chunkSize;
            }

            public int Compare(Chunk chunk0, Chunk chunk1)
            {
                float d0;
                float d1;
                CalculateDistanceSquared(chunk0, out d0);
                CalculateDistanceSquared(chunk1, out d1);

                if (d0 == d1) return 0;
                return (d0 < d1) ? -1 : 1;
            }

            void CalculateDistanceSquared(Chunk chunk, out float distanceSquared)
            {
                var chunkPosition = chunk.Position.ToVector3();

                chunkPosition.X *= chunkSize.X;
                chunkPosition.Y *= chunkSize.Y;
                chunkPosition.Z *= chunkSize.Z;
                chunkPosition.X += 0.5f;
                chunkPosition.Y += 0.5f;
                chunkPosition.Z += 0.5f;

                Vector3.DistanceSquared(ref EyePosition, ref chunkPosition, out distanceSquared);
            }
        }

        #endregion

#if DEBUG

        public static bool ChunkBoundingBoxVisible { get; set; }

        public static bool Wireframe { get; set; }

#endif

        public const int DefaultUpdateCapacity = 1000;

        static readonly Logger logger = new Logger(typeof(ChunkManager).Name);

        static readonly BlendState colorWriteDisable = new BlendState
        {
            ColorWriteChannels = ColorWriteChannels.None
        };

        // TODO
        //
        // 実行で最適と思われる値を調べて決定する。
        const ushort defaultVertexCapacity = 10000;

        const ushort defaultIndexCapacity = 10000;

        Region region;

        IChunkStore chunkStore;

        VectorI3 chunkSize;

        Vector3 inverseChunkSize;

        // TODO
        // 初期容量の調整。
        //

        ChunkCollection activeChunks = new ChunkCollection();

        Queue<Chunk> updatingChunks = new Queue<Chunk>();

        // 更新の最大試行数。
        int updateCapacity = DefaultUpdateCapacity;

        // 更新の開始インデックス。
        int updateOffset = 0;

        List<Chunk> workingChunks = new List<Chunk>();

        // チャンク数はパーティション数に等しい。
        // このため、ここでは最大チャンク数を決定できない。

        ConcurrentPool<Chunk> chunkPool;

        // 最大チャンク数をここで決定できないことから、
        // 同様に最大メッシュ数もここで決定できない。

        ConcurrentPool<ChunkMesh> chunkMeshPool;

        Pool<InterChunkMesh> interChunkMeshPool;

        Pool<VertexBuffer> vertexBufferPool;

        Pool<IndexBuffer> indexBufferPool;

        ChunkMeshUpdateManager chunkMeshUpdateManager;

        BoundingFrustum frustum = new BoundingFrustum(Matrix.Identity);

        Vector3 eyePosition;

        ChunkDistanceComparer chunkDistanceComparer;

        List<Chunk> occlusionQueryChunks = new List<Chunk>();

        List<Chunk> opaqueChunks = new List<Chunk>();

        List<Chunk> translucentChunks = new List<Chunk>();

#if DEBUG

        BasicEffect debugEffect;

        BoundingBoxDrawer boundingBoxDrawer;

        bool closing;

        bool closed;

#endif

        public VectorI3 ChunkSize
        {
            get { return chunkSize; }
        }

        public ChunkManager(Region region, IChunkStore chunkStore, VectorI3 chunkSize)
        {
            if (region == null) throw new ArgumentNullException("region");
            if (chunkStore == null) throw new ArgumentNullException("chunkStore");

            this.region = region;
            this.chunkStore = chunkStore;
            this.chunkSize = chunkSize;

            inverseChunkSize.X = 1 / (float) chunkSize.X;
            inverseChunkSize.Y = 1 / (float) chunkSize.Y;
            inverseChunkSize.Z = 1 / (float) chunkSize.Z;

            chunkPool = new ConcurrentPool<Chunk>(CreateChunk);
            chunkMeshPool = new ConcurrentPool<ChunkMesh>(CreateChunkMesh);
            interChunkMeshPool = new Pool<InterChunkMesh>(CreateInterChunkMesh)
            {
                MaxCapacity = 20
            };
            vertexBufferPool = new Pool<VertexBuffer>(CreateVertexBuffer);
            indexBufferPool = new Pool<IndexBuffer>(CreateIndexBuffer);
            chunkMeshUpdateManager = new ChunkMeshUpdateManager(region, this);
            chunkDistanceComparer = new ChunkDistanceComparer(chunkSize);

            InitializeDebugTools();
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

            DebugUpdateRegionMonitor();

            // 長時間のロックを避けるために、一時的に作業リストへコピー。
            lock (activeChunks)
            {
                int index = updateOffset;
                bool cycled = false;
                while (updatingChunks.Count < updateCapacity)
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

            Debug.Assert(updatingChunks.Count <= updateCapacity);

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

                if (chunk.InterMesh == null)
                {
                    // 中間メッシュ未設定ならば設定を試行。

                    if (closing)
                    {
                        // クローズが開始したならば新規の更新要求は破棄。
                        chunk.ExitUpdate();
                    }
                    else if (BorrowInterChunkMesh(chunk))
                    {
                        chunk.InterMesh.Completed = false;
                        chunkMeshUpdateManager.EnqueueChunk(chunk);
                    }
                    else
                    {
                        // 中間メッシュが枯渇しているため更新を次回の試行に委ねる。
                        chunk.ExitUpdate();
                    }
                }
                else if (chunk.InterMesh.Completed)
                {
                    // 中間メッシュが更新完了ならば Mesh を更新。

                    // メッシュ枯渇ならば次回の試行とする。
                    if (!BorrowChunkMesh(chunk)) continue;

                    UpdateChunkMeshBuffer(chunk);

                    ReturnInterChunkMesh(chunk.InterMesh);
                    chunk.InterMesh = null;

                    chunk.MeshDirty = false;
                    chunk.ExitUpdate();
                }
            }

            Debug.Assert(updatingChunks.Count == 0);

            chunkMeshUpdateManager.Update();
        }

        public void Draw(View view, Projection projection)
        {
            DebugSetEffectTechnique();

            // 長時間のロックを避けるために、一時的に作業リストへコピー。
            lock (activeChunks)
            {
                // 複製ループに入る前に十分な容量を保証。
                workingChunks.Capacity = activeChunks.Count;

                for (int i = 0; i < activeChunks.Count; i++)
                    workingChunks.Add(activeChunks[i]);
            }

            //================================================================
            //
            // Frustum Culling
            //

            View.GetEyePosition(ref view.Matrix, out eyePosition);
            frustum.Matrix = view.Matrix * projection.Matrix;

            foreach (var chunk in workingChunks)
            {
                if (!chunk.EnterDraw()) continue;

                var mesh = chunk.Mesh;
                if (mesh == null || !chunk.IsInFrustum(frustum))
                {
                    chunk.ExitDraw();
                    continue;
                }

                if (mesh.Opaque.VertexCount != 0)
                    opaqueChunks.Add(chunk);

                if (mesh.Translucent.VertexCount != 0)
                    translucentChunks.Add(chunk);
            }

            opaqueChunks.Sort(chunkDistanceComparer);
            translucentChunks.Sort(chunkDistanceComparer);

            var pass = region.ChunkEffect.BackingEffect.CurrentTechnique.Passes[0];

            //================================================================
            //
            // OcclusionQuery
            //

            region.GraphicsDevice.BlendState = colorWriteDisable;
            region.GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

            foreach (var chunk in opaqueChunks)
            {
                Matrix world;
                chunk.CreateWorldMatrix(out world);

                region.ChunkEffect.World = world;

                pass.Apply();

                chunk.Mesh.Opaque.UpdateOcclusion();
            }

            //================================================================
            //
            // Real drawing
            //

            //----------------------------------------------------------------
            // Opaque

            region.GraphicsDevice.BlendState = BlendState.Opaque;
            region.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            foreach (var chunk in opaqueChunks)
            {
                if (chunk.Mesh.Opaque.Occluded)
                {
                    DebugIncrementRegionMonitorOccludedChunkCount();
                    continue;
                }

                Matrix world;
                chunk.CreateWorldMatrix(out world);

                region.ChunkEffect.World = world;
                
                pass.Apply();

                chunk.Mesh.Opaque.Draw();

                DebugRegionMonitorAddChunkVertexCount(chunk.Mesh.Opaque);
            }

            //----------------------------------------------------------------
            // Translucent

            //foreach (var chunk in translucentChunks)
            //    DrawChunkMeshPart(chunk.ActiveMesh.Translucent);

            //================================================================
            //
            // Debug
            //

            DebugDrawChunkBoundingBoxes(view, projection);

            DebugUpdateRegionMonitorVisibleChunkCounts();

            //================================================================
            //
            // Exit
            //

            opaqueChunks.Clear();
            translucentChunks.Clear();

            foreach (var chunk in workingChunks) chunk.ExitDraw();
            workingChunks.Clear();
        }

        // 非同期呼び出し。
        public Chunk ActivateChunk(ref VectorI3 position)
        {
            var chunk = chunkPool.Borrow();
            if (chunk == null) return null;

            Debug.Assert(!chunk.Active);

            if (!chunkStore.GetChunk(ref position, chunk))
            {
                chunk.Position = position;

                foreach (var procedure in region.ChunkProcesures)
                    procedure.Generate(chunk);
            }

            chunk.OnActivated();

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

            // 定義に変更があるならば永続化領域を更新。
            if (chunk.DefinitionDirty) chunkStore.AddChunk(chunk);

            chunk.OnPassivated();
            chunk.ExitPassivate();

            ReturnChunk(chunk);

            return true;
        }

        public void Close()
        {
            if (closing || closed) return;

            closing = true;
        }

        public void GetNearbyActiveChunks(ref VectorI3 position, out NearbyChunks nearbyChunks)
        {
            nearbyChunks = new NearbyChunks();

            foreach (var side in CubicSide.Items)
            {
                var nearbyPosition = position + side.Direction;

                Chunk nearbyChunk;
                lock (activeChunks)
                    activeChunks.TryGetItem(ref nearbyPosition, out nearbyChunk);

                nearbyChunks[side] = nearbyChunk;
            }
        }

        Chunk CreateChunk()
        {
            return new Chunk(chunkSize);
        }

        ChunkMesh CreateChunkMesh()
        {
            return new ChunkMesh(region.GraphicsDevice);
        }

        InterChunkMesh CreateInterChunkMesh()
        {
            return new InterChunkMesh();
        }

        VertexBuffer CreateVertexBuffer()
        {
            return new VertexBuffer(region.GraphicsDevice, typeof(VertexPositionNormalTexture), defaultVertexCapacity, BufferUsage.WriteOnly);
        }

        IndexBuffer CreateIndexBuffer()
        {
            return new IndexBuffer(region.GraphicsDevice, IndexElementSize.SixteenBits, defaultIndexCapacity, BufferUsage.WriteOnly);
        }

        void ReturnChunk(Chunk chunk)
        {
            chunk.Clear();

            ReturnChunkMesh(chunk);
            ReturnInterChunkMesh(chunk);

            chunkPool.Return(chunk);
        }

        void BorrowBuffer(ChunkMeshPart meshPart)
        {
            if (meshPart.VertexBuffer == null)
                meshPart.VertexBuffer = vertexBufferPool.Borrow();
            if (meshPart.IndexBuffer == null)
                meshPart.IndexBuffer = indexBufferPool.Borrow();
        }

        void ReturnBuffer(ChunkMeshPart meshPart)
        {
            if (meshPart.VertexBuffer != null)
            {
                vertexBufferPool.Return(meshPart.VertexBuffer);
                meshPart.VertexBuffer = null;
                meshPart.VertexCount = 0;
            }
            if (meshPart.IndexBuffer != null)
            {
                indexBufferPool.Return(meshPart.IndexBuffer);
                meshPart.IndexBuffer = null;
                meshPart.IndexCount = 0;
            }
        }

        bool BorrowChunkMesh(Chunk chunk)
        {
            if (chunk.Mesh == null)
                chunk.Mesh = chunkMeshPool.Borrow();

            return chunk.Mesh != null;
        }

        void ReturnChunkMesh(Chunk chunk)
        {
            if (chunk.Mesh != null)
            {
                ReturnChunkMesh(chunk.Mesh);
                chunk.Mesh = null;
            }
        }

        void ReturnChunkMesh(ChunkMesh chunkMesh)
        {
            ReturnBuffer(chunkMesh.Opaque);
            ReturnBuffer(chunkMesh.Translucent);

            chunkMeshPool.Return(chunkMesh);
        }

        bool BorrowInterChunkMesh(Chunk chunk)
        {
            if (chunk.InterMesh == null)
                chunk.InterMesh = interChunkMeshPool.Borrow();

            return chunk.InterMesh != null;
        }

        void ReturnInterChunkMesh(Chunk chunk)
        {
            if (chunk.InterMesh != null)
            {
                ReturnInterChunkMesh(chunk.InterMesh);
                chunk.InterMesh = null;
            }
        }

        void ReturnInterChunkMesh(InterChunkMesh interChunkMesh)
        {
            interChunkMesh.Opaque.Clear();
            interChunkMesh.Translucent.Clear();
            interChunkMeshPool.Return(interChunkMesh);
        }

        void UpdateChunkMeshBuffer(Chunk chunk)
        {
            var activeMesh = chunk.Mesh;
            var pendingMesh = chunk.InterMesh;

            UpdateChunkMeshPartBuffer(pendingMesh.Opaque, activeMesh.Opaque);
            UpdateChunkMeshPartBuffer(pendingMesh.Translucent, activeMesh.Translucent);
        }

        void UpdateChunkMeshPartBuffer(InterChunkMeshPart source, ChunkMeshPart destination)
        {
            if (0 < source.VertexCount && 0 < source.IndexCount)
            {
                BorrowBuffer(destination);
                source.Populate(destination);
            }
            else
            {
                ReturnBuffer(destination);
            }
        }

        [Conditional("DEBUG")]
        void InitializeDebugTools()
        {
            debugEffect = new BasicEffect(region.GraphicsDevice);
            debugEffect.AmbientLightColor = Vector3.One;
            debugEffect.VertexColorEnabled = true;
            boundingBoxDrawer = new BoundingBoxDrawer(region.GraphicsDevice);
        }

        [Conditional("DEBUG")]
        void DebugUpdateRegionMonitor()
        {
            var m = region.Monitor;
            m.TotalChunkCount = chunkPool.TotalObjectCount;
            m.ActiveChunkCount = activeChunks.Count;
            m.TotalChunkMeshCount = chunkMeshPool.TotalObjectCount;
            m.PassiveChunkMeshCount = chunkMeshPool.Count;
            m.TotalInterChunkMeshCount = interChunkMeshPool.TotalObjectCount;
            m.PassiveInterChunkMeshCount = interChunkMeshPool.Count;
            m.TotalVertexBufferCount = vertexBufferPool.TotalObjectCount;
            m.PassiveVertexBufferCount = vertexBufferPool.Count;
        }

        [Conditional("DEBUG")]
        void DebugRegionMonitorAddChunkVertexCount(ChunkMeshPart meshPart)
        {
            region.Monitor.AddChunkVertexCount(meshPart.VertexCount);
            region.Monitor.AddChunkIndexCount(meshPart.IndexCount);
        }

        [Conditional("DEBUG")]
        void DebugSetEffectTechnique()
        {
            var chunkEffect = region.ChunkEffect;
            var realEffect = chunkEffect.BackingEffect;

            if (Wireframe)
            {
                realEffect.CurrentTechnique = chunkEffect.WireframeTequnique;
            }
            else
            {
                realEffect.CurrentTechnique = chunkEffect.DefaultTequnique;
            }
        }

        [Conditional("DEBUG")]
        void DebugIncrementRegionMonitorOccludedChunkCount()
        {
            region.Monitor.IncrementOccludedOpaqueChunkCount();
        }

        [Conditional("DEBUG")]
        void DebugUpdateRegionMonitorVisibleChunkCounts()
        {
            var m = region.Monitor;
            m.VisibleOpaqueChunkCount = opaqueChunks.Count;
            m.VisibleTranslucentChunkCount = translucentChunks.Count;
        }

        [Conditional("DEBUG")]
        void DebugDrawChunkBoundingBoxes(View view, Projection projection)
        {
            if (!ChunkBoundingBoxVisible) return;

            debugEffect.View = view.Matrix;
            debugEffect.Projection = projection.Matrix;

            foreach (var chunk in workingChunks)
            {
                if (!chunk.Drawing) continue;

                var box = chunk.BoundingBox;
                boundingBoxDrawer.Draw(ref box, debugEffect);
            }
        }
    }
}
