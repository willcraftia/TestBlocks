﻿#region Using

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Collections;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Framework.Threading;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class ChunkManager
    {
        Region region;

        IChunkStore chunkStore;

        VectorI3 chunkSize;

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

        ChunkMeshUpdateManager chunkMeshUpdateManager;

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

            chunkPool = new ConcurrentPool<Chunk>(() => { return new Chunk(chunkSize); });
            chunkMeshPool = new ConcurrentPool<ChunkMesh>(() => { return new ChunkMesh(region.GraphicsDevice); });
            chunkMeshUpdateManager = new ChunkMeshUpdateManager(region, this);
        }

        //
        // Chunk にブロックを設定する箇所でもロックが必要なんじゃなかろうか。
        // サイズ変更は発生しないので、配列要素の byte ではプリミティブ型だからロック不要になるかな？
        //

        public void Update()
        {
            // 長時間のロックを避けるために、一度、リストへコピーする。
            lock (activeChunks)
            {
                // 複製ループに入る前に、十分な容量を先に保証しておく。
                workingChunks.Capacity = activeChunks.Count;

                for (int i = 0; i < activeChunks.Count; i++)
                    workingChunks.Add(activeChunks[i]);
            }

            foreach (var chunk in workingChunks)
            {
                if (!chunk.Dirty) continue;

                if (chunk.PendingMesh == null)
                {
                    // PendingMesh 未設定ならば、更新要求を追加。

                    var chunkMesh = chunkMeshPool.Borrow();
                    if (chunkMesh == null) return;

                    chunk.PendingMesh = chunkMesh;

                    // 非同期な更新要求を登録。
                    chunkMeshUpdateManager.EnqueueChunk(chunk);
                }
                else if (chunk.PendingMesh.IsLoaded)
                {
                    // PendingMesh のロードが完了していれば、ActiveMesh を更新。

                    // 古い ActiveMesh をプールへ戻す。
                    var oldMesh = chunk.ActiveMesh;
                    if (oldMesh != null)
                    {
                        oldMesh.Clear();
                        chunkMeshPool.Return(oldMesh);
                    }

                    // PendingMesh を新しい ActiveMesh として設定する。
                    chunk.ActiveMesh = chunk.PendingMesh;
                    chunk.PendingMesh = null;

                    // VertexBuffer/IndexBuffer への反映
                    chunk.ActiveMesh.BuildBuffers();

                    // Dirty フラグを倒す。
                    chunk.Dirty = false;
                }
            }

            workingChunks.Clear();

            // 更新処理を実行。
            chunkMeshUpdateManager.Update();
        }

        public void Draw()
        {
        }

        // 非同期呼び出し。
        public void ActivateChunk(ref VectorI3 position)
        {
            var chunk = chunkPool.Borrow();
            if (chunk == null) throw new InvalidOperationException("No pooled chunk exists.");

            if (!chunkStore.GetChunk(ref position, chunk))
            {
                chunk.Position = position;

                foreach (var procedure in region.ChunkProcesures)
                    procedure.Generate(chunk);
            }

            // Register
            lock (activeChunks)
            {
                activeChunks.Add(chunk);
            }
        }

        // 非同期呼び出し。
        public void PassivateChunk(ref VectorI3 position)
        {
            Chunk chunk;
            if (!TryGetActiveChunk(ref position, out chunk)) return;

            chunkStore.AddChunk(chunk);

            // Deregister
            lock (activeChunks)
            {
                activeChunks.Remove(chunk);
            }

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
            chunkPosition.X += position.X / chunkSize.X;
            chunkPosition.Y += position.Y / chunkSize.Y;
            chunkPosition.Z += position.Z / chunkSize.Z;

            Chunk targetChunk;
            if (!TryGetActiveChunk(ref chunkPosition, out targetChunk))
                return Block.EmptyIndex;

            var relativeX = position.X % chunkSize.X;
            var relativeY = position.Y % chunkSize.Y;
            var relativeZ = position.Z % chunkSize.Z;
            if (relativeX < 0) relativeX += chunkSize.X;
            if (relativeY < 0) relativeY += chunkSize.Y;
            if (relativeZ < 0) relativeZ += chunkSize.Z;
            return targetChunk[relativeX, relativeY, relativeZ];
        }
    }
}
