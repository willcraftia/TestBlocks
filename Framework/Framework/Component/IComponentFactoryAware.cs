#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Component
{
    public interface IComponentFactoryAware
    {
        //
        // プロパティ形式にすると ComponentInfo で検知されてしまうので注意。
        // これを綺麗に回避する方法がないためメソッドとしている。
        //

        void SetComponentFactory(ComponentFactory factory);
    }
}
