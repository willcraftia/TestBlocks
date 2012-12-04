#region Using

using System;
using Willcraftia.Xna.Framework.Collections;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class BlockCollection : KeyedList<byte, Block>
    {
        public BlockCollection(int capacity)
            : base(capacity)
        {
        }

        public bool TryGetItem(byte key, out Block item)
        {
            return Dictionary.TryGetValue(key, out item);
        }

        protected override byte GetKeyForItem(Block item)
        {
            return item.Index;
        }
    }
}
