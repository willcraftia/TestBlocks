#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class ShadowSceneEffect : Effect, IEffectMatrices
    {
        public const ShadowMap.Techniques DefaultShadowMapTechnique = ShadowMap.Techniques.Classic;

        //====================================================================
        // EffectParameter

        EffectParameter projection;

        EffectParameter view;

        EffectParameter world;

        EffectParameter depthBias;
        
        EffectParameter splitCount;
        
        EffectParameter splitDistances;
        
        EffectParameter splitLightViewProjections;
        
        EffectParameter[] shadowMaps;

        //--------------------------------------------------------------------
        // Classic specific

        EffectParameter shadowMapSize;

        EffectParameter shadowMapTexelSize;

        //--------------------------------------------------------------------
        // PCF specific

        EffectParameter pcfOffsetsParameter;

        //====================================================================
        // EffectTechnique

        ShadowMap.Techniques shadowMapTechnique;

        EffectTechnique classicTechnique;

        EffectTechnique pcf2x2Technique;

        EffectTechnique pcf3x3Technique;

        EffectTechnique vsmTechnique;

        // I/F
        public Matrix Projection
        {
            get { return projection.GetValueMatrix(); }
            set { projection.SetValue(value); }
        }

        // I/F
        public Matrix View
        {
            get { return view.GetValueMatrix(); }
            set { view.SetValue(value); }
        }

        // I/F
        public Matrix World
        {
            get { return world.GetValueMatrix(); }
            set { world.SetValue(value); }
        }

        public float DepthBias
        {
            get { return depthBias.GetValueSingle(); }
            set { depthBias.SetValue(value); }
        }

        public int SplitCount
        {
            get { return splitCount.GetValueInt32(); }
            set { splitCount.SetValue(value); }
        }

        public float[] SplitDistances
        {
            get { return splitDistances.GetValueSingleArray(ShadowMapSettings.MaxSplitCount); }
            set { splitDistances.SetValue(value); }
        }

        public Matrix[] SplitLightViewProjections
        {
            get { return splitLightViewProjections.GetValueMatrixArray(ShadowMapSettings.MaxSplitCount); }
            set { splitLightViewProjections.SetValue(value); }
        }

        Texture2D[] shadowMapBuffer = new Texture2D[ShadowMapSettings.MaxSplitCount];

        public Texture2D[] SplitShadowMaps
        {
            get
            {
                for (int i = 0; i < ShadowMapSettings.MaxSplitCount; i++)
                    shadowMapBuffer[i] = shadowMaps[i].GetValueTexture2D();
                return shadowMapBuffer;
            }
            set
            {
                if (value == null) return;

                for (int i = 0; i < value.Length; i++)
                    shadowMaps[i].SetValue(value[i]);
            }
        }

        //--------------------------------------------------------------------
        // Classic & PCF specific

        // PCF の場合、PCF テクニックを設定する前に必ず設定していなければならない。
        public int ShadowMapSize
        {
            get { return shadowMapSize.GetValueInt32(); }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException("value");

                shadowMapSize.SetValue(value);
                shadowMapTexelSize.SetValue(1 / (float) value);
            }
        }

        //
        //--------------------------------------------------------------------

        public ShadowMap.Techniques ShadowMapTechnique
        {
            get { return shadowMapTechnique; }
            set
            {
                shadowMapTechnique = value;

                switch (shadowMapTechnique)
                {
                    case ShadowMap.Techniques.Vsm:
                        CurrentTechnique = vsmTechnique;
                        break;
                    case ShadowMap.Techniques.Pcf2x2:
                        CurrentTechnique = pcf2x2Technique;
                        InitializePcfKernel(2);
                        break;
                    case ShadowMap.Techniques.Pcf3x3:
                        CurrentTechnique = pcf3x3Technique;
                        InitializePcfKernel(3);
                        break;
                    default:
                        CurrentTechnique = classicTechnique;
                        break;
                }
            }
        }

        public ShadowSceneEffect(Effect cloneSource)
            : base(cloneSource)
        {
            world = Parameters["World"];
            view = Parameters["View"];
            projection = Parameters["Projection"];

            depthBias = Parameters["DepthBias"];
            splitCount = Parameters["SplitCount"];
            splitDistances = Parameters["SplitDistances"];
            splitLightViewProjections = Parameters["SplitLightViewProjections"];

            shadowMaps = new EffectParameter[ShadowMapSettings.MaxSplitCount];
            for (int i = 0; i < shadowMaps.Length; i++)
                shadowMaps[i] = Parameters["ShadowMap" + i];

            shadowMapSize = Parameters["ShadowMapSize"];
            shadowMapTexelSize = Parameters["ShadowMapTexelSize"];

            pcfOffsetsParameter = Parameters["PcfOffsets"];

            classicTechnique = Techniques["Classic"];
            pcf2x2Technique = Techniques["Pcf2x2"];
            pcf3x3Technique = Techniques["Pcf3x3"];
            vsmTechnique = Techniques["Vsm"];

            ShadowMapTechnique = DefaultShadowMapTechnique;
        }

        //====================================================================
        // PCF specific

        void InitializePcfKernel(int kernelSize)
        {
            var texelSize = shadowMapTexelSize.GetValueSingle();

            int start;
            if (kernelSize % 2 == 0)
            {
                start = -(kernelSize / 2) + 1;
            }
            else
            {
                start = -(kernelSize - 1) / 2;
            }
            var end = start + kernelSize;

            var tapCount = kernelSize * kernelSize;
            var offsets = new Vector2[tapCount];

            int i = 0;
            for (int y = start; y < end; y++)
            {
                for (int x = start; x < end; x++)
                {
                    offsets[i++] = new Vector2(x, y) * texelSize;
                }
            }

            pcfOffsetsParameter.SetValue(offsets);
        }

        //
        //====================================================================
    }
}
