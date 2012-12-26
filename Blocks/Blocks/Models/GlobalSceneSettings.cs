#region Using

using System;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public static class GlobalSceneSettings
    {
        public static bool FogEnabled { get; set; }

        static GlobalSceneSettings()
        {
            FogEnabled = true;
        }
    }
}
