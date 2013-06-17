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

        List<IntVector3> lightUpdatedPositions;

        RefAction<IntVector3> clearSkylightLevelAction;

        RefAction<IntVector3> rediffuseSkylightLevelAction;

        public List<Chunk> AffectedChunks { get; private set; }

        public ChunkLightUpdater(ChunkManager manager)
        {
            if (manager == null) throw new ArgumentNullException("manager");

            this.manager = manager;

            localWorld = new LocalWorld(manager, new IntVector3(3));
            lightUpdatedPositions = new List<IntVector3>(manager.ChunkSize.Y);
            AffectedChunks = new List<Chunk>(3 * 3 * 3);
            clearSkylightLevelAction = new RefAction<IntVector3>(ClearSkylightLevel);
            rediffuseSkylightLevelAction = new RefAction<IntVector3>(RediffuseSkylightLevel);
        }

        public void UpdateSkylightLevelByBlockRemoved(ref IntVector3 absoluteBlockPosition)
        {
            AffectedChunks.Clear();

            var chunkPosition = manager.GetChunkPositionByBlockPosition(absoluteBlockPosition);

            localWorld.FetchByCenter(chunkPosition);

            AddAffectedChunk(ref absoluteBlockPosition);

            var level = GetMaxNeighborSkylightLevel(ref absoluteBlockPosition);

            if (level <= 1)
            {
                localWorld.SetSkylightLevel(absoluteBlockPosition, (byte) 0);
                return;
            }

            if (level == Chunk.MaxSkylightLevel)
            {
                localWorld.SetSkylightLevel(absoluteBlockPosition, level);
            }
            else
            {
                localWorld.SetSkylightLevel(absoluteBlockPosition, (byte) (level - 1));
            }

            PushSkylight(ref absoluteBlockPosition, level);

            localWorld.Clear();
        }

        public void UpdateSkylightLevelByBlockCreated(ref IntVector3 absoluteBlockPosition)
        {
            AffectedChunks.Clear();

            var chunkPosition = manager.GetChunkPositionByBlockPosition(absoluteBlockPosition);

            localWorld.FetchByCenter(chunkPosition);

            AddAffectedChunk(ref absoluteBlockPosition);

            var oldLevel = localWorld.GetSkylightLevel(absoluteBlockPosition);

            if (oldLevel == 0)
            {
                // 元より光が届いていないため、ブロックの配置による影響はない。
                localWorld.SetSkylightLevel(absoluteBlockPosition, (byte) 0);
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

        void RecalculateSkylightFrom(ref IntVector3 absoluteBlockPosition)
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
                    var topPosition = new IntVector3(x, max.Y + 1, z);
                    var topLevel = localWorld.GetSkylightLevel(topPosition);
                    
                    int y = max.Y;
                    var position = new IntVector3(x, 0, z);

                    if (topLevel == Chunk.MaxSkylightLevel && CanPenetrateLight(ref topPosition))
                    {
                        for (; min.Y <= y; y--)
                        {
                            position.Y = y;
                            if (!CanPenetrateLight(ref position)) break;

                            localWorld.SetSkylightLevel(position, Chunk.MaxSkylightLevel);
                        }
                    }

                    for (; min.Y <= y; y--)
                    {
                        position.Y = y;
                        localWorld.SetSkylightLevel(position, (byte) 0);
                    }
                }
            }

            var diffuseBounds = new IntBoundingBox();
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
                        var position = new IntVector3(x, y, z);
                        DiffuseSkylight(ref position, ref diffuseBounds);
                    }
                }
            }
        }

        void RediffuseSkylightFrom(ref IntVector3 absoluteBlockPosition, byte oldLevel)
        {
            var diamond = new IntBoundingDiamond(absoluteBlockPosition, oldLevel);
            diamond.ForEach(clearSkylightLevelAction);

            diamond.Level = oldLevel + 1;
            diamond.ForEach(rediffuseSkylightLevelAction, oldLevel + 1);
        }

        void ClearSkylightLevel(ref IntVector3 absoluteBlockPosition)
        {
            localWorld.SetSkylightLevel(absoluteBlockPosition, (byte) 0);
        }

        void RediffuseSkylightLevel(ref IntVector3 absoluteBlockPosition)
        {
            var level = localWorld.GetSkylightLevel(absoluteBlockPosition);
            if (1 < level)
                PushSkylight(ref absoluteBlockPosition, level);
        }

        byte GetMaxNeighborSkylightLevel(ref IntVector3 absoluteBlockPosition)
        {
            byte maxLevel = 0;
            for (int i = 0; i < Side.Count; i++)
            {
                var side = Side.Items[i];

                var neighborBlockPosition = absoluteBlockPosition + side.Direction;

                var level = localWorld.GetSkylightLevel(neighborBlockPosition);
                if (maxLevel < level && CanPenetrateLight(ref neighborBlockPosition))
                    maxLevel = level;
            }

            return maxLevel;
        }

        void PushSkylight(ref IntVector3 absoluteBlockPosition, byte level)
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
                    if (localWorld.GetSkylightLevel(bottomNeighborPosition) == Chunk.MaxSkylightLevel)
                        break;

                    // 光を通さない位置に到達したら終了。
                    if (!CanPenetrateLight(ref bottomNeighborPosition)) break;

                    localWorld.SetSkylightLevel(bottomNeighborPosition, Chunk.MaxSkylightLevel);

                    // 更新記録を追加。
                    lightUpdatedPositions.Add(bottomNeighborPosition);

                    // 影響のあったチャンクを記録。
                    AddAffectedChunk(ref bottomNeighborPosition);
                }
            }

            for (int i = 0; i < lightUpdatedPositions.Count; i++)
            {
                var position = lightUpdatedPositions[i];
                DiffuseSkylight(ref position);
            }

            lightUpdatedPositions.Clear();
        }

        void DiffuseSkylight(ref IntVector3 absoluteBlockPosition)
        {
            // 1 以下はこれ以上拡散できない。
            var level = localWorld.GetSkylightLevel(absoluteBlockPosition);
            if (level <= 1) return;

            if (!CanPenetrateLight(ref absoluteBlockPosition)) return;

            var diffuseLevel = (byte) (level - 1);

            for (int i = 0; i < Side.Count; i++)
            {
                var side = Side.Items[i];

                var neighborBlockPosition = absoluteBlockPosition + side.Direction;

                // 光レベルの高い位置へは拡散しない。
                if (diffuseLevel <= localWorld.GetSkylightLevel(neighborBlockPosition)) continue;

                if (!CanPenetrateLight(ref neighborBlockPosition)) continue;

                localWorld.SetSkylightLevel(neighborBlockPosition, diffuseLevel);
                DiffuseSkylight(ref neighborBlockPosition);

                // 影響のあったチャンクを記録。
                AddAffectedChunk(ref neighborBlockPosition);
            }
        }

        void DiffuseSkylight(ref IntVector3 absoluteBlockPosition, ref IntBoundingBox bounds)
        {
            var level = localWorld.GetSkylightLevel(absoluteBlockPosition);
            if (level <= 1) return;

            if (!CanPenetrateLight(ref absoluteBlockPosition)) return;

            var diffuseLevel = (byte) (level - 1);

            for (int i = 0; i < Side.Count; i++)
            {
                var side = Side.Items[i];

                var neighborBlockPosition = absoluteBlockPosition + side.Direction;

                // 範囲外の位置については拡散をシミュレートしない。
                if (!bounds.Contains(ref neighborBlockPosition)) continue;

                if (diffuseLevel <= localWorld.GetSkylightLevel(neighborBlockPosition)) continue;

                if (!CanPenetrateLight(ref neighborBlockPosition)) continue;

                localWorld.SetSkylightLevel(neighborBlockPosition, diffuseLevel);
                DiffuseSkylight(ref neighborBlockPosition, ref bounds);

                // 影響のあったチャンクを記録。
                AddAffectedChunk(ref neighborBlockPosition);
            }
        }

        bool CanPenetrateLight(ref IntVector3 absoluteBlockPosition)
        {
            var block = localWorld.GetBlock(absoluteBlockPosition);
            return ChunkLightBuilder.CanPenetrateLight(block);
        }

        void AddAffectedChunk(ref IntVector3 absoluteBlockPosition)
        {
            var affectedChunk = localWorld.GetChunk(absoluteBlockPosition);

            if (!AffectedChunks.Contains(affectedChunk))
                AffectedChunks.Add(affectedChunk);
        }
    }
}
