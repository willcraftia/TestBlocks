#region Using

using System;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public interface IBiomeManagerCore
    {
        Biome GetBiome(Chunk chunk);
    }
}
