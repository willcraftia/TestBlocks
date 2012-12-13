#region Using

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

#endregion

namespace Willcraftia.Xna.Framework.Component
{
    public sealed class NamedComponentFactory
    {
        #region Holder

        class Holder
        {
            public NamedComponentDefinition Definition;

            public ComponentInfo ComponentInfo;

            public object Component;
        }

        #endregion

        #region HolderCollection

        class HolderCollection : KeyedCollection<string, Holder>
        {
            protected override string GetKeyForItem(Holder item)
            {
                return item.Definition.Name;
            }
        }

        #endregion

        IComponentTypeRegistory typeRegistory = DefaultComponentTypeRegistory.Instance;

        ComponentInfoCollection componentInfoCache = new ComponentInfoCollection();

        HolderCollection holders = new HolderCollection();

        public object this[string componentName]
        {
            get
            {
                if (componentName == null) throw new ArgumentNullException("componentName");
                return holders[componentName].Component;
            }
        }

        public NamedComponentFactory() { }

        public NamedComponentFactory(IComponentTypeRegistory typeRegistory)
        {
            if (typeRegistory == null) throw new ArgumentNullException("typeRegistory");

            this.typeRegistory = typeRegistory;
        }

        public ComponentInfo GetComponentInfo(string typeName)
        {
            if (typeName == null) throw new ArgumentNullException("typeName");

            var type = typeRegistory.ResolveType(typeName);
            return GetComponentInfo(type);
        }

        public ComponentInfo GetComponentInfo(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");

            if (componentInfoCache.Contains(type)) return componentInfoCache[type];

            var componentInfo = new ComponentInfo(type);
            componentInfoCache.Add(componentInfo);
            return componentInfo;
        }

        public bool ContainsComponentName(string componentName)
        {
            if (componentName == null) throw new ArgumentNullException("componentName");

            return holders.Contains(componentName);
        }

        public bool ContainsComponent(object component)
        {
            if (component == null) throw new ArgumentNullException("component");

            foreach (var holder in holders)
            {
                if (holder.Component == component) return true;
            }

            return false;
        }

        public string GetComponentName(object component)
        {
            if (component == null) throw new ArgumentNullException("component");

            foreach (var holder in holders)
            {
                if (holder.Component == component) return holder.Definition.Name;
            }

            throw new ArgumentException("Component not found.");
        }

        void AssertContainsComponentName(string componentName)
        {
            if (!ContainsComponentName(componentName))
                throw new InvalidOperationException("Component not found: " + componentName);
        }

        object CreateComponent(ComponentInfo componentInfo)
        {
            var component = componentInfo.CreateInstance();

            var namedComponentFactoryAware = component as INamedComponentFactoryAware;
            if (namedComponentFactoryAware != null) namedComponentFactoryAware.NamedComponentFactory = this;

            return component;
        }

        object CreateComponent(ComponentInfo componentInfo, string componentName)
        {
            var component = CreateComponent(componentInfo);

            var componentNameAware = component as IComponentNameAware;
            if (componentNameAware != null) componentNameAware.ComponentName = componentName;

            return component;
        }

        //====================================================================
        //
        // For editors.
        //

        public void AddComponent(string componentName, Type type)
        {
            AddComponent(componentName, GetComponentInfo(type));
        }

        public void AddComponent(string componentName, string typeName)
        {
            AddComponent(componentName, GetComponentInfo(typeName));
        }

        public void AddComponent(string componentName, ComponentInfo componentInfo)
        {
            if (componentName == null) throw new ArgumentNullException("componentName");
            if (componentInfo == null) throw new ArgumentNullException("componentInfo");

            if (!componentInfoCache.Contains(componentInfo))
                componentInfoCache.Add(componentInfo);

            ComponentPropertyDefinition[] propertyDefinitions = null;
            if (!CollectionHelper.IsNullOrEmpty(componentInfo.Properties))
                propertyDefinitions = new ComponentPropertyDefinition[componentInfo.Properties.Count];

            var typeName = typeRegistory.ResolveTypeName(componentInfo.ComponentType);

            var holder = new Holder
            {
                Definition = new NamedComponentDefinition
                {
                    Name = componentName,
                    Component = new ComponentDefinition
                    {
                        Type = typeName,
                        Properties = propertyDefinitions
                    }
                },
                ComponentInfo = componentInfo
            };

            holders.Add(holder);
        }

        public void SetPropertyValue(string componentName, string propertyName, object propertyValue)
        {
            if (componentName == null) throw new ArgumentNullException("componentName");
            if (propertyName == null) throw new ArgumentNullException("propertyName");
            AssertContainsComponentName(componentName);

            var holder = holders[componentName];
            var componentInfo = holder.ComponentInfo;

            var propertyIndex = componentInfo.GetPropertyIndex(propertyName);
            if (propertyIndex < 0) throw new ArgumentNullException("Property not found: " + propertyName);

            holder.Definition.Component.Properties[propertyIndex].Name = propertyName;
            holder.Definition.Component.Properties[propertyIndex].Value = Convert.ToString(propertyValue);
        }

        public void SetComponentReference(string componentName, string propertyName, string referenceName)
        {
            SetPropertyValue(componentName, propertyName, referenceName);
        }

        public void RemoveComponent(string componentName)
        {
            holders.Remove(componentName);
        }

