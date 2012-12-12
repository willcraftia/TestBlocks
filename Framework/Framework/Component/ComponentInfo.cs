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

        ComponentPropertyCollection references;

        public Type Type { get; private set; }

        public ReadOnlyCollection<PropertyInfo> Properties
        {
            get { return properties == null ? readOnlyEmpty : properties.AsReadOnly(); }
        }

        public ReadOnlyCollection<PropertyInfo> References
        {
            get { return references == null ? readOnlyEmpty : references.AsReadOnly(); }
        }

        internal ComponentInfo(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (!IsComponent(type)) throw new ArgumentException("Type not component: " + type);

            Type = type;

            foreach (var property in type.GetProperties())
            {
                if (IsPropertyIngnored(property) || !property.CanRead || !property.CanWrite)
                    continue;

                if (IsComponent(property.PropertyType))
                {
                    if (references == null) references = new ComponentPropertyCollection();
                    references.Add(property);
                }
                else
                {
                    if (properties == null) properties = new ComponentPropertyCollection();
                    properties.Add(property);
                }
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

        public bool ReferenceExists(string name)
        {
            if (name == null) throw new ArgumentNullException("name");

            return references != null && references.Contains(name);
        }

        public void SetPropertyValue(object instance, string name, object value)
        {
            if (instance == null) throw new ArgumentNullException("instance");
            if (name == null) throw new ArgumentNullException("name");

            var property = GetProperty(name);
            var realValue = Convert.ChangeType(value, property.PropertyType, null);
            property.SetValue(instance, realValue, null);
        }

        public void SetReferencedComponent(object instance, string name, object referencedComponent)
        {
            if (instance == null) throw new ArgumentNullException("instance");
            if (name == null) throw new ArgumentNullException("name");

            var property = GetReference(name);
            property.SetValue(instance, referencedComponent, null);
        }

        public string GetReferenceName(object instance, object referencedComponent)
        {
            if (instance == null) throw new ArgumentNullException("instance");
            if (referencedComponent == null) throw new ArgumentNullException("referencedComponent");

            if (properties == null) return null;

            foreach (var property in properties)
            {
                if (property.GetValue(instance, null) == referencedComponent)
                    return property.Name;
            }

            return null;
        }

        public object GetPropertyValue(object instance, string name)
        {
            if (instance == null) throw new ArgumentNullException("instance");
            if (name == null) throw new ArgumentNullException("name");

            var property = GetProperty(name);
            return property.GetValue(instance, null);
        }

        public object GetReferencedComponent(object instance, string name)
        {
            if (instance == null) throw new ArgumentNullException("instance");
            if (name == null) throw new ArgumentNullException("name");

            var property = GetReference(name);
            return property.GetValue(instance, null);
        }

        public void UnbindReferencedComponent(object instance, object target)
        {
            if (instance == null) throw new ArgumentNullException("instance");
            if (target == null) throw new ArgumentNullException("target");

            if (references == null) return;

            foreach (var property in references)
            {
                var reference = property.GetValue(instance, null);
                if (reference == target) property.SetValue(instance, null, null);
            }
        }

        bool IsPropertyIngnored(PropertyInfo property)
        {
            var attribute = AttributeHelper.GetAttribute<ComponentPropertyAttribute>(property);
            if (attribute == null) return false;

            return attribute.Ignored;
        }

        bool IsComponent(Type type)
        {
            return AttributeHelper.HasAttribute<ComponentAttribute>(type);
        }

        PropertyInfo GetProperty(string name)
        {
            if (!PropertyExists(name)) throw new InvalidOperationException("Property not found: " + name);

            return properties[name];
        }

        PropertyInfo GetReference(string name)
        {
            if (!ReferenceExists(name)) throw new InvalidOperationException("Referece not found: " + name);

            return references[name];
        }
    }
}
