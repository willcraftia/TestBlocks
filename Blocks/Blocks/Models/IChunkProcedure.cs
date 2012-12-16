#region Using

using System;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public interface IChunkProcedure
    {
        Region Region { get; set; }

        void Generate(Chunk instance);
    }
}
