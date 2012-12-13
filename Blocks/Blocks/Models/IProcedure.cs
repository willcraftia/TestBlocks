#region Using

using System;
using Willcraftia.Xna.Framework.Component;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public interface IProcedure<T> : IComponentBundleFactoryAware, IComponentNameAware
    {
        new ComponentBundleFactory ComponentBundleFactory { get; set; }

        new string ComponentName { get; set; }

        void Generate(T instance);
    }
}
