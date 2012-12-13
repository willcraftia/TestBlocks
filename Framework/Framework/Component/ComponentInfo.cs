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
        static readonly ReadOnlyCollection<PropertyInfo> readOnlyEmpty =
            new ReadOnlyCollection<PropertyInfo>(new List<PropertyInfo>(0));

        ComponentPropertyCollection properties;

        public Type ComponentType { get; private set; }

        public ReadOnlyCollection<PropertyInfo> Properties
        {
            get { return properties == null ? readOnlyEmpty : properties.AsReadOnly(); }
        }

        public ComponentInfo(Type componentType)
        {
            if (componentType == null) throw new ArgumentNullException("componentType");

            ComponentType = componentType;

            foreach (var property in componentType.GetProperties())
            {
                if (IsIgnoredProperty(property) || !property.CanRead || !property.CanWrite)
                    continue;

                if (properties == null) properties = new ComponentPropertyCollection();
                properties.Add(property);
            }
        }

        public object CreateInstance()
        {
            var instance = ComponentType.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);

            var componentInfoAware = instance as IComponentInfoAware;
            if (componentInfoAware != null) componentInfoAware.ComponentInfo = this;

            return instance;
        }

        public bool PropertyExists(string propertyName)
        {
            if (propertyName == null) throw new ArgumentNullException("propertyName");

            return properties != null && properties.Contains(propertyName);
        }

        public int GetPropertyIndex(string propertyName)
        {
            if (!PropertyExists(propertyName)) return -1;

            var property = properties[propertyName];
            return properties.IndexOf(property);
        }

        public Type GetPropertyType(string propertyName)
        {
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

        bool IsIgnoredProperty(PropertyInfo property)
        {
            return Attribute.IsDefined(property, typeof(ComponentPropertyIgnoredAttribute));
        }

        PropertyInfo GetProperty(string propertyName)
        {
            if (!PropertyExists(propertyName))
                throw new InvalidOperationException("Property not found: " + propertyName);

            return properties[propertyName];
        }
    }
}
