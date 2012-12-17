#region Using

using System;
using Willcraftia.Xna.Framework.Collections;
using Willcraftia.Xna.Framework.Content;
using Willcraftia.Xna.Framework.IO;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class BlockCatalog : KeyedList<byte, Block>, IAsset
    {
        // I/F
        public IResource Resource { get; set; }

        public string Name { get; set; }

        public BlockCatalog(int capacity)
            : base(capacity)
        {
        }

        protected override byte GetKeyForItem(Block item)
        {
            return item.Index;
        }
    }
}
