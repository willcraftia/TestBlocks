#region Using

using System;
using System.Reflection;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Blocks.Models;
using Willcraftia.Xna.Blocks.Serialization;

#endregion

namespace Willcraftia.Xna.Blocks.Assets
{
    public sealed class Procedures
    {
        public static IProcedure<T> ToProcedure<T>(ref ProcedureDefinition definition)
        {
            var type = Type.GetType(definition.Type);
            if (type == null)
                throw new InvalidOperationException("Type not found: " + definition.Type);

            var instance = (IProcedure<T>) type.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);

            if (definition.Properties != null)
            {
                foreach (var propertyDef in definition.Properties)
                {
                    var propertyInfo = type.GetProperty(propertyDef.Name);
                    if (propertyInfo == null)
                        throw new InvalidOperationException("Invalid property name: " + propertyDef.Name);

                    propertyInfo.SetValue(instance, propertyDef.Value, null);
                }
            }

            return instance;
        }

        public static void ToDefinition<T>(IProcedure<T> procedure, out ProcedureDefinition result)
        {
            var type = procedure.GetType();

            result = new ProcedureDefinition
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
                        Value = Convert.ToString(properties[i].GetValue(procedure, null))
                    };
                }
            }
        }
    }
}
