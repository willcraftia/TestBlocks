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

        LocalWorld localWorld;

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

            localWorld = new LocalWorld(manager, new VectorI3(3));
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

            // 周囲のチャンクを収集。
            localWorld.FetchByCenter(Chunk.Position);

            var chunkSize = manager.ChunkSize;
            var relativeBlockPosition = new VectorI3();

#if OCCLUSION_SPACE_TEST
            Array.Clear(spaceMap, 0, spaceMap.Length);

            // 解放状態マップを構築。
            for (relativeBlockPosition.Z = 0; relativeBlockPosition.Z < chunkSize.Z; relativeBlockPosition.Z++)
                for (relativeBlockPosition.Y = 0; relativeBlockPosition.Y < chunkSize.Y; relativeBlockPosition.Y++)
                    for (relativeBlockPosition.X = 0; relativeBlockPosition.X < chunkSize.X; relativeBlockPosition.X++)
                    {
                        TestOpen(ref relativeBlockPosition);

                        Debug.Assert(spaceMap[relativeBlockPosition.X, relativeBlockPosition.Y, relativeBlockPosition.Z] != SpaceTypes.Mark);
                    }
#endif

            // メッシュを更新。
            for (relativeBlockPosition.Z = 0; relativeBlockPosition.Z < chunkSize.Z; relativeBlockPosition.Z++)
                for (relativeBlockPosition.Y = 0; relativeBlockPosition.Y < chunkSize.Y; relativeBlockPosition.Y++)
                    for (relativeBlockPosition.X = 0; relativeBlockPosition.X < chunkSize.X; relativeBlockPosition.X++)
                        Execute(ref relativeBlockPosition);

            localWorld.Clear();

            completed = true;
        }

#if OCCLUSION_SPACE_TEST
        void TestOpen(ref VectorI3 relativeBlockPosition)
        {
            int markCount = 0;

            if (spaceMap[relativeBlockPosition.X, relativeBlockPosition.Y, relativeBlockPosition.Z] == SpaceTypes.None)
            {
                // None ならば検査を開始。
                // Mark は検査前であるため存在し得ない。
                // Open/Closed 確定の要素は検査不要。
                TestOpen(ref relativeBlockPosition, ref markCount);
            }
        }

        bool TestOpen(ref VectorI3 relativeBlockPosition, ref int markCount)
        {
            var blockIndex = Chunk[relativeBlockPosition.X, relativeBlockPosition.Y, relativeBlockPosition.Z];
            if (blockIndex != Block.EmptyIndex)
            {
                var block = Chunk.Region.BlockCatalog[blockIndex];
                
                // 空、流体、半透明ではないブロックならば不要な位置であるため None のまま。
                // 呼び出し元で隣接位置の探索を試行。
                if (!block.Fluid && !block.Translucent) return false;
            }

            var chunkSize = manager.ChunkSize;

            // 空、流体、半透明ブロックである場合。
            if (relativeBlockPosition.X == 0 || relativeBlockPosition.X == (chunkSize.X - 1) ||
                relativeBlockPosition.Y == 0 || relativeBlockPosition.Y == (chunkSize.Y - 1) ||
                relativeBlockPosition.Z == 0 || relativeBlockPosition.Z == (chunkSize.Z - 1))
            {
                // 縁にあるならば Open 確定。
                spaceMap[relativeBlockPosition.X, relativeBlockPosition.Y, relativeBlockPosition.Z] = SpaceTypes.Open;
            }

            var currentResult = spaceMap[relativeBlockPosition.X, relativeBlockPosition.Y, relativeBlockPosition.Z];

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
            spaceMap[relativeBlockPosition.X, relativeBlockPosition.Y, relativeBlockPosition.Z] = SpaceTypes.Mark;
            markCount++;

            foreach (var side in CubicSide.Items)
            {
                var neighborPosition = relativeBlockPosition + side.Direction;

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

        void Execute(ref VectorI3 relativeBlockPosition)
        {
            VectorI3 absoluteBlockPosition;
            Chunk.GetAbsoluteBlockPosition(ref relativeBlockPosition, out absoluteBlockPosition);

            var blockIndex = Chunk.GetBlockIndex(ref relativeBlockPosition);

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

                // 面隣接ブロックの座標
                var absoluteNeighborBlockPosition = absoluteBlockPosition + side.Direction;

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
                var neighborBlockIndex = localWorld.GetBlockIndex(ref absoluteNeighborBlockPosition);

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

                float lightIntensity = 1;

                // 面隣接ブロックにおける天空光による光量を取得。
                var skyLight = localWorld.GetSkyLight(ref absoluteNeighborBlockPosition);

                lightIntensity *= (skyLight / 15f);

                // 環境光遮蔽を計算。
                var ambientOcclusion = CalculateAmbientOcclusion(ref absoluteNeighborBlockPosition, side);

                lightIntensity *= ambientOcclusion;

                // 光量に基づいた頂点色を計算。
                var vertexColor = new Color(lightIntensity, lightIntensity, lightIntensity);

                // メッシュを追加。
                if (block.Fluid || block.Translucent)
                {
                    AddMesh(ref relativeBlockPosition, ref vertexColor, meshPart, Translucent);
                }
                else
                {
                    AddMesh(ref relativeBlockPosition, ref vertexColor, meshPart, Opaque);
                }
            }
        }

        float CalculateAmbientOcclusion(ref VectorI3 absoluteNeighborBlockPosition, CubicSide side)
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
                var occluderPosition = absoluteNeighborBlockPosition + s.Direction;

                // 遮蔽対象のブロックのインデックスを取得。
                var occluderBlockIndex = localWorld.GetBlockIndex(ref occluderPosition);

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
                vertex.Position -= manager.MeshOffset;

                destination.AddVertex(ref vertex);
            }
        }
    }
}
