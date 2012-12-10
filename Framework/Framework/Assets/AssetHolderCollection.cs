#region Using

using System;
using Willcraftia.Xna.Framework.Collections;
using Willcraftia.Xna.Framework.IO;

#endregion

namespace Willcraftia.Xna.Framework.Assets
{
    public sealed class AssetHolderCollection : KeyedList<IResource, AssetHolder>
    {
        public bool TryGetItem(IResource key, out AssetHolder item)
        {
            return Dictionary.TryGetValue(key, out item);
        }

        protected override IResource GetKeyForItem(AssetHolder item)
        {
            return item.Resource;
        }
    }
}
