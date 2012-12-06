#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Plugins
{
    public interface IPlugin
    {
        void Load(IPluginHostRegistory hostRegistory);
    }
}
