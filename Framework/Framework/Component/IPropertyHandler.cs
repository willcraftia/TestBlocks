#region Using

using System;
using System.Reflection;

#endregion

namespace Willcraftia.Xna.Framework.Component
{
    public interface IPropertyHandler
    {
        bool SetPropertyValue(ComponentFactory factory, object component, PropertyInfo property, string propertyValue);
    }
}
