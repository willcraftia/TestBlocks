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

            settings.ShadowMapEnabled = definition.ShadowMapEnabled;
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

            settings.SssmEnabled = definition.SssmEnabled;
            settings.Sssm.MapScale = definition.Sssm.MapScale;
            settings.Sssm.BlurEnabled = definition.Sssm.BlurEnabled;
            settings.Sssm.Blur.Radius = definition.Sssm.Blur.Radius;
            settings.Sssm.Blur.Amount = definition.Sssm.Blur.Amount;

            //----------------------------------------------------------------
            // スクリーン スペース アンビエント オクルージョン

            settings.SsaoEnabled = definition.SsaoEnabled;
            settings.Ssao.MapScale = definition.Ssao.MapScale;
            settings.Ssao.Blur.Radius = definition.Ssao.Blur.Radius;
            settings.Ssao.Blur.Amount = definition.Ssao.Blur.Amount;

            //----------------------------------------------------------------
            // エッジ強調

            settings.EdgeEnabled = definition.EdgeEnabled;
            settings.Edge.MapScale = definition.Edge.MapScale;

            //----------------------------------------------------------------
            // ブルーム

            settings.BloomEnabled = definition.BloomEnabled;
            settings.Bloom.MapScale = definition.Bloom.MapScale;
            settings.Bloom.Blur.Radius = definition.Bloom.Blur.Radius;
            settings.Bloom.Blur.Amount = definition.Bloom.Blur.Amount;

            //----------------------------------------------------------------
            // 被写界深度

            settings.DofEnabled = definition.DofEnabled;
            settings.Dof.MapScale = definition.Dof.MapScale;
            settings.Dof.Blur.Radius = definition.Dof.Blur.Radius;
            settings.Dof.Blur.Amount = definition.Dof.Blur.Amount;

            //----------------------------------------------------------------
            // カラー オーバラップ

            settings.ColorOverlapEnabled = definition.ColorOverlapEnabled;

            //----------------------------------------------------------------
            // モノクローム

            settings.MonochromeEnabled = definition.MonochromeEnabled;

            //----------------------------------------------------------------
            // 走査線

            settings.ScanlineEnabled = definition.ScanlineEnabled;

            //----------------------------------------------------------------
            // レンズ フレア

            settings.LensFlareEnabled = definition.LensFlareEnabled;

            return settings;
        }

        public void Save(IResource resource, object asset)
        {
            var settings = asset as GraphicsSettings;

            var definition = new GraphicsSettingsDefinition
            {
                //------------------------------------------------------------
                // シャドウ マップ

                ShadowMapEnabled = settings.ShadowMapEnabled,
                ShadowMap = new ShadowMapSettingsDefinition
                {
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

                SssmEnabled = settings.SssmEnabled,
                Sssm = new SssmSettingsDefinition
                {
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

                SsaoEnabled = settings.SsaoEnabled,
                Ssao = new SsaoSettingsDefinition
                {
                    MapScale = settings.Ssao.MapScale,
                    Blur = new BlurSettingsDefinition
                    {
                        Radius = settings.Ssao.Blur.Radius,
                        Amount = settings.Ssao.Blur.Amount
                    }
                },

                //------------------------------------------------------------
                // エッジ強調

                EdgeEnabled = settings.EdgeEnabled,
                Edge = new EdgeSettingsDefinition
                {
                    MapScale = settings.Edge.MapScale
                },

                //------------------------------------------------------------
                // ブルーム

                BloomEnabled = settings.BloomEnabled,
                Bloom = new BloomSettingsDefinition
                {
                    MapScale = settings.Bloom.MapScale,
                    Blur = new BlurSettingsDefinition
                    {
                        Radius = settings.Ssao.Blur.Radius,
                        Amount = settings.Ssao.Blur.Amount
                    }
                },

                //------------------------------------------------------------
                // 被写界深度

                DofEnabled = settings.DofEnabled,
                Dof = new DofSettingsDefinition
                {
                    MapScale = settings.Dof.MapScale,
                    Blur = new BlurSettingsDefinition
                    {
                        Radius = settings.Dof.Blur.Radius,
                        Amount = settings.Dof.Blur.Amount
                    }
                },

                //------------------------------------------------------------
                // カラー オーバラップ

                ColorOverlapEnabled = settings.ColorOverlapEnabled,

                //------------------------------------------------------------
                // モノクローム

                MonochromeEnabled = settings.MonochromeEnabled,

                //------------------------------------------------------------
                // 走査線

                ScanlineEnabled = settings.ScanlineEnabled,

                //------------------------------------------------------------
                // レンズ フレア

                LensFlareEnabled = settings.LensFlareEnabled
            };

            serializer.Serialize(resource, definition);
        }
    }
}
