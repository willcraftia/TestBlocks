#region Using

using System;
using Willcraftia.Xna.Framework.Content;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public interface IBiomeManager : IAsset
    {
        IBiome GetBiome(Chunk chunk);
    }
}
