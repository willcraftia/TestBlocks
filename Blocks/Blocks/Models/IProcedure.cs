#region Using

using System;
using Willcraftia.Xna.Framework.Component;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public interface IProcedure<T> : IComponentFactoryAware, IComponentNameAware
    {
        new ComponentFactory ComponentFactory { get; set; }

        new string ComponentName { get; set; }

        void Generate(T instance);
    }
}
