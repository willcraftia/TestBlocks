﻿#region Using

using System;
using Willcraftia.Xna.Framework.Content;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Blocks.Models;
using Willcraftia.Xna.Blocks.Serialization;

#endregion

namespace Willcraftia.Xna.Blocks.Content
{
    public sealed class SceneSettingsLoader : IAssetLoader
    {
        DefinitionSerializer serializer = new DefinitionSerializer(typeof(SceneSettingsDefinition));

        public object Load(IResource resource)
        {
            var definition = (SceneSettingsDefinition) serializer.Deserialize(resource);

            var sceneSettings = new SceneSettings
            {
                EarthRotationEnabled = definition.EarthRotationEnabled,
                MidnightSunDirection = definition.MidnightSunDirection,
                MidnightMoonDirection = definition.MidnightMoonDirection,
                MiddayAmbientLightColor = definition.MiddayAmbientLightColor,
                MidnightAmbientLightColor = definition.MidnightAmbientLightColor,
                SunlightDiffuseColor = definition.SunlightDiffuseColor,
                SunlightSpecularColor = definition.SunlightSpecularColor,
                MoonlightDiffuseColor = definition.MoonlightDiffuseColor,
                MoonlightSpecularColor = definition.MoonlightSpecularColor,
                SecondsPerDay = definition.SecondsPerDay
            };
            sceneSettings.Initialize();

            return sceneSettings;
        }

        public void Save(IResource resource, object asset)
        {
            var sceneSettings = asset as SceneSettings;

            var definition = new SceneSettingsDefinition
            {
                EarthRotationEnabled = sceneSettings.EarthRotationEnabled,
                MidnightSunDirection = sceneSettings.MidnightSunDirection,
                MidnightMoonDirection = sceneSettings.MidnightMoonDirection,
                MiddayAmbientLightColor = sceneSettings.MiddayAmbientLightColor,
                MidnightAmbientLightColor = sceneSettings.MidnightAmbientLightColor,
                SunlightDiffuseColor = sceneSettings.SunlightDiffuseColor,
                SunlightSpecularColor = sceneSettings.SunlightSpecularColor,
                MoonlightDiffuseColor = sceneSettings.MoonlightDiffuseColor,
                MoonlightSpecularColor = sceneSettings.MoonlightSpecularColor,
                SecondsPerDay = sceneSettings.SecondsPerDay
            };

            serializer.Serialize(resource, definition);
        }
    }
}