#region Using

using System;
using Willcraftia.Xna.Framework.Collections;

#endregion

namespace Willcraftia.Xna.Framework.Assets
{
    public sealed class AssetHolderCollection : KeyedList<IUri, AssetHolder>
    {
        public bool TryGetItem(IUri key, out AssetHolder item)
        {
            return Dictionary.TryGetValue(key, out item);
        }

        protected override IUri GetKeyForItem(AssetHolder item)
        {
            return item.Uri;
        }
    }
}
