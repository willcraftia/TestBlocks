#region Using

using System;
using System.Collections.Generic;

#endregion

namespace Willcraftia.Xna.Framework.Component
{
    public sealed class ComponentTypeRegistory
    {
        Dictionary<string, Type> typeDictionary;

        Dictionary<Type, string> reverseTypeDictionary;

        public ComponentTypeRegistory() { }

        public ComponentTypeRegistory(int capacity)
        {
            typeDictionary = new Dictionary<string, Type>(capacity);
            reverseTypeDictionary = new Dictionary<Type, string>(capacity);
        }

        public Type GetType(string typeName)
        {
            if (typeName == null) throw new ArgumentNullException("typeName");

            if (typeDictionary != null)
            {
                Type type;
                if (typeDictionary.TryGetValue(typeName, out type))
                    return type;
            }

            throw new ArgumentException("Unknown type name: " + typeName);
        }

        public string GetTypeDefinitionName(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");

            if (reverseTypeDictionary != null)
            {
                string definitionName;
                if (reverseTypeDictionary.TryGetValue(type, out definitionName))
                    return definitionName;
            }

            throw new ArgumentException("Type not supported: " + type.FullName);
        }

        public void SetTypeDefinitionName(Type type)
        {
            SetTypeDefinitionName(type, type.Name);
        }

        public void SetTypeDefinitionName(Type type, string definitionName)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (definitionName == null) throw new ArgumentNullException("definitionName");

            if (typeDictionary == null)
            {
                typeDictionary = new Dictionary<string, Type>();
                reverseTypeDictionary = new Dictionary<Type, string>();
            }
            else
            {
                string oldAlias;
                if (reverseTypeDictionary.TryGetValue(type, out oldAlias))
                    typeDictionary.Remove(oldAlias);
            }

            typeDictionary[definitionName] = type;
            reverseTypeDictionary[type] = definitionName;
        }
    }
}
