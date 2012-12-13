#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Component
{
    public sealed class DefaultTypeRegistory : ITypeRegistory
    {
        public static readonly DefaultTypeRegistory Instance = new DefaultTypeRegistory();

        DefaultTypeRegistory() { }

        public Type ResolveType(string typeName)
        {
            if (typeName == null) throw new ArgumentNullException("typeName");

            return Type.GetType(typeName, true);
        }

        public string ResolveTypeName(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");

            return type.AssemblyQualifiedName;
        }
    }
}
