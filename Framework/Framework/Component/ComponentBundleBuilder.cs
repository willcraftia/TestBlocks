#region Using

using System;
using System.Collections.Generic;

#endregion

namespace Willcraftia.Xna.Framework.Component
{
    public sealed class ComponentBundleBuilder
    {
        ComponentInfoManager componentInfoManager;

        List<object> components = new List<object>();

        Dictionary<string, object> nameComponentMap = new Dictionary<string, object>();

        Dictionary<object, string> externalReferenceMap;

        public ComponentBundleBuilder(ComponentInfoManager componentInfoManager)
        {
            if (componentInfoManager == null) throw new ArgumentNullException("componentInfoManager");

            this.componentInfoManager = componentInfoManager;
        }

        public void Add(string name, object component)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (component == null) throw new ArgumentNullException("component");
            if (nameComponentMap.ContainsKey(name)) throw new ArgumentException("Name duplicated: " + name);
            if (components.Contains(component)) throw new ArgumentException("Component duplicated.");

            components.Add(component);
            nameComponentMap[name] = component;
        }

        public void AddExternalReference(object component, string uri)
        {
            if (component == null) throw new ArgumentNullException("component");
            if (uri == null) throw new ArgumentNullException("uri");

            if (externalReferenceMap == null) externalReferenceMap = new Dictionary<object, string>();
            externalReferenceMap[component] = uri;
        }

        public bool Contains(string name)
        {
            return nameComponentMap.ContainsKey(name);
        }

        public void Remove(string name)
        {
            object component;
            if (nameComponentMap.TryGetValue(name, out component))
            {
                components.Remove(component);
                nameComponentMap.Remove(name);
            }
        }

        public void Clear()
        {
            components.Clear();
            nameComponentMap.Clear();
            if (externalReferenceMap != null) externalReferenceMap.Clear();
        }

        public string GetName(object component)
        {
            foreach (var nameComponentPair in nameComponentMap)
            {
                if (nameComponentPair.Value == component)
                    return nameComponentPair.Key;
            }

            throw new ArgumentException("Component not found.");
        }

        public void BuildDefinition(out ComponentBundleDefinition definition)
        {
            AddNamelessComponents();

            definition = new ComponentBundleDefinition();

            if (components.Count == 0) return;

            definition.Components = new ComponentDefinition[components.Count];
            for (int i = 0; i < components.Count; i++)
            {
                var component = components[i];
                var componentInfo = componentInfoManager.GetComponentInfo(component.GetType());

                definition.Components[i] = new ComponentDefinition
                {
                    Name = GetName(component),
                    Type = componentInfoManager.GetTypeDefinitionName(componentInfo)
                };

                if (componentInfo.PropertyCount == 0) continue;

                definition.Components[i].Properties = new PropertyDefinition[componentInfo.PropertyCount];
                for (int j = 0; j < componentInfo.PropertyCount; j++)
                {
                    var property = componentInfo.GetProperty(j);
                    var propertyType = property.PropertyType;

                    var propertyName = property.Name;
                    var propertyValue = property.GetValue(component, null);

                    string stringValue = null;
                    if (propertyType == typeof(string))
                    {
                        stringValue = propertyValue as string;
                    }
                    else if (propertyType.IsEnum)
                    {
                        stringValue = Convert.ToString(propertyValue);
                    }
                    else if (typeof(IConvertible).IsAssignableFrom(propertyType))
                    {
                        var convertible = propertyValue as IConvertible;
                        stringValue = convertible.ToString();
                    }
                    else if (components.Contains(propertyValue))
                    {
                        stringValue = GetName(propertyValue);
                    }
                    else if (externalReferenceMap != null && externalReferenceMap.ContainsKey(propertyValue))
                    {
                        stringValue = externalReferenceMap[propertyValue];
                    }

                    definition.Components[i].Properties[j] = new PropertyDefinition
                    {
                        Name = propertyName,
                        Value = stringValue
                    };
                }
            }
        }

        void AddNamelessComponents()
        {
            for (int i = 0; i < components.Count; i++)
                AddNamelessComponent(components[i]);
        }

        void AddNamelessComponent(object component)
        {
            var componentInfo = componentInfoManager.GetComponentInfo(component.GetType());

            if (componentInfo.PropertyCount == 0) return;

            for (int j = 0; j < componentInfo.PropertyCount; j++)
            {
                var property = componentInfo.GetProperty(j);
                var propertyType = property.PropertyType;

                var propertyName = property.Name;
                var propertyValue = property.GetValue(component, null);

                if (propertyType == typeof(string))
                    continue;

                if (propertyType.IsEnum)
                    continue;

                if (typeof(IConvertible).IsAssignableFrom(propertyType))
                    continue;

                if (components.Contains(propertyValue))
                    continue;

                if (externalReferenceMap != null && externalReferenceMap.ContainsKey(propertyValue))
                    continue;

                var ownerComponentName = GetName(component);
                var componentName = ownerComponentName + "_" + propertyName;
                Add(componentName, propertyValue);

                // Recursively
                AddNamelessComponent(propertyValue);
            }
        }
    }
}
