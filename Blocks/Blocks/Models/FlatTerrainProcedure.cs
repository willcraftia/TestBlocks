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
                                    blockIndex = Region.BlockCatalog.SandIndex;
                                    break;
                                case BiomeElement.Forest:
                                    blockIndex = Region.BlockCatalog.DirtIndex;
                                    break;
                                case BiomeElement.Mountains:
                                    blockIndex = Region.BlockCatalog.StoneIndex;
                                    break;
                                case BiomeElement.Plains:
                                    blockIndex = Region.BlockCatalog.GrassIndex;
                                    break;
                                case BiomeElement.Snow:
                                    blockIndex = Region.BlockCatalog.SnowIndex;
                                    break;
                            }
                        }
                        else
                        {
                            // Below the horizon.
                            switch (biomeElement)
                            {
                                case BiomeElement.Desert:
                                    blockIndex = Region.BlockCatalog.SandIndex;
                                    break;
                                case BiomeElement.Forest:
                                    blockIndex = Region.BlockCatalog.DirtIndex;
                                    break;
                                case BiomeElement.Mountains:
                                    blockIndex = Region.BlockCatalog.StoneIndex;
                                    break;
                                case BiomeElement.Plains:
                                    blockIndex = Region.BlockCatalog.DirtIndex;
                                    break;
                                case BiomeElement.Snow:
                                    blockIndex = Region.BlockCatalog.SnowIndex;
                                    break;
                            }
                        }

                        chunk[x, y, z] = blockIndex;
                    }
                }
            }
        }
    }
}
