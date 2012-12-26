#region Using

using System;
using System.Reflection;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework.Component
{
    public sealed class XnaTypePropertyStringfier : IPropertyStringfier
    {
        public static readonly XnaTypePropertyStringfier Instance = new XnaTypePropertyStringfier();

        XnaTypePropertyStringfier() { }

        // I/F
        public bool CanConvertToString(object component, PropertyInfo property, object propertyValue)
        {
            var propertyType = property.PropertyType;

            if (propertyType == typeof(Vector2)) return true;
            if (propertyType == typeof(Vector3)) return true;
            if (propertyType == typeof(Vector4)) return true;
            if (propertyType == typeof(Color)) return true;

            return false;
        }

        // I/F
        public bool ConvertToString(object component, PropertyInfo property, object propertyValue, out string stringValue)
        {
            var propertyType = property.PropertyType;

            if (propertyType == typeof(Vector2))
            {
                Vector2ToString(propertyValue, out stringValue);
                return true;
            }
            if (propertyType == typeof(Vector3))
            {
                Vector3ToString(propertyValue, out stringValue);
                return true;
            }
            if (propertyType == typeof(Vector4))
            {
                Vector4ToString(propertyValue, out stringValue);
                return true;
            }
            if (propertyType == typeof(Color))
            {
                ColorToString(propertyValue, out stringValue);
                return true;
            }

            stringValue = null;
            return false;
        }

        void Vector2ToString(object propertyValue, out string stringValue)
        {
            var value = (Vector2) propertyValue;
            stringValue = value.X + " " + value.Y;
        }

        void Vector3ToString(object propertyValue, out string stringValue)
        {
            var value = (Vector3) propertyValue;
            stringValue = value.X + " " + value.Y + " " + value.Z;
        }

        void Vector4ToString(object propertyValue, out string stringValue)
        {
            var value = (Vector4) propertyValue;
            stringValue = value.X + " " + value.Y + " " + value.Z + " " + value.W;
        }

        void ColorToString(object propertyValue, out string stringValue)
        {
            var value = (Color) propertyValue;
            stringValue = value.PackedValue.ToString();
        }
    }
}
