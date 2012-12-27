﻿#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Blocks.Serialization
{
    public struct SceneSettingsDefinition
    {
        public bool EarthRotationEnabled;

        public Vector3 MidnightSunDirection;

        public Vector3 MidnightMoonDirection;

        public Vector3 MiddayAmbientLightColor;

        public Vector3 MidnightAmbientLightColor;

        public Vector3 SunlightDiffuseColor;

        public Vector3 SunlightSpecularColor;

        public Vector3 MoonlightDiffuseColor;

        public Vector3 MoonlightSpecularColor;

        public float SecondsPerDay;
    }
}