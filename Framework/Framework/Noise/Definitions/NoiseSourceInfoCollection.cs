#region Using

using System;
using System.Collections.ObjectModel;

#endregion

namespace Willcraftia.Xna.Framework.Noise.Definitions
{
    public sealed class NoiseSourceInfoCollection : KeyedCollection<Type, NoiseSourceInfo>
    {
        protected override Type GetKeyForItem(NoiseSourceInfo item)
        {
            return item.Type;
        }
    }
}
