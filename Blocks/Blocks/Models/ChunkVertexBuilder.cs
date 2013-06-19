#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class ChunkVertexBuilder
    {
        public delegate void Callback(ChunkVertexBuilder vertexBuilder);

        static readonly Vector3 blockMeshOffset = new Vector3(0.5f);

        ChunkMeshManager meshManager;

        ChunkVertices[, ,] opaques;

        ChunkVertices[, ,] translucences;

        public Chunk Chunk { get; private set; }

        public ChunkVertexBuilder(ChunkMeshManager meshManager)
        {
            if (meshManager == null) throw new ArgumentNullException("meshManager");

            this.meshManager = meshManager;

            var segmentCount = meshManager.MeshSegments.X * meshManager.MeshSegments.Y * meshManager.MeshSegments.Z;
            opaques = new ChunkVertices[meshManager.MeshSegments.X, meshManager.MeshSegments.Y, meshManager.MeshSegments.Z];
            translucences = new ChunkVertices[meshManager.MeshSegments.X, meshManager.MeshSegments.Y, meshManager.MeshSegments.Z];

            for (int z = 0; z < meshManager.MeshSegments.Z; z++)
            {
                for (int y = 0; y < meshManager.MeshSegments.Y; y++)
                {
                    for (int x = 0; x < meshManager.MeshSegments.X; x++)
                    {
                        var segmentPosition = new IntVector3(x, y, z);
                        opaques[x, y, z] = new ChunkVertices();
                        translucences[x, y, z] = new ChunkVertices();
                    }
                }
            }
        }

        public ChunkVertices GetVertices(int segmentX, int segmentY, int segmentZ, bool translucence)
        {
            if (translucence)
            {
                return translucences[segmentX, segmentY, segmentZ];
            }
            else
            {
                return opaques[segmentX, segmentY, segmentZ];
            }
        }

        public void Initialize(Chunk chunk)
        {
            if (chunk == null) throw new ArgumentNullException("chunk");

            // メモ
            //
            // ここで隣接チャンクが揃っているか否かを検査していたが、
            // その検査は意味を成さない。
            // 非同期処理の最中に隣接チャンクが非アクティブ化される場合があるが、
            // 非同期処理では隣接チャンクがない場合に単に空チャンクを仮定して処理を進めている。

            Chunk = chunk;
        }

        public bool ConsumedAll()
        {
            for (int z = 0; z < meshManager.MeshSegments.Z; z++)
            {
                for (int y = 0; y < meshManager.MeshSegments.Y; y++)
                {
                    for (int x = 0; x < meshManager.MeshSegments.X; x++)
                    {
                        if (!opaques[x, y, z].Consumed || !translucences[x, y, z].Consumed)
                            return false;
                    }
                }
            }

            return true;
        }

        public void Clear()
        {
            Chunk = null;

            for (int z = 0; z < meshManager.MeshSegments.Z; z++)
            {
                for (int y = 0; y < meshManager.MeshSegments.Y; y++)
                {
                    for (int x = 0; x < meshManager.MeshSegments.X; x++)
                    {
                        opaques[x, y, z].Clear();
                        translucences[x, y, z].Clear();
                    }
                }
            }
        }

        public void Execute()
        {
            Debug.Assert(Chunk != null);

            // メッシュを更新。
            for (int z = 0; z < meshManager.MeshSegments.Z; z++)
            {
                for (int y = 0; y < meshManager.MeshSegments.Y; y++)
                {
                    for (int x = 0; x < meshManager.MeshSegments.X; x++)
                    {
                        BuildSegment(x, y, z);
                    }
                }
            }
        }

        void BuildSegment(int segmentX, int segmentY, int segmentZ)
        {
            for (int z = 0; z < ChunkMeshManager.MeshSize.Z; z++)
            {
                for (int y = 0; y < ChunkMeshManager.MeshSize.Y; y++)
                {
                    for (int x = 0; x < ChunkMeshManager.MeshSize.X; x++)
                    {
                        BuildBlock(segmentX, segmentY, segmentZ, x, y, z);
                    }
                }
            }
        }

        void BuildBlock(int segmentX, int segmentY, int segmentZ, int x, int y, int z)
        {
            // チャンク内相対ブロック位置。
            var blockPosition = new IntVector3
            {
                X = segmentX * ChunkMeshManager.MeshSize.X + x,
                Y = segmentY * ChunkMeshManager.MeshSize.Y + y,
                Z = segmentZ * ChunkMeshManager.MeshSize.Z + z
            };

            // 現在ブロック インデックス。
            var blockIndex = Chunk.GetBlockIndex(blockPosition);

            // 空ならば頂点なし。
            if (Block.EmptyIndex == blockIndex) return;

            // 現在ブロック。
            var block = Chunk.Region.BlockCatalog[blockIndex];

            // 現在ブロック位置。
            var chunkBlock = new ChunkBlock(Chunk, blockPosition);

            // MeshPart が必ずしも平面であるとは限らないが、
            // ここでは平面を仮定して隣接状態を考える。
            for (int i = 0; i < Side.Count; i++)
            {
                var side = Side.Items[i];

                var meshPart = block.Mesh.MeshParts[side];

                // 対象面が存在しない場合はスキップ。
                if (meshPart == null) continue;

                // 面隣接ブロック位置。
                var neighborChunkBlock = chunkBlock.GetNeighbor(side);

                // 面隣接ブロック インデックス。
                var neighborBlockIndex = neighborChunkBlock.GetBlockIndex();

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

                // 面隣接ブロック環境光遮蔽。
                lightIntensity *= CalculateAmbientOcclusion(ref neighborChunkBlock, side);

                // 光量に基づいた頂点色。
                var vertexColor = new Color(lightIntensity, lightIntensity, lightIntensity);

                // メッシュ追加。
                ChunkVertices vertices;
                if (block.Fluid || block.Translucent)
                {
                    vertices = translucences[segmentX, segmentY, segmentZ];
                }
                else
                {
                    vertices = opaques[segmentX, segmentY, segmentZ];
                }
                AddMesh(x, y, z, ref vertexColor, meshPart, vertices);
            }
        }

        float CalculateAmbientOcclusion(ref ChunkBlock chunkBlock, Side side)
        {
            const float occlusionPerFace = 1 / 5f;

            // 1 は一切遮蔽されていない状態を表す。
            float occlustion = 1;

            var mySide = side.Reverse();

            // 面隣接ブロックに対して面隣接ブロックが存在する場合、遮蔽と判定。
            for (int i = 0; i < Side.Count; i++)
            {
                var s = Side.Items[i];

                // 自身に対する方向はスキップ。
                if (mySide == s) continue;

                // 遮蔽ブロック位置。
                var occluderBlockLocation = chunkBlock.GetNeighbor(s);

                // 遮蔽ブロック インデックス。
                var occluderBlockIndex = occluderBlockLocation.GetBlockIndex();

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

        void AddMesh(int x, int y, int z, ref Color color, MeshPart source, ChunkVertices destination)
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

                // ブロック位置へ移動。
                vertex.Position.X += x;
                vertex.Position.Y += y;
                vertex.Position.Z += z;

                // ブロックの MeshPart はその中心に原点があるため、半ブロック移動。
                vertex.Position += blockMeshOffset;

                // チャンク メッシュはメッシュの中心位置を原点とするため、半メッシュ移動。
                vertex.Position -= meshManager.MeshOffset;

                destination.AddVertex(ref vertex);
            }
        }
    }
}
