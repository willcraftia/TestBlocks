#region Using

using System;
using Willcraftia.Xna.Framework.Collections;

#endregion

namespace Willcraftia.Xna.Framework.Landscape
{
    internal sealed class ClusterCollection : KeyedList<VectorI3, Cluster>
    {
        internal ClusterCollection(int capacity)
            : base(capacity)
        {
        }

        protected override VectorI3 GetKeyForItem(Cluster item)
        {
            return item.Position;
        }
    }
}
