#region Using

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

#endregion

namespace Willcraftia.Xna.Framework.Component
{
    public sealed class ComponentFactory
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

        ComponentTypeRegistory typeRegistory;

        ComponentInfoCollection componentInfoCollection = new ComponentInfoCollection();

        HolderCollection holders = new HolderCollection();

        public object this[string componentName]
        {
            get
            {
                if (componentName == null) throw new ArgumentNullException("componentName");
                return holders[componentName].Component;
            }
        }

        public ComponentFactory() { }

        public ComponentFactory(ComponentTypeRegistory typeRegistory)
        {
            this.typeRegistory = typeRegistory;
        }

        public ComponentInfo GetComponentInfo(string typeAlias)
        {
            if (typeAlias == null) throw new ArgumentNullException("typeAlias");

            var type = ResolveType(typeAlias);
            return GetComponentInfo(type);
        }

        public ComponentInfo GetComponentInfo(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");

            if (componentInfoCollection.Contains(type)) return componentInfoCollection[type];

            var componentInfo = new ComponentInfo(type);
            componentInfoCollection.Add(componentInfo);
            return componentInfo;
        }

        public Type ResolveType(string typeName)
        {
            Type type;
            if (typeRegistory != null)
            {
                type = typeRegistory.ResolveType(typeName);
            }
            else
            {
                type = Type.GetType(typeName, true);
            }
            return type;
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

            if (!componentInfoCollection.Contains(componentInfo))
                componentInfoCollection.Add(componentInfo);

            var component = componentInfo.CreateInstance();

            var componentFactoryAware = component as IComponentFactoryAware;
            if (componentFactoryAware != null) componentFactoryAware.ComponentFactory = this;

            var componentInfoAware = component as IComponentInfoAware;
            if (componentInfoAware != null) componentInfoAware.ComponentInfo = componentInfo;

            var componentNameAware = component as IComponentNameAware;
            if (componentNameAware != null) componentNameAware.ComponentName = componentName;

            var holder = new Holder
            {
                ComponentInfo = componentInfo,
                Component = component
            };

            // Create the initial definition.
            CreateNamedComponentDefinition(componentName, componentInfo, component, out holder.Definition);

            holders.Add(holder);
        }

        string ResolveTypeAlias(Type type)
        {
            if (typeRegistory == null) return type.FullName;

            return typeRegistory.ResolveAlias(type);
        }

        void CreateNamedComponentDefinition(string componentName, ComponentInfo componentInfo, object component,
            out NamedComponentDefinition definition)
        {
            definition = new NamedComponentDefinition
            {
                Name = componentName,
                Component = new ComponentDefinition
                {
                    Type = ResolveTypeAlias(componentInfo.Type)
                }
            };

            var properties = componentInfo.Properties;
            if (!CollectionHelper.IsNullOrEmpty(properties))
            {
                definition.Component.Properties = new ComponentPropertyDefinition[properties.Count];
                for (int i = 0; i < properties.Count; i++)
                {
                    var name = properties[i].Name;

                    string value = null;
                    if (!componentInfo.IsComponentReference(name))
                        value = Convert.ToString(componentInfo.GetPropertyValue(component, name));

                    definition.Component.Properties[i] = new ComponentPropertyDefinition
                    {
                        Name = name,
                        Value = value
                    };
                }
            }
        }

        public void SetPropertyValue(string componentName, string propertyName, object propertyValue)
        {
            if (componentName == null) throw new ArgumentNullException("componentName");
            if (propertyName == null) throw new ArgumentNullException("propertyName");
            AssertContainsComponentName(componentName);

            var holder = holders[componentName];
            var componentInfo = holder.ComponentInfo;
            var component = holder.Component;

            string referencedName = null;
            if (componentInfo.IsComponentReference(propertyName))
            {
                referencedName = propertyValue as string;
                if (referencedName == null)
                    throw new ArgumentException("Invalid value type: " + propertyValue.GetType());
                AssertContainsComponentName(referencedName);

                propertyValue = holders[referencedName].Component;
            }

            componentInfo.SetPropertyValue(holder.Component, propertyName, propertyValue);

            // Refresh the definition.
            var properties = holder.Definition.Component.Properties;
            if (!ArrayHelper.IsNullOrEmpty(properties))
            {
                for (int i = 0; i < properties.Length; i++)
                {
                    if (properties[i].Name == propertyName)
                    {
                        if (referencedName == null)
                        {
                            properties[i].Value = Convert.ToString(propertyValue);
                        }
                        else
                        {
                            properties[i].Value = referencedName;
                        }
                    }
                }
            }
        }

