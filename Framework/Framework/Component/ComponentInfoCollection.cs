#region Using

using System;
using System.Collections.ObjectModel;

#endregion

namespace Willcraftia.Xna.Framework.Component
{
    public sealed class ComponentInfoCollection : KeyedCollection<Type, ComponentInfo>
    {
        protected override Type GetKeyForItem(ComponentInfo item)
        {
            return item.Type;
        }
    }
}
