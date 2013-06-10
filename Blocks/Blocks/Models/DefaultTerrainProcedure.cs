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
        }

        // I/F
        public void Generate(Chunk chunk)
        {
            var chunkSize = chunk.Size;

            // バイオームを取得。
            // 選択されるブロックはバイオームに従う。
            var biome = Region.BiomeManager.GetBiome(chunk);

            for (int x = 0; x < chunkSize.X; x++)
            {
                // チャンク空間における相対ブロック位置をブロック空間の位置へ変換。
                var absoluteX = chunk.GetAbsoluteBlockPositionX(x);

                for (int z = 0; z < chunkSize.Z; z++)
                {
                    // チャンク空間における相対ブロック位置をブロック空間の位置へ変換。
                    var absoluteZ = chunk.GetAbsoluteBlockPositionZ(z);

                    // この XZ  におけるバイオーム要素を取得。
                    var biomeElement = biome.GetBiomeElement(absoluteX, absoluteZ);

                    bool topBlockExists = false;
                    for (int y = chunkSize.Y - 1; 0 <= y; y--)
                    {
                        // チャンク空間における相対ブロック位置をブロック空間の位置へ変換。
                        var absoluteY = chunk.GetAbsoluteBlockPositionY(y);

                        // 地形密度を取得。
                        var density = biome.TerrainNoise.Sample(absoluteX, absoluteY, absoluteZ);

                        byte blockIndex = Block.EmptyIndex;
                        if (0 < density)
                        {
                            // 密度 1 はブロック有り

                            if (!topBlockExists)
                            {
                                // トップ ブロックを検出。
                                blockIndex = GetBlockIndexAtTop(biomeElement);

                                topBlockExists = true;
                            }
                            else
                            {
                                blockIndex = GetBlockIndexBelowTop(biomeElement);
                            }
                        }
                        else
                        {
                            // 密度 0 はブロック無し
                            
                            // トップ ブロックを見つけていた場合はそれを OFF とする。
                            topBlockExists = false;
                        }

                        var position = new IntVector3(x, y, z);
                        chunk.SetBlockIndex(ref position, blockIndex);
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
