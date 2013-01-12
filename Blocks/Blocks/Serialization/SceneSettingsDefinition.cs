#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Blocks.Serialization
{
    public struct SceneSettingsDefinition
    {
        public Vector3 MidnightSunDirection;

        public Vector3 MidnightMoonDirection;

        public Vector3 ShadowColor;

        public Vector3 SunlightDiffuseColor;

        public Vector3 SunlightSpecularColor;

        public bool SunlightEnabled;

        public Vector3 MoonlightDiffuseColor;

        public Vector3 MoonlightSpecularColor;

        public bool MoonlightEnabled;

        public TimeColorDefinition[] SkyColors;

        public TimeColorDefinition[] AmbientLightColors;

        public bool InitialFogEnabled;

        public float InitialFogStartRate;

        public float InitialFogEndRate;

        public float SecondsPerDay;

        public bool TimeStopped;

        public float FixedSecondsPerDay;
    }
}
