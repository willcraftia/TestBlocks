#region Using

using System;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public interface IBiomeManagerComponent
    {
        Biome GetBiome(Chunk chunk);
    }
}
