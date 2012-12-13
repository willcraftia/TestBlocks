#region Using

using System;
using Willcraftia.Xna.Framework.Component;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public interface IProcedure<T> : INamedComponentFactoryAware, IComponentNameAware
    {
        new NamedComponentFactory NamedComponentFactory { get; set; }

        new string ComponentName { get; set; }

        void Generate(T instance);
    }
}
