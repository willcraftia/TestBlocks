#region Using

using System;
using Willcraftia.Xna.Framework.Component;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class FlatTerrainProcedure : IProcedure<Chunk>
    {
        public ComponentBundleFactory ComponentBundleFactory { get; set; }

        public string ComponentName { get; set; }

        public void Generate(Chunk instance)
        {
        }
    }
}
