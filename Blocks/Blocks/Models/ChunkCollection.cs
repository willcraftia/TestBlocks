#region Using

using System;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Collections;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class ChunkCollection : KeyedList<VectorI3, Chunk>
    {
        public bool TryGetItem(ref VectorI3 key, out Chunk item)
        {
            return Dictionary.TryGetValue(key, out item);
        }

        protected override VectorI3 GetKeyForItem(Chunk item)
        {
            return item.Position;
        }
    }
}
