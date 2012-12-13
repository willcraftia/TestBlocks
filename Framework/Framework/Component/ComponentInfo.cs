#region Using

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

#endregion

namespace Willcraftia.Xna.Framework.Component
{
    public sealed class ComponentInfo
    {
        static readonly PropertyInfo[] emptryProperties = new PropertyInfo[0];

        static readonly string[] emptyPropertyNames = new string[0];

        static readonly ReadOnlyCollection<string> emptyReadOnlyPropertyNames = new ReadOnlyCollection<string>(emptyPropertyNames);

        PropertyInfo[] properties;

        string[] propertyNames;

        ReadOnlyCollection<string> readOnlyPropertyNames;

        public Type ComponentType { get; private set; }

        public IList<string> PropertyNames
        {
            get
            {
                if (readOnlyPropertyNames == null)
                {
                    if (propertyNames.Length == 0)
                    {
                        readOnlyPropertyNames = emptyReadOnlyPropertyNames;
                    }
                    else
                    {
                        readOnlyPropertyNames = new ReadOnlyCollection<string>(propertyNames);
                    }
                }
                return readOnlyPropertyNames;
            }
        }

        public int PropertyCount
        {
            get { return properties.Length; }
        }

        public ComponentInfo(Type componentType)
        {
            if (componentType == null) throw new ArgumentNullException("componentType");

            ComponentType = componentType;

            var definedProperties = componentType.GetProperties();

            int validPropertyCount = 0;
            for (int i = 0; i < definedProperties.Length; i++)
            {
                var property = definedProperties[i];
                if (!property.CanRead || !property.CanWrite || IsIgnoredProperty(property))
                {
                    // 後の処理を省くために、無効なプロパティのインデックスに null を設定する。
                    // null ならば無効なプロパティであったということ。
                    definedProperties[i] = null;
                }
                else
                {
                    // 有効なプロパティを数えておく。
                    validPropertyCount++;
                }
            }

            if (validPropertyCount == definedProperties.Length)
            {
                // 同じサイズならば properties には definedProperties をそのまま設定し、
                // propertyNames を新たに作成する。

                properties = definedProperties;
                propertyNames = new string[validPropertyCount];

                int index = 0;
                foreach (var property in definedProperties)
                {
                    if (property != null)
                    {
                        propertyNames[index] = property.Name;
                        index++;
                    }
                }
            }
            else if (validPropertyCount == 0)
            {
                // 異なるサイズかつ有効なプロパティがないならば、
                // properties/propertyNames には空配列を設定する。

                properties = emptryProperties;
                propertyNames = emptyPropertyNames;
            }
            else
            {
                // 異なるサイズかつ有効なプロパティがあるならば、
                // properties/propertyNames を新たに作成する。

                properties = new PropertyInfo[validPropertyCount];
                propertyNames = new string[validPropertyCount];

                int index = 0;
                foreach (var property in definedProperties)
                {
                    if (property != null)
                    {
                        properties[index] = property;
                        propertyNames[index] = property.Name;
                        index++;
                    }
                }
            }
        }

        public object CreateInstance()
        {
            return ComponentType.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
        }

        public bool PropertyExists(string propertyName)
        {
            if (propertyName == null) throw new ArgumentNullException("propertyName");

            return -1 < GetPropertyIndex(propertyName);
        }

        public int GetPropertyIndex(string propertyName)
        {
            if (propertyName == null) throw new ArgumentNullException("propertyName");

            return Array.IndexOf(propertyNames, propertyName);
        }

        public Type GetPropertyType(string propertyName)
        {
            if (propertyName == null) throw new ArgumentNullException("propertyName");

            return GetProperty(propertyName).PropertyType;
        }

        public void SetPropertyValue(object instance, string propertyName, object propertyValue)
        {
            if (instance == null) throw new ArgumentNullException("instance");
            if (propertyName == null) throw new ArgumentNullException("propertyName");

            var property = GetProperty(propertyName);
            property.SetValue(instance, propertyValue, null);
        }

        public object GetPropertyValue(object instance, string propertyName)
        {
            if (instance == null) throw new ArgumentNullException("instance");
            if (propertyName == null) throw new ArgumentNullException("propertyName");

            var property = GetProperty(propertyName);
            return property.GetValue(instance, null);
        }

        PropertyInfo GetProperty(string propertyName)
        {
            var index = GetPropertyIndex(propertyName);
            if (index < 0)
                throw new InvalidOperationException("Property not found: " + propertyName);

            return properties[index];
        }

        bool IsIgnoredProperty(PropertyInfo property)
        {
            return Attribute.IsDefined(property, typeof(PropertyIgnoredAttribute));
        }
    }
}
