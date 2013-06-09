#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Landscape;
using Willcraftia.Xna.Blocks.Models;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    /// <summary>
    /// 一定領域に含まれるブロックを一纏めで管理するクラスです。
    /// </summary>
    public sealed class Chunk : Partition
    {
        public const byte MaxSkylightLevel = 15;

        /// <summary>
        /// チャンク マネージャ。
        /// </summary>
        ChunkManager manager;

        /// <summary>
        /// チャンクが属するリージョン。
        /// </summary>
        Region region;

        /// <summary>
        /// チャンク データ。
        /// </summary>
        /// <remarks>
        /// 初めて空ブロック以外が設定される際には、
        /// チャンク マネージャでプーリングされているデータを借り、
        /// このフィールドへ設定します。
        /// 一方、全てが空ブロックになる際には、
        /// このフィールドに設定されていたデータをチャンク マネージャへ返却し、
        /// このフィールドには null を設定します。
        /// 
        /// この仕組により、空 (そら) を表すチャンクではメモリが節約され、
        /// また、null の場合にメッシュ更新を要求しないことで、無駄なメッシュ更新を回避できます。
        /// </remarks>
        ChunkData data;

        volatile ChunkLightState lightState;

        /// <summary>
        /// 不透明メッシュ配列。
        /// 添字はセグメント位置です。
        /// </summary>
        ChunkMesh[, ,] opaqueMeshes;

        /// <summary>
        /// 半透明メッシュ。
        /// 添字はセグメント位置です。
        /// </summary>
        ChunkMesh[, ,] translucentMeshes;

        /// <summary>
        /// チャンクのサイズを取得します。
        /// </summary>
        public VectorI3 Size
        {
            get { return manager.ChunkSize; }
        }

        /// <summary>
        /// チャンクが属するリージョンを取得します。
        /// </summary>
        public Region Region
        {
            get { return region; }
        }

        /// <summary>
        /// チャンクのシーン ノードを取得します。
        /// </summary>
        public SceneNode Node { get; private set; }

        /// <summary>
        /// 光レベルの構築状態を取得します。
        /// </summary>
        public ChunkLightState LightState
        {
            get { return lightState; }
            set { lightState = value; }
        }

        /// <summary>
        /// メッシュ更新のための頂点ビルダを取得または設定します。
        /// </summary>
        public ChunkVerticesBuilder VerticesBuilder { get; internal set; }

        /// <summary>
        /// 非空ブロックの総数を取得します。
        /// </summary>
        public int SolidCount
        {
            get
            {
                if (data == null) return 0;
                return data.SolidCount;
            }
        }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        /// <param name="manager">チャンク マネージャ。</param>
        public Chunk(ChunkManager manager)
        {
            if (manager == null) throw new ArgumentNullException("manager");

            this.manager = manager;

            opaqueMeshes = new ChunkMesh[manager.MeshSegments.X, manager.MeshSegments.Y, manager.MeshSegments.Z];
            translucentMeshes = new ChunkMesh[manager.MeshSegments.X, manager.MeshSegments.Y, manager.MeshSegments.Z];

            Node = manager.CreateNode();
        }

        /// <summary>
        /// 初期化します。
        /// </summary>
        /// <param name="position">チャンクの位置。</param>
        /// <param name="region">リージョン。</param>
        public void Initialize(VectorI3 position, Region region)
        {
            Position = position;
            this.region = region;

            ActivationCompleted = false;
            PassivationCompleted = false;
        }

        /// <summary>
        /// 開放します。
        /// </summary>
        public void Release()
        {
            Position = VectorI3.Zero;

            for (int z = 0; z < manager.MeshSegments.Z; z++)
            {
                for (int y = 0; y < manager.MeshSegments.Y; y++)
                {
                    for (int x = 0; x < manager.MeshSegments.X; x++)
                    {
                        if (opaqueMeshes[x, y, z] != null)
                        {
                            DetachMesh(opaqueMeshes[x, y, z]);
                            opaqueMeshes[x, y, z] = null;
                        }
                        if (translucentMeshes[x, y, z] != null)
                        {
                            DetachMesh(translucentMeshes[x, y, z]);
                            translucentMeshes[x, y, z] = null;
                        }
                    }
                }
            }

            if (VerticesBuilder != null)
            {
                manager.ReleaseVerticesBuilder(VerticesBuilder);
                VerticesBuilder = null;
            }

            if (data != null)
            {
                manager.ReturnData(data);
                data = null;
            }

            region = null;

            lightState = ChunkLightState.WaitBuildLocal;

            ActivationCompleted = false;
            PassivationCompleted = false;
        }

        /// <summary>
        /// 指定のチャンク サイズで起こりうる最大の頂点数を計算します。
        /// </summary>
        /// <param name="chunkSize">チャンク サイズ。</param>
        /// <returns>最大頂点数。</returns>
        public static int CalculateMaxVertexCount(VectorI3 chunkSize)
        {
            // ブロックが交互に配置されるチャンクで頂点数が最大であると考える。
            //
            //      16 * 16 * 16 で考えた場合は以下の通り。
            //
            //          各 Y について 8 * 8 = 64 ブロック
            //          全 Y で 64 * 16 = 1024 ブロック
            //          ブロックは 4 * 6 = 24 頂点
            //          計 1024 * 24 = 24576 頂点
            //          インデックスは 4 頂点に対して 6
            //          計 (24576 / 4) * 6 = 36864 インデックス
            //
            //      16 * 256 * 16 で考えた場合は以下の通り。
            //
            //          各 Y について 8 * 8 = 64 ブロック
            //          全 Y で 64 * 256 = 16384 ブロック
            //          ブロックは 4 * 6 = 24 頂点
            //          計 16384 * 24 = 393216 頂点
            //          インデックスは 4 頂点に対して 6
            //          計 (393216 / 4) * 6 = 589824 インデックス

            int x = chunkSize.X / 2;
            int z = chunkSize.Z / 2;
            int xz = x * z;
            int blocks = xz * chunkSize.Y;
            const int perBlock = 4 * 6;
            return blocks * perBlock;
        }

        /// <summary>
        /// 指定の頂点数に対して必要となるインデックス数を計算します。
        /// </summary>
        /// <param name="vertexCount">頂点数。</param>
        /// <returns>インデックス数。</returns>
        public static int CalculateIndexCount(int vertexCount)
        {
            return (vertexCount / 4) * 6;
        }

        public Chunk GetNeighborChunk(CubicSide side)
        {
            var neighborPosition = Position + side.Direction;
            return manager.GetChunk(ref neighborPosition);
        }

        public bool Contains(int x, int y, int z)
        {
            return 0 <= x && x < manager.ChunkSize.X &&
                0 <= y && y < manager.ChunkSize.Y &&
                0 <= z && z < manager.ChunkSize.Z;
        }

        public bool Contains(ref VectorI3 position)
        {
            return Contains(position.X, position.Y, position.Z);
        }

        public Block GetBlock(int x, int y, int z)
        {
            var blockIndex = GetBlockIndex(x, y, z);
            if (blockIndex == Block.EmptyIndex) return null;

            return region.BlockCatalog[blockIndex];
        }

        public Block GetBlock(ref VectorI3 position)
        {
            return GetBlock(position.X, position.Y, position.Z);
        }

        public byte GetBlockIndex(int x, int y, int z)
        {
            if (data == null) return Block.EmptyIndex;

            return data.GetBlockIndex(x, y, z);
        }

        public byte GetBlockIndex(ref VectorI3 position)
        {
            return GetBlockIndex(position.X, position.Y, position.Z);
        }

        public void SetBlockIndex(int x, int y, int z, byte blockIndex)
        {
            if (data == null)
            {
                // データが null で空ブロックを設定しようとする場合は、
                // データに変更がないため、即座に処理を終えます。
                if (blockIndex == Block.EmptyIndex) return;

                // 非空ブロックを設定しようとする場合は、
                // チャンク マネージャからデータを借りる必要があります。
                data = manager.BorrowData();
            }

            // 同じインデックスならば更新しない。
            if (data.GetBlockIndex(x, y, z) == blockIndex) return;

            data.SetBlockIndex(x, y, z, blockIndex);

            if (data.SolidCount == 0)
            {
                // 全てが空ブロックになったならば、
                // データをチャンク マネージャへ返します。
                manager.ReturnData(data);
                data = null;
            }
        }

        public void SetBlockIndex(ref VectorI3 position, byte blockIndex)
        {
            SetBlockIndex(position.X, position.Y, position.Z, blockIndex);
        }

        public byte GetSkylightLevel(int x, int y, int z)
        {
            if (data == null) return MaxSkylightLevel;

            return data.GetSkylightLevel(x, y, z);
        }

        public byte GetSkylightLevel(ref VectorI3 position)
        {
            return GetSkylightLevel(position.X, position.Y, position.Z);
        }

        public void SetSkylightLevel(int x, int y, int z, byte value)
        {
            // 完全空チャンクの場合、光量データは不要。
            if (data == null) return;

            data.SetSkylightLevel(x, y, z, value);
        }

        public void SetSkylightLevel(ref VectorI3 position, byte value)
        {
            SetSkylightLevel(position.X, position.Y, position.Z, value);
        }

        public void FillSkylightLevels(byte level)
        {
            // 空チャンクの場合は消去不能。
            if (data != null) data.FillSkylightLevels(level);
        }

        public void ClearSkylightLevels()
        {
            // 空チャンクの場合は消去不能。
            if (data != null) data.ClearSkylightLevels();
        }

        public int GetRelativeBlockPositionX(int absoluteBlockPositionX)
        {
            return absoluteBlockPositionX - (Position.X * manager.ChunkSize.X);
        }

        public int GetRelativeBlockPositionY(int absoluteBlockPositionY)
        {
            return absoluteBlockPositionY - (Position.Y * manager.ChunkSize.Y);
        }

        public int GetRelativeBlockPositionZ(int absoluteBlockPositionZ)
        {
            return absoluteBlockPositionZ - (Position.Z * manager.ChunkSize.Z);
        }

        public void GetRelativeBlockPosition(ref VectorI3 absoluteBlockPosition, out VectorI3 result)
        {
            result = new VectorI3
            {
                X = GetRelativeBlockPositionX(absoluteBlockPosition.X),
                Y = GetRelativeBlockPositionY(absoluteBlockPosition.Y),
                Z = GetRelativeBlockPositionZ(absoluteBlockPosition.Z)
            };
        }

        public int GetAbsoluteBlockPositionX(int relativeBlockPositionX)
        {
            return Position.X * manager.ChunkSize.X + relativeBlockPositionX;
        }

        public int GetAbsoluteBlockPositionY(int relativeBlockPositionY)
        {
            return Position.Y * manager.ChunkSize.Y + relativeBlockPositionY;
        }

        public int GetAbsoluteBlockPositionZ(int relativeBlockPositionZ)
        {
            return Position.Z * manager.ChunkSize.Z + relativeBlockPositionZ;
        }

        public void GetAbsoluteBlockPosition(ref VectorI3 relativeBlockPosition, out VectorI3 result)
        {
            result = new VectorI3
            {
                X = GetAbsoluteBlockPositionX(relativeBlockPosition.X),
                Y = GetAbsoluteBlockPositionY(relativeBlockPosition.Y),
                Z = GetAbsoluteBlockPositionZ(relativeBlockPosition.Z)
            };
        }

        public ChunkMesh GetOpaqueMesh(int segmentX, int segmentY, int segmentZ)
        {
            return opaqueMeshes[segmentX, segmentY, segmentZ];
        }

        public ChunkMesh GetTranslucentMesh(int segmentX, int segmentY, int segmentZ)
        {
            return translucentMeshes[segmentX, segmentY, segmentZ];
        }

        public void SetOpaqueMesh(int segmentX, int segmentY, int segmentZ, ChunkMesh mesh)
        {
            var oldMesh = opaqueMeshes[segmentX, segmentY, segmentZ];
            if (oldMesh != null)
                DetachMesh(oldMesh);

            opaqueMeshes[segmentX, segmentY, segmentZ] = mesh;

            if (mesh != null)
                AttachMesh(mesh);
        }

        public void SetTranslucentMesh(int segmentX, int segmentY, int segmentZ, ChunkMesh mesh)
        {
            var oldMesh = translucentMeshes[segmentX, segmentY, segmentZ];
            if (oldMesh != null)
                DetachMesh(oldMesh);

            translucentMeshes[segmentX, segmentY, segmentZ] = mesh;

            if (mesh != null)
                AttachMesh(mesh);
        }

        /// <summary>
        /// リージョンが提供するチャンク ストアに永続化されている場合、
        /// チャンク ストアからチャンクをロードします。
        /// リージョンが提供するチャンク ストアに永続化されていない場合、
        /// リージョンが提供するチャンク プロシージャから自動生成します。
        /// </summary>
        protected override void ActivateOverride()
        {
            Debug.Assert(region != null);

            var d = manager.BorrowData();

            if (manager.ChunkStore.GetChunk(region.ChunkStoreKey, Position, d))
            {
                if (d.SolidCount == 0)
                {
                    // 全てが空ブロックならば返却。
                    manager.ReturnData(d);
                }
                else
                {
                    data = d;
                }
            }
            else
            {
                // 永続化されていないならば自動生成。
                for (int i = 0; i < region.ChunkProcesures.Count; i++)
                    region.ChunkProcesures[i].Generate(this);
            }

            // 空チャンクは光レベル構築が不要 (全位置の光レベルが最大)。
            if (data == null)
                lightState = ChunkLightState.Complete;

            base.ActivateOverride();
        }

        /// <summary>
        /// チャンクをチャンク ストアへ永続化します。
        /// </summary>
        protected override void PassivateOverride()
        {
            Debug.Assert(region != null);

            if (data != null)
            {
                manager.ChunkStore.AddChunk(region.ChunkStoreKey, Position, data);
            }
            else
            {
                // 空データで永続化。
                manager.ChunkStore.AddChunk(region.ChunkStoreKey, Position, manager.EmptyData);
            }

            base.PassivateOverride();
        }

        void AttachMesh(ChunkMesh mesh)
        {
            // ノードへ追加。
            Node.Objects.Add(mesh);
        }

        void DetachMesh(ChunkMesh mesh)
        {
            // ノードから削除。
            Node.Objects.Remove(mesh);

            // マネージャへ削除要求。
            manager.DisposeMesh(mesh);
        }
    }
}
