#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Component
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ComponentPropertyAttribute : Attribute
    {
        public bool Ignored { get; set; }
    }
}
