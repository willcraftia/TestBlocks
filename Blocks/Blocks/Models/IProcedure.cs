#region Using

using System;
using Willcraftia.Xna.Framework.Component;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public interface IProcedure<T> : IComponentBundleFactoryAware, IComponentNameAware
    {
        ComponentBundleFactory GetComponentBundleFactory();

        string GetComponentName();

        void SetRegion(Region region);

        void Generate(T instance);
    }
}
