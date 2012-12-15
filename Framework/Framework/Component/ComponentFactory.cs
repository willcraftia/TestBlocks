﻿#region Using

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
            public ComponentDefinition ComponentDefinition;

            public ComponentInfo ComponentInfo;

            public object Component;
        }

        #endregion

        #region HolderCollection

        class HolderCollection : KeyedCollection<string, Holder>
        {
            protected override string GetKeyForItem(Holder item)
            {
                return item.ComponentDefinition.Name;
            }
        }

        #endregion

        #region ComponentInfoCollection

        class ComponentInfoCollection : KeyedCollection<Type, ComponentInfo>
        {
            protected override Type GetKeyForItem(ComponentInfo item)
            {
                return item.ComponentType;
            }
        }

        #endregion

        static readonly TypeHandler defaultTypeHandler = new TypeHandler();

        ComponentTypeRegistory typeRegistory;

        Dictionary<Type, ITypeHandler> typeHandlerMap;

        ComponentInfoCollection componentInfoCache = new ComponentInfoCollection();

        HolderCollection holders = new HolderCollection();

        public List<IPropertyHandler> PropertyHandlers { get; private set; }

        public object this[string componentName]
        {
            get
            {
                if (componentName == null) throw new ArgumentNullException("componentName");
                return holders[componentName].Component;
            }
        }

        public ComponentFactory(ComponentTypeRegistory typeRegistory)
            : this(typeRegistory, null)
        {
        }

        public ComponentFactory(ComponentTypeRegistory typeRegistory, IPropertyHandler propertyHandler)
        {
            if (typeRegistory == null) throw new ArgumentNullException("typeRegistory");

            this.typeRegistory = typeRegistory;

            PropertyHandlers = new List<IPropertyHandler>();
            PropertyHandlers.Add(DefaultPropertyHandler.Instance);
            PropertyHandlers.Add(new ComponentPropertyHandler(this));
        }

        public void AddTypeHandler(Type type, ITypeHandler typeHandler)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (typeHandler == null) throw new ArgumentNullException("typeHandler");

            if (typeHandlerMap == null) typeHandlerMap = new Dictionary<Type, ITypeHandler>();
            typeHandlerMap[type] = typeHandler;
        }

        ITypeHandler GetTypeHandler(Type type)
        {
            ITypeHandler typeHandler;
            if (typeHandlerMap == null || !typeHandlerMap.TryGetValue(type, out typeHandler))
                typeHandler = defaultTypeHandler;

            return typeHandler;
        }

        public ComponentInfo GetComponentInfo(string typeName)
        {
            if (typeName == null) throw new ArgumentNullException("typeName");

            var type = typeRegistory.GetType(typeName);
            return GetComponentInfo(type);
        }

        public ComponentInfo GetComponentInfo(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");

            if (componentInfoCache.Contains(type)) return componentInfoCache[type];

            var typeHandler = GetTypeHandler(type);
            var componentInfo = new ComponentInfo(type, typeHandler);
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
                if (holder.Component == component) return holder.ComponentDefinition.Name;
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

            if (!componentInfoCache.Contains(componentInfo))
                componentInfoCache.Add(componentInfo);

            PropertyDefinition[] propertyDefinitions = null;
            if (0 < componentInfo.PropertyCount)
                propertyDefinitions = new PropertyDefinition[componentInfo.PropertyCount];

            var typeDefinitionName = typeRegistory.GetTypeDefinitionName(componentInfo.ComponentType);

            var holder = new Holder
            {
                ComponentDefinition = new ComponentDefinition
                {
                    Name = componentName,
                    Type = typeDefinitionName,
                    Properties = propertyDefinitions
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

            holder.ComponentDefinition.Properties[propertyIndex].Name = propertyName;
            holder.ComponentDefinition.Properties[propertyIndex].Value = Convert.ToString(propertyValue);
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

        public void Build(ref ComponentBundleDefinition definition)
        {
            AddComponentBundleDefinition(ref definition);
            Build();
        }

        public void Build()
        {
            foreach (var holder in holders)
            {
                if (holder.Component == null)
                    holder.Component = holder.ComponentInfo.CreateInstance();
            }

            foreach (var holder in holders)
            {
                var componentInfo = holder.ComponentInfo;
                var component = holder.Component;

                var propertyDefinitions = holder.ComponentDefinition.Properties;
                if (ArrayHelper.IsNullOrEmpty(propertyDefinitions)) continue;

                for (int i = 0; i < propertyDefinitions.Length; i++)
                    PopulateProperty(componentInfo, component, ref propertyDefinitions[i]);

                var initializable = component as IInitializingComponent;
                if (initializable != null) initializable.Initialize();
            }
        }

        void PopulateProperty(ComponentInfo componentInfo, object component, ref PropertyDefinition propertyDefinition)
        {
            var propertyName = propertyDefinition.Name;
            if (string.IsNullOrEmpty(propertyName)) return;

            var property = componentInfo.GetProperty(propertyName);
            var propertyValue = propertyDefinition.Value;

            foreach (var handler in PropertyHandlers)
            {
                if (handler.SetPropertyValue(component, property, propertyValue))
                    return;
            }

            throw new InvalidOperationException("Property not handled: " + propertyName);
        }

        //
        //====================================================================

        //====================================================================
        //
        // For deserialization.
        //

        public void AddComponentBundleDefinition(ref ComponentBundleDefinition definition)
        {
            if (ArrayHelper.IsNullOrEmpty(definition.Components)) return;

            for (int i = 0; i < definition.Components.Length; i++)
                AddComponentDefinition(ref definition.Components[i]);
        }

        public void AddComponentDefinition(ref ComponentDefinition definition)
        {
            if (ContainsComponentName(definition.Name))
                throw new ArgumentException("Component name duplicated: " + definition.Name);

            var internalDefinition = new ComponentDefinition
            {
                Name = definition.Name,
                Type = definition.Type
            };

            var componentInfo = GetComponentInfo(definition.Type);

            if (0 < componentInfo.PropertyCount)
            {
                internalDefinition.Properties = new PropertyDefinition[componentInfo.PropertyCount];

                var propertyDefinitions = definition.Properties;
                if (!ArrayHelper.IsNullOrEmpty(propertyDefinitions))
                {
                    for (int i = 0; i < propertyDefinitions.Length; i++)
                    {
                        var propertyName = propertyDefinitions[i].Name;
                        var propertyValue = propertyDefinitions[i].Value;

                        var propertyIndex = componentInfo.GetPropertyIndex(propertyName);
                        internalDefinition.Properties[propertyIndex] = new PropertyDefinition
                        {
                            Name = propertyName,
                            Value = propertyValue
                        };
                    }
                }
            }

            var holder = new Holder
            {
                ComponentDefinition = internalDefinition,
                ComponentInfo = GetComponentInfo(definition.Type)
            };

            holders.Add(holder);
        }

        //
        //====================================================================

        //====================================================================
        //
        // For serialization
        //

        public void GetComponentBundleDefinition(out ComponentBundleDefinition definition)
        {
            definition = new ComponentBundleDefinition();

            if (!CollectionHelper.IsNullOrEmpty(holders))
            {
                definition.Components = new ComponentDefinition[holders.Count];

                for (int i = 0; i < holders.Count; i++)
                {
                    definition.Components[i] = new ComponentDefinition
                    {
                        Name = holders[i].ComponentDefinition.Name,
                        Type = holders[i].ComponentDefinition.Type
                    };

                    //
                    // クラス内部の BundleEntryDefinition では、
                    // ComponentInfo から得られる全てのプロパティに対して PropertyDefinition 配列を確保している。
                    // 外部へ出す BundleEntryDefinition では、それら配列のうち名前が設定されたものを抽出する。
                    // 名前が設定された PropertyDefinition とは、Component インスタンスのプロパティのデフォルト値を
                    // 上書きするプロパティ値を意味する。
                    //

                    var propertyDefinitions = holders[i].ComponentDefinition.Properties;
                    if (!ArrayHelper.IsNullOrEmpty(propertyDefinitions))
                    {
                        int definedPropertyCount = 0;
                        for (int j = 0; j < propertyDefinitions.Length; j++)
                        {
                            if (!string.IsNullOrEmpty(propertyDefinitions[j].Name))
                                definedPropertyCount++;
                        }

                        definition.Components[i].Properties = new PropertyDefinition[definedPropertyCount];
                        for (int j = 0, k = 0; j < propertyDefinitions.Length; j++, k++)
                        {
                            if (!string.IsNullOrEmpty(propertyDefinitions[j].Name))
                            {
                                definition.Components[i].Properties[k] = new PropertyDefinition
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
