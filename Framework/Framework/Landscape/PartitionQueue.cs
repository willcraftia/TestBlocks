#region Using

using System;
using Willcraftia.Xna.Framework.Collections;

#endregion

namespace Willcraftia.Xna.Framework.Landscape
{
    public sealed class PartitionQueue : KeyedQueue<VectorI3, Partition>
    {
        public PartitionQueue(int capacity)
            : base(capacity)
        {
        }

        public bool TryGetItem(ref VectorI3 key, out Partition item)
        {
            return Dictionary.TryGetValue(key, out item);
        }

        protected override VectorI3 GetKeyForItem(Partition item)
        {
            return item.Position;
        }
    }
}
