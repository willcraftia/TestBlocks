#region Using

using System;
using System.Reflection;

#endregion

namespace Willcraftia.Xna.Framework.Component
{
    public sealed class ComponentPropertyStringfier : IPropertyStringfier
    {
        ComponentBundleBuilder componentBundleBuilder;

        public ComponentPropertyStringfier(ComponentBundleBuilder componentBundleBuilder)
        {
            if (componentBundleBuilder == null) throw new ArgumentNullException("componentBundleBuilder");

            this.componentBundleBuilder = componentBundleBuilder;
        }

        // I/F
        public bool CanConvertToString(object component, PropertyInfo property, object propertyValue)
        {
            if (componentBundleBuilder.Contains(propertyValue)) return true;
            if (componentBundleBuilder.ContainsExternalReference(propertyValue)) return true;

            return false;
        }

        // I/F
        public bool ConvertToString(object component, PropertyInfo property, object propertyValue, out string stringValue)
        {
            if (componentBundleBuilder.Contains(propertyValue))
            {
                stringValue = componentBundleBuilder.GetName(propertyValue);
                return true;
            }

            if (componentBundleBuilder.ContainsExternalReference(propertyValue))
            {
                stringValue = componentBundleBuilder.GetExternalReferenceName(propertyValue);
                return true;
            }

            stringValue = null;
            return false;
        }
    }
}
