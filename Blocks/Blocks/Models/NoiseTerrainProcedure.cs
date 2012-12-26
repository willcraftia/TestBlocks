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

        const int scale = 64;

        const int heightOffset = 256;

        const float inverseScale = 1 / (float) scale;

        const int xzScale = 32;

        const float inverseXzScale = 1 / (float) xzScale;

        // I/F
        public void Generate(Chunk chunk)
        {
            var position = chunk.Position;
            var biome = Region.BiomeManager.GetBiome(chunk);

            // TODO!!!!!
            //var density = new Select
            //{
            //    LowerSource = new Const { Value = 0 },
            //    LowerBound = 0f,
            //    UpperSource = new Const { Value = 1 },
            //    UpperBound = 1000,
            //    Controller = new Displace
            //    {
            //        DisplaceX = new Const { Value = 0 },
            //        DisplaceY = biome.TerrainNoise,
            //        DisplaceZ = new Const { Value = 0 },
            //        Source = biome.DensityNoise
            //    }
            //};

            for (int x = 0; x < chunkSize.X; x++)
            {
                var absoluteX = chunk.CalculateBlockPositionX(x);
                //var noiseX = absoluteX * inverseChunkSize.X;
                var noiseX = absoluteX * inverseXzScale;

                for (int z = 0; z < chunkSize.Z; z++)
                {
                    var absoluteZ = chunk.CalculateBlockPositionZ(z);
                    //var noiseZ = absoluteZ * inverseChunkSize.Z;
                    var noiseZ = absoluteZ * inverseXzScale;
                    var biomeElement = biome.GetBiomeElement(absoluteX, absoluteZ);

                    var terrain = biome.TerrainNoise.Sample(noiseX, 0, noiseZ);
                    int height = (int) (terrain * scale) + heightOffset;

                    for (int y = chunkSize.Y - 1; 0 <= y; y--)
                    {
                        var absoluteY = chunk.CalculateBlockPositionY(y);
                        //var noiseY = absoluteY * inverseScale;

                        byte blockIndex = Block.EmptyIndex;

                        if (height == absoluteY)
                        {
                            blockIndex = GetBlockIndexAtTop(biomeElement);
                        }
                        else if (absoluteY < height)
                        {
                            blockIndex = GetBlockIndexBelowTop(biomeElement);
                        }

                        //var d = density.Sample(noiseX, noiseY, noiseZ);
                        //if (0 < d)
                        //{
                        //    if (height == absoluteY)
                        //    {
                        //        blockIndex = GetBlockIndexAtTop(biomeElement);
                        //    }
                        //    else if (absoluteY < height)
                        //    {
                        //        blockIndex = GetBlockIndexBelowTop(biomeElement);
                        //    }
                        //}

                        chunk[x, y, z] = blockIndex;
                    }
                }
            }
        }

        byte GetBlockIndexAtTop(BiomeElement biomeElement)
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

        byte GetBlockIndexBelowTop(BiomeElement biomeElement)
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
