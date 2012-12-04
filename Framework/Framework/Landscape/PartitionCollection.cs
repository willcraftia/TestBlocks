#region Using

using System;
using Willcraftia.Xna.Framework.Collections;

#endregion

namespace Willcraftia.Xna.Framework.Landscape
{
    public sealed class PartitionCollection : KeyedList<VectorI3, Partition>
    {
        public bool TryGetItem(ref VectorI3 key, out Partition item)
        {
            return Dictionary.TryGetValue(key, out item);
        }

        protected override VectorI3 GetKeyForItem(Partition item)
        {
            return item.GridPosition;
        }
    }
}
