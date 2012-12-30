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

            var sceneSettings = new BlocksSceneSettings
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

            if (!ArrayHelper.IsNullOrEmpty(definition.ColorTable))
            {
                for (int i = 0; i < definition.ColorTable.Length; i++)
                {
                    var skyColor = ToSkyColor(ref definition.ColorTable[i]);
                    sceneSettings.ColorTable.AddColor(skyColor);
                }
            }

            sceneSettings.Initialize();

            return sceneSettings;
        }

        public void Save(IResource resource, object asset)
        {
            var sceneSettings = asset as BlocksSceneSettings;

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

            if (sceneSettings.ColorTable.Count != 0)
            {
                definition.ColorTable = new SkyColorDefinition[sceneSettings.ColorTable.Count];
                int index = 0;
                foreach (var skyColor in sceneSettings.ColorTable)
                {
                    definition.ColorTable[index++] = new SkyColorDefinition
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
