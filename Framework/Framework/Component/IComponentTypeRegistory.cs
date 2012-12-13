#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Component
{
    public interface IComponentTypeRegistory
    {
        Type ResolveType(string typeName);

        string ResolveTypeName(Type type);
    }
}
