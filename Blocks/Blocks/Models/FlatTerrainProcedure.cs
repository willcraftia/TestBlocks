﻿#region Using

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
                    var biomeElement = GetBiomeElement(chunk, biome, x, z);

                    for (int y = 0; y < size.Y; y++)
                        Generate(chunk, x, y, z, biomeElement);
                }
            }
        }

        BiomeElement GetBiomeElement(Chunk chunk, IBiome biome, int x, int z)
        {
            var absoluteX = chunk.CalculateBlockPositionX(x);
            var absoluteZ = chunk.CalculateBlockPositionZ(z);
            return biome.GetBiomeElement(absoluteX, absoluteZ);
        }

        void Generate(Chunk chunk, int x, int y, int z, BiomeElement biomeElement)
        {
            byte index = Block.EmptyIndex;

            if (Height == y)
            {
                // Horizon.
                switch (biomeElement)
                {
                    case BiomeElement.Desert:
                        index = Region.BlockCatalog.SandIndex;
                        break;
                    case BiomeElement.Forest:
                        index = Region.BlockCatalog.DirtIndex;
                        break;
                    case BiomeElement.Mountains:
                        index = Region.BlockCatalog.StoneIndex;
                        break;
                    case BiomeElement.Plains:
                        index = Region.BlockCatalog.GrassIndex;
                        break;
                    case BiomeElement.Snow:
                        index = Region.BlockCatalog.SnowIndex;
                        break;
                }
            }
            else
            {
                // Below the horizon.
                switch (biomeElement)
                {
                    case BiomeElement.Desert:
                        index = Region.BlockCatalog.SandIndex;
                        break;
                    case BiomeElement.Forest:
                        index = Region.BlockCatalog.DirtIndex;
                        break;
                    case BiomeElement.Mountains:
                        index = Region.BlockCatalog.StoneIndex;
                        break;
                    case BiomeElement.Plains:
                        index = Region.BlockCatalog.DirtIndex;
                        break;
                    case BiomeElement.Snow:
                        index = Region.BlockCatalog.SnowIndex;
                        break;
                }
            }

            chunk[x, y, z] = index;
        }
    }
}
