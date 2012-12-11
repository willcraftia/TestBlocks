#region Using

using System;
using System.Reflection;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Blocks.Models;
using Willcraftia.Xna.Blocks.Serialization;

#endregion

namespace Willcraftia.Xna.Blocks.Assets
{
    public sealed class ComponentHelper
    {
        public static T ToComponent<T>(ref ComponentDefinition definition)
        {
            var type = Type.GetType(definition.Type);
            if (type == null)
                throw new InvalidOperationException("Type not found: " + definition.Type);

            if (!typeof(T).IsAssignableFrom(type))
                throw new InvalidOperationException("Unexpected type: " + type.FullName);

            var instance = type.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);

            if (definition.Properties != null)
            {
                foreach (var propertyDefinition in definition.Properties)
                {
                    var propertyInfo = type.GetProperty(propertyDefinition.Name);
                    if (propertyInfo == null)
                        throw new InvalidOperationException("Invalid property name: " + propertyDefinition.Name);

                    propertyInfo.SetValue(instance, propertyDefinition.Value, null);
                }
            }

            return (T) instance;
        }

        public static void ToDefinition(object component, out ComponentDefinition result)
        {
            var type = component.GetType();

            result = new ComponentDefinition
            {
                Type = type.FullName
            };

            var properties = type.GetProperties();
            if (!ArrayHelper.IsNullOrEmpty(properties))
            {
                result.Properties = new PropertyDefinition[properties.Length];
                for (int i = 0; i < properties.Length; i++)
                {
                    result.Properties[i] = new PropertyDefinition
                    {
                        Name = properties[i].Name,
                        Value = Convert.ToString(properties[i].GetValue(component, null))
                    };
                }
            }
        }
    }
}
