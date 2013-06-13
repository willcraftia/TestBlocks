#region Using

using System;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Component;
using Willcraftia.Xna.Framework.IO;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class MaxVerticesChunkProcedure: IChunkProcedure
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

            for (int x = 0; x < chunkSize.X; x++)
            {
                for (int z = 0; z < chunkSize.Z; z++)
                {
                    for (int y = 0; y < chunkSize.Y; y++)
                    {
                        var blockIndex = Block.EmptyIndex;

                        if (y % 2 == 0)
                        {
                            if (x % 2 == 0 && z % 2 == 0)
                                blockIndex = Region.BlockCatalog.DirtIndex;
                        }
                        else
                        {
                            if (x % 2 == 1 && z % 2 == 1)
                                blockIndex = Region.BlockCatalog.DirtIndex;
                        }

                        chunk.SetBlockIndex(x, y, z, blockIndex);
                    }
                }
            }
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
