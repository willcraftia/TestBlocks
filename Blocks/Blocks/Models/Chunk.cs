#region Using

using System;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;
using Willcraftia.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class Chunk
    {
        VectorI3 size;

        VectorI3 position;

        Vector3 worldPosition;

        BoundingBox boundingBox;

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

        public VectorI3 Size
        {
            get { return size; }
        }

        public VectorI3 Position
        {
            get { return position; }
            set
            {
                position = value;

                worldPosition.X = position.X * size.X;
                worldPosition.Y = position.Y * size.Y;
                worldPosition.Z = position.Z * size.Z;

                boundingBox.Min = worldPosition;
                boundingBox.Max = new Vector3
                {
                    X = worldPosition.X + size.X,
                    Y = worldPosition.Y + size.Y,
                    Z = worldPosition.Z + size.Z
                };
            }
        }

        public Vector3 WorldPosition
        {
            get { return worldPosition; }
        }

        public BoundingBox BoundingBox
        {
            get { return boundingBox; }
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

        public Chunk(VectorI3 size)
        {
            this.size = size;

            blockIndices = new byte[size.X * size.Y * size.Z];
        }

        public void OnActivated()
        {
            lock (activeLock) active = true;

            MeshDirty = true;
        }

        public void OnPassivated()
        {
            lock (activeLock) active = false;
        }

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

        public void ExitUpdate()
        {
            updating = false;
        }

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

        public void ExitDraw()
        {
            drawing = false;
        }

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

        public void ExitPassivate()
        {
            passivating = false;
        }

        public void OnNeighborActivated(CubicSide side)
        {
            lock (activeNeighborsLock)
            {
                activeNeighbors |= side.ToFlags();
            }
        }

        public void OnNeighborPassivated(CubicSide side)
        {
            lock (activeNeighborsLock)
            {
                var flag = side.ToFlags();

                if ((activeNeighbors & flag) == flag)
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

        public void Clear()
        {
            Array.Clear(blockIndices, 0, blockIndices.Length);
            
            activeNeighbors = CubicSide.Flags.None;
            neighborsReferencedOnUpdate = CubicSide.Flags.None;

            MeshDirty = true;
            DefinitionDirty = false;
        }

        public bool Contains(ref VectorI3 blockPosition)
        {
            return 0 <= blockPosition.X && blockPosition.X < size.X &&
                0 <= blockPosition.Y && blockPosition.Y < size.Y &&
                0 <= blockPosition.Z && blockPosition.Z < size.Z;
        }

        public bool IsInFrustum(BoundingFrustum frustum)
        {
            ContainmentType containmentType;
            frustum.Contains(ref boundingBox, out containmentType);

            return containmentType != ContainmentType.Disjoint;
        }
    }
}
