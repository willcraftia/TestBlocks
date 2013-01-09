#region Using

using System;
using Willcraftia.Xna.Framework.Content;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Blocks.Models;
using Willcraftia.Xna.Blocks.Serialization;

#endregion

namespace Willcraftia.Xna.Blocks.Content
{
    public sealed class GraphicsSettingsLoader : IAssetLoader
    {
        DefinitionSerializer serializer = new DefinitionSerializer(typeof(GraphicsSettingsDefinition));

        public object Load(IResource resource)
        {
            var definition = (GraphicsSettingsDefinition) serializer.Deserialize(resource);

            var settings = new GraphicsSettings();

            //----------------------------------------------------------------
            // シャドウ マップ

            settings.ShadowMap.Enabled = definition.ShadowMap.Enabled;
            settings.ShadowMap.Technique = definition.ShadowMap.Technique;
            settings.ShadowMap.Size = definition.ShadowMap.Size;
            settings.ShadowMap.DepthBias = definition.ShadowMap.DepthBias;
            settings.ShadowMap.FarPlaneDistance = definition.ShadowMap.FarPlaneDistance;
            settings.ShadowMap.SplitCount = definition.ShadowMap.SplitCount;
            settings.ShadowMap.SplitLambda = definition.ShadowMap.SplitLambda;
            settings.ShadowMap.VsmBlur.Radius = definition.ShadowMap.VsmBlur.Radius;
            settings.ShadowMap.VsmBlur.Amount = definition.ShadowMap.VsmBlur.Amount;

            return settings;
        }

        public void Save(IResource resource, object asset)
        {
            var settings = asset as GraphicsSettings;

            var definition = new GraphicsSettingsDefinition
            {
                //------------------------------------------------------------
                // シャドウ マップ

                ShadowMap = new ShadowMapSettingsDefinition
                {
                    Enabled = settings.ShadowMap.Enabled,
                    Technique = settings.ShadowMap.Technique,
                    Size = settings.ShadowMap.Size,
                    DepthBias = settings.ShadowMap.DepthBias,
                    FarPlaneDistance = settings.ShadowMap.FarPlaneDistance,
                    SplitCount = settings.ShadowMap.SplitCount,
                    SplitLambda = settings.ShadowMap.SplitLambda,
                    VsmBlur = new BlurSettingsDefinition
                    {
                        Radius = settings.ShadowMap.VsmBlur.Radius,
                        Amount = settings.ShadowMap.VsmBlur.Amount
                    }
                }
            };

            serializer.Serialize(resource, definition);
        }
    }
}
