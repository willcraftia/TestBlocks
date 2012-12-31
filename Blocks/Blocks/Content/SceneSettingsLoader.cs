#region Using

using System;
using Willcraftia.Xna.Framework;
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
                ShadowColor = definition.ShadowColor,
                SecondsPerDay = definition.SecondsPerDay,
                TimeStopped = definition.TimeStopped,
                FixedSecondsPerDay = definition.FixedSecondsPerDay
            };

            sceneSettings.Sunlight.DiffuseColor = definition.SunlightDiffuseColor;
            sceneSettings.Sunlight.SpecularColor = definition.SunlightSpecularColor;
            sceneSettings.Sunlight.Enabled = definition.SunlightEnabled;
            sceneSettings.Moonlight.DiffuseColor = definition.MoonlightDiffuseColor;
            sceneSettings.Moonlight.SpecularColor = definition.MoonlightSpecularColor;
            sceneSettings.Moonlight.Enabled = definition.MoonlightEnabled;

            if (!ArrayHelper.IsNullOrEmpty(definition.SkyColorTable))
            {
                for (int i = 0; i < definition.SkyColorTable.Length; i++)
                {
                    var skyColor = ToSkyColor(ref definition.SkyColorTable[i]);
                    sceneSettings.SkyColorTable.AddColor(skyColor);
                }
            }

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
                ShadowColor = sceneSettings.ShadowColor,
                SunlightDiffuseColor = sceneSettings.Sunlight.DiffuseColor,
                SunlightSpecularColor = sceneSettings.Sunlight.SpecularColor,
                SunlightEnabled = sceneSettings.Sunlight.Enabled,
                MoonlightDiffuseColor = sceneSettings.Moonlight.DiffuseColor,
                MoonlightSpecularColor = sceneSettings.Moonlight.SpecularColor,
                MoonlightEnabled = sceneSettings.Moonlight.Enabled,
                SecondsPerDay = sceneSettings.SecondsPerDay,
                TimeStopped = sceneSettings.TimeStopped,
                FixedSecondsPerDay = sceneSettings.FixedSecondsPerDay
            };

            if (sceneSettings.SkyColorTable.Count != 0)
            {
                definition.SkyColorTable = new SkyColorDefinition[sceneSettings.SkyColorTable.Count];
                int index = 0;
                foreach (var skyColor in sceneSettings.SkyColorTable)
                {
                    definition.SkyColorTable[index++] = new SkyColorDefinition
                    {
                        Time = skyColor.Time,
                        Color = skyColor.Color
                    };
                }
            }

            serializer.Serialize(resource, definition);
        }

        SkyColor ToSkyColor(ref SkyColorDefinition definition)
        {
            return new SkyColor(definition.Time, definition.Color);
        }
    }
}
