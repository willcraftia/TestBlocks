#region Using

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class ChunkVerticesBuilder
    {
        #region CloseChunks

        /// <summary>
        /// 隣接チャンク集合を管理するクラスです。
        /// </summary>
        sealed class CloseChunks
        {
            /// <summary>
            /// チャンクのサイズ。
            /// </summary>
            VectorI3 chunkSize;

            /// <summary>
            /// 隣接チャンク集合。
            /// (1, 1, 1) が中心位置。
            /// </summary>
            Chunk[, ,] chunks = new Chunk[3, 3, 3];

            /// <summary>
            /// 指定の位置のチャンクを取得または設定します。
            /// インデックスは、(0, 0, 0) を中心位置としたオフセット値として [-1, 1] で指定します。
            /// </summary>
            /// <param name="x">X オフセット。</param>
            /// <param name="y">Y オフセット</param>
            /// <param name="z">Z オフセット。</param>
            /// <returns>チャンク。</returns>
            public Chunk this[int x, int y, int z]
            {
                get { return chunks[x + 1, y + 1, z + 1]; }
                set { chunks[x + 1, y + 1, z + 1] = value; }
            }

            /// <summary>
            /// インスタンスを生成します。
            /// </summary>
            /// <param name="chunkSize">チャンクのサイズ。</param>
            public CloseChunks(VectorI3 chunkSize)
            {
                if (chunkSize.X < 1 || chunkSize.Y < 1 || chunkSize.Z < 1)
                    throw new ArgumentOutOfRangeException("chunkSize");

                this.chunkSize = chunkSize;
            }

            /// <summary>
            /// 状態を初期化します。
            /// </summary>
            public void Clear()
            {
                Array.Clear(chunks, 0, chunks.Length);
            }

            /// <summary>
            /// 指定の位置にあるブロックのインデックスを取得します。
            /// 指定するブロックの位置は、中心チャンク内における相対位置です。
            /// 指定の位置が中心チャンク内に無い場合、隣接チャンクから取得を試みますが、
            /// いずれの隣接チャンクにも含まれない場合には、未定として null を返します。
            /// </summary>
            /// <param name="blockPosition">中心チャンク内における相対ブロック位置。</param>
            /// <returns>
            /// ブロックのインデックス、あるいは、指定のブロック位置が範囲外ならば null。
            /// </returns>
            public byte? GetBlockIndex(ref VectorI3 blockPosition)
            {
                var x = (blockPosition.X < 0) ? -1 : (blockPosition.X < chunkSize.X) ? 0 : 1;
                var y = (blockPosition.Y < 0) ? -1 : (blockPosition.Y < chunkSize.Y) ? 0 : 1;
                var z = (blockPosition.Z < 0) ? -1 : (blockPosition.Z < chunkSize.Z) ? 0 : 1;

                // メモ
                //
                // 実際には、隣接チャンク集合には、
                // ゲーム スレッドにて非アクティブ化されてしまったものも含まれる可能性がある。
                // しかし、それら全てを同期することは負荷が高いため、
                // ここでは無視している。
                // なお、非アクティブ化されたものが含まれる場合、
                // 再度、メッシュ更新要求が発生するはずである。

                // ブロックが隣接チャンクに含まれている場合。
                var closeChunk = this[x, y, z];

                // 隣接チャンクがないならば未定として null。
                if (closeChunk == null) return null;

                // 隣接チャンクにおける相対ブロック座標を算出。
                var relativeX = blockPosition.X % chunkSize.X;
                var relativeY = blockPosition.Y % chunkSize.Y;
                var relativeZ = blockPosition.Z % chunkSize.Z;
                if (relativeX < 0) relativeX += chunkSize.X;
                if (relativeY < 0) relativeY += chunkSize.Y;
                if (relativeZ < 0) relativeZ += chunkSize.Z;

                return closeChunk[relativeX, relativeY, relativeZ];
            }
        }

        #endregion

#if OCCLUSION_SPACE_TEST
        #region SpaceTypes

        /// <summary>
        /// あるブロック位置における空間の解放および閉塞状態を示す列挙型です。
        /// </summary>
        enum SpaceTypes
        {
            /// <summary>
            /// 不明あるいは未使用。
            /// </summary>
            None = 0,
            /// <summary>
            /// 探索で通過した位置のマーキング。
            /// </summary>
            Mark,
            /// <summary>
            /// 解放空間。
            /// </summary>
            Open,
            /// <summary>
            /// 閉塞空間。
            /// </summary>
            Closed
        }

        #endregion
#endif

        static readonly Vector3 blockMeshOffset = new Vector3(0.5f);

        volatile bool completed;

        ChunkManager manager;

        CloseChunks closeChunks;

#if OCCLUSION_SPACE_TEST
        SpaceTypes[, ,] spaceMap;
#endif

        public Chunk Chunk { get; internal set; }

        public ChunkVertices Translucent { get; private set; }

        public ChunkVertices Opaque { get; private set; }

        public bool Completed
        {
            get { return completed; }
            set { completed = value; }
        }

        public Action ExecuteAction { get; private set; }

        public ChunkVerticesBuilder(ChunkManager manager)
        {
            if (manager == null) throw new ArgumentNullException("manager");

            this.manager = manager;

            closeChunks = new CloseChunks(manager.ChunkSize);
            Opaque = new ChunkVertices(manager.ChunkSize);
            Translucent = new ChunkVertices(manager.ChunkSize);
            ExecuteAction = new Action(Execute);
#if OCCLUSION_SPACE_TEST
            spaceMap = new SpaceTypes[manager.ChunkSize.X, manager.ChunkSize.Y, manager.ChunkSize.Z];
#endif
        }

        void Execute()
        {
            Debug.Assert(!completed);
            Debug.Assert(Chunk != null);

            // 隣接チャンク集合を構築。
            var centerPosition = Chunk.Position;
            for (int z = -1; z <= 1; z++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        // 8 つの隅は不要。
                        if (x != 0 && y != 0 && z != 0) continue;

                        // 中心には自身を設定。
                        if (x == 0 && y == 0 && z == 0)
                        {
                            closeChunks[0, 0, 0] = Chunk;
                            continue;
                        }

                        var closePosition = centerPosition;
                        closePosition.X += x;
                        closePosition.Y += y;
                        closePosition.Z += z;

                        closeChunks[x, y, z] = manager[closePosition] as Chunk;
                    }
                }
            }

            var chunkSize = manager.ChunkSize;
            var position = new VectorI3();

