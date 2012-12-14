#region Using

using System;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class DefinedBiomeManagerComponent : CatalogedBiomeManagerComponent
    {
        protected override byte GetBiomeIndex(Chunk chunk)
        {
            throw new NotImplementedException();
        }
    }
}
