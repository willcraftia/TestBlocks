#region Using

using System;
using System.Collections.ObjectModel;

#endregion

namespace Willcraftia.Xna.Framework.Diagnostics
{
    public sealed class TimeRulerMarkerCollection : KeyedCollection<string, TimeRulerMarker>
    {
        protected override string GetKeyForItem(TimeRulerMarker item)
        {
            return item.Name;
        }
    }
}
