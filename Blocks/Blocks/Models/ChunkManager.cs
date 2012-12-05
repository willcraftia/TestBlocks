#region Using

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Framework.Threading;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class ChunkManager
    {
        Region region;

        IChunkStore chunkStore;

        // TODO
        // 初期容量の調整。
        //

        ChunkCollection activeChunks = new ChunkCollection();

        List<Chunk> workingChunks = new List<Chunk>();

        Pool<Chunk> chunkPool;

        Pool<ChunkMesh> chunkMeshPool;

        ChunkMeshUpdateManager chunkMeshUpdateManager;

        public ChunkManager(Region region, IChunkStore chunkStore)
        {
            if (region == null) throw new ArgumentNullException("region");
            if (chunkStore == null) throw new ArgumentNullException("chunkStore");

            this.region = region;
            this.chunkStore = chunkStore;

            chunkPool = new Pool<Chunk>(() => { return new Chunk(region.ChunkSize); });
            chunkMeshPool = new Pool<ChunkMesh>(() => { return new ChunkMesh(); });
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
                    workingChunks[i] = activeChunks[i];
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
                    oldMesh.Clear();
                    chunkMeshPool.Return(oldMesh);

                    // PendingMesh を新しい ActiveMesh として設定する。
                    chunk.ActiveMesh = chunk.PendingMesh;
                    chunk.PendingMesh = null;

                    // TODO
                    // vertexbuffer への反映


                    // Dirty フラグを倒す。
                    chunk.Dirty = false;
                }
            }

            workingChunks.Clear();

            // 更新処理を実行。
            chunkMeshUpdateManager.Update();
        }

        // 非同期呼び出し。
        public void ActivateChunk(ref VectorI3 position)
        {
            var chunk = chunkPool.Borrow();
            if (chunk == null) throw new InvalidOperationException("Any new chunk can not be created.");

            if (!chunkStore.GetChunk(ref position, chunk))
            {
                chunk.Position = position;

                foreach (var generator in region.ChunkGenerators)
                    generator.Build(chunk);
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

            lock (chunk)
            {
                chunkStore.AddChunk(chunk);

                // Deregister
                lock (activeChunks)
                {
                    activeChunks.Remove(chunk);
                }

                chunk.Clear();
            }
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
            var chunkSize = region.ChunkSize;
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
            return targetChunk[relativeX, relativeY, relativeZ];
        }
    }
}
