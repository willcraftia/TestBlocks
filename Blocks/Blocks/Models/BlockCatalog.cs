#region Using

using System;
using Willcraftia.Xna.Framework.IO;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class BlockCatalog : IAsset
    {
        // I/F
        public IResource Resource { get; set; }

        public string Name { get; set; }

        public BlockCollection Blocks { get; private set; }

        public BlockCatalog(int capacity)
        {
            Blocks = new BlockCollection(capacity);
        }
    }
}
