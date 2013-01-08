#region Using

using System;
using Willcraftia.Xna.Framework.Collections;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class CameraCollection : KeyedList<string, ICamera>
    {
        public CameraCollection(int capacity) : base(capacity) { }

        protected override string GetKeyForItem(ICamera item)
        {
            return item.Name;
        }
    }
}
