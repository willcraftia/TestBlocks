#region Using

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

#endregion

namespace Willcraftia.Xna.Framework.Component
{
    public sealed class ComponentContainer
    {
        #region Holder

        class Holder
        {
            public ComponentDefinition Definition;

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

        ComponentInfoCollection componentInfoCollection = new ComponentInfoCollection();

        HolderCollection holders = new HolderCollection();

        Dictionary<string, Type> typeDictionary;

        Dictionary<Type, string> reverseTypeDictionary;

        public object this[string name]
        {
            get
            {
                if (name == null) throw new ArgumentNullException("name");
                return holders[name].Component;
            }
        }

        public Type ResolveType(string typeName)
        {
            if (typeName == null) throw new ArgumentNullException("typeName");

            Type type;
            if (typeDictionary != null && typeDictionary.TryGetValue(typeName, out type))
                return type;

            type = Type.GetType(typeName);
            if (type == null) throw new ArgumentException("Unknown type: " + typeName);

            return type;
        }

        public string ResolveAlias(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");

            string alias;
            if (reverseTypeDictionary != null && reverseTypeDictionary.TryGetValue(type, out alias))
                return alias;

            throw new ArgumentException("Unknown type: " + type);
        }

        public void RegisterAlias(Type type)
        {
            RegisterAlias(type.Name, type);
        }

        public void RegisterAlias(string alias, Type type)
        {
            if (alias == null) throw new ArgumentNullException("alias");
            if (type == null) throw new ArgumentNullException("type");

            if (typeDictionary == null)
            {
                typeDictionary = new Dictionary<string, Type>();
                reverseTypeDictionary = new Dictionary<Type, string>();
            }
            typeDictionary[alias] = type;
            reverseTypeDictionary[type] = alias;
        }

        public ComponentInfo GetComponentInfo(string alias)
        {
            if (alias == null) throw new ArgumentNullException("alias");

            var type = ResolveType(alias);
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

        public void AddComponent(string componentName, ComponentInfo componentInfo)
        {
            if (componentName == null) throw new ArgumentNullException("componentName");
            if (componentInfo == null) throw new ArgumentNullException("componentInfo");

            var component = componentInfo.CreateInstance();

            var holder = new Holder
            {
                Component = component,
                ComponentInfo = componentInfo
            };

            // Create the initial definition.
            CreateDefinition(componentName, componentInfo, component, out holder.Definition);

            holders.Add(holder);
        }

        void CreateDefinition(string componentName, ComponentInfo componentInfo, object component, out ComponentDefinition definition)
        {
            definition = new ComponentDefinition
            {
                Name = componentName,
                Type = ResolveAlias(componentInfo.Type)
            };

            var properties = componentInfo.Properties;
            if (!CollectionHelper.IsNullOrEmpty(properties))
            {
                definition.Properties = new ComponentPropertyDefinition[properties.Count];
                for (int i = 0; i < properties.Count; i++)
                {
                    var name = properties[i].Name;
                    var value = componentInfo.GetPropertyValue(component, name);
                    definition.Properties[i] = new ComponentPropertyDefinition
                    {
                        Name = name,
                        Value = Convert.ToString(value)
                    };
                }
            }

            var references = componentInfo.References;
            if (!CollectionHelper.IsNullOrEmpty(references))
            {
                definition.References = new ComponentPropertyDefinition[references.Count];
                for (int i = 0; i < references.Count; i++)
                {
                    definition.References[i] = new ComponentPropertyDefinition
                    {
                        Name = references[i].Name
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

            componentInfo.SetPropertyValue(holder.Component, propertyName, propertyValue);

            // Refresh the definition.
            var properties = holder.Definition.Properties;
            if (!ArrayHelper.IsNullOrEmpty(properties))
            {
                for (int i = 0; i < properties.Length; i++)
                {
                    if (properties[i].Name == propertyName)
                        properties[i].Value = Convert.ToString(propertyValue);
                }
            }
        }

        public void SetReferencedComponent(string componentName, string propertyName, string referencedName)
        {
            if (componentName == null) throw new ArgumentNullException("componentName");
            if (propertyName == null) throw new ArgumentNullException("propertyName");
            AssertContainsComponentName(componentName);
            AssertContainsComponentName(referencedName);

            var holder = holders[componentName];
            var componentInfo = holder.ComponentInfo;
            var component = holder.Component;
            var referencedComponent = holders[referencedName].Component;

            componentInfo.SetReferencedComponent(component, propertyName, referencedComponent);

            // Refresh the definition.
            var references = holder.Definition.References;
            if (!ArrayHelper.IsNullOrEmpty(references))
            {
                for (int i = 0; i < references.Length; i++)
                {
                    if (references[i].Name == propertyName)
                        references[i].Value = referencedName;
                }
            }
        }

        public void UnbindReferencedComponent(string componentName, string propertyName)
        {
            if (componentName == null) throw new ArgumentNullException("componentName");
            if (propertyName == null) throw new ArgumentNullException("propertyName");
            AssertContainsComponentName(componentName);

            var holder = holders[componentName];
            var componentInfo = holder.ComponentInfo;
            var component = holder.Component;

            componentInfo.SetReferencedComponent(component, propertyName, null);

            // Refresh the definition.
            var references = holder.Definition.References;
            if (!ArrayHelper.IsNullOrEmpty(references))
            {
                for (int i = 0; i < references.Length; i++)
                {
                    if (references[i].Name == propertyName)
                        references[i].Value = null;
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
                    UnbindReferencedComponent(cursorHolder.Definition.Name, name);
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

        public void Initialize(ComponentDefinition[] definitions)
        {
            for (int i = 0; i < definitions.Length; i++)
                AddDefinition(ref definitions[i]);

            BindAll();
        }

        void AddDefinition(ref ComponentDefinition definition)
        {
            if (ContainsComponentName(definition.Name)) throw new ArgumentException("Component name duplicated: " + definition.Name);

            var componentInfo = GetComponentInfo(definition.Type);
            var component = componentInfo.CreateInstance();

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

                var references = holder.Definition.References;
                if (!ArrayHelper.IsNullOrEmpty(references))
                {
                    for (int i = 0; i < references.Length; i++)
                    {
                        var referencedComponent = holders[references[i].Value].Component;
                        componentInfo.SetReferencedComponent(component, references[i].Name, referencedComponent);
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

        public void GetDefinitions(out ComponentDefinition[] definitions)
        {
            definitions = new ComponentDefinition[holders.Count];

            for (int i = 0; i < holders.Count; i++)
                definitions[i] = holders[i].Definition;
        }

        //
        //====================================================================
    }
}
