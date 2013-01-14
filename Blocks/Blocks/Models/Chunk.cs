#region Using

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;
using Willcraftia.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class Chunk
    {
        // チャンク サイズは頂点数に密接に関係するため定数値とする。
        public static VectorI3 Size
        {
            get { return new VectorI3(16); }
        }

        public static VectorI3 HalfSize
        {
            get { return new VectorI3(8); }
        }

        VectorI3 size = Size;

        VectorI3 position;

        Vector3 worldPosition;

        byte[] blockIndices;

        volatile bool active;

        volatile bool updating;

        volatile bool drawing;

        volatile bool passivating;

        volatile CubicSide.Flags activeNeighbors;

        volatile CubicSide.Flags neighborsReferencedOnUpdate;

        object activeLock = new object();

        object activeNeighborsLock = new object();

        ChunkMesh opaqueMesh;

        ChunkMesh translucentMesh;

        public Region Region { get; private set; }

        public VectorI3 Position
        {
            get { return position; }
            set
            {
                position = value;

                worldPosition.X = position.X * size.X;
                worldPosition.Y = position.Y * size.Y;
                worldPosition.Z = position.Z * size.Z;
            }
        }

        public Vector3 WorldPosition
        {
            get { return worldPosition; }
        }

        public byte this[int x, int y, int z]
        {
            get
            {
                if (x < 0 || size.X < x) throw new ArgumentOutOfRangeException("x");
                if (y < 0 || size.Y < y) throw new ArgumentOutOfRangeException("y");
                if (z < 0 || size.Z < z) throw new ArgumentOutOfRangeException("z");

                var index = x + y * size.X + z * size.X * size.Y;
                return blockIndices[index];
            }
            set
            {
                if (x < 0 || size.X < x) throw new ArgumentOutOfRangeException("x");
                if (y < 0 || size.Y < y) throw new ArgumentOutOfRangeException("y");
                if (z < 0 || size.Z < z) throw new ArgumentOutOfRangeException("z");

                var index = x + y * size.X + z * size.X * size.Y;
                blockIndices[index] = value;

                MeshDirty = true;
                DefinitionDirty = true;
            }
        }

        public int Count
        {
            get { return blockIndices.Length; }
        }

        public CubicSide.Flags ActiveNeighbors
        {
            get { return activeNeighbors; }
        }

        public CubicSide.Flags NeighborsReferencedOnUpdate
        {
            get { return neighborsReferencedOnUpdate; }
            set { neighborsReferencedOnUpdate = value; }
        }

        // 外部からブロックを設定した場合などに true とする。
        // true の場合は非アクティブ化でキャッシュを更新。
        // false の場合はキャッシュの更新が不要である。
        public bool DefinitionDirty { get; set; }

        public bool MeshDirty { get; set; }

        public ChunkMesh OpaqueMesh
        {
            get { return opaqueMesh; }
            set
            {
                if (opaqueMesh != null) opaqueMesh.Chunk = null;

                opaqueMesh = value;

                if (opaqueMesh != null) opaqueMesh.Chunk = this;
            }
        }

        public ChunkMesh TranslucentMesh
        {
            get { return translucentMesh; }
            set
            {
                if (translucentMesh != null) translucentMesh.Chunk = null;

                translucentMesh = value;

                if (translucentMesh != null) translucentMesh.Chunk = this;
            }
        }

        public InterChunk InterChunk { get; set; }

        public bool Active
        {
            get { return active; }
        }

        // Updating と Drawing は個別にフラグを持つ必要がある。
        // 例えば、Updating = true 中に Drawing = true となった場合、
        // 描画の終了では Drawing = false としたいだけであり、
        // Updating には関与したくない。
        // 逆に、更新中は Drawing には関与したくない。

        public bool Updating
        {
            get { return updating; }
        }

        public bool Drawing
        {
            get { return drawing; }
        }

        public Chunk()
        {
            blockIndices = new byte[size.X * size.Y * size.Z];
        }

        public void OnActivated(Region region)
        {
            lock (activeLock) active = true;

            // リージョンをバインド。
            Region = region;

            MeshDirty = true;
        }

        public void OnPassivated()
        {
            lock (activeLock) active = false;

            // この後、チャンクはプールへ戻されるため、チャンクの状態を初期化しておく。
            Array.Clear(blockIndices, 0, blockIndices.Length);

            activeNeighbors = CubicSide.Flags.None;
            neighborsReferencedOnUpdate = CubicSide.Flags.None;

            // リージョンをアンバインド。
            Region = null;

            MeshDirty = true;
            DefinitionDirty = false;
        }

        /// <summary>
        /// 更新ロックの取得を試行します。
        /// 他のスレッドがロック中の場合、非アクティブな場合、非アクティブ化中の場合は、
        /// 更新ロックの取得に失敗します。
        /// 更新ロックの取得に成功した場合には、
        /// 必ず ExitUpdate メソッドでロックを解放しなければなりません。
        /// </summary>
        /// <returns>
        /// true (更新ロックを取得できた場合)、false (それ以外の場合)。
        /// </returns>
        public bool EnterUpdate()
        {
            if (!Monitor.TryEnter(activeLock)) return false;

            try
            {
                if (!active) return false;
                if (passivating) return false;

                updating = true;
                return true;
            }
            finally
            {
                Monitor.Exit(activeLock);
            }
        }

        /// <summary>
        /// EnterUpdate メソッドで取得した更新ロックを開放します。
        /// </summary>
        public void ExitUpdate()
        {
            updating = false;
        }

        /// <summary>
        /// 描画ロックの取得を試行します。
        /// 他のスレッドがロック中の場合、非アクティブな場合、非アクティブ化中の場合は、
        /// 描画ロックの取得に失敗します。
        /// 描画ロックの取得に成功した場合には、
        /// 必ず ExitDraw メソッドでロックを解放しなければなりません。
        /// </summary>
        /// <returns>
        /// true (描画ロックを取得できた場合)、false (それ以外の場合)。
        /// </returns>
        public bool EnterDraw()
        {
            if (!Monitor.TryEnter(activeLock)) return false;

            try
            {
                if (!active) return false;
                if (passivating) return false;

                drawing = true;
                return true;
            }
            finally
            {
                Monitor.Exit(activeLock);
            }
        }

        /// <summary>
        /// EnterDraw メソッドで取得した描画ロックを開放します。
        /// </summary>
        public void ExitDraw()
        {
            drawing = false;
        }

        /// <summary>
        /// 非アクティブ化ロックの取得を試行します。
        /// 他のスレッドがロック中の場合、非アクティブな場合、更新ロック中の場合、描画ロック中の場合は、
        /// 非アクティブ化ロックの取得に失敗します。
        /// 非アクティブ化ロックの取得に成功した場合には、
        /// 必ず ExitPassivate メソッドでロックを解放しなければなりません。
        /// </summary>
        /// <returns>
        /// true (非アクティブ化ロックを取得できた場合)、false (それ以外の場合)。
        /// </returns>
        public bool EnterPassivate()
        {
            if (!Monitor.TryEnter(activeLock)) return false;

            try
            {
                if (!active) return false;
                if (updating || drawing) return false;

                passivating = true;
                return true;
            }
            finally
            {
                Monitor.Exit(activeLock);
            }
        }

        /// <summary>
        /// EnterPassivate メソッドで取得した描画ロックを開放します。
        /// </summary>
        public void ExitPassivate()
        {
            passivating = false;
        }

        /// <summary>
        /// 隣接チャンクのアクティブ化が完了した場合に呼び出されます。
        /// </summary>
        /// <param name="side">アクティブ化が完了した隣接チャンクの方向。</param>
        public void OnNeighborActivated(CubicSide side)
        {
            // パーティションがパーティション マネージャで完全に非アクティブになる前に
            // チャンクはチャンク マネージャで非アクティブになる。
            // このため、非アクティブな場合、パーティションからの通知を無視しなければならない。
            if (!active) return;

            lock (activeNeighborsLock)
            {
                activeNeighbors |= side.ToFlags();
            }
        }

        /// <summary>
        /// 隣接チャンクが非アクティブ化が完了した場合に呼び出されます。
        /// </summary>
        /// <param name="side">非アクティブ化が完了した隣接チャンクの方向。</param>
        public void OnNeighborPassivated(CubicSide side)
        {
            // パーティションがパーティション マネージャで完全に非アクティブになる前に
            // チャンクはチャンク マネージャで非アクティブになる。
            // このため、非アクティブな場合、パーティションからの通知を無視しなければならない。
            if (!active) return;

            lock (activeNeighborsLock)
            {
                var flag = side.ToFlags();

                Debug.Assert((activeNeighbors & flag) == flag);

                activeNeighbors ^= flag;
            }
        }

        public void CreateWorldMatrix(out Matrix result)
        {
            Matrix.CreateTranslation(ref worldPosition, out result);
        }

        public int CalculateBlockPositionX(int x)
        {
            return position.X * size.X + x;
        }

        public int CalculateBlockPositionY(int y)
        {
            return position.Y * size.Y + y;
        }

        public int CalculateBlockPositionZ(int z)
        {
            return position.Z * size.Z + z;
        }

        public void Read(BinaryReader reader)
        {
            var p = new VectorI3();

            p.X = reader.ReadInt32();
            p.Y = reader.ReadInt32();
            p.Z = reader.ReadInt32();

            // BoundingBox/WorldPosition を同時に更新しなければならない。
            Position = p;

            for (int i = 0; i < blockIndices.Length; i++)
                blockIndices[i] = reader.ReadByte();

            MeshDirty = true;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(position.X);
            writer.Write(position.Y);
            writer.Write(position.Z);

            for (int i = 0; i < blockIndices.Length; i++)
                writer.Write(blockIndices[i]);
        }

        public bool Contains(ref VectorI3 blockPosition)
        {
            return 0 <= blockPosition.X && blockPosition.X < size.X &&
                0 <= blockPosition.Y && blockPosition.Y < size.Y &&
                0 <= blockPosition.Z && blockPosition.Z < size.Z;
        }
    }
}
