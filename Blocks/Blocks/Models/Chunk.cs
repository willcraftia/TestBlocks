#region Using

using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework;
using Willcraftia.Xna.Framework;
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
        /// 
        /// 初めて空ブロック以外が設定される際には、
        /// チャンク マネージャでプーリングされているデータを借り、
        /// このフィールドへ設定します。
        /// 一方、全てが空ブロックになる際には、
        /// このフィールドに設定されていたデータをチャンク マネージャへ返却し、
        /// このフィールドには null を設定します。
        /// 
        /// この仕組により、空 (そら) を表すチャンクではメモリが節約され、
        /// また、null の場合にメッシュ更新を要求しないことで、無駄なメッシュ更新を回避できます。
        /// </summary>
        ChunkData data;

        /// <summary>
        /// 
        /// </summary>
        bool dataChanged;

        /// <summary>
        /// チャンクがアクティブであるか否かを示す値。
        /// </summary>
        /// <value>
        /// true (アクティブな場合)、false (それ以外の場合)。
        /// </value>
        volatile bool active;

        /// <summary>
        /// 不透明メッシュ。
        /// </summary>
        ChunkMesh opaqueMesh;

        /// <summary>
        /// 半透明メッシュ。
        /// </summary>
        ChunkMesh translucentMesh;

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
        /// ブロックのインデックスを取得または設定します。
        /// ブロック位置は、チャンク空間における相対座標で指定します。
        /// </summary>
        /// <param name="x">チャンク空間における相対ブロック位置 X。</param>
        /// <param name="y">チャンク空間における相対ブロック位置 Y。</param>
        /// <param name="z">チャンク空間における相対ブロック位置 Z。</param>
        /// <returns></returns>
        public byte this[int x, int y, int z]
        {
            get
            {
                if (data == null) return Block.EmptyIndex;

                return data[x, y, z];
            }
            set
            {
                if (data == null)
                {
                    // データが null で空ブロックを設定しようとする場合は、
                    // データに変更がないため、即座に処理を終えます。
                    if (value == Block.EmptyIndex) return;

                    // 非空ブロックを設定しようとする場合は、
                    // チャンク マネージャからデータを借りる必要があります。
                    data = manager.BorrowChunkData();
                    dataChanged = true;
                }

                data[x, y, z] = value;

                if (data.SolidCount == 0)
                {
                    // 全てが空ブロックになったならば、
                    // データをチャンク マネージャへ返します。
                    manager.ReturnChunkData(data);
                    data = null;
                    dataChanged = true;
                }
            }
        }

        /// <summary>
        /// 不透明メッシュを取得または設定します。
        /// </summary>
        public ChunkMesh OpaqueMesh
        {
            get { return opaqueMesh; }
            internal set { opaqueMesh = value; }
        }

        /// <summary>
        /// 半透明メッシュを取得または設定します。
        /// </summary>
        public ChunkMesh TranslucentMesh
        {
            get { return translucentMesh; }
            internal set { translucentMesh = value; }
        }

        /// <summary>
        /// メッシュ更新のための頂点ビルダを取得または設定します。
        /// </summary>
        public ChunkVerticesBuilder VerticesBuilder { get; internal set; }

        /// <summary>
        /// チャンクがアクティブであるか否かを示す値を取得します。
        /// </summary>
        /// <value>
        /// true (アクティブな場合)、false (それ以外の場合)。
        /// </value>
        public bool Active
        {
            get { return active; }
        }

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

        /// <summary>
        /// メッシュの更新を要求します。
        /// ブロックの状態を変更した場合、このメソッドを呼び出し、
        /// メッシュの更新をチャンク マネージャへ依頼します。
        /// </summary>
        public void RequestUpdateMesh()
        {
            manager.RequestUpdateMesh(Position);
        }

        /// <summary>
        /// チャンクが属するリージョンを探索して関連付けます。
        /// </summary>
        /// <returns></returns>
        protected override bool InitializeOverride()
        {
            // 対象リージョンの取得。
            if (!manager.TryGetRegion(ref Position, out region))
                throw new InvalidOperationException("Region not found: " + Position);

            return base.InitializeOverride();
        }

        /// <summary>
        /// 内部状態を初期化します。
        /// </summary>
        protected override void ReleaseOverride()
        {
            if (OpaqueMesh != null)
            {
                manager.DisposeChunkMesh(OpaqueMesh);
                OpaqueMesh = null;
            }
            if (TranslucentMesh != null)
            {
                manager.DisposeChunkMesh(TranslucentMesh);
                TranslucentMesh = null;
            }
            if (VerticesBuilder != null)
            {
                manager.ReleaseVerticesBuilder(VerticesBuilder);
                VerticesBuilder = null;
            }

            if (data != null)
            {
                manager.ReturnChunkData(data);
                data = null;
            }
            dataChanged = false;

            region = null;

            base.ReleaseOverride();
        }

        /// <summary>
        /// Active プロパティを true に設定します。
        /// </summary>
        protected override void OnActivated()
        {
            active = true;

            // メッシュ更新要求を追加。
            // データが空の場合は更新するメッシュが無い。
            if (data != null) RequestUpdateMesh();

            base.OnActivated();
        }

        /// <summary>
        /// Active プロパティを false に設定します。
        /// </summary>
        protected override void OnPassivated()
        {
            active = false;

            base.OnPassivated();
        }

        /// <summary>
        /// チャンクに対するロックの取得を試行します。
        /// 他のスレッドがロック中の場合は、ロックの取得に失敗します。
        /// ロックの取得に成功した場合は、
        /// 必ず ExitLock メソッドでロックを解放しなければなりません。
        /// </summary>
        /// <returns>
        /// true (ロックを取得できた場合)、false (それ以外の場合)。
        /// </returns>
        public bool EnterLock()
        {
            //if (!Monitor.TryEnter(this)) return false;

            //if (!active) return false;

            return Monitor.TryEnter(this);
        }

        /// <summary>
        /// EnterLock メソッドで取得した更新ロックを開放します。
        /// </summary>
        public void ExitLock()
        {
            Monitor.Exit(this);
        }

        /// <summary>
        /// リージョンが提供するチャンク ストアに永続化されている場合、
        /// チャンク ストアからチャンクをロードします。
        /// リージョンが提供するチャンク ストアに永続化されていない場合、
        /// リージョンが提供するチャンク プロシージャから自動生成します。
        /// </summary>
        /// <returns></returns>
        protected override bool ActivateOverride()
        {
            Debug.Assert(region != null);
            Debug.Assert(!active);

            var d = manager.BorrowChunkData();

            if (region.ChunkStore.GetChunk(Position, d))
            {
                if (d.SolidCount == 0)
                {
                    // 全てが空ブロックならば返却。
                    manager.ReturnChunkData(d);
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

            return base.ActivateOverride();
        }

        /// <summary>
        /// チャンクをチャンク ストアへ永続化します。
        /// </summary>
        /// <returns></returns>
        protected override bool PassivateOverride()
        {
            Debug.Assert(region != null);
            Debug.Assert(active);

            if (!EnterLock()) return false;

            if (data != null)
            {
                // 変更があるならば永続化。
                if (dataChanged || data.Dirty) Region.ChunkStore.AddChunk(Position, data);
            }
            else
            {
                // 変更があるならば空データで永続化。
                if (dataChanged) Region.ChunkStore.AddChunk(Position, manager.EmptyChunkData);
            }

            ExitLock();

            return base.PassivateOverride();
        }

        /// <summary>
        /// メッシュ更新をチャンク マネージャへ要求します。
        /// </summary>
        public override void OnNeighborActivated(Partition neighbor, CubicSide side)
        {
            // 非アクティブな場合、通知を無視。
            if (!active) return;

            // メッシュ更新要求を追加。
            // データが空の場合は更新するメッシュが無い。
            if (data != null) RequestUpdateMesh();

            base.OnNeighborActivated(neighbor, side);
        }

        /// <summary>
        /// メッシュ更新をチャンク マネージャへ要求します。
        /// </summary>
        public override void OnNeighborPassivated(Partition neighbor, CubicSide side)
        {
            // 非アクティブな場合、通知を無視。
            if (!active) return;

            // メッシュ更新要求を追加。
            // データが空の場合は更新するメッシュが無い。
            if (data != null) RequestUpdateMesh();

            base.OnNeighborPassivated(neighbor, side);
        }

        public int CalculateBlockPositionX(int x)
        {
            return Position.X * manager.ChunkSize.X + x;
        }

        public int CalculateBlockPositionY(int y)
        {
            return Position.Y * manager.ChunkSize.Y + y;
        }

        public int CalculateBlockPositionZ(int z)
        {
            return Position.Z * manager.ChunkSize.Z + z;
        }

        public bool Contains(ref VectorI3 blockPosition)
        {
            return 0 <= blockPosition.X && blockPosition.X < manager.ChunkSize.X &&
                0 <= blockPosition.Y && blockPosition.Y < manager.ChunkSize.Y &&
                0 <= blockPosition.Z && blockPosition.Z < manager.ChunkSize.Z;
        }
    }
}
