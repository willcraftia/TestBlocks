#region Using

using System;
using System.Reflection;
using Willcraftia.Xna.Framework.Component;
using Willcraftia.Xna.Framework.Content;
using Willcraftia.Xna.Framework.IO;

#endregion

namespace Willcraftia.Xna.Blocks.Component
{
    public sealed class AssetPropertyHandler : IPropertyHandler
    {
        // 最後の IPropertyHandler として登録し、
        // それまでの IPropertyHandler で拾えなかったプロパティがアセット参照であると仮定して処理を進める。

        public AssetManager AssetManager { private get; set; }

        public ResourceManager ResourceManager { private get; set; }

        public IResource BaseResource { private get; set; }

        public bool SetPropertyValue(object component, PropertyInfo property, string propertyValue)
        {
            if (propertyValue == null) return false;

            var resource = ResourceManager.Load(BaseResource, propertyValue);
            // アセットは型が明確であるため、プロパティ型によるロードで問題がない。
            var asset = AssetManager.Load(resource, property.PropertyType);

            property.SetValue(component, asset, null);

            return true;
        }
    }
}
