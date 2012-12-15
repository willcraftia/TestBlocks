#region Using

using System;
using System.Reflection;

#endregion

namespace Willcraftia.Xna.Framework.Component
{
    public sealed class DefaultPropertyHandler : IPropertyHandler
    {
        public static readonly DefaultPropertyHandler Instance = new DefaultPropertyHandler();

        DefaultPropertyHandler() { }

        // I/F
        public bool SetPropertyValue(object component, PropertyInfo property, string propertyValue)
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

            return false;
        }
    }
}
