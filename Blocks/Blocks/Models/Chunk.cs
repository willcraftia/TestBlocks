#region Using

using System;
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

        bool dataChanged;

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
        /// BuildLocalLights デリゲートのキャッシュ。
        /// </summary>
        Action buildLocalLightsTask;

        /// <summary>
        /// PropagateLights デリゲートのキャッシュ。
        /// </summary>
        Action propagateLightsTask;

        /// <summary>
        /// 現在実行中タスクの優先度。
        /// </summary>
        ChunkTaskPriorities currentTaskPriority;

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

            buildLocalLightsTask = new Action(BuildLocalLights);
            propagateLightsTask = new Action(PropagateLights);
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
            dataChanged = false;

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

        public Block GetBlock(int x, int y, int z)
        {
            var blockIndex = GetBlockIndex(x, y, z);
            if (blockIndex == Block.EmptyIndex) return null;

            return region.BlockCatalog[blockIndex];
        }

        public Block GetBlock(ref VectorI3 position)
        {
            var blockIndex = GetBlockIndex(ref position);
            if (blockIndex == Block.EmptyIndex) return null;

            return region.BlockCatalog[blockIndex];
        }

        public byte GetBlockIndex(int x, int y, int z)
        {
            if (data == null) return Block.EmptyIndex;

            return data.GetBlockIndex(x, y, z);
        }

        public byte GetBlockIndex(ref VectorI3 position)
        {
            if (data == null) return Block.EmptyIndex;

            return data.GetBlockIndex(ref position);
        }

        public void SetBlockIndex(ref VectorI3 position, byte blockIndex)
        {
            if (data == null)
            {
                // データが null で空ブロックを設定しようとする場合は、
                // データに変更がないため、即座に処理を終えます。
                if (blockIndex == Block.EmptyIndex) return;

                // 非空ブロックを設定しようとする場合は、
                // チャンク マネージャからデータを借りる必要があります。
                data = manager.BorrowData();
                dataChanged = true;
            }

            data.SetBlockIndex(ref position, blockIndex);

            if (data.SolidCount == 0)
            {
                // 全てが空ブロックになったならば、
                // データをチャンク マネージャへ返します。
                manager.ReturnData(data);
                data = null;
                dataChanged = true;
            }
        }

        public byte GetSkylightLevel(int x, int y, int z)
        {
            if (data == null) return 15;

            return data.GetSkylightLevel(x, y, z);
        }

        public byte GetSkylightLevel(ref VectorI3 position)
        {
            if (data == null) return 15;

            return data.GetSkylightLevel(ref position);
        }

        public void SetSkylightLevel(int x, int y, int z, byte value)
        {
            // 完全空チャンクの場合、光量データは不要。
            if (data == null) return;

            data.SetSkylightLevel(x, y, z, value);
        }

        public void SetSkylightLevel(ref VectorI3 position, byte value)
        {
            // 完全空チャンクの場合、光量データは不要。
            if (data == null) return;

            data.SetSkylightLevel(ref position, value);
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

        public Action GetTask(ChunkTaskTypes type, ChunkTaskPriorities priority)
        {
            currentTaskPriority = priority;

            switch (type)
            {
                case ChunkTaskTypes.BuildLocalLights:
                    return buildLocalLightsTask;
                case ChunkTaskTypes.PropagateLights:
                    return propagateLightsTask;
            }

            throw new InvalidOperationException();
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

            if (region.ChunkStore.GetChunk(Position, d))
            {
                if (d.SolidCount == 0)
                {
                    // 全てが空ブロックならば返却。
                    manager.ReturnData(d);
                }
                else
                {
                    // 空ブロック以外を含むならば自身へバインド。
                    data = d;
                }
            }
            else
            {
                // 永続化されていないならば自動生成。
                foreach (var procedure in region.ChunkProcesures)
                    procedure.Generate(this);
            }

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
                // 変更があるならば永続化。
                if (dataChanged || data.Dirty) Region.ChunkStore.AddChunk(Position, data);
            }
            else
            {
                // 変更があるならば空データで永続化。
                if (dataChanged) Region.ChunkStore.AddChunk(Position, manager.EmptyData);
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

        public void BuildLocalLights()
        {
            if (SolidCount == 0)
            {
                lightState = ChunkLightState.Complete;
                return;
            }

            var topNeighborChunk = GetNeighborChunk(CubicSide.Top);
            if (topNeighborChunk == null || topNeighborChunk.LightState < ChunkLightState.WaitPropagate)
            {
                // 再試行。
                manager.RequestChunkTask(ref Position, ChunkTaskTypes.BuildLocalLights, currentTaskPriority);
                return;
            }

            FallSkylight(topNeighborChunk);
            DiffuseSkylight();

            lightState = ChunkLightState.WaitPropagate;

            // 隣接チャンクへの伝播を要求。
            manager.RequestChunkTask(ref Position, ChunkTaskTypes.PropagateLights, currentTaskPriority);
        }

        void FallSkylight(Chunk topNeighborChunk)
        {
            for (int z = 0; z < manager.ChunkSize.Z; z++)
            {
                for (int x = 0; x < manager.ChunkSize.X; x++)
                {
                    Block topBlock = null;

                    // 上隣接チャンクの対象位置に直射日光が到達していないならば、
                    // 上隣接チャンク内で既に遮蔽状態となっている。
                    if (topNeighborChunk != null && topNeighborChunk.GetSkylightLevel(x, 0, z) < 15)
                        continue;

                    // 上から順に擬似直射日光の到達を試行。
                    for (int y = manager.ChunkSize.Y - 1; 0 <= y; y--)
                    {
                        if (topBlock == null || topBlock.Translucent)
                        {
                            // 上が空ブロック、あるいは、半透明ブロックならば、直射日光が到達。
                            SetSkylightLevel(x, y, z, 15);

                            // 次のループのために今のブロックを上のブロックとして設定。
                            topBlock = GetBlock(x, y, z);
                        }
                        else
                        {
                            // 上が不透明ブロックならば、以下全ての位置は遮蔽状態。
                            break;
                        }
                    }
                }
            }
        }

        void DiffuseSkylight()
        {
            for (int z = 0; z < manager.ChunkSize.Z; z++)
            {
                for (int x = 0; x < manager.ChunkSize.X; x++)
                {
                    for (int y = manager.ChunkSize.Y - 1; 0 <= y; y--)
                    {
                        var blockPosition = new VectorI3(x, y, z);
                        DiffuseSkylight(ref blockPosition);
                    }
                }
            }
        }

        void DiffuseSkylight(ref VectorI3 blockPosition)
        {
            // 1 以下はこれ以上拡散できない。
            var level = GetSkylightLevel(ref blockPosition);
            if (level <= 1) return;

            if (!CanPenetrateLight(ref blockPosition)) return;

            foreach (var side in CubicSide.Items)
            {
                var neighborBlockPosition = blockPosition + side.Direction;

                // チャンク外はスキップ。
                if (neighborBlockPosition.X < 0 || manager.ChunkSize.X <= neighborBlockPosition.X ||
                    neighborBlockPosition.Y < 0 || manager.ChunkSize.Y <= neighborBlockPosition.Y ||
                    neighborBlockPosition.Z < 0 || manager.ChunkSize.Z <= neighborBlockPosition.Z)
                    continue;

                // 光レベルの高い位置へは拡散しない。
                var diffuseLevel = (byte) (level - 1);
                if (diffuseLevel <= GetSkylightLevel(ref neighborBlockPosition)) continue;

                if (!CanPenetrateLight(ref neighborBlockPosition)) continue;

                SetSkylightLevel(ref neighborBlockPosition, diffuseLevel);
                DiffuseSkylight(ref neighborBlockPosition);
            }
        }

        /// <summary>
        /// 隣接チャンクの光を自チャンクへ伝播させます。
        /// </summary>
        /// <remarks>
        /// ここでは、まず、隣接チャンクから、自チャンクに隣接する位置の光レベルを参照し、
        /// これに隣接する自チャンクの位置へ光を拡散させます。
        /// 続いて、ここで更新した光レベルに基づき、自チャンク内部に向かって光を拡散させます。
        /// 以上により、隣接チャンクを含めた自チャンクの光レベルが完成するため、
        /// 最後にメッシュ更新を要求します。
        /// </remarks>
        public void PropagateLights()
        {
            if (SolidCount == 0)
            {
                lightState = ChunkLightState.Complete;
                return;
            }

            var neighbors = new Neighbors<Chunk>();
            foreach (var side in CubicSide.Items)
            {
                var neighbor = GetPropagatableNeighborChunk(side);
                if (neighbor == null)
                {
                    // 再試行。
                    manager.RequestChunkTask(ref Position, ChunkTaskTypes.PropagateLights, currentTaskPriority);
                    return;
                }
                neighbors[side] = neighbor;
            }

            for (int y = 0; y < manager.ChunkSize.Y; y++)
            {
                for (int x = 0; x < manager.ChunkSize.X; x++)
                {
                    var frontBlockPosition = new VectorI3(x, y, manager.ChunkSize.Z - 1);
                    var frontNeighborBlockPosition = new VectorI3(x, y, 0);
                    PropagateLights(ref frontBlockPosition, neighbors.Front, ref frontNeighborBlockPosition);

                    var backBlockPosition = new VectorI3(x, y, 0);
                    var backNeighborBlockPosition = new VectorI3(x, y, manager.ChunkSize.Z - 1);
                    PropagateLights(ref backBlockPosition, neighbors.Back, ref backNeighborBlockPosition);
                }
            }

            for (int z = 0; z < manager.ChunkSize.Z; z++)
            {
                for (int y = 0; y < manager.ChunkSize.Y; y++)
                {
                    var leftBlockPosition = new VectorI3(0, y, z);
                    var leftBlockNeighborPosition = new VectorI3(manager.ChunkSize.X - 1, y, z);
                    PropagateLights(ref leftBlockPosition, neighbors.Left, ref leftBlockNeighborPosition);

                    var rightBlockPosition = new VectorI3(manager.ChunkSize.X - 1, y, z);
                    var rightBlockNeighborPosition = new VectorI3(0, y, z);
                    PropagateLights(ref rightBlockPosition, neighbors.Right, ref rightBlockNeighborPosition);
                }
            }

            lightState = ChunkLightState.Complete;

            manager.RequestUpdateMesh(ref Position, ChunkMeshUpdatePriorities.Normal);
        }

        void PropagateLights(ref VectorI3 blockPosition, Chunk neighborChunk, ref VectorI3 neighborBlockPosition)
        {
            var level = neighborChunk.GetSkylightLevel(ref neighborBlockPosition);
            if (level <= 1) return;

            var diffuseLevel = (byte) (level - 1);
            if (diffuseLevel <= GetSkylightLevel(ref blockPosition)) return;

            if (!neighborChunk.CanPenetrateLight(ref neighborBlockPosition)) return;
            if (!CanPenetrateLight(ref blockPosition)) return;

            SetSkylightLevel(ref blockPosition, diffuseLevel);
            DiffuseSkylight(ref blockPosition);
        }

        bool CanPenetrateLight(ref VectorI3 blockPosition)
        {
            var block = GetBlock(ref blockPosition);
            return block == null || block.Translucent;
        }

        Chunk GetPropagatableNeighborChunk(CubicSide side)
        {
            var neighbor = GetNeighborChunk(side);
            if (neighbor == null || neighbor.LightState < ChunkLightState.WaitPropagate)
                return null;
            
            return neighbor;
        }

        Chunk GetNeighborChunk(CubicSide side)
        {
            var neighborPosition = Position + side.Direction;
            return manager.GetChunk(ref neighborPosition);
        }
    }
}
