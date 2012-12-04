#region Using

using System;
using Willcraftia.Xna.Framework.Collections;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class TileCollection : KeyedList<byte, Tile>
    {
        public TileCollection(int capacity)
            : base(capacity)
        {
        }

        public bool TryGetItem(byte key, out Tile item)
        {
            return Dictionary.TryGetValue(key, out item);
        }

        protected override byte GetKeyForItem(Tile item)
        {
            return item.Index;
        }
    }
}
