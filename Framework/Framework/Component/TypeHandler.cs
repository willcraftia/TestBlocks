#region Using

using System;
using System.Reflection;

#endregion

namespace Willcraftia.Xna.Framework.Component
{
    public sealed class TypeHandler : ITypeHandler
    {
        static readonly PropertyInfo[] emptryProperties = new PropertyInfo[0];

        // I/F
        public PropertyInfo[] GetProperties(Type type)
        {
            var definedProperties = type.GetProperties();

            int validPropertyCount = 0;
            for (int i = 0; i < definedProperties.Length; i++)
            {
                var property = definedProperties[i];
                if (IsValidProperty(property))
                {
                    // 有効なプロパティを数えておく。
                    validPropertyCount++;
                }
                else
                {
                    // 後の処理を省くために、無効なプロパティのインデックスに null を設定する。
                    // null ならば無効なプロパティであったということ。
                    definedProperties[i] = null;
                }
            }

            PropertyInfo[] properties;

            if (validPropertyCount == definedProperties.Length)
            {
                // 全て有効なプロパティならば definedProperties をそのまま利用。
                // あるいは、プロパティそのものが無い場合もそのまま利用。
                properties = definedProperties;
            }
            else if (validPropertyCount == 0)
            {
                // 有効なプロパティがないならば空配列。
                properties = emptryProperties;
            }
            else
            {
                // 幾つかの有効なプロパティがあるならば配列を新たに作成。
                properties = new PropertyInfo[validPropertyCount];

                int index = 0;
                foreach (var property in definedProperties)
                {
                    if (property != null)
                    {
                        properties[index] = property;
                        index++;
                    }
                }
            }

            return properties;
        }

        // I/F
        public object CreateInstance(Type type)
        {
            return type.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
        }

        bool IsValidProperty(PropertyInfo property)
        {
            return property.CanRead && property.CanWrite && !IsIgnored(property);
        }

        bool IsIgnored(PropertyInfo property)
        {
            return Attribute.IsDefined(property, typeof(PropertyIgnoredAttribute));
        }
    }
}
