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
                //MiddayAmbientLightColor = definition.MiddayAmbientLightColor,
                //MidnightAmbientLightColor = definition.MidnightAmbientLightColor,
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

            if (!ArrayHelper.IsNullOrEmpty(definition.SkyColors))
            {
                for (int i = 0; i < definition.SkyColors.Length; i++)
                {
                    var timeColor = ToTimeColor(ref definition.SkyColors[i]);
                    sceneSettings.SkyColors.AddColor(timeColor);
                }
            }

            if (!ArrayHelper.IsNullOrEmpty(definition.AmbientLightColors))
            {
                for (int i = 0; i < definition.AmbientLightColors.Length; i++)
                {
                    var timeColor = ToTimeColor(ref definition.AmbientLightColors[i]);
                    sceneSettings.AmbientLightColors.AddColor(timeColor);
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
                //MiddayAmbientLightColor = sceneSettings.MiddayAmbientLightColor,
                //MidnightAmbientLightColor = sceneSettings.MidnightAmbientLightColor,
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

            if (sceneSettings.SkyColors.Count != 0)
            {
                definition.SkyColors = new TimeColorDefinition[sceneSettings.SkyColors.Count];
                int index = 0;
                foreach (var timeColor in sceneSettings.SkyColors)
                {
                    definition.SkyColors[index++] = new TimeColorDefinition
                    {
                        Time = timeColor.Time,
                        Color = timeColor.Color
                    };
                }
            }

            if (sceneSettings.AmbientLightColors.Count != 0)
            {
                definition.AmbientLightColors = new TimeColorDefinition[sceneSettings.AmbientLightColors.Count];
                int index = 0;
                foreach (var timeColor in sceneSettings.AmbientLightColors)
                {
                    definition.AmbientLightColors[index++] = new TimeColorDefinition
                    {
                        Time = timeColor.Time,
                        Color = timeColor.Color
                    };
                }
            }

            serializer.Serialize(resource, definition);
        }

        TimeColor ToTimeColor(ref TimeColorDefinition definition)
        {
            return new TimeColor(definition.Time, definition.Color);
        }
    }
}