        public void Clear()
        {
            holders.Clear();
        }

        public void Build()
        {
            foreach (var holder in holders)
            {
                if (holder.Component == null)
                    holder.Component = CreateComponent(holder.ComponentInfo, holder.Definition.Name);
            }

            foreach (var holder in holders)
            {
                var componentInfo = holder.ComponentInfo;

                var propertyDefinitions = holder.Definition.Component.Properties;
                if (ArrayHelper.IsNullOrEmpty(propertyDefinitions)) continue;

                for (int i = 0; i < propertyDefinitions.Length; i++)
                {
                    var propertyName = propertyDefinitions[i].Name;
                    if (string.IsNullOrEmpty(propertyName)) continue;

                    var propertyType = componentInfo.GetPropertyType(propertyName);
                    var propertyValueString = propertyDefinitions[i].Value;

                    bool propertyHandled = false;
                    if (propertyType == typeof(string))
                    {
                        componentInfo.SetPropertyValue(holder.Component, propertyName, propertyValueString);
                        propertyHandled = true;
                    }
                    else if (typeof(IConvertible).IsAssignableFrom(propertyType))
                    {
                        var convertedValue = Convert.ChangeType(propertyValueString, propertyType, null);
                        componentInfo.SetPropertyValue(holder.Component, propertyName, convertedValue);
                        propertyHandled = true;
                    }
                    else if (ContainsComponentName(propertyValueString))
                    {
                        var referencedComponent = this[propertyValueString];
                        if (propertyType.IsAssignableFrom(referencedComponent.GetType()))
                        {
                            componentInfo.SetPropertyValue(holder.Component, propertyName, referencedComponent);
                            propertyHandled = true;
                        }
                    }

                    if (!propertyHandled)
                        throw new InvalidOperationException("Property not handled: " + propertyName);
                }
            }
        }

        //
        //====================================================================

        //====================================================================
        //
        // For deserialization.
        //

        public void Initialize(ref ComponentBundleDefinition definition)
        {
            if (ArrayHelper.IsNullOrEmpty(definition.NamedComponents)) return;

            for (int i = 0; i < definition.NamedComponents.Length; i++)
                AddComponentDefinition(ref definition.NamedComponents[i]);
        }

        void AddComponentDefinition(ref NamedComponentDefinition definition)
        {
            if (ContainsComponentName(definition.Name))
                throw new ArgumentException("Component name duplicated: " + definition.Name);

            var internalDefinition = new NamedComponentDefinition
            {
                Name = definition.Name,
                Component = new ComponentDefinition
                {
                    Type = definition.Component.Type
                }
            };

            var componentInfo = GetComponentInfo(definition.Component.Type);

            var properties = componentInfo.Properties;
            if (!CollectionHelper.IsNullOrEmpty(properties))
            {
                internalDefinition.Component.Properties = new ComponentPropertyDefinition[properties.Count];

                var propertyDefinitions = definition.Component.Properties;
                if (!ArrayHelper.IsNullOrEmpty(propertyDefinitions))
                {
                    for (int i = 0; i < propertyDefinitions.Length; i++)
                    {
                        var propertyName = propertyDefinitions[i].Name;
                        var propertyValue = propertyDefinitions[i].Value;

                        var propertyIndex = componentInfo.GetPropertyIndex(propertyName);
                        internalDefinition.Component.Properties[propertyIndex] = new ComponentPropertyDefinition
                        {
                            Name = propertyName,
                            Value = propertyValue
                        };
                    }
                }
            }

            var holder = new Holder
            {
                Definition = internalDefinition,
                ComponentInfo = GetComponentInfo(definition.Component.Type)
            };

            holders.Add(holder);
        }

        //
        //====================================================================

        //====================================================================
        //
        // For serialization
        //

        public void GetDefinition(out ComponentBundleDefinition definition)
        {
            definition = new ComponentBundleDefinition();

            if (!CollectionHelper.IsNullOrEmpty(holders))
            {
                definition.NamedComponents = new NamedComponentDefinition[holders.Count];

                for (int i = 0; i < holders.Count; i++)
                {
                    definition.NamedComponents[i] = new NamedComponentDefinition
                    {
                        Name = holders[i].Definition.Name,
                        Component = new ComponentDefinition
                        {
                            Type = holders[i].Definition.Component.Type
                        }
                    };

                    var propertyDefinitions = holders[i].Definition.Component.Properties;
                    if (!ArrayHelper.IsNullOrEmpty(propertyDefinitions))
                    {
                        int definedPropertyCount = 0;
                        for (int j = 0; j < propertyDefinitions.Length; j++)
                        {
                            if (!string.IsNullOrEmpty(propertyDefinitions[j].Name))
                                definedPropertyCount++;
                        }

                        definition.NamedComponents[i].Component.Properties = new ComponentPropertyDefinition[definedPropertyCount];
                        for (int j = 0, k = 0; j < propertyDefinitions.Length; j++, k++)
                        {
                            if (!string.IsNullOrEmpty(propertyDefinitions[j].Name))
                            {
                                definition.NamedComponents[i].Component.Properties[k] = new ComponentPropertyDefinition
                                {
                                    Name = propertyDefinitions[j].Name,
                                    Value = propertyDefinitions[j].Value
                                };
                            }
                        }
                    }
                }
            }
        }

        //
        //====================================================================
    }
}
