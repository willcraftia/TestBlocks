#region Using

using System;
using System.Collections.ObjectModel;
using System.Reflection;

#endregion

namespace Willcraftia.Xna.Framework.Noise.Definitions
{
    public sealed class NoisePropertyInfoCollection : KeyedCollection<string, PropertyInfo>
    {
        ReadOnlyCollection<PropertyInfo> readOnly;

        public ReadOnlyCollection<PropertyInfo> AsReadOnly()
        {
            if (readOnly == null) readOnly = new ReadOnlyCollection<PropertyInfo>(Items);
            return readOnly;
        }

        protected override string GetKeyForItem(PropertyInfo item)
        {
            return item.Name;
        }
    }
}
