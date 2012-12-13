#region Using

using System;
using System.Collections.Generic;

#endregion

namespace Willcraftia.Xna.Framework.Component
{
    public sealed class AliasTypeRegistory : ITypeRegistory
    {
        Dictionary<string, Type> typeDictionary;

        Dictionary<Type, string> reverseTypeDictionary;

        // I/F
        public Type ResolveType(string typeName)
        {
            if (typeName == null) throw new ArgumentNullException("typeName");

            Type type;
            if (typeDictionary != null && typeDictionary.TryGetValue(typeName, out type))
                return type;

            return Type.GetType(typeName, true);
        }

        // I/F
        public string ResolveTypeName(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");

            string alias;
            if (reverseTypeDictionary != null && reverseTypeDictionary.TryGetValue(type, out alias))
                return alias;

            return type.AssemblyQualifiedName;
        }

        public void SetTypeAlias(Type type)
        {
            SetTypeAlias(type, type.Name);
        }

        public void SetTypeAlias(Type type, string alias)
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
