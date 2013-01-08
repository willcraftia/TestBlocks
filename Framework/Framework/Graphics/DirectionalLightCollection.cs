#region Using

using System;
using Willcraftia.Xna.Framework.Collections;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class DirectionalLightCollection : KeyedList<string, DirectionalLight>
    {
        public DirectionalLightCollection(int capacity) : base(capacity) { }

        protected override string GetKeyForItem(DirectionalLight item)
        {
            return item.Name;
        }
    }
}
