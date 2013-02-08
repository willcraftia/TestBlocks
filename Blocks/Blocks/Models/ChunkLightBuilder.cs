﻿#region Using

using System;
using Willcraftia.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class ChunkLightBuilder
    {
        public static bool CanPenetrateLight(Block block)
        {
            return block == null || block.Translucent;
        }

        public static void BuildLocalLights(Chunk chunk)
        {
            if (chunk == null) throw new ArgumentNullException("chunk");

            chunk.ClearSkylightLevels();

            if (chunk.SolidCount == 0)
            {
                chunk.LightState = ChunkLightState.Complete;
                return;
            }

            var topNeighborChunk = chunk.GetNeighborChunk(CubicSide.Top);
            if (topNeighborChunk == null || topNeighborChunk.LightState < ChunkLightState.WaitPropagate)
            {
                // 再試行。
                return;
            }

            FallSkylight(chunk, topNeighborChunk);
            DiffuseSkylight(chunk);

            chunk.LightState = ChunkLightState.WaitPropagate;
        }

        /// <summary>
        /// 対象チャンクに対して、その隣接チャンクの光を伝播させます。
        /// </summary>
        /// <remarks>
        /// ここでは、まず、隣接チャンクから、対象チャンクに隣接する位置の光レベルを参照し、
        /// これに隣接する対象チャンクの位置へ光を拡散させます。
        /// 続いて、ここで更新した光レベルに基づき、対象チャンク内部に向かって光を拡散させます。
        /// 以上により、隣接チャンクを含めた対象チャンクの光レベルが完成するため、
        /// 最後にメッシュ更新を要求します。
        /// </remarks>
        public static void PropagateLights(Chunk chunk)
        {
            if (chunk == null) throw new ArgumentNullException("chunk");

            if (chunk.SolidCount == 0)
            {
                chunk.LightState = ChunkLightState.Complete;
                return;
            }

            var neighbors = new ChunkNeighbors();
            foreach (var side in CubicSide.Items)
            {
                var neighbor = GetPropagatableNeighborChunk(chunk, side);
                if (neighbor == null)
                {
                    // 再試行。
                    return;
                }
                neighbors[side] = neighbor;
            }

            var size = chunk.Size;

            for (int y = 0; y < size.Y; y++)
            {
                for (int x = 0; x < size.X; x++)
                {
                    var frontBlockPosition = new VectorI3(x, y, size.Z - 1);
                    var frontNeighborBlockPosition = new VectorI3(x, y, 0);
                    PropagateLights(chunk, ref frontBlockPosition, neighbors.Front, ref frontNeighborBlockPosition);

                    var backBlockPosition = new VectorI3(x, y, 0);
                    var backNeighborBlockPosition = new VectorI3(x, y, size.Z - 1);
                    PropagateLights(chunk, ref backBlockPosition, neighbors.Back, ref backNeighborBlockPosition);
                }
            }

            for (int z = 0; z < size.Z; z++)
            {
                for (int y = 0; y < size.Y; y++)
                {
                    var leftBlockPosition = new VectorI3(0, y, z);
                    var leftBlockNeighborPosition = new VectorI3(size.X - 1, y, z);
                    PropagateLights(chunk, ref leftBlockPosition, neighbors.Left, ref leftBlockNeighborPosition);

                    var rightBlockPosition = new VectorI3(size.X - 1, y, z);
                    var rightBlockNeighborPosition = new VectorI3(0, y, z);
                    PropagateLights(chunk, ref rightBlockPosition, neighbors.Right, ref rightBlockNeighborPosition);
                }
            }

            chunk.LightState = ChunkLightState.Complete;
        }

        static bool CanPenetrateLight(Chunk chunk, ref VectorI3 blockPosition)
        {
            var block = chunk.GetBlock(ref blockPosition);
            return CanPenetrateLight(block);
        }

        static void FallSkylight(Chunk chunk, Chunk topNeighborChunk)
        {
            var size = chunk.Size;

            for (int z = 0; z < size.Z; z++)
            {
                for (int x = 0; x < size.X; x++)
                {
                    Block topBlock = null;

                    // 上隣接チャンクの対象位置に直射日光が到達していないならば、
                    // 上隣接チャンク内で既に遮蔽状態となっている。
                    if (topNeighborChunk != null && topNeighborChunk.GetSkylightLevel(x, 0, z) < Chunk.MaxSkylightLevel)
                        continue;

                    // 上から順に擬似直射日光の到達を試行。
                    for (int y = size.Y - 1; 0 <= y; y--)
                    {
                        if (topBlock == null || topBlock.Translucent)
                        {
                            // 上が空ブロック、あるいは、半透明ブロックならば、直射日光が到達。
                            chunk.SetSkylightLevel(x, y, z, Chunk.MaxSkylightLevel);

                            // 次のループのために今のブロックを上のブロックとして設定。
                            topBlock = chunk.GetBlock(x, y, z);
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

        static void DiffuseSkylight(Chunk chunk)
        {
            var size = chunk.Size;

            for (int z = 0; z < size.Z; z++)
            {
                for (int x = 0; x < size.X; x++)
                {
                    for (int y = size.Y - 1; 0 <= y; y--)
                    {
                        var blockPosition = new VectorI3(x, y, z);
                        DiffuseSkylight(chunk, ref blockPosition);
                    }
                }
            }
        }

        static void PropagateLights(Chunk chunk, ref VectorI3 blockPosition, Chunk neighborChunk, ref VectorI3 neighborBlockPosition)
        {
            var level = neighborChunk.GetSkylightLevel(ref neighborBlockPosition);
            if (level <= 1) return;

            var diffuseLevel = (byte) (level - 1);
            if (diffuseLevel <= chunk.GetSkylightLevel(ref blockPosition)) return;

            if (!CanPenetrateLight(neighborChunk, ref neighborBlockPosition)) return;
            if (!CanPenetrateLight(chunk, ref blockPosition)) return;

            chunk.SetSkylightLevel(ref blockPosition, diffuseLevel);
            DiffuseSkylight(chunk, ref blockPosition);
        }

        static Chunk GetPropagatableNeighborChunk(Chunk chunk, CubicSide side)
        {
            var neighbor = chunk.GetNeighborChunk(side);
            if (neighbor == null || neighbor.LightState < ChunkLightState.WaitPropagate)
                return null;

            return neighbor;
        }

        static void DiffuseSkylight(Chunk chunk, ref VectorI3 blockPosition)
        {
            // 1 以下はこれ以上拡散できない。
            var level = chunk.GetSkylightLevel(ref blockPosition);
            if (level <= 1) return;

            if (!CanPenetrateLight(chunk, ref blockPosition)) return;

            foreach (var side in CubicSide.Items)
            {
                var neighborBlockPosition = blockPosition + side.Direction;

                // チャンク外はスキップ。
                if (!chunk.Contains(ref neighborBlockPosition)) continue;

                // 光レベルの高い位置へは拡散しない。
                var diffuseLevel = (byte) (level - 1);
                if (diffuseLevel <= chunk.GetSkylightLevel(ref neighborBlockPosition)) continue;

                if (!CanPenetrateLight(chunk, ref neighborBlockPosition)) continue;

                chunk.SetSkylightLevel(ref neighborBlockPosition, diffuseLevel);
                DiffuseSkylight(chunk, ref neighborBlockPosition);
            }
        }
    }
}