#region Using

using System;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public interface IBiomeManager : IAsset
    {
        IBiome GetBiome(Chunk chunk);
    }
}
