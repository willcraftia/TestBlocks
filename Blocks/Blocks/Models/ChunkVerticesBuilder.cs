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
        static readonly Vector3 blockMeshOffset = new Vector3(0.5f);

        volatile bool completed;

        VectorI3 chunkSize;

        Vector3 chunkMeshOffset;

        public Chunk Chunk { get; internal set; }

        public CloseChunks CloseChunks { get; private set; }

        public ChunkVertices Translucent { get; private set; }

        public ChunkVertices Opaque { get; private set; }

        public bool Completed
        {
            get { return completed; }
            set { completed = value; }
        }

        public Action ExecuteAction { get; private set; }

        public ChunkVerticesBuilder(VectorI3 chunkSize)
        {
            CloseChunks = new CloseChunks();
            Opaque = new ChunkVertices(chunkSize);
            Translucent = new ChunkVertices(chunkSize);
            ExecuteAction = new Action(Execute);
        }

        void Execute()
        {
            Debug.Assert(!completed);
            Debug.Assert(Chunk != null);
            Debug.Assert(Chunk.Active);
            Debug.Assert(Chunk.Updating);
            Debug.Assert(Chunk.MeshDirty);

            chunkSize = Chunk.Size;

            var halfChunkSize = chunkSize;
            halfChunkSize.X /= 2;
            halfChunkSize.Y /= 2;
            halfChunkSize.Z /= 2;

            chunkMeshOffset = halfChunkSize.ToVector3();

            // メッシュを更新。
            var blockPosition = new VectorI3();
            for (blockPosition.Z = 0; blockPosition.Z < chunkSize.Z; blockPosition.Z++)
                for (blockPosition.Y = 0; blockPosition.Y < chunkSize.Y; blockPosition.Y++)
                    for (blockPosition.X = 0; blockPosition.X < chunkSize.X; blockPosition.X++)
                        Execute(ref blockPosition);

            completed = true;
        }

        // blockPosition はチャンク内の相対ブロック座標。
        void Execute(ref VectorI3 blockPosition)
        {
            var blockIndex = Chunk[blockPosition.X, blockPosition.Y, blockPosition.Z];

            // 空ならば頂点は存在しない。
            if (Block.EmptyIndex == blockIndex) return;

            var block = Chunk.Region.BlockCatalog[blockIndex];

            // MeshPart が必ずしも平面であるとは限らないが、
            // ここでは平面を仮定して隣接状態を考える。

            foreach (var side in CubicSide.Items)
            {
                var meshPart = block.Mesh.MeshParts[side];

                // 対象面が存在しない場合はスキップ。
                if (meshPart == null) continue;

                // 面隣接ブロックの座標 (現在のチャンク内での相対ブロック座標)
                var closeBlockPosition = blockPosition + side.Direction;

                // 面隣接ブロックを探索。
                var closeBlockIndex = GetBlockIndex(ref closeBlockPosition, side);

                // 未定の場合は面なしとする。
                // 正確な描画には面なしとすべきではないが、
                // 未定の場合に面を無視することで膨大な数の頂点を節約できる。
                // このように節約しない場合、メモリ不足へ容易に到達する。
                if (closeBlockIndex == null) continue;

                if (closeBlockIndex != Block.EmptyIndex)
                {
                    // 面隣接ブロックとの関係から対象面の要否を判定。
                    var closeBlock = Chunk.Region.BlockCatalog[closeBlockIndex.Value];

                    // 半透明な連続した流体ブロックを並べる際、流体ブロック間の面は不要。
                    // ※流体ブロックは常に半透明を仮定して処理。
                    if (closeBlock.Fluid && block.Fluid) continue;

                    // 対象面が半透明ではないならば、隣接ブロックにより不可視面となるため不要。
                    if (!block.IsTranslucentTile(side)) continue;
                }

                // 環境光遮蔽を計算。
                var ambientOcclusion = CalculateAmbientOcclusion(ref closeBlockPosition, side);

                // 環境光遮蔽に基づいた頂点色を計算。
                // 一切の遮蔽が無い場合は Color.White、
                // 現在対象とする面を遮蔽するブロックが存在する程に Color.Block へ近づく。
                var vertexColor = new Color(ambientOcclusion, ambientOcclusion, ambientOcclusion);

                // メッシュを追加。
                if (block.Fluid || block.IsTranslucentTile(side))
                {
                    AddMesh(ref blockPosition, ref vertexColor, meshPart, Translucent);
                }
                else
                {
                    AddMesh(ref blockPosition, ref vertexColor, meshPart, Opaque);
                }
            }
        }

        float CalculateAmbientOcclusion(ref VectorI3 closeBlockPosition, CubicSide side)
        {
            const float occlusionPerFace = 1 / 5f;

            // 1 は一切遮蔽されていない状態を表す。
            float occlustion = 1;

            var mySide = side.Reverse();

            // 面隣接ブロックに対して面隣接ブロックが存在する場合、遮蔽有りと判定。
            foreach (var s in CubicSide.Items)
            {
                // 自身に対する方向はスキップ。
                if (mySide == s) continue;

                // 遮蔽対象のブロック位置を算出。
                var occluderBlockPosition = closeBlockPosition + s.Direction;

                // 遮蔽対象のブロックのインデックスを取得。
                var occluderBlockIndex = GetBlockIndex(ref occluderBlockPosition, s);

                // 未定と空の場合は遮蔽無し。
                if (occluderBlockIndex == null || occluderBlockIndex == Block.EmptyIndex) continue;

                // ブロック情報を取得。
                var occluderBlock = Chunk.Region.BlockCatalog[occluderBlockIndex.Value];

                // 対象とする面が存在しない場合は遮蔽無しとする。
                if (occluderBlock.Mesh.MeshParts[s.Reverse()] == null) continue;

                // 流体ブロックは遮蔽無しとする。
                if (occluderBlock.Fluid) continue;

                // 遮蔽面が半透明の場合は遮蔽無しとする。
                if (occluderBlock.IsTranslucentTile(s.Reverse())) continue;

                // 遮蔽度で減算。
                occlustion -= occlusionPerFace;
            }

            return occlustion;
        }

        byte? GetBlockIndex(ref VectorI3 blockPosition, CubicSide side)
        {
            var x = (blockPosition.X < 0) ? -1 : (blockPosition.X < chunkSize.X) ? 0 : 1;
            var y = (blockPosition.Y < 0) ? -1 : (blockPosition.Y < chunkSize.Y) ? 0 : 1;
            var z = (blockPosition.Z < 0) ? -1 : (blockPosition.Z < chunkSize.Z) ? 0 : 1;

            if (x == 0 && y == 0 && z == 0)
            {
                // ブロックが対象チャンクに含まれている場合。
                return Chunk[blockPosition.X, blockPosition.Y, blockPosition.Z];
            }
            else
            {
                // メモ
                //
                // 実際には、隣接チャンク集合には、
                // ゲーム スレッドにて非アクティブ化されてしまったものも含まれる可能性がある。
                // しかし、それら全てを同期することは負荷が高いため、
                // ここでは無視している。
                // なお、非アクティブ化されたものが含まれる場合、
                // 再度、メッシュ更新要求が発生するはずである。

                // ブロックが隣接チャンクに含まれている場合。
                var closeChunk = CloseChunks[x, y, z];

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

        void AddMesh(ref VectorI3 blockPosition, ref Color color, MeshPart source, ChunkVertices destination)
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
                vertex.Position.X += blockPosition.X;
                vertex.Position.Y += blockPosition.Y;
                vertex.Position.Z += blockPosition.Z;

                // ブロックの MeshPart はその中心に原点があるため、半ブロック移動。
                vertex.Position += blockMeshOffset;

                // チャンク メッシュはチャンクの中心位置を原点とするため、半チャンク移動。
                vertex.Position -= chunkMeshOffset;

                destination.AddVertex(ref vertex);
            }
        }
    }
}
