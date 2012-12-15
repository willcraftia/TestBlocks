#region Using

using System;
using System.Reflection;

#endregion

namespace Willcraftia.Xna.Framework.Component
{
    public sealed class ComponentPropertyHandler : IPropertyHandler
    {
        ComponentFactory componentFactory;

        public ComponentPropertyHandler(ComponentFactory componentFactory)
        {
            if (componentFactory == null) throw new ArgumentNullException("componentFactory");

            this.componentFactory = componentFactory;
        }

        // I/F
        public bool SetPropertyValue(object component, PropertyInfo property, string propertyValue)
        {
            if (propertyValue == null) return false;

            if (componentFactory.ContainsComponentName(propertyValue))
            {
                var propertyType = property.PropertyType;
                var referencedComponent = componentFactory[propertyValue];

                if (propertyType.IsAssignableFrom(referencedComponent.GetType()))
                {
                    property.SetValue(component, referencedComponent, null);
                    return true;
                }
            }

            return false;
        }
    }
}
