#region Using

using System;
using System.Collections.Generic;

#endregion

namespace Willcraftia.Xna.Framework
{
    public static class CollectionHelper
    {
        public static bool IsNullOrEmpty<T>(ICollection<T> collection)
        {
            return collection == null || collection.Count == 0;
        }
    }
}
