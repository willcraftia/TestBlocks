#region Using

using System;
using System.Reflection;

#endregion

namespace Willcraftia.Xna.Framework.Component
{
    public interface ITypeHandler
    {
        PropertyInfo[] GetProperties(Type type);

        object CreateInstance(Type type);
    }
}
