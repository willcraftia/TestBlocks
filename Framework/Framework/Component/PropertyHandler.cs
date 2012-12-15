#region Using

using System;
using System.Reflection;

#endregion

namespace Willcraftia.Xna.Framework.Component
{
    public class PropertyHandler : IPropertyHandler
    {
        public virtual bool SetPropertyValue(ComponentFactory factory, object component, PropertyInfo property, string propertyValue)
        {
            var propertyType = property.PropertyType;

            if (propertyType == typeof(string))
            {
                property.SetValue(component, propertyValue, null);
                return true;
            }

            if (propertyType.IsEnum)
            {
                var convertedValue = Enum.Parse(propertyType, propertyValue, true);
                property.SetValue(component, convertedValue, null);
                return true;
            }

            if (typeof(IConvertible).IsAssignableFrom(propertyType))
            {
                var convertedValue = Convert.ChangeType(propertyValue, propertyType, null);
                property.SetValue(component, convertedValue, null);
                return true;
            }

            if (factory.ContainsComponentName(propertyValue))
            {
                var referencedComponent = factory[propertyValue];
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
