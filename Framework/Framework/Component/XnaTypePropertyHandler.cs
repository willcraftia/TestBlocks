#region Using

using System;
using System.Reflection;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework.Component
{
    public sealed class XnaTypePropertyHandler : IPropertyHandler
    {
        public static readonly XnaTypePropertyHandler Instance = new XnaTypePropertyHandler();

        XnaTypePropertyHandler() { }

        public bool SetPropertyValue(object component, PropertyInfo property, string propertyValue)
        {
            var propertyType = property.PropertyType;

            if (propertyType == typeof(Vector2))
            {
                SetVector2(component, property, propertyValue);
                return true;
            }
            if (propertyType == typeof(Vector3))
            {
                SetVector3(component, property, propertyValue);
                return true;
            }
            if (propertyType == typeof(Vector4))
            {
                SetVector4(component, property, propertyValue);
                return true;
            }
            if (propertyType == typeof(Color))
            {
                SetColor(component, property, propertyValue);
                return true;
            }
            
            return false;
        }

        void SetVector2(object component, PropertyInfo property, string propertyValue)
        {
            var elements = propertyValue.Split(' ');
            if (elements.Length == 1)
            {
                var value = new Vector2(float.Parse(elements[0]));
                property.SetValue(component, value, null);
            }
            else if (elements.Length == 2)
            {
                var value = new Vector2
                {
                    X = float.Parse(elements[0]),
                    Y = float.Parse(elements[1])
                };
                property.SetValue(component, value, null);
            }
            else
            {
                throw new FormatException("Invalid Vector2 format: " + propertyValue);
            }
        }

        void SetVector3(object component, PropertyInfo property, string propertyValue)
        {
            var elements = propertyValue.Split(' ');
            if (elements.Length == 1)
            {
                var value = new Vector3(float.Parse(elements[0]));
                property.SetValue(component, value, null);
            }
            else if (elements.Length == 3)
            {
                var value = new Vector3
                {
                    X = float.Parse(elements[0]),
                    Y = float.Parse(elements[1]),
                    Z = float.Parse(elements[2])
                };
                property.SetValue(component, value, null);
            }
            else
            {
                throw new FormatException("Invalid Vector3 format: " + propertyValue);
            }
        }

        void SetVector4(object component, PropertyInfo property, string propertyValue)
        {
            var elements = propertyValue.Split(' ');
            if (elements.Length == 1)
            {
                var value = new Vector4(float.Parse(elements[0]));
                property.SetValue(component, value, null);
            }
            else if (elements.Length == 4)
            {
                var value = new Vector4
                {
                    X = float.Parse(elements[0]),
                    Y = float.Parse(elements[1]),
                    Z = float.Parse(elements[2]),
                    W = float.Parse(elements[3])
                };
                property.SetValue(component, value, null);
            }
            else
            {
                throw new FormatException("Invalid Vector3 format: " + propertyValue);
            }
        }

        void SetColor(object component, PropertyInfo property, string propertyValue)
        {
            // パック値として取得
            var packedValue = uint.Parse(propertyValue);
            var value = new Color
            {
                A = (byte) (packedValue >> 24),
                R = (byte) (packedValue >> 16),
                G = (byte) (packedValue >> 8),
                B = (byte) (packedValue)
            };
            property.SetValue(component, value, null);
        }
    }
}
