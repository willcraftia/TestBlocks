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

        // TODO
        volatile bool drawing;

        /// <summary>
        /// 非アクティブ化ロック中であるか否かを示す値。
        /// </summary>
        /// <value>
        /// true (非アクティブ化ロック中の場合)、false (それ以外の場合)。
        /// </value>
        volatile bool passivating;

        /// <summary>
        /// アクティブな隣接チャンクのフラグ。
        /// </summary>
        CubicSide.Flags activeNeighbors;

        /// <summary>
        /// メッシュ更新時に参照された隣接チャンクのフラグ。
        /// </summary>
        CubicSide.Flags neighborsReferencedOnUpdate;

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

                MeshDirty = true;
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

        /// <summary>
        /// アクティブな隣接チャンクをフラグで取得します。
        /// </summary>
        public CubicSide.Flags ActiveNeighbors
        {
            get { return activeNeighbors; }
        }

        /// <summary>
        /// メッシュ更新時に参照された隣接チャンクをフラグで取得します。
        /// </summary>
        public CubicSide.Flags NeighborsReferencedOnUpdate
        {
            get { return neighborsReferencedOnUpdate; }
            set { neighborsReferencedOnUpdate = value; }
        }

        // 外部からブロックを設定した場合などに true とする。
        // true の場合は非アクティブ化でキャッシュを更新。
        // false の場合はキャッシュの更新が不要である。
        public bool DefinitionDirty { get; set; }

        /// <summary>
        /// メッシュ更新が必要であるか否かを示す値を取得または設定します。
        /// チャンクからのブロック参照を変更した場合などに true へ設定します。
        /// </summary>
        /// <value>
        /// true (メッシュ更新が必要な場合)、false (それ以外の場合)。
        /// </value>
        public bool MeshDirty { get; set; }

        /// <summary>
        /// 不透明メッシュを取得または設定します。
        /// </summary>
        public ChunkMesh OpaqueMesh
        {
            get { return opaqueMesh; }
            internal set
            {
                if (opaqueMesh != null) opaqueMesh.Chunk = null;

                opaqueMesh = value;

                if (opaqueMesh != null) opaqueMesh.Chunk = this;
            }
        }

        /// <summary>
        /// 半透明メッシュを取得または設定します。
        /// </summary>
        public ChunkMesh TranslucentMesh
        {
            get { return translucentMesh; }
            internal set
            {
                if (translucentMesh != null) translucentMesh.Chunk = null;

                translucentMesh = value;

                if (translucentMesh != null) translucentMesh.Chunk = this;
            }
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

        // Updating と Drawing は個別にフラグを持つ必要がある。
        // 例えば、Updating = true 中に Drawing = true となった場合、
        // 描画の終了では Drawing = false としたいだけであり、
        // Updating には関与したくない。
        // 逆に、更新中は Drawing には関与したくない。

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

        // TODO
        // 描画ロックって必要？
        // 描画中に非アクティブ化することはもうなくなったはず。
        public bool Drawing
        {
            get { return drawing; }
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
        /// チャンクが属するリージョンを探索して関連付けます。
        /// </summary>
        /// <returns></returns>
        protected override bool InitializeOverride()
        {
            // 対象リージョンの取得。
            var position = Position;
            if (!regionManager.TryGetRegion(ref position, out region))
                throw new InvalidOperationException("Region not found: " + position);

            MeshDirty = true;

            return base.InitializeOverride();
        }

        /// <summary>
        /// 内部状態を初期化します。
        /// </summary>
        protected override void ReleaseOverride()
        {
            Array.Clear(blockIndices, 0, blockIndices.Length);

            activeNeighbors = CubicSide.Flags.None;
            neighborsReferencedOnUpdate = CubicSide.Flags.None;

            region = null;

            MeshDirty = true;
            DefinitionDirty = false;

            base.ReleaseOverride();
        }

        /// <summary>
        /// Active プロパティを true に設定します。
        /// </summary>
        protected override void OnActivated()
        {
            lock (activeLock) active = true;

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
        /// ActiveNeighbors プロパティへアクティブ化された隣接パーティションの方向フラグを追加します。
        /// </summary>
        public override void OnNeighborActivated(Partition neighbor, CubicSide side)
        {
            // 非アクティブな場合、通知を無視。
            if (!active) return;

            activeNeighbors |= side.ToFlags();

            base.OnNeighborActivated(neighbor, side);
        }

        /// <summary>
        /// ActiveNeighbors プロパティから非アクティブ化された隣接パーティションの方向フラグを削除します。
        /// </summary>
        public override void OnNeighborPassivated(Partition neighbor, CubicSide side)
        {
            // 非アクティブな場合、通知を無視。
            if (!active) return;

            var flag = side.ToFlags();

            Debug.Assert((activeNeighbors & flag) == flag);

            activeNeighbors ^= flag;

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

            MeshDirty = true;
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