#if OCCLUSION_SPACE_TEST
            Array.Clear(spaceMap, 0, spaceMap.Length);

            // 解放状態マップを構築。
            for (position.Z = 0; position.Z < chunkSize.Z; position.Z++)
                for (position.Y = 0; position.Y < chunkSize.Y; position.Y++)
                    for (position.X = 0; position.X < chunkSize.X; position.X++)
                    {
                        TestOpen(ref position);

                        Debug.Assert(spaceMap[position.X, position.Y, position.Z] != SpaceTypes.Mark);
                    }
#endif

            // メッシュを更新。
            for (position.Z = 0; position.Z < chunkSize.Z; position.Z++)
                for (position.Y = 0; position.Y < chunkSize.Y; position.Y++)
                    for (position.X = 0; position.X < chunkSize.X; position.X++)
                        Execute(ref position);

            closeChunks.Clear();

            completed = true;
        }

#if OCCLUSION_SPACE_TEST
        void TestOpen(ref VectorI3 position)
        {
            int markCount = 0;

            if (spaceMap[position.X, position.Y, position.Z] == SpaceTypes.None)
            {
                // None ならば検査を開始。
                // Mark は検査前であるため存在し得ない。
                // Open/Closed 確定の要素は検査不要。
                TestOpen(ref position, ref markCount);
            }
        }

        bool TestOpen(ref VectorI3 position, ref int markCount)
        {
            var blockIndex = Chunk[position.X, position.Y, position.Z];
            if (blockIndex != Block.EmptyIndex)
            {
                var block = Chunk.Region.BlockCatalog[blockIndex];
                
                // 空、流体、半透明ではないブロックならば不要な位置であるため None のまま。
                // 呼び出し元で隣接位置の探索を試行。
                if (!block.Fluid && !block.Translucent) return false;
            }

            var chunkSize = manager.ChunkSize;

            // 空、流体、半透明ブロックである場合。
            if (position.X == 0 || position.X == (chunkSize.X - 1) ||
                position.Y == 0 || position.Y == (chunkSize.Y - 1) ||
                position.Z == 0 || position.Z == (chunkSize.Z - 1))
            {
                // 縁にあるならば Open 確定。
                spaceMap[position.X, position.Y, position.Z] = SpaceTypes.Open;
            }

            var currentResult = spaceMap[position.X, position.Y, position.Z];

            if (currentResult == SpaceTypes.Open)
            {
                // Open 確定ならば Mark に設定した位置も全て Open 確定。
                if (0 < markCount)
                {
                    for (int z = 0; z < chunkSize.Z; z++)
                        for (int y = 0; y < chunkSize.Y; y++)
                            for (int x = 0; x < chunkSize.X; x++)
                            {
                                if (spaceMap[x, y, z] == SpaceTypes.Mark)
                                    spaceMap[x, y, z] = SpaceTypes.Open;
                            }
                }
                // 処理完了。
                return true;
            }

            if (currentResult == SpaceTypes.Closed)
            {
                // ※ここの処理には入り得ないが念のため。
                // Closed 確定ならば Mark に設定した位置も全て Closed 確定。
                if (0 < markCount)
                {
                    for (int z = 0; z < chunkSize.Z; z++)
                        for (int y = 0; y < chunkSize.Y; y++)
                            for (int x = 0; x < chunkSize.X; x++)
                            {
                                if (spaceMap[x, y, z] == SpaceTypes.Mark)
                                    spaceMap[x, y, z] = SpaceTypes.Closed;
                            }
                }
                // 処理完了。
                return true;
            }

            if (currentResult == SpaceTypes.Mark)
            {
                // Mark ならば Mark のままとし、呼び出し元での他の隣接位置の検査に委ねる。
                return false;
            }

            // None ならば Mark を設定して隣接位置を検査。
            spaceMap[position.X, position.Y, position.Z] = SpaceTypes.Mark;
            markCount++;

            foreach (var side in CubicSide.Items)
            {
                var neighborPosition = position + side.Direction;

                if (TestOpen(ref neighborPosition, ref markCount))
                {
                    // 戻り値が処理完了を表す true ならば処理完了。
                    return true;
                }

                // 戻り値が処理未完を表す false ならば次の隣接位置の検査を試行。
            }

            // 再帰的に隣接位置を検査しても以前未完のままならば、
            // 最早辿れる位置が無いため、Closed 確定。
            if (0 < markCount)
            {
                for (int z = 0; z < chunkSize.Z; z++)
                    for (int y = 0; y < chunkSize.Y; y++)
                        for (int x = 0; x < chunkSize.X; x++)
                        {
                            if (spaceMap[x, y, z] == SpaceTypes.Mark)
                                spaceMap[x, y, z] = SpaceTypes.Closed;
                        }
            }
            // 処理終了。
            return true;
        }
