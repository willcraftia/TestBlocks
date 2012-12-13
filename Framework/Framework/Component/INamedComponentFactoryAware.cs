#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Component
{
    public interface INamedComponentFactoryAware
    {
        NamedComponentFactory NamedComponentFactory { set; }
    }
}
