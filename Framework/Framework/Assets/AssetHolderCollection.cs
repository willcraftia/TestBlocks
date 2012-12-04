#region Using

using System;
using Willcraftia.Xna.Framework.Collections;

#endregion

namespace Willcraftia.Xna.Framework.Assets
{
    public sealed class AssetHolderCollection : KeyedList<Uri, AssetHolder>
    {
        public bool TryGetItem(Uri key, out AssetHolder item)
        {
            return Dictionary.TryGetValue(key, out item);
        }

        protected override Uri GetKeyForItem(AssetHolder item)
        {
            return item.Uri;
        }
    }
}
