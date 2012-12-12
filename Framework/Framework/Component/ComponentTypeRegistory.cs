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

        public Type ResolveType(string typeName)
        {
            if (typeName == null) throw new ArgumentNullException("typeName");

            Type type;
            if (typeDictionary != null && typeDictionary.TryGetValue(typeName, out type))
                return type;

            return Type.GetType(typeName, true);
        }

        public string ResolveAlias(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");

            string alias;
            if (reverseTypeDictionary != null && reverseTypeDictionary.TryGetValue(type, out alias))
                return alias;

            throw new ArgumentException("Unknown type: " + type);
        }

        public void SetAlias(Type type)
        {
            SetAlias(type, type.Name);
        }

        public void SetAlias(Type type, string alias)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (alias == null) throw new ArgumentNullException("alias");

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

            typeDictionary[alias] = type;
            reverseTypeDictionary[type] = alias;
        }
    }
}
