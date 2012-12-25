#region Using

using System;
using Willcraftia.Xna.Framework.Content;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public interface IChunkProcedure : IAsset
    {
        Region Region { get; set; }

        void Initialize();

        void Generate(Chunk chunk);
    }
}