        public void SetComponentReference(string componentName, string propertyName, string referenceName)
        {
            SetPropertyValue(componentName, propertyName, referenceName);
        }

        public void UnbindComponentReference(string componentName, string propertyName)
        {
            if (componentName == null) throw new ArgumentNullException("componentName");
            if (propertyName == null) throw new ArgumentNullException("propertyName");
            AssertContainsComponentName(componentName);

            var holder = holders[componentName];
            var componentInfo = holder.ComponentInfo;
            var component = holder.Component;

            if (!componentInfo.IsComponentReference(propertyName)) return;

            componentInfo.SetPropertyValue(component, propertyName, null);

            // Refresh the definition.
            var properties = holder.Definition.Component.Properties;
            if (!ArrayHelper.IsNullOrEmpty(properties))
            {
                for (int i = 0; i < properties.Length; i++)
                {
                    if (properties[i].Name == propertyName)
                        properties[i].Value = null;
                }
            }
        }

        public void RemoveComponent(string componentName)
        {
            if (!holders.Contains(componentName)) return;

            var holder = holders[componentName];
            var componentInfo = holder.ComponentInfo;
            var component = holder.Component;

            for (int i = 0; i < holders.Count; i++)
            {
                var cursorHolder = holders[i];
                if (cursorHolder.Definition.Name == componentName) continue;

                // Remove all references.
                var cursorComponent = cursorHolder.Component;
                var cursorComponentInfo = cursorHolder.ComponentInfo;

                var name = cursorComponentInfo.GetReferenceName(cursorComponent, component);
                if (name != null)
                    UnbindComponentReference(cursorHolder.Definition.Name, name);
            }

            holders.Remove(holder);
        }

        public void Clear()
        {
            holders.Clear();
        }

        //
        //====================================================================

        //====================================================================
        //
        // For deserialization.
        //

        public void Initialize(ref ComponentBundleDefinition definition)
        {
            if (ArrayHelper.IsNullOrEmpty(definition.Components)) return;

            for (int i = 0; i < definition.Components.Length; i++)
                AddComponentDefinition(ref definition.Components[i]);

            BindAll();
        }

        void AddComponentDefinition(ref NamedComponentDefinition definition)
        {
            if (ContainsComponentName(definition.Name))
                throw new ArgumentException("Component name duplicated: " + definition.Name);

            var componentInfo = GetComponentInfo(definition.Component.Type);
            var component = componentInfo.CreateInstance();

            var properties = definition.Component.Properties;
            if (!ArrayHelper.IsNullOrEmpty(properties))
            {
                for (int i = 0; i < properties.Length; i++)
                {
                    var name = properties[i].Name;
                    var value = properties[i].Value;

                    if (!componentInfo.IsComponentReference(name))
                        componentInfo.SetPropertyValue(component, name, value);
                }
            }

            var holder = new Holder
            {
                Definition = definition,
                ComponentInfo = componentInfo,
                Component = component
            };

            holders.Add(holder);
        }

        void BindAll()
        {
            foreach (var holder in holders)
            {
                var componentInfo = holder.ComponentInfo;
                var component = holder.Component;

                var properties = holder.Definition.Component.Properties;
                if (!ArrayHelper.IsNullOrEmpty(properties))
                {
                    for (int i = 0; i < properties.Length; i++)
                    {
                        if (componentInfo.IsComponentReference(properties[i].Name))
                        {
                            var referencedComponent = holders[properties[i].Value].Component;
                            componentInfo.SetPropertyValue(component, properties[i].Name, referencedComponent);
                        }
                    }
                }
            }
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
                definition.Components = new NamedComponentDefinition[holders.Count];

                for (int i = 0; i < holders.Count; i++)
                    definition.Components[i] = holders[i].Definition;
            }
        }

        //
        //====================================================================
    }
}
