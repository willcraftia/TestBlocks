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

        static readonly Logger logger = new Logger(typeof(ChunkManager).Name);

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

        List<Chunk> workingChunks = new List<Chunk>();

        // Chunk 数は Partition 数に対応するため、
        // Chunk Pool のサイズを ChunkManager で独自に設定することはできず、
        // Partition.Activate から連なる ActiveChunk の呼び出しでは、
        // 必ず Chunk を提供できなければならない。

        ConcurrentPool<Chunk> chunkPool;

        //
        // Chunk.ActiveMesh と Chunk.PendingMesh の切替があるため、
        // ChunkMesh 総数は Chunk 数の二倍まで到達しうる。
        // そして、Chunk 総数をここで決定できないことから、
        // 同様に ChunkMesh 総数をここで決定することもできない。
        //

        ConcurrentPool<ChunkMesh> chunkMeshPool;

        Pool<InterChunkMesh> interChunkMeshPool;

        Pool<DynamicVertexBuffer> vertexBufferPool;

        Pool<DynamicIndexBuffer> indexBufferPool;

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
            vertexBufferPool = new Pool<DynamicVertexBuffer>(CreateVertexBuffer);
            indexBufferPool = new Pool<DynamicIndexBuffer>(CreateIndexBuffer);
            chunkMeshUpdateManager = new ChunkMeshUpdateManager(region, this);
            chunkDistanceComparer = new ChunkDistanceComparer(chunkSize);

            InitializeDebugTools();
        }

        [Conditional("DEBUG")]
        void InitializeDebugTools()
        {
            debugEffect = new BasicEffect(region.GraphicsDevice);
            debugEffect.AmbientLightColor = Vector3.One;
            debugEffect.VertexColorEnabled = true;
            boundingBoxDrawer = new BoundingBoxDrawer(region.GraphicsDevice);
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

        DynamicVertexBuffer CreateVertexBuffer()
        {
            return new DynamicVertexBuffer(region.GraphicsDevice, typeof(VertexPositionNormalTexture), defaultVertexCapacity, BufferUsage.WriteOnly);
        }

        DynamicIndexBuffer CreateIndexBuffer()
        {
            return new DynamicIndexBuffer(region.GraphicsDevice, IndexElementSize.SixteenBits, defaultIndexCapacity, BufferUsage.WriteOnly);
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
            }
            if (meshPart.IndexBuffer != null)
            {
                indexBufferPool.Return(meshPart.IndexBuffer);
                meshPart.IndexBuffer = null;
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
                source.Populate(destination.VertexBuffer, destination.IndexBuffer);
            }
            else
            {
                ReturnBuffer(destination);
            }
        }

        public void Update()
        {
            // 長時間のロックを避けるために、一時的に作業リストへコピー。
            lock (activeChunks)
            {
                // 複製ループに入る前に十分な容量を保証。
                workingChunks.Capacity = activeChunks.Count;

                for (int i = 0; i < activeChunks.Count; i++)
                    workingChunks.Add(activeChunks[i]);
            }

            foreach (var chunk in workingChunks)
            {
                lock (chunk)
                {
                    // 処理中に非アクティブになっているならスキップ。
                    if (!chunk.Active) continue;

                    chunk.Updating = true;
                }

                if (!chunk.Dirty)
                {
                    // Dirty ではない Chunk は更新しない。
                    chunk.Updating = false;
                    continue;
                }

                if (chunk.InterMesh == null)
                {
                    // InterMesh 未設定ならば設定を試行。
                    if (BorrowInterChunkMesh(chunk))
                    {
                        // 非同期な更新要求を追加。
                        chunk.InterMesh.Completed = false;
                        chunkMeshUpdateManager.EnqueueChunk(chunk);
                    }
                    else
                    {
                        // InterMesh の更新を次回の試行とする。
                        chunk.Updating = false;
                    }
                }
                else if (chunk.InterMesh.Completed)
                {
                    // InterMesh が更新完了ならば Mesh を更新。

                    if (!BorrowChunkMesh(chunk)) continue;

                    // VertexBuffer/IndexBuffer への反映。
                    UpdateChunkMeshBuffer(chunk);

                    // InterMesh をプールへ戻す。
                    ReturnInterChunkMesh(chunk.InterMesh);
                    chunk.InterMesh = null;

                    // 更新終了としてマークする。
                    chunk.Dirty = false;
                    chunk.Updating = false;
                }
            }

            workingChunks.Clear();

            // 更新処理を実行。
            chunkMeshUpdateManager.Update();
        }

        static readonly BlendState colorWriteDisable = new BlendState
        {
            ColorWriteChannels = ColorWriteChannels.None
        };

        public void Draw(View view, Projection projection)
        {
            // 長時間のロックを避けるために、一時的に作業リストへコピー。
            lock (activeChunks)
            {
                // 複製ループに入る前に十分な容量を保証。
                workingChunks.Capacity = activeChunks.Count;

                for (int i = 0; i < activeChunks.Count; i++)
                    workingChunks.Add(activeChunks[i]);
            }

            View.GetEyePosition(ref view.Matrix, out eyePosition);
            frustum.Matrix = view.Matrix * projection.Matrix;

            for (int i = 0; i < workingChunks.Count; i++)
            {
                var chunk = workingChunks[i];

                lock (chunk)
                {
                    // 処理中に非アクティブになっているならスキップ。
                    if (!chunk.Active) continue;

                    chunk.Drawing = true;
                }
                
                var mesh = chunk.Mesh;
                if (mesh == null)
                {
                    // 一度も構築処理が行われていないならばスキップ。
                    chunk.Drawing = false;
                    continue;
                }
                
                // フラスタム カリング。
                if (!IsInViewFrustum(chunk))
                {
                    // カリングされた Chunk はスキップ。
                    chunk.Drawing = false;
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

            region.GraphicsDevice.BlendState = BlendState.Opaque;
            region.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            foreach (var chunk in opaqueChunks)
            {
                Matrix world;
                chunk.CreateWorldMatrix(out world);

                region.ChunkEffect.World = world;
                
                pass.Apply();

                chunk.Mesh.Opaque.Draw();
            }

            //foreach (var chunk in translucentChunks)
            //    DrawChunkMeshPart(chunk.ActiveMesh.Translucent);

            DrawChunkBoundingBoxes(view, projection);

            opaqueChunks.Clear();
            translucentChunks.Clear();

            // 描画終了としてマーク。
            foreach (var chunk in workingChunks)
                chunk.Drawing = false;

            workingChunks.Clear();
        }

        [Conditional("DEBUG")]
        void DrawChunkBoundingBoxes(View view, Projection projection)
        {
            if (!region.ChunkBoundingBoxVisible) return;

            debugEffect.View = view.Matrix;
            debugEffect.Projection = projection.Matrix;

            foreach (var chunk in workingChunks)
            {
                if (!chunk.Drawing) continue;

                var box = chunk.BoundingBox;
                boundingBoxDrawer.Draw(ref box, debugEffect);
            }
        }

        // 非同期呼び出し。
        public Chunk GetChunk(ref VectorI3 position)
        {
            lock (activeChunks)
            {
                return activeChunks[position];
            }
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

            // Register
            lock (activeChunks)
            {
                chunk.Dirty = true;
                chunk.Active = true;
                activeChunks.Add(chunk);
            }

            return chunk;
        }

        // 非同期呼び出し。
        public bool PassivateChunk(Chunk chunk)
        {
            if (chunk == null) throw new ArgumentNullException("chunk");

            Debug.Assert(chunk.Active);

            lock (chunk)
            {
                // 更新中あるいは描画中ならばパッシベーション失敗。
                if (chunk.Updating || chunk.Drawing) return false;
            }

            // Deregister
            lock (activeChunks)
            {
                chunk.Active = false;
                activeChunks.Remove(chunk);
            }

            // 永続化。
            chunkStore.AddChunk(chunk);

            // プールへ戻す。
            ReturnChunk(chunk);

            return true;
        }

        public bool TryGetActiveChunk(ref VectorI3 position, out Chunk chunk)
        {
            lock (activeChunks)
            {
                return activeChunks.TryGetItem(ref position, out chunk);
            }
        }

        public byte FindActiveBlockIndex(Chunk baseChunk, ref VectorI3 position)
        {
            // position が baseChunk に含まれる場合。
            if (baseChunk.Contains(ref position))
                return baseChunk[position.X, position.Y, position.Z];

            // position が baseChunk に含まれない場合は、それを含むアクティブな Chunk を探す。
            var chunkPosition = baseChunk.Position;
            chunkPosition.X += MathExtension.Floor(position.X * inverseChunkSize.X);
            chunkPosition.Y += MathExtension.Floor(position.Y * inverseChunkSize.Y);
            chunkPosition.Z += MathExtension.Floor(position.Z * inverseChunkSize.Z);

            Chunk targetChunk;
            if (!TryGetActiveChunk(ref chunkPosition, out targetChunk))
                return Block.EmptyIndex;

            var relativeX = position.X % chunkSize.X;
            var relativeY = position.Y % chunkSize.Y;
            var relativeZ = position.Z % chunkSize.Z;
            if (relativeX < 0) relativeX += chunkSize.X;
            if (relativeY < 0) relativeY += chunkSize.Y;
            if (relativeZ < 0) relativeZ += chunkSize.Z;

            lock (targetChunk)
            {
                // 処理中に非アクティブになっているなら空と判定。
                if (!targetChunk.Active) return Block.EmptyIndex;

                return targetChunk[relativeX, relativeY, relativeZ];
            }
        }

        bool IsInViewFrustum(Chunk chunk)
        {
            var box = chunk.BoundingBox;

            ContainmentType containmentType;
            frustum.Contains(ref box, out containmentType);

            return containmentType != ContainmentType.Disjoint;
        }
    }
}
