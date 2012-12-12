#region Using

using System;
using System.Reflection;

#endregion

namespace Willcraftia.Xna.Framework
{
    public static class AttributeHelper
    {
        public static T GetAttribute<T>(MemberInfo memberInfo) where T : Attribute
        {
            return GetAttribute(memberInfo, typeof(T)) as T;
        }

        public static Attribute GetAttribute(MemberInfo memberInfo, Type attributeType)
        {
            return Attribute.GetCustomAttribute(memberInfo, attributeType, true);
        }

        public static bool HasAttribute<T>(MemberInfo memberInfo) where T : Attribute
        {
            return HasAttribute(memberInfo, typeof(T));
        }

        public static bool HasAttribute(MemberInfo memberInfo, Type attributeType)
        {
            return GetAttribute(memberInfo, attributeType) != null;
        }
    }
}
