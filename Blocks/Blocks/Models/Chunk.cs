﻿#region Using

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
        ChunkManager chunkManager;

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

        bool dataChanged;

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
            get { return chunkManager.ChunkSize; }
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
                    data = chunkManager.BorrowChunkData();
                    dataChanged = true;
                }

                data[x, y, z] = value;

                if (data.SolidCount == 0)
                {
                    // 全てが空ブロックになったならば、
                    // データをチャンク マネージャへ返します。
                    chunkManager.ReturnChunkData(data);
                    data = null;
                    dataChanged = true;
                }
            }
        }

        public byte this[VectorI3 position]
        {
            get { return this[position.X, position.Y, position.Z]; }
            set { this[position.X, position.Y, position.Z] = value; }
        }

        public SceneNode Node { get; private set; }

        /// <summary>
        /// 不透明メッシュを取得または設定します。
        /// </summary>
        public ChunkMesh OpaqueMesh
        {
            get { return opaqueMesh; }
            internal set
            {
                if (opaqueMesh != null)
                    DetachMesh(opaqueMesh);

                opaqueMesh = value;

                if (opaqueMesh != null)
                    AttachMesh(opaqueMesh);
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
                if (translucentMesh != null)
                    DetachMesh(translucentMesh);

                translucentMesh = value;

                if (translucentMesh != null)
                    AttachMesh(translucentMesh);
            }
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
        /// <param name="chunkManager">チャンク マネージャ。</param>
        public Chunk(ChunkManager chunkManager)
        {
            if (chunkManager == null) throw new ArgumentNullException("chunkManager");

            this.chunkManager = chunkManager;

            Node = new SceneNode(chunkManager.SceneManager, "Chunk" + chunkManager.CreateChunkNodeId());
        }

        /// <summary>
        /// 初期化します。
        /// </summary>
        /// <param name="position">チャンクの位置。</param>
        /// <param name="region">リージョン。</param>
        internal void Initialize(VectorI3 position, Region region)
        {
            Position = position;

            // 対象リージョンの取得。
            this.region = region;

            ActivationCompleted = false;
            PassivationCompleted = false;
        }

        /// <summary>
        /// 開放します。
        /// </summary>
        internal void Release()
        {
            Position = VectorI3.Zero;

            if (opaqueMesh != null)
            {
                DetachMesh(opaqueMesh);
                opaqueMesh = null;
            }
            if (translucentMesh != null)
            {
                DetachMesh(translucentMesh);
                translucentMesh = null;
            }
            if (VerticesBuilder != null)
            {
                chunkManager.ReleaseVerticesBuilder(VerticesBuilder);
                VerticesBuilder = null;
            }

            if (data != null)
            {
                chunkManager.ReturnChunkData(data);
                data = null;
            }
            dataChanged = false;

            region = null;

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

        /// <summary>
        /// メッシュの更新を要求します。
        /// ブロックの状態を変更した場合、このメソッドを呼び出し、
        /// メッシュの更新をチャンク マネージャへ依頼します。
        /// </summary>
        public void RequestUpdateMesh()
        {
            chunkManager.RequestUpdateMesh(Position);
        }

        /// <summary>
        /// 絶対ブロック位置から相対ブロック位置を取得します。
        /// </summary>
        /// <param name="absoluteBlockPosition">絶対ブロック位置。</param>
        /// <returns>相対ブロック位置。</returns>
        public VectorI3 GetRelativeBlockPosition(VectorI3 absoluteBlockPosition)
        {
            VectorI3 result;
            chunkManager.GetRelativeBlockPosition(ref Position, ref absoluteBlockPosition, out result);
            return result;
        }

        /// <summary>
        /// 相対ブロック位置から絶対ブロック位置を取得します。
        /// </summary>
        /// <param name="relativeBlockPosition">相対ブロック位置。</param>
        /// <returns>絶対ブロック位置。</returns>
        public VectorI3 GetAbsoluteBlockPosition(VectorI3 relativeBlockPosition)
        {
            VectorI3 result;
            chunkManager.GetAbsoluteBlockPosition(ref Position, ref relativeBlockPosition, out result);
            return result;
        }

        public int GetAbsoluteBlockPositionX(int relativeBlockPositionX)
        {
            return chunkManager.GetAbsoluteBlockPositionX(Position.X, relativeBlockPositionX);
        }

        public int GetAbsoluteBlockPositionY(int relativeBlockPositionY)
        {
            return chunkManager.GetAbsoluteBlockPositionY(Position.Y, relativeBlockPositionY);
        }

        public int GetAbsoluteBlockPositionZ(int relativeBlockPositionZ)
        {
            return chunkManager.GetAbsoluteBlockPositionZ(Position.Z, relativeBlockPositionZ);
        }

        public bool Contains(ref VectorI3 blockPosition)
        {
            return 0 <= blockPosition.X && blockPosition.X < chunkManager.ChunkSize.X &&
                0 <= blockPosition.Y && blockPosition.Y < chunkManager.ChunkSize.Y &&
                0 <= blockPosition.Z && blockPosition.Z < chunkManager.ChunkSize.Z;
        }

        /// <summary>
        /// メッシュ更新をチャンク マネージャへ要求します。
        /// </summary>
        protected override void OnActivated()
        {
            // メッシュ更新要求を追加。
            // データが空の場合は更新するメッシュが無い。
            if (data != null) RequestUpdateMesh();

            // アクティブ化完了でノードを追加。
            chunkManager.ChunkRootNode.Children.Add(Node);

            base.OnActivated();
        }

        protected override void OnPassivating()
        {
            // 非アクティブ化開始でノードを削除。
            chunkManager.ChunkRootNode.Children.Remove(Node);

            base.OnPassivating();
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

            var d = chunkManager.BorrowChunkData();

            if (region.ChunkStore.GetChunk(Position, d))
            {
                if (d.SolidCount == 0)
                {
                    // 全てが空ブロックならば返却。
                    chunkManager.ReturnChunkData(d);
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
                if (dataChanged) Region.ChunkStore.AddChunk(Position, chunkManager.EmptyChunkData);
            }

            base.PassivateOverride();
        }

        /// <summary>
        /// メッシュ更新をチャンク マネージャへ要求します。
        /// </summary>
        protected override void OnNeighborActivated(Partition neighbor, CubicSide side)
        {
            // メッシュ更新要求を追加。
            // データが空の場合は更新するメッシュが無い。
            if (data != null) RequestUpdateMesh();

            base.OnNeighborActivated(neighbor, side);
        }

        /// <summary>
        /// メッシュ更新をチャンク マネージャへ要求します。
        /// </summary>
        protected override void OnNeighborPassivated(Partition neighbor, CubicSide side)
        {
            // メッシュ更新要求を追加。
            // データが空の場合は更新するメッシュが無い。
            if (data != null) RequestUpdateMesh();

            base.OnNeighborPassivated(neighbor, side);
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
            chunkManager.DisposeChunkMesh(mesh);
        }
    }
}
