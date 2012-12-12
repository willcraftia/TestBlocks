#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Component
{
    public interface IComponentFactoryAware
    {
        ComponentFactory ComponentFactory { set; }
    }
}