#endif

        // position はチャンク内の相対ブロック座標。
        void Execute(ref VectorI3 position)
        {
            var blockIndex = Chunk[position.X, position.Y, position.Z];

            // 空ならば頂点は存在しない。
            if (Block.EmptyIndex == blockIndex) return;

            var block = Chunk.Region.BlockCatalog[blockIndex];

            var chunkSize = manager.ChunkSize;

            // MeshPart が必ずしも平面であるとは限らないが、
            // ここでは平面を仮定して隣接状態を考える。

            foreach (var side in CubicSide.Items)
            {
                var meshPart = block.Mesh.MeshParts[side];

                // 対象面が存在しない場合はスキップ。
                if (meshPart == null) continue;

                // 面隣接ブロックの座標 (現在のチャンク内での相対ブロック座標)
                var neighborPosition = position + side.Direction;

#if OCCLUSION_SPACE_TEST
                // 自チャンク内位置ならば、閉塞空間判定を試行。
                if (0 <= neighborPosition.X && neighborPosition.X < chunkSize.X &&
                    0 <= neighborPosition.Y && neighborPosition.Y < chunkSize.Y &&
                    0 <= neighborPosition.Z && neighborPosition.Z < chunkSize.Z)
                {
                    if (spaceMap[neighborPosition.X, neighborPosition.Y, neighborPosition.Z] == SpaceTypes.Closed)
                    {
                        // 隣接位置が閉塞空間ならば面は不要。
                        continue;
                    }
                }
#endif

                // 面隣接ブロックを探索。
                var neighborBlockIndex = closeChunks.GetBlockIndex(ref neighborPosition);

                // 未定の場合は面なしとする。
                // 正確な描画には面なしとすべきではないが、
                // 未定の場合に面を無視することで膨大な数の頂点を節約できる。
                // このように節約しない場合、メモリ不足へ容易に到達する。
                if (neighborBlockIndex == null) continue;

                if (neighborBlockIndex != Block.EmptyIndex)
                {
                    // 面隣接ブロックとの関係から対象面の要否を判定。
                    var neighborBlock = Chunk.Region.BlockCatalog[neighborBlockIndex.Value];

                    // 半透明な連続した流体ブロックを並べる際、流体ブロック間の面は不要。
                    // ※流体ブロックは常に半透明を仮定して処理。
                    if (neighborBlock.Fluid && block.Fluid) continue;

                    // 隣接ブロックが半透明ではないならば、不可視面となるため不要。
                    if (!neighborBlock.Translucent) continue;
                }

                // 環境光遮蔽を計算。
                var ambientOcclusion = CalculateAmbientOcclusion(ref neighborPosition, side);

                // 環境光遮蔽に基づいた頂点色を計算。
                // 一切の遮蔽が無い場合は Color.White、
                // 現在対象とする面を遮蔽するブロックが存在する程に Color.Block へ近づく。
                var vertexColor = new Color(ambientOcclusion, ambientOcclusion, ambientOcclusion);

                // メッシュを追加。
                if (block.Fluid || block.Translucent)
                {
                    AddMesh(ref position, ref vertexColor, meshPart, Translucent);
                }
                else
                {
                    AddMesh(ref position, ref vertexColor, meshPart, Opaque);
                }
            }
        }

        float CalculateAmbientOcclusion(ref VectorI3 neighborPosition, CubicSide side)
        {
            const float occlusionPerFace = 1 / 5f;

            // 1 は一切遮蔽されていない状態を表す。
            float occlustion = 1;

            var mySide = side.Reverse();

            // 面隣接ブロックに対して面隣接ブロックが存在する場合、遮蔽と判定。
            foreach (var s in CubicSide.Items)
            {
                // 自身に対する方向はスキップ。
                if (mySide == s) continue;

                // 遮蔽対象のブロック位置を算出。
                var occluderPosition = neighborPosition + s.Direction;

                // 遮蔽対象のブロックのインデックスを取得。
                var occluderBlockIndex = closeChunks.GetBlockIndex(ref occluderPosition);

                // 未定と空の場合は非遮蔽。
                if (occluderBlockIndex == null || occluderBlockIndex == Block.EmptyIndex) continue;

                // ブロック情報を取得。
                var occluderBlock = Chunk.Region.BlockCatalog[occluderBlockIndex.Value];

                // 対象とする面が存在しない場合は非遮蔽。
                if (occluderBlock.Mesh.MeshParts[s.Reverse()] == null) continue;

                // 流体ブロックは非遮蔽。
                if (occluderBlock.Fluid) continue;

                // 半透明ブロックは非遮蔽。
                if (occluderBlock.Translucent) continue;

                // 遮蔽度で減算。
                occlustion -= occlusionPerFace;
            }

            return occlustion;
        }

        void AddMesh(ref VectorI3 position, ref Color color, MeshPart source, ChunkVertices destination)
        {
            foreach (var index in source.Indices)
                destination.AddIndex(index);

            var vertices = source.Vertices;
            for (int i = 0; i < vertices.Length; i++)
            {
                var sourceVertex = vertices[i];

                var vertex = new VertexPositionNormalColorTexture
                {
                    Position = sourceVertex.Position,
                    Normal = sourceVertex.Normal,
                    Color = color,
                    TextureCoordinate = sourceVertex.TextureCoordinate
                };

                // チャンク座標内での位置へ移動。
                vertex.Position.X += position.X;
                vertex.Position.Y += position.Y;
                vertex.Position.Z += position.Z;

                // ブロックの MeshPart はその中心に原点があるため、半ブロック移動。
                vertex.Position += blockMeshOffset;

                // チャンク メッシュはチャンクの中心位置を原点とするため、半チャンク移動。
                vertex.Position -= manager.ChunkMeshOffset;

                destination.AddVertex(ref vertex);
            }
        }
    }
}
