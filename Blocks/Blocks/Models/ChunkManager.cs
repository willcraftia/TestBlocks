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

        ConcurrentPool<InterChunkMeshPart> interChunkMeshPartPool;

        Pool<DynamicVertexBuffer> vertexBufferPool;

        Pool<DynamicIndexBuffer> indexBufferPool;

        ChunkMeshUpdateManager chunkMeshUpdateManager;

        BoundingFrustum frustum = new BoundingFrustum(Matrix.Identity);

        Vector3 eyePosition;

        ChunkDistanceComparer chunkDistanceComparer;

        List<Chunk> opaqueChunks = new List<Chunk>();

        List<Chunk> translucentChunks = new List<Chunk>();

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
            interChunkMeshPartPool = new ConcurrentPool<InterChunkMeshPart>(CreateInterChunkMeshPart)
            {
                MaxCapacity = 20
            };
            vertexBufferPool = new Pool<DynamicVertexBuffer>(CreateVertexBuffer);
            indexBufferPool = new Pool<DynamicIndexBuffer>(CreateIndexBuffer);
            chunkMeshUpdateManager = new ChunkMeshUpdateManager(region, this);
            chunkDistanceComparer = new ChunkDistanceComparer(chunkSize);
        }

        Chunk CreateChunk()
        {
            return new Chunk(chunkSize);
        }

        ChunkMesh CreateChunkMesh()
        {
            return new ChunkMesh(region.GraphicsDevice);
        }

        InterChunkMeshPart CreateInterChunkMeshPart()
        {
            return new InterChunkMeshPart();
        }

        DynamicVertexBuffer CreateVertexBuffer()
        {
            return new DynamicVertexBuffer(region.GraphicsDevice, typeof(VertexPositionNormalTexture), defaultVertexCapacity, BufferUsage.WriteOnly);
        }

        DynamicIndexBuffer CreateIndexBuffer()
        {
            return new DynamicIndexBuffer(region.GraphicsDevice, IndexElementSize.SixteenBits, defaultIndexCapacity, BufferUsage.WriteOnly);
        }

        //
        // Chunk にブロックを設定する箇所でもロックが必要なんじゃなかろうか。
        // サイズ変更は発生しないので、配列要素の byte ではプリミティブ型だからロック不要になるかな？
        //

        ChunkMesh CreatePendingMesh()
        {
            var chunkMesh = chunkMeshPool.Borrow();
            if (chunkMesh == null) return null;

            var interOpaquePart = interChunkMeshPartPool.Borrow();
            if (interOpaquePart == null)
            {
                chunkMeshPool.Return(chunkMesh);
                return null;
            }

            var interTranslucentPart = interChunkMeshPartPool.Borrow();
            if (interOpaquePart == null)
            {
                chunkMeshPool.Return(chunkMesh);
                interChunkMeshPartPool.Return(interOpaquePart);
                return null;
            }

            chunkMesh.Opaque.InterChunkMeshPart = interOpaquePart;
            chunkMesh.Translucent.InterChunkMeshPart = interTranslucentPart;

            return chunkMesh;
        }

        void ReturnActiveMesh(ChunkMesh chunkMesh)
        {
            if (chunkMesh.Opaque.VertexBuffer != null)
            {
                vertexBufferPool.Return(chunkMesh.Opaque.VertexBuffer);
                chunkMesh.Opaque.VertexBuffer = null;
            }
            if (chunkMesh.Opaque.IndexBuffer != null)
            {
                indexBufferPool.Return(chunkMesh.Opaque.IndexBuffer);
                chunkMesh.Opaque.IndexBuffer = null;
            }

            if (chunkMesh.Translucent.VertexBuffer != null)
            {
                vertexBufferPool.Return(chunkMesh.Translucent.VertexBuffer);
                chunkMesh.Translucent.VertexBuffer = null;
            }
            if (chunkMesh.Translucent.IndexBuffer != null)
            {
                indexBufferPool.Return(chunkMesh.Translucent.IndexBuffer);
                chunkMesh.Translucent.IndexBuffer = null;
            }

            chunkMesh.Clear();
            chunkMeshPool.Return(chunkMesh);
        }

        void BuildActiveMeshBuffers(ChunkMesh chunkMesh)
        {
            if (0 < chunkMesh.Opaque.InterChunkMeshPart.VertexCount &&
                0 < chunkMesh.Opaque.InterChunkMeshPart.IndexCount)
            {
                chunkMesh.Opaque.VertexBuffer = vertexBufferPool.Borrow();
                chunkMesh.Opaque.IndexBuffer = indexBufferPool.Borrow();
                chunkMesh.Opaque.BuildBuffer();

            }

            if (0 < chunkMesh.Translucent.InterChunkMeshPart.VertexCount &&
                0 < chunkMesh.Translucent.InterChunkMeshPart.IndexCount)
            {
                chunkMesh.Translucent.VertexBuffer = vertexBufferPool.Borrow();
                chunkMesh.Translucent.IndexBuffer = indexBufferPool.Borrow();
                chunkMesh.Translucent.BuildBuffer();
            }
        }

        void ReturnInterChunkMeshParts(ChunkMesh chunkMesh)
        {
            chunkMesh.Opaque.InterChunkMeshPart.Clear();
            interChunkMeshPartPool.Return(chunkMesh.Opaque.InterChunkMeshPart);
            chunkMesh.Opaque.InterChunkMeshPart = null;

            chunkMesh.Translucent.InterChunkMeshPart.Clear();
            interChunkMeshPartPool.Return(chunkMesh.Translucent.InterChunkMeshPart);
            chunkMesh.Translucent.InterChunkMeshPart = null;
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

                if (chunk.PendingMesh == null)
                {
                    // PendingMesh 未設定ならば、更新要求を追加。
                    chunk.PendingMesh = CreatePendingMesh();
                    if (chunk.PendingMesh != null)
                    {
                        // 非同期な更新要求を登録。
                        chunkMeshUpdateManager.EnqueueChunk(chunk);
                    }
                    else
                    {
                        // PendingMesh の更新を次回の試行とする。
                        chunk.Updating = false;
                    }
                }
                else if (chunk.PendingMesh.Loaded)
                {
                    // PendingMesh がロード完了ならば ActiveMesh を更新。

                    // 古い ActiveMesh をプールへ戻す。
                    if (chunk.ActiveMesh != null)
                        ReturnActiveMesh(chunk.ActiveMesh);

                    // PendingMesh を新しい ActiveMesh として設定する。
                    chunk.ActiveMesh = chunk.PendingMesh;
                    chunk.PendingMesh = null;

                    // VertexBuffer/IndexBuffer への反映
                    BuildActiveMeshBuffers(chunk.ActiveMesh);

                    // 全ての InterChunkMeshPart をプールへ戻す。
                    ReturnInterChunkMeshParts(chunk.ActiveMesh);

                    // 更新終了としてマークする。
                    chunk.Dirty = false;
                    chunk.Updating = false;
                }
            }

            workingChunks.Clear();

            // 更新処理を実行。
            chunkMeshUpdateManager.Update();
        }

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
                
                var activeMesh = chunk.ActiveMesh;
                if (activeMesh == null)
                {
                    // まだ ActiveMesh の構築が完了していないならばスキップ。
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

                if (activeMesh.Opaque.VertexCount != 0)
                    opaqueChunks.Add(chunk);

                if (activeMesh.Translucent.VertexCount != 0)
                    translucentChunks.Add(chunk);
            }

            opaqueChunks.Sort(chunkDistanceComparer);
            translucentChunks.Sort(chunkDistanceComparer);

            var pass = region.ChunkEffect.BackingEffect.CurrentTechnique.Passes[0];

            foreach (var chunk in opaqueChunks)
            {
                Matrix world;
                chunk.CreateWorldMatrix(out world);

                region.ChunkEffect.World = world;
                
                pass.Apply();

                chunk.ActiveMesh.Opaque.Draw();
            }

            //foreach (var chunk in translucentChunks)
            //    DrawChunkMeshPart(chunk.ActiveMesh.Translucent);

            opaqueChunks.Clear();
            translucentChunks.Clear();

            // 描画終了としてマーク。
            foreach (var chunk in workingChunks)
                chunk.Drawing = false;

            workingChunks.Clear();
        }

        // 非同期呼び出し。
        public void ActivateChunk(ref VectorI3 position)
        {
            var chunk = chunkPool.Borrow();
            if (chunk == null) throw new InvalidOperationException("No pooled chunk exists.");

            Debug.Assert(!chunk.Active);

            if (!chunkStore.GetChunk(ref position, chunk))
            {
                logger.Info("Generate: {0}", position);

                chunk.Position = position;

                foreach (var procedure in region.ChunkProcesures)
                    procedure.Generate(chunk);
            }

            // Register
            lock (activeChunks)
            {
                chunk.Active = true;
                activeChunks.Add(chunk);
            }
        }

        // 非同期呼び出し。
        public bool PassivateChunk(ref VectorI3 position)
        {
            Chunk chunk;
            if (!TryGetActiveChunk(ref position, out chunk)) return false;

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

            chunkStore.AddChunk(chunk);

            chunk.Clear();

            if (chunk.ActiveMesh != null)
            {
                chunkMeshPool.Return(chunk.ActiveMesh);
                chunk.ActiveMesh = null;
            }

            if (chunk.PendingMesh != null)
            {
                chunkMeshPool.Return(chunk.PendingMesh);
                chunk.PendingMesh = null;
            }

            chunkPool.Return(chunk);
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
