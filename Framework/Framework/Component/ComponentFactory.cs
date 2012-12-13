#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Component
{
    public sealed class ComponentFactory
    {
        ITypeRegistory typeRegistory = DefaultTypeRegistory.Instance;

        ComponentInfoCollection componentInfoCollection = new ComponentInfoCollection();

        public ComponentFactory() { }

        public ComponentFactory(ITypeRegistory typeRegistory)
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

            if (componentInfoCollection.Contains(type)) return componentInfoCollection[type];

            var componentInfo = new ComponentInfo(type);
            componentInfoCollection.Add(componentInfo);
            return componentInfo;
        }

        public object CreateComponent(ComponentInfo componentInfo)
        {
            var component = componentInfo.CreateInstance();

            var componentFactoryAware = component as IComponentFactoryAware;
            if (componentFactoryAware != null) componentFactoryAware.ComponentFactory = this;

            return component;
        }

        public object CreateComponent(ref ComponentDefinition definition)
        {
            var componentInfo = GetComponentInfo(definition.Type);
            var component = CreateComponent(componentInfo);

            var propertyDefinitions = definition.Properties;
            if (!ArrayHelper.IsNullOrEmpty(propertyDefinitions))
            {
                for (int i = 0; i < propertyDefinitions.Length; i++)
                {
                    var name = propertyDefinitions[i].Name;
                    var value = propertyDefinitions[i].Value;

                    var propertyName = propertyDefinitions[i].Name;
                    if (string.IsNullOrEmpty(propertyName)) continue;

                    var propertyType = componentInfo.GetPropertyType(propertyName);
                    var propertyValueString = propertyDefinitions[i].Value;

                    bool propertyHandled = false;
                    if (propertyType == typeof(string))
                    {
                        componentInfo.SetPropertyValue(component, propertyName, propertyValueString);
                        propertyHandled = true;
                    }
                    else if (typeof(IConvertible).IsAssignableFrom(propertyType))
                    {
                        var convertedValue = Convert.ChangeType(propertyValueString, propertyType, null);
                        componentInfo.SetPropertyValue(component, propertyName, convertedValue);
                        propertyHandled = true;
                    }

                    if (!propertyHandled)
                        throw new InvalidOperationException("Property not handled: " + propertyName);
                }
            }

            return component;
        }

        public void CreateDefinition(object component, out ComponentDefinition definition)
        {
            var componentInfo = GetComponentInfo(component.GetType());

            definition = new ComponentDefinition
            {
                Type = typeRegistory.ResolveTypeName(componentInfo.ComponentType)
            };

            var propertyNames = componentInfo.PropertyNames;
            if (!CollectionHelper.IsNullOrEmpty(propertyNames))
            {
                definition.Properties = new PropertyDefinition[propertyNames.Count];
                for (int i = 0; i < propertyNames.Count; i++)
                {
                    var name = propertyNames[i];
                    var value = Convert.ToString(componentInfo.GetPropertyValue(component, name));

                    definition.Properties[i] = new PropertyDefinition
                    {
                        Name = name,
                        Value = value
                    };
                }
            }
        }
    }
}
