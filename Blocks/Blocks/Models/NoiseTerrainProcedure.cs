﻿#region Using

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

        public INoiseSource Noise { get; set; }

        public NoiseTerrainProcedure()
        {
        }

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
            var biome = Region.BiomeManager.GetBiome(chunk);

            for (int x = 0; x < chunkSize.X; x++)
            {
                for (int z = 0; z < chunkSize.Z; z++)
                {
                    var biomeElement = GetBiomeElement(chunk, biome, x, z);

                    for (int y = 0; y < chunkSize.Y; y++)
                        Generate(chunk, x, y, z, biomeElement);
                }
            }
        }

        void Generate(Chunk chunk, int x, int y, int z, BiomeElement biomeElement)
        {
            var position = chunk.Position;

            var absoluteX = chunk.CalculateBlockPositionX(x);
            var absoluteY = chunk.CalculateBlockPositionY(y);
            var absoluteZ = chunk.CalculateBlockPositionZ(z);
            
            var noiseX = absoluteX * inverseChunkSize.X;
            var noiseY = absoluteY * inverseChunkSize.Y;
            var noiseZ = absoluteZ * inverseChunkSize.Z;

            var value = Noise.Sample(noiseX, noiseY, noiseZ);

            byte index = Block.EmptyIndex;
            if (value != 0) index = Region.BlockCatalog.Grass.Index;

            chunk[x, y, z] = index;
        }

        BiomeElement GetBiomeElement(Chunk chunk, IBiome biome, int x, int z)
        {
            var absoluteX = chunk.CalculateBlockPositionX(x);
            var absoluteZ = chunk.CalculateBlockPositionZ(z);
            return biome.GetBiomeElement(absoluteX, absoluteZ);
        }

        #region ToString

        public override string ToString()
        {
            return "[Uri:" + ((Resource != null) ? Resource.AbsoluteUri : string.Empty) + "]";
        }

        #endregion
    }
}
