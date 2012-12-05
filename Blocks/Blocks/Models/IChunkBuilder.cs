#region Using

using System;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public interface IChunkBuilder
    {
        void PopulateProperty(string name, string value);

        void Build(Chunk chunk);
    }
}
