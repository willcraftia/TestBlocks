#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Component
{
    public interface ITypeRegistory
    {
        Type ResolveType(string typeName);

        string ResolveTypeName(Type type);
    }
}
