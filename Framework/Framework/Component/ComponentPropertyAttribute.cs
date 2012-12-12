#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Component
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ComponentPropertyIgnoredAttribute : Attribute
    {
    }
}
