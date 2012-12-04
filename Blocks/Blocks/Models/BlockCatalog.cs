#region Using

using System;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class BlockCatalog
    {
        public Uri Uri { get; set; }

        public string Name { get; set; }

        public BlockCollection Blocks { get; private set; }

        public BlockCatalog(int capacity)
        {
            Blocks = new BlockCollection(capacity);
        }
    }
}
