#region Using

using System;
using System.Collections.Generic;

#endregion

namespace Willcraftia.Xna.Framework.Component
{
    public sealed class ComponentFactory
    {
        ComponentInfoManager componentInfoManager;

        Dictionary<string, object> nameComponentMap;

        public List<IPropertyHandler> PropertyHandlers { get; private set; }

        public object this[string name]
        {
            get
            {
                if (name == null) throw new ArgumentNullException("name");
                if (nameComponentMap == null) throw new KeyNotFoundException("Invalid key: " + name);
                
                return nameComponentMap[name];
            }
        }

        public ComponentFactory(ComponentInfoManager componentInfoManager)
        {
            if (componentInfoManager == null) throw new ArgumentNullException("componentInfoManager");

            this.componentInfoManager = componentInfoManager;

            PropertyHandlers = new List<IPropertyHandler>();
            PropertyHandlers.Add(DefaultPropertyHandler.Instance);
            PropertyHandlers.Add(XnaTypePropertyHandler.Instance);
            PropertyHandlers.Add(new ComponentPropertyHandler(this));
        }

        public bool Contains(string name)
        {
            if (name == null) throw new ArgumentNullException("componentName");

            return nameComponentMap.ContainsKey(name);
        }

        public void Clear()
        {
            nameComponentMap.Clear();
        }

        public void Build(ref ComponentBundleDefinition definition)
        {
            if (ArrayHelper.IsNullOrEmpty(definition.Components)) return;

            nameComponentMap = new Dictionary<string, object>(definition.Components.Length);

            for (int i = 0; i < definition.Components.Length; i++)
            {
                var componentInfo = componentInfoManager.GetComponentInfo(definition.Components[i].Type);
                var name = definition.Components[i].Name;
                var component = componentInfo.CreateInstance();
                nameComponentMap[name] = component;
            }

            for (int i = 0; i < definition.Components.Length; i++)
            {
                var componentInfo = componentInfoManager.GetComponentInfo(definition.Components[i].Type);
                var name = definition.Components[i].Name;
                var component = nameComponentMap[name];

                var propertyDefinitions = definition.Components[i].Properties;
                if (ArrayHelper.IsNullOrEmpty(propertyDefinitions)) continue;

                for (int j = 0; j < propertyDefinitions.Length; j++)
                    PopulateProperty(componentInfo, component, ref propertyDefinitions[j]);
            }

            for (int i = 0; i < definition.Components.Length; i++)
            {
                var name = definition.Components[i].Name;
                var component = nameComponentMap[name];

                var initializable = component as IInitializingObject;
                if (initializable != null) initializable.Initialize();
            }
        }

        void PopulateProperty(ComponentInfo componentInfo, object component, ref PropertyDefinition propertyDefinition)
        {
            var propertyName = propertyDefinition.Name;
            if (string.IsNullOrEmpty(propertyName)) return;

            var property = componentInfo.GetProperty(propertyName);
            var propertyValue = propertyDefinition.Value;

            for (int i = 0; i < PropertyHandlers.Count; i++)
            {
                var handler = PropertyHandlers[i];
                if (handler.SetPropertyValue(component, property, propertyValue))
                    return;
            }

            throw new InvalidOperationException("Property not handled: " + propertyName);
        }
    }
}
