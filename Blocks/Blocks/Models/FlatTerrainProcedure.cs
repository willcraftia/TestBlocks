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
            Height = 256;
        }

        // I/F
        public void Initialize() { }

        // I/F
        public void Generate(Chunk chunk)
        {
            var chunkSize = chunk.Size;
            var chunkPosition = chunk.Position;
            var biome = Region.BiomeManager.GetBiome(chunk);

            for (int x = 0; x < chunkSize.X; x++)
            {
                for (int z = 0; z < chunkSize.Z; z++)
                {
                    var biomeElement = GetBiomeElement(chunk, biome, x, z);

                    for (int y = 0; y < chunkSize.Y; y++)
                        Generate(chunk, ref chunkSize, ref chunkPosition, x, y, z, biomeElement);
                }
            }
        }

        BiomeElement GetBiomeElement(Chunk chunk, IBiome biome, int x, int z)
        {
            var absoluteX = chunk.GetAbsoluteBlockPositionX(x);
            var absoluteZ = chunk.GetAbsoluteBlockPositionZ(z);
            return biome.GetBiomeElement(absoluteX, absoluteZ);
        }

        void Generate(Chunk chunk, ref IntVector3 chunkSize, ref IntVector3 chunkPosition, int x, int y, int z, BiomeElement biomeElement)
        {
            var h = chunkPosition.Y * chunkSize.Y + y;

            byte index = Block.EmptyIndex;

            if (Height == h)
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
            else if (h < Height)
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

            chunk.SetBlockIndex(x, y, z, index);
        }

        #region ToString

        public override string ToString()
        {
            return "[Uri:" + ((Resource != null) ? Resource.AbsoluteUri : string.Empty) + "]";
        }

        #endregion
    }
}
