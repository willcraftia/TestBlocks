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

            //----------------------------------------------------------------
            // スクリーン スペース シャドウ マッピング

            settings.Sssm.Enabled = definition.Sssm.Enabled;
            settings.Sssm.MapScale = definition.Sssm.MapScale;
            settings.Sssm.BlurEnabled = definition.Sssm.BlurEnabled;
            settings.Sssm.Blur.Radius = definition.Sssm.Blur.Radius;
            settings.Sssm.Blur.Amount = definition.Sssm.Blur.Amount;

            //----------------------------------------------------------------
            // スクリーン スペース アンビエント オクルージョン

            settings.Ssao.Enabled = definition.Ssao.Enabled;
            settings.Ssao.MapScale = definition.Ssao.MapScale;
            settings.Ssao.Blur.Radius = definition.Ssao.Blur.Radius;
            settings.Ssao.Blur.Amount = definition.Ssao.Blur.Amount;

            //----------------------------------------------------------------
            // エッジ強調

            settings.Edge.Enabled = definition.Edge.Enabled;
            settings.Edge.MapScale = definition.Edge.MapScale;

            //----------------------------------------------------------------
            // ブルーム

            settings.Bloom.Enabled = definition.Bloom.Enabled;
            settings.Bloom.MapScale = definition.Bloom.MapScale;
            settings.Bloom.Blur.Radius = definition.Bloom.Blur.Radius;
            settings.Bloom.Blur.Amount = definition.Bloom.Blur.Amount;

            //----------------------------------------------------------------
            // 被写界深度

            settings.Dof.Enabled = definition.Dof.Enabled;
            settings.Dof.MapScale = definition.Dof.MapScale;
            settings.Dof.Blur.Radius = definition.Dof.Blur.Radius;
            settings.Dof.Blur.Amount = definition.Dof.Blur.Amount;

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
                },

                //------------------------------------------------------------
                // スクリーン スペース シャドウ マッピング

                Sssm = new SssmSettingsDefinition
                {
                    Enabled = settings.Sssm.Enabled,
                    MapScale = settings.Sssm.MapScale,
                    BlurEnabled = settings.Sssm.BlurEnabled,
                    Blur = new BlurSettingsDefinition
                    {
                        Radius = settings.Sssm.Blur.Radius,
                        Amount = settings.Sssm.Blur.Amount
                    }
                },

                //------------------------------------------------------------
                // スクリーン スペース アンビエント オクルージョン

                Ssao = new SsaoSettingsDefinition
                {
                    Enabled = settings.Ssao.Enabled,
                    MapScale = settings.Ssao.MapScale,
                    Blur = new BlurSettingsDefinition
                    {
                        Radius = settings.Ssao.Blur.Radius,
                        Amount = settings.Ssao.Blur.Amount
                    }
                },

                //------------------------------------------------------------
                // エッジ強調

                Edge = new EdgeSettingsDefinition
                {
                    Enabled = settings.Edge.Enabled,
                    MapScale = settings.Edge.MapScale
                },

                //------------------------------------------------------------
                // ブルーム

                Bloom = new BloomSettingsDefinition
                {
                    Enabled = settings.Bloom.Enabled,
                    MapScale = settings.Bloom.MapScale,
                    Blur = new BlurSettingsDefinition
                    {
                        Radius = settings.Ssao.Blur.Radius,
                        Amount = settings.Ssao.Blur.Amount
                    }
                },

                //------------------------------------------------------------
                // 被写界深度

                Dof = new DofSettingsDefinition
                {
                    Enabled = settings.Dof.Enabled,
                    MapScale = settings.Dof.MapScale,
                    Blur = new BlurSettingsDefinition
                    {
                        Radius = settings.Dof.Blur.Radius,
                        Amount = settings.Dof.Blur.Amount
                    }
                }

            };

            serializer.Serialize(resource, definition);
        }
    }
}
