#region Using

using System;
using Willcraftia.Xna.Framework.Component;
using Willcraftia.Xna.Framework.IO;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class FlatTerrainProcedure : IChunkProcedure
    {
        [PropertyIgnored]
        public Region Region { get; set; }

        public void Generate(Chunk instance)
        {
        }
    }
}
