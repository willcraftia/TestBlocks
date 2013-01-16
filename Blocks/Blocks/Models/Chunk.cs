#region Using

using System;
using System.Diagnostics;
using System.IO;
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
        ChunkManager chunkManager;

        /// <summary>
        /// リージョン マネージャ。
        /// </summary>
        RegionManager regionManager;

        /// <summary>
        /// チャンクが属するリージョン。
        /// </summary>
        Region region;

        /// <summary>
        /// チャンクのサイズ。
        /// </summary>
        VectorI3 size;

        /// <summary>
        /// チャンクが参照するブロックのインデックス。
        /// </summary>
        byte[] blockIndices;

        /// <summary>
        /// Active プロパティの同期のためのロック オブジェクト。
        /// </summary>
        object activeLock = new object();

        /// <summary>
        /// チャンクがアクティブであるか否かを示す値。
        /// </summary>
        /// <value>
        /// true (アクティブな場合)、false (それ以外の場合)。
        /// </value>
        volatile bool active;

        /// <summary>
        /// 更新ロック中であるか否かを示す値。
        /// </summary>
        /// <value>
        /// true (更新ロック中の場合)、false (それ以外の場合)。
        /// </value>
        volatile bool updating;

        /// <summary>
        /// 非アクティブ化ロック中であるか否かを示す値。
        /// </summary>
        /// <value>
        /// true (非アクティブ化ロック中の場合)、false (それ以外の場合)。
        /// </value>
        volatile bool passivating;

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
            get { return size; }
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

                DefinitionDirty = true;
            }
        }

        /// <summary>
        /// ブロックの総数を取得します。
        /// </summary>
        public int Count
        {
            get { return blockIndices.Length; }
        }

        // 外部からブロックを設定した場合などに true とする。
        // true の場合は非アクティブ化でキャッシュを更新。
        // false の場合はキャッシュの更新が不要である。
        public bool DefinitionDirty { get; set; }

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
        /// 更新ロック中であるか否かを示す値を取得します。
        /// 更新ロック中の場合、Busy プロパティも true となり、
        /// 更新ロックが解放されるまで非アクティブ化されない状態となります。
        /// </summary>
        /// <value>
        /// true (更新ロック中の場合)、false (それ以外の場合)。
        /// </value>
        public bool Updating
        {
            get { return updating; }
        }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        /// <param name="chunkManager">チャンク マネージャ。</param>
        /// <param name="regionManager">リージョン マネージャ。</param>
        public Chunk(ChunkManager chunkManager, RegionManager regionManager)
        {
            if (chunkManager == null) throw new ArgumentNullException("chunkManager");
            if (regionManager == null) throw new ArgumentNullException("regionManager");

            this.chunkManager = chunkManager;
            this.regionManager = regionManager;

            size = chunkManager.ChunkSize;

            blockIndices = new byte[size.X * size.Y * size.Z];
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
            chunkManager.RequestUpdateMesh(position);
        }

        /// <summary>
        /// チャンクが属するリージョンを探索して関連付けます。
        /// </summary>
        /// <returns></returns>
        protected override bool InitializeOverride()
        {
            // 対象リージョンの取得。
            var position = Position;
            if (!regionManager.TryGetRegion(ref position, out region))
                throw new InvalidOperationException("Region not found: " + position);

            return base.InitializeOverride();
        }

        /// <summary>
        /// 内部状態を初期化します。
        /// </summary>
        protected override void ReleaseOverride()
        {
            Array.Clear(blockIndices, 0, blockIndices.Length);

            region = null;

            DefinitionDirty = false;

            base.ReleaseOverride();
        }

        /// <summary>
        /// Active プロパティを true に設定します。
        /// </summary>
        protected override void OnActivated()
        {
            lock (activeLock) active = true;

            RequestUpdateMesh();

            base.OnActivated();
        }

        /// <summary>
        /// Active プロパティを false に設定します。
        /// </summary>
        protected override void OnPassivated()
        {
            lock (activeLock) active = false;

            base.OnPassivated();
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

                // 更新中は非アクティブ化を拒否。
                Busy = true;

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
            Busy = false;
        }

        /// <summary>
        /// 非アクティブ化ロックの取得を試行します。
        /// 他のスレッドがロック中の場合、非アクティブな場合、更新ロック中の場合は、
        /// 非アクティブ化ロックの取得に失敗します。
        /// 非アクティブ化ロックの取得に成功した場合には、
        /// 必ず ExitPassivate メソッドでロックを解放しなければなりません。
        /// </summary>
        /// <returns>
        /// true (非アクティブ化ロックを取得できた場合)、false (それ以外の場合)。
        /// </returns>
        bool EnterPassivate()
        {
            if (!Monitor.TryEnter(activeLock)) return false;

            try
            {
                if (!active) return false;
                if (updating) return false;

                passivating = true;
                return true;
            }
            finally
            {
                Monitor.Exit(activeLock);
            }
        }

        /// <summary>
        /// EnterPassivate メソッドで取得した非アクティブ化ロックを開放します。
        /// </summary>
        void ExitPassivate()
        {
            passivating = false;
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

            var position = Position;

            if (!region.ChunkStore.GetChunk(ref position, this))
            {
                foreach (var procedure in region.ChunkProcesures)
                    procedure.Generate(this);
            }

            return base.ActivateOverride();
        }

        /// <summary>
        /// チャンクをチャンク ストアへ永続化します。
        /// また、関連付けられているメッシュを開放します。
        /// </summary>
        /// <returns></returns>
        protected override bool PassivateOverride()
        {
            Debug.Assert(region != null);
            Debug.Assert(active);

            if (!EnterPassivate()) return false;

            if (OpaqueMesh != null)
            {
                chunkManager.DisposeChunkMesh(OpaqueMesh);
                OpaqueMesh = null;
            }
            if (TranslucentMesh != null)
            {
                chunkManager.DisposeChunkMesh(TranslucentMesh);
                TranslucentMesh = null;
            }
            if (VerticesBuilder != null)
            {
                chunkManager.ReleaseVerticesBuilder(VerticesBuilder);
                VerticesBuilder = null;
            }

            // 定義に変更があるならば永続化領域を更新。
            if (DefinitionDirty) Region.ChunkStore.AddChunk(this);

            ExitPassivate();

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
            RequestUpdateMesh();

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
            RequestUpdateMesh();

            base.OnNeighborPassivated(neighbor, side);
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
            //var p = new VectorI3();

            //p.X = reader.ReadInt32();
            //p.Y = reader.ReadInt32();
            //p.Z = reader.ReadInt32();

            for (int i = 0; i < blockIndices.Length; i++)
                blockIndices[i] = reader.ReadByte();
        }

        public void Write(BinaryWriter writer)
        {
            //writer.Write(position.X);
            //writer.Write(position.Y);
            //writer.Write(position.Z);

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
