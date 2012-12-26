#region Using

using System;
using System.Reflection;

#endregion

namespace Willcraftia.Xna.Framework.Component
{
    public sealed class DefaultPropertyStringfier : IPropertyStringfier
    {
        public static readonly DefaultPropertyStringfier Instance = new DefaultPropertyStringfier();

        DefaultPropertyStringfier() { }

        // I/F
        public bool CanConvertToString(object component, PropertyInfo property, object propertyValue)
        {
            var propertyType = property.PropertyType;

            if (propertyType == typeof(string)) return true;
            if (propertyType.IsEnum) return true;
            if (typeof(IConvertible).IsAssignableFrom(propertyType)) return true;

            return false;
        }

        // I/F
        public bool ConvertToString(object component, PropertyInfo property, object propertyValue, out string stringValue)
        {
            var propertyType = property.PropertyType;

            if (propertyType == typeof(string))
            {
                stringValue = propertyValue as string;
                return true;
            }
            
            if (propertyType.IsEnum)
            {
                stringValue = Convert.ToString(propertyValue);
                return true;
            }
            
            if (typeof(IConvertible).IsAssignableFrom(propertyType))
            {
                var convertible = propertyValue as IConvertible;
                stringValue = convertible.ToString();
                return true;
            }

            stringValue = null;
            return false;
        }
    }
}
