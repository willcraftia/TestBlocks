#region Using

using System;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Component;
using Willcraftia.Xna.Framework.IO;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class FlatTerrainProcedure : IChunkProcedure
    {
        public const int DefaultHeight = 256;

        // I/F
        [PropertyIgnored]
        public IResource Resource { get; set; }

        // I/F
        [PropertyIgnored]
        public Region Region { get; set; }

        public string Name { get; set; }

        public int Height { get; set; }

        public FlatTerrainProcedure()
        {
            Height = DefaultHeight;
        }

        // I/F
        public void Generate(Chunk chunk)
        {
            var grassIndex = ResolveBlockIndex(TerrainBlockTypes.Grass);
            var dirtIndex = ResolveBlockIndex(TerrainBlockTypes.Dirt);
            var mantleIndex = ResolveBlockIndex(TerrainBlockTypes.Mantle);
            var sandIndex = ResolveBlockIndex(TerrainBlockTypes.Sand);
            var snowIndex = ResolveBlockIndex(TerrainBlockTypes.Snow);
            var stoneIndex = ResolveBlockIndex(TerrainBlockTypes.Stone);

            var biome = Region.BiomeManager.GetBiome(chunk);

            var size = chunk.Size;

            for (int x = 0; x < size.X; x++)
            {
                for (int z = 0; z < size.Z; z++)
                {
                    var absoluteX = chunk.CalculateBlockPositionX(x);
                    var absoluteZ = chunk.CalculateBlockPositionZ(z);
                    var biomeElement = biome.GetBiomeElement(absoluteX, absoluteZ);

                    for (int y = 0; y < size.Y; y++)
                    {
                        byte blockIndex = Block.EmptyIndex;

                        if (Height == y)
                        {
                            // Horizon.
                            switch (biomeElement)
                            {
                                case BiomeElement.Desert:
                                    blockIndex = sandIndex;
                                    break;
                                case BiomeElement.Forest:
                                    blockIndex = dirtIndex;
                                    break;
                                case BiomeElement.Mountains:
                                    blockIndex = stoneIndex;
                                    break;
                                case BiomeElement.Plains:
                                    blockIndex = grassIndex;
                                    break;
                                case BiomeElement.Snow:
                                    blockIndex = snowIndex;
                                    break;
                            }
                        }
                        else
                        {
                            // Below the horizon.
                            switch (biomeElement)
                            {
                                case BiomeElement.Desert:
                                    blockIndex = sandIndex;
                                    break;
                                case BiomeElement.Forest:
                                    blockIndex = dirtIndex;
                                    break;
                                case BiomeElement.Mountains:
                                    blockIndex = stoneIndex;
                                    break;
                                case BiomeElement.Plains:
                                    blockIndex = dirtIndex;
                                    break;
                                case BiomeElement.Snow:
                                    blockIndex = snowIndex;
                                    break;
                            }
                        }

                        chunk[x, y, z] = blockIndex;
                    }
                }
            }
        }

        byte ResolveBlockIndex(TerrainBlockTypes terrainBlockType)
        {
            var byteType = (byte) terrainBlockType;
            if (Region.BlockCatalog.Contains(byteType))
                return byteType;

            return Block.EmptyIndex;
        }
    }
}
