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

        public Type Type { get; private set; }

        public ReadOnlyCollection<PropertyInfo> Properties
        {
            get { return properties == null ? readOnlyEmpty : properties.AsReadOnly(); }
        }

        public ComponentInfo(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (!IsComponentType(type)) throw new ArgumentException("Type not component: " + type);

            Type = type;

            foreach (var property in type.GetProperties())
            {
                if (IsIgnoredProperty(property) || !property.CanRead || !property.CanWrite)
                    continue;

                if (properties == null) properties = new ComponentPropertyCollection();
                properties.Add(property);
            }
        }

        public object CreateInstance()
        {
            return Type.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
        }

        public bool PropertyExists(string name)
        {
            if (name == null) throw new ArgumentNullException("name");

            return properties != null && properties.Contains(name);
        }

        public void SetPropertyValue(object instance, string propertyName, object propertyValue)
        {
            if (instance == null) throw new ArgumentNullException("instance");
            if (propertyName == null) throw new ArgumentNullException("propertyName");

            var property = GetProperty(propertyName);

            if (!property.PropertyType.IsAssignableFrom(propertyValue.GetType()))
                propertyValue = Convert.ChangeType(propertyValue, property.PropertyType, null);
            
            property.SetValue(instance, propertyValue, null);
        }

        public string GetReferenceName(object instance, object referencedComponent)
        {
            if (instance == null) throw new ArgumentNullException("instance");
            if (referencedComponent == null) throw new ArgumentNullException("referencedComponent");

            if (properties == null) return null;

            foreach (var property in properties)
            {
                if (!IsComponentReferenceProperty(property)) continue;

                if (property.GetValue(instance, null) == referencedComponent)
                    return property.Name;
            }

            return null;
        }

        public object GetPropertyValue(object instance, string propertyName)
        {
            if (instance == null) throw new ArgumentNullException("instance");
            if (propertyName == null) throw new ArgumentNullException("propertyName");

            var property = GetProperty(propertyName);
            return property.GetValue(instance, null);
        }

        public void UnbindComponentReference(object instance, object target)
        {
            if (instance == null) throw new ArgumentNullException("instance");
            if (target == null) throw new ArgumentNullException("target");

            if (properties == null) return;

            foreach (var property in properties)
            {
                if (!IsComponentReferenceProperty(property)) continue;

                var reference = property.GetValue(instance, null);
                if (reference == target) property.SetValue(instance, null, null);
            }
        }

        public bool IsComponentReference(string propertyName)
        {
            var property = GetProperty(propertyName);
            return IsComponentReferenceProperty(property);
        }

        bool IsIgnoredProperty(PropertyInfo property)
        {
            return Attribute.IsDefined(property, typeof(ComponentPropertyIgnoredAttribute));
        }

        bool IsComponentReferenceProperty(PropertyInfo property)
        {
            return IsComponentType(property.PropertyType);
        }

        bool IsComponentType(Type type)
        {
            return Attribute.IsDefined(type, typeof(ComponentAttribute));
        }

        PropertyInfo GetProperty(string propertyName)
        {
            if (!PropertyExists(propertyName))
                throw new InvalidOperationException("Property not found: " + propertyName);

            return properties[propertyName];
        }
    }
}
