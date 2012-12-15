#region Using

using System;
using Willcraftia.Xna.Framework.Component;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class ChunkProcedure
    {
        public ChunkProcedureComponent Component { get; set; }

        public Region Region { get; set; }

        public void Generate(Chunk chunk)
        {
        }
    }
}
