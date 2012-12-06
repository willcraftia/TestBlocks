#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Plugins
{
    public interface IPluginHostRegistory
    {
        object this[string key] { get; }
    }
}
