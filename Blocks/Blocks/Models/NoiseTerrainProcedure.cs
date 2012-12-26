#region Using

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Component;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Framework.Noise;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class NoiseTerrainProcedure : IChunkProcedure
    {
        VectorI3 chunkSize;

        Vector3 inverseChunkSize;

        // I/F
        [PropertyIgnored]
        public IResource Resource { get; set; }

        // I/F
        [PropertyIgnored]
        public Region Region { get; set; }

        public string Name { get; set; }

        // I/F
        public void Initialize()
        {
            chunkSize = Region.ChunkSize;

            inverseChunkSize.X = 1 / (float) chunkSize.X;
            inverseChunkSize.Y = 1 / (float) chunkSize.Y;
            inverseChunkSize.Z = 1 / (float) chunkSize.Z;
        }

        // I/F
        public void Generate(Chunk chunk)
        {
            var position = chunk.Position;
            var biome = Region.BiomeManager.GetBiome(chunk);

            for (int x = 0; x < chunkSize.X; x++)
            {
                var absoluteX = chunk.CalculateBlockPositionX(x);
                var noiseX = absoluteX * inverseChunkSize.X;

                for (int z = 0; z < chunkSize.Z; z++)
                {
                    var absoluteZ = chunk.CalculateBlockPositionZ(z);
                    var noiseZ = absoluteZ * inverseChunkSize.Z;
                    var biomeElement = biome.GetBiomeElement(absoluteX, absoluteZ);

                    int firstY = -1;
                    for (int y = chunkSize.Y - 1; 0 <= y; y--)
                    {
                        var absoluteY = chunk.CalculateBlockPositionY(y);
                        var noiseY = absoluteY * inverseChunkSize.Y;

                        var terrain = biome.TerrainNoise.Sample(noiseX, noiseY, noiseZ);

                        byte blockIndex = Block.EmptyIndex;
                        if (terrain != 0)
                        {
                            if (firstY == -1) firstY = y;

                            // TODO ひとまずの回避でしかない。
                            if (firstY == y && y != chunkSize.Y - 1)
                            {
                                blockIndex = GetBlockIndexAtHorizon(biomeElement);
                            }
                            else
                            {
                                blockIndex = GetBlockIndexBelowHorizon(biomeElement);
                            }
                        }

                        chunk[x, y, z] = blockIndex;
                    }
                }
            }
        }

        byte GetBlockIndexAtHorizon(BiomeElement biomeElement)
        {
            switch (biomeElement)
            {
                case BiomeElement.Desert:
                    return Region.BlockCatalog.SandIndex;
                case BiomeElement.Forest:
                    return Region.BlockCatalog.DirtIndex;
                case BiomeElement.Mountains:
                    return Region.BlockCatalog.StoneIndex;
                case BiomeElement.Plains:
                    return Region.BlockCatalog.GrassIndex;
                case BiomeElement.Snow:
                    return Region.BlockCatalog.SnowIndex;
            }

            throw new InvalidOperationException();
        }

        byte GetBlockIndexBelowHorizon(BiomeElement biomeElement)
        {
            switch (biomeElement)
            {
                case BiomeElement.Desert:
                    return Region.BlockCatalog.SandIndex;
                case BiomeElement.Forest:
                    return Region.BlockCatalog.DirtIndex;
                case BiomeElement.Mountains:
                    return Region.BlockCatalog.StoneIndex;
                case BiomeElement.Plains:
                    return Region.BlockCatalog.DirtIndex;
                case BiomeElement.Snow:
                    return Region.BlockCatalog.SnowIndex;
            }

            throw new InvalidOperationException();
        }

        #region ToString

        public override string ToString()
        {
            return "[Uri:" + ((Resource != null) ? Resource.AbsoluteUri : string.Empty) + "]";
        }

        #endregion
    }
}
