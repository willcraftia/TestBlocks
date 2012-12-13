#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Component
{
    public sealed class DefaultComponentTypeRegistory : IComponentTypeRegistory
    {
        public static readonly DefaultComponentTypeRegistory Instance = new DefaultComponentTypeRegistory();

        DefaultComponentTypeRegistory() { }

        public Type ResolveType(string typeName)
        {
            if (typeName == null) throw new ArgumentNullException("typeName");

            return Type.GetType(typeName, true);
        }

        public string ResolveTypeName(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");

            return type.FullName;
        }
    }
}
