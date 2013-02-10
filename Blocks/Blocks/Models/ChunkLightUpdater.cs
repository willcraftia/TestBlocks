#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Willcraftia.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class ChunkLightUpdater
    {
        // TODO
        //
        // 今は非同期に同一チャンクに対して光レベル構築が実行されている。
        // これを排他制御しなければならない。

        ChunkManager manager;

        LocalWorld localWorld;

        List<VectorI3> lightUpdatedPositions;

        RefAction<VectorI3> clearSkylightLevelAction;

        RefAction<VectorI3> rediffuseSkylightLevelAction;

        public List<VectorI3> AffectedChunkPositions { get; private set; }

        public ChunkLightUpdater(ChunkManager manager)
        {
            if (manager == null) throw new ArgumentNullException("manager");

            this.manager = manager;

            localWorld = new LocalWorld(manager, new VectorI3(3));
            lightUpdatedPositions = new List<VectorI3>(manager.ChunkSize.Y);
            AffectedChunkPositions = new List<VectorI3>(3 * 3 * 3);
            clearSkylightLevelAction = new RefAction<VectorI3>(ClearSkylightLevel);
            rediffuseSkylightLevelAction = new RefAction<VectorI3>(RediffuseSkylightLevel);
        }

        public void UpdateSkylightLevelByBlockRemoved(ref VectorI3 absoluteBlockPosition)
        {
            AffectedChunkPositions.Clear();

            VectorI3 chunkPosition;
            manager.GetChunkPositionByBlockPosition(ref absoluteBlockPosition, out chunkPosition);

            localWorld.FetchByCenter(chunkPosition);

            AffectedChunkPositions.Add(chunkPosition);

            var level = GetMaxNeighborSkylightLevel(ref absoluteBlockPosition);

            if (level <= 1)
            {
                localWorld.SetSkylightLevel(ref absoluteBlockPosition, (byte) 0);
                return;
            }

            localWorld.SetSkylightLevel(ref absoluteBlockPosition, (byte) (level - 1));

            PushSkylight(ref absoluteBlockPosition, level);

            localWorld.Clear();
        }

        public void UpdateSkylightLevelByBlockCreated(ref VectorI3 absoluteBlockPosition)
        {
            AffectedChunkPositions.Clear();

            VectorI3 chunkPosition;
            manager.GetChunkPositionByBlockPosition(ref absoluteBlockPosition, out chunkPosition);

            localWorld.FetchByCenter(chunkPosition);

            AffectedChunkPositions.Add(chunkPosition);

            var oldLevel = localWorld.GetSkylightLevel(ref absoluteBlockPosition);

            if (oldLevel == 0)
            {
                // 元より光が届いていないため、ブロックの配置による影響はない。
                localWorld.SetSkylightLevel(ref absoluteBlockPosition, (byte) 0);
                return;
            }

            if (oldLevel < Chunk.MaxSkylightLevel)
            {
                // 直射日光が届いていないため、部分的な再計算で済む。
                RediffuseSkylightFrom(ref absoluteBlockPosition, oldLevel);
                return;
            }

            if (oldLevel == Chunk.MaxSkylightLevel)
            {
                // 直射日光を遮るため、範囲を広げて再計算。
                RecalculateSkylightFrom(ref absoluteBlockPosition);
                return;
            }
        }

        void RecalculateSkylightFrom(ref VectorI3 absoluteBlockPosition)
        {
            var maxDistance = Chunk.MaxSkylightLevel - 1;

            var min = absoluteBlockPosition;
            min.X -= maxDistance;
            min.Y -= maxDistance;
            min.Z -= maxDistance;

            var max = absoluteBlockPosition;
            max.X += maxDistance;
            max.Y += maxDistance;
            max.Z += maxDistance;

            for (int z = min.Z; z < max.Z; z++)
            {
                for (int x = min.X; x < max.X; x++)
                {
                    var topPosition = new VectorI3(x, max.Y + 1, z);
                    var topLevel = localWorld.GetSkylightLevel(ref topPosition);
                    
                    int y = max.Y;
                    var position = new VectorI3(x, 0, z);

                    if (topLevel == Chunk.MaxSkylightLevel && CanPenetrateLight(ref topPosition))
                    {
                        for (; min.Y <= y; y--)
                        {
                            position.Y = y;
                            if (!CanPenetrateLight(ref position)) break;

                            localWorld.SetSkylightLevel(ref position, Chunk.MaxSkylightLevel);
                        }
                    }

                    for (; min.Y <= y; y--)
                    {
                        position.Y = y;
                        localWorld.SetSkylightLevel(ref position, (byte) 0);
                    }
                }
            }

            var diffuseBounds = new BoundingBoxI();
            diffuseBounds.Min.X = min.X;
            diffuseBounds.Min.Y = min.Y;
            diffuseBounds.Min.Z = min.Z;
            diffuseBounds.Size.X = max.X - diffuseBounds.Min.X;
            diffuseBounds.Size.Y = max.Y - diffuseBounds.Min.Y;
            diffuseBounds.Size.Z = max.Z - diffuseBounds.Min.Z;

            for (int z = min.Z - 1; z < max.Z + 1; z++)
            {
                for (int x = min.X - 1; x < max.X + 1; x++)
                {
                    for (int y = max.Y; min.Y <= y; y--)
                    {
                        var position = new VectorI3(x, y, z);
                        DiffuseSkylight(ref position, ref diffuseBounds);
                    }
                }
            }
        }

        void RediffuseSkylightFrom(ref VectorI3 absoluteBlockPosition, byte oldLevel)
        {
            var diamond = new BoundingDiamondI(absoluteBlockPosition, oldLevel);
            diamond.ForEach(clearSkylightLevelAction);

            diamond.Level = oldLevel + 1;
            diamond.ForEach(rediffuseSkylightLevelAction, oldLevel + 1);
        }

        void ClearSkylightLevel(ref VectorI3 absoluteBlockPosition)
        {
            localWorld.SetSkylightLevel(ref absoluteBlockPosition, (byte) 0);
        }

        void RediffuseSkylightLevel(ref VectorI3 absoluteBlockPosition)
        {
            var level = localWorld.GetSkylightLevel(ref absoluteBlockPosition);
            if (1 < level)
                PushSkylight(ref absoluteBlockPosition, level);
        }

        byte GetMaxNeighborSkylightLevel(ref VectorI3 absoluteBlockPosition)
        {
            byte maxLevel = 0;
            for (int i = 0; i < CubicSide.Count; i++)
            {
                var side = CubicSide.Items[i];

                var neighborBlockPosition = absoluteBlockPosition + side.Direction;

                var level = localWorld.GetSkylightLevel(ref neighborBlockPosition);
                if (maxLevel < level && CanPenetrateLight(ref neighborBlockPosition))
                    maxLevel = level;
            }

            return maxLevel;
        }

        void PushSkylight(ref VectorI3 absoluteBlockPosition, byte level)
        {
            lightUpdatedPositions.Add(absoluteBlockPosition);

            var minY = localWorld.Min.Y * manager.ChunkSize.Y;

            // 直射日光のシミュレーション。
            if (level == Chunk.MaxSkylightLevel && minY < absoluteBlockPosition.Y)
            {
                var bottomNeighborPosition = absoluteBlockPosition;
                for (int y = absoluteBlockPosition.Y - 1; minY <= y; y--)
                {
                    bottomNeighborPosition.Y = y;

                    // 最大レベルならば、それ以上は直射日光をシミュレートしなくて良い。
                    if (localWorld.GetSkylightLevel(ref bottomNeighborPosition) == Chunk.MaxSkylightLevel)
                        break;

                    // 光を通さない位置に到達したら終了。
                    if (!CanPenetrateLight(ref bottomNeighborPosition)) break;

                    localWorld.SetSkylightLevel(ref bottomNeighborPosition, Chunk.MaxSkylightLevel);

                    // 更新記録を追加。
                    lightUpdatedPositions.Add(bottomNeighborPosition);

                    // 影響のあったチャンクを記録。
                    VectorI3 chunkPosition;
                    manager.GetChunkPositionByBlockPosition(ref bottomNeighborPosition, out chunkPosition);
                    if (!AffectedChunkPositions.Contains(chunkPosition))
                        AffectedChunkPositions.Add(chunkPosition);
                }
            }

            for (int i = 0; i < lightUpdatedPositions.Count; i++)
            {
                var position = lightUpdatedPositions[i];
                DiffuseSkylight(ref position);
            }

            lightUpdatedPositions.Clear();
        }

        void DiffuseSkylight(ref VectorI3 absoluteBlockPosition)
        {
            // 1 以下はこれ以上拡散できない。
            var level = localWorld.GetSkylightLevel(ref absoluteBlockPosition);
            if (level <= 1) return;

            if (!CanPenetrateLight(ref absoluteBlockPosition)) return;

            var diffuseLevel = (byte) (level - 1);

            for (int i = 0; i < CubicSide.Count; i++)
            {
                var side = CubicSide.Items[i];

                var neighborBlockPosition = absoluteBlockPosition + side.Direction;

                // 光レベルの高い位置へは拡散しない。
                if (diffuseLevel <= localWorld.GetSkylightLevel(ref neighborBlockPosition)) continue;

                if (!CanPenetrateLight(ref neighborBlockPosition)) continue;

                localWorld.SetSkylightLevel(ref neighborBlockPosition, diffuseLevel);
                DiffuseSkylight(ref neighborBlockPosition);

                // 影響のあったチャンクを記録。
                VectorI3 chunkPosition;
                manager.GetChunkPositionByBlockPosition(ref neighborBlockPosition, out chunkPosition);
                if (!AffectedChunkPositions.Contains(chunkPosition))
                    AffectedChunkPositions.Add(chunkPosition);
            }
        }

        void DiffuseSkylight(ref VectorI3 absoluteBlockPosition, ref BoundingBoxI bounds)
        {
            var level = localWorld.GetSkylightLevel(ref absoluteBlockPosition);
            if (level <= 1) return;

            if (!CanPenetrateLight(ref absoluteBlockPosition)) return;

            var diffuseLevel = (byte) (level - 1);

            for (int i = 0; i < CubicSide.Count; i++)
            {
                var side = CubicSide.Items[i];

                var neighborBlockPosition = absoluteBlockPosition + side.Direction;

                // 範囲外の位置については拡散をシミュレートしない。
                if (!bounds.Contains(ref neighborBlockPosition)) continue;

                if (diffuseLevel <= localWorld.GetSkylightLevel(ref neighborBlockPosition)) continue;

                if (!CanPenetrateLight(ref neighborBlockPosition)) continue;

                localWorld.SetSkylightLevel(ref neighborBlockPosition, diffuseLevel);
                DiffuseSkylight(ref neighborBlockPosition, ref bounds);

                // 影響のあったチャンクを記録。
                VectorI3 chunkPosition;
                manager.GetChunkPositionByBlockPosition(ref neighborBlockPosition, out chunkPosition);
                if (!AffectedChunkPositions.Contains(chunkPosition))
                    AffectedChunkPositions.Add(chunkPosition);
            }
        }

        bool CanPenetrateLight(ref VectorI3 absoluteBlockPosition)
        {
            var block = localWorld.GetBlock(ref absoluteBlockPosition);
            return ChunkLightBuilder.CanPenetrateLight(block);
        }
    }
}
