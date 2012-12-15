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
        static readonly string[] emptyPropertyNames = new string[0];

        ITypeHandler typeHandler;

        PropertyInfo[] properties;

        string[] propertyNames;

        public Type ComponentType { get; private set; }

        public int PropertyCount
        {
            get { return properties.Length; }
        }

        public ComponentInfo(Type componentType, ITypeHandler typeHandler)
        {
            if (componentType == null) throw new ArgumentNullException("componentType");
            if (typeHandler == null) throw new ArgumentNullException("typeHandler");

            ComponentType = componentType;
            this.typeHandler = typeHandler;

            properties = typeHandler.GetProperties(componentType);
            if (properties.Length == 0)
            {
                propertyNames = emptyPropertyNames;
            }
            else
            {
                propertyNames = new string[properties.Length];
                for (int i = 0; i < properties.Length; i++)
                    propertyNames[i] = properties[i].Name;
            }
        }

        public object CreateInstance()
        {
            return typeHandler.CreateInstance(ComponentType);
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

        public PropertyInfo GetProperty(string propertyName)
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
