#region Using

using System;
using System.Reflection;

#endregion

namespace Willcraftia.Xna.Framework.Component
{
    public interface IPropertyStringfier
    {
        bool CanConvertToString(object component, PropertyInfo property, object propertyValue);

        bool ConvertToString(object component, PropertyInfo property, object propertyValue, out string stringValue);
    }
}
