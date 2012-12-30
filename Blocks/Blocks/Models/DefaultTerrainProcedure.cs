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
    public sealed class DefaultTerrainProcedure : IChunkProcedure
    {
        static readonly VectorI3 chunkSize = Chunk.Size;

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
            inverseChunkSize.X = 1 / (float) chunkSize.X;
            inverseChunkSize.Y = 1 / (float) chunkSize.Y;
            inverseChunkSize.Z = 1 / (float) chunkSize.Z;
        }

        // 高さスケールは 16 以下が妥当。これ以上は高低差が激しくなり過ぎる。
        const int scale = 16;

        // 少しだけ余分に補正が必要（ノイズのフラクタルには [-1,1] に従わないものもあるため）。
        const int heightOffset = 256 - scale - 8;

        const float inverseScale = 1 / (float) scale;

        // 幅スケールは 16 から 32 辺りが妥当。
        // 小さすぎると微細な高低差が増えすぎる（ノイズ定義で期待した状態よりも平地が少なくなりすぎる）。
        // 大きすぎると高低差が少なくなり過ぎる（ノイズ定義で期待した状態よりも平地が多くなりすぎる）。
        const int xzScale = 16;

        const float inverseXzScale = 1 / (float) xzScale;

        // I/F
        public void Generate(Chunk chunk)
        {
            var position = chunk.Position;
            var biome = Region.BiomeManager.GetBiome(chunk);

            for (int x = 0; x < chunkSize.X; x++)
            {
                var absoluteX = chunk.CalculateBlockPositionX(x);
                var noiseX = absoluteX * inverseXzScale;

                for (int z = 0; z < chunkSize.Z; z++)
                {
                    var absoluteZ = chunk.CalculateBlockPositionZ(z);
                    var noiseZ = absoluteZ * inverseXzScale;
                    var biomeElement = biome.GetBiomeElement(absoluteX, absoluteZ);

                    var terrain = biome.TerrainNoise.Sample(noiseX, 0, noiseZ);
                    int height = (int) (terrain * scale) + heightOffset;

                    for (int y = chunkSize.Y - 1; 0 <= y; y--)
                    {
                        var absoluteY = chunk.CalculateBlockPositionY(y);

                        byte blockIndex = Block.EmptyIndex;

                        if (height == absoluteY)
                        {
                            blockIndex = GetBlockIndexAtTop(biomeElement);
                        }
                        else if (absoluteY < height)
                        {
                            blockIndex = GetBlockIndexBelowTop(biomeElement);
                        }

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
