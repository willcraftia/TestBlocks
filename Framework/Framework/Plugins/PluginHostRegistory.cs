#region Using

using System;
using System.Collections.Generic;

#endregion

namespace Willcraftia.Xna.Framework.Plugins
{
    public sealed class PluginHostRegistory : IPluginHostRegistory
    {
        Dictionary<string, object> keyHostMap = new Dictionary<string, object>();

        public object this[string key]
        {
            get { return keyHostMap[key]; }
            set { keyHostMap[key] = value; }
        }

        public PluginHostRegistory() { }
    }
}
