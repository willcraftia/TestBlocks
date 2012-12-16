#region Using

using System;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class DefinedBiomeManager : CatalogedBiomeManager
    {
        protected override byte GetBiomeIndex(Chunk chunk)
        {
            throw new NotImplementedException();
        }
    }
}
