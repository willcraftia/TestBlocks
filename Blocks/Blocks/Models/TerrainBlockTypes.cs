#region Using

using System;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public enum TerrainBlockTypes : byte
    {
        Air = Block.EmptyIndex,
        Dirt = 1,
        Grass = 2,
        Mantle = 3,
        Sand = 4,
        Snow = 5,
        Stone = 6
    }
}
