#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class ShadowSceneEffect : IEffectShadowScene
    {
        public const ShadowMapTechniques DefaultShadowMapTechnique = ShadowMapTechniques.Vsm;

        //====================================================================
        // Real Effect

        Effect backingEffect;

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
        // PCF specific

        EffectParameter pcfOffsetsParameter;

        //====================================================================
        // EffectTechnique

        ShadowMapTechniques technique;

        EffectTechnique classicTechnique;

        EffectTechnique pcf2x2Technique;

        EffectTechnique pcf3x3Technique;

        EffectTechnique vsmTechnique;

        //====================================================================
        // Cached pass

        EffectPass currentPass;

        // I/F
        public Matrix World
        {
            get { return world.GetValueMatrix(); }
            set { world.SetValue(value); }
        }

        public Matrix View
        {
            get { return view.GetValueMatrix(); }
            set { view.SetValue(value); }
        }

        public Matrix Projection
        {
            get { return projection.GetValueMatrix(); }
            set { projection.SetValue(value); }
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
        // PCF specific

        // PCF へテクニックを変更する前に必ず設定していなければならない。
        public int ShadowMapSize { get; set; }

        //
        //--------------------------------------------------------------------

        public ShadowMapTechniques Technique
        {
            get { return technique; }
            set
            {
                technique = value;

                switch (technique)
                {
                    case ShadowMapTechniques.Vsm:
                        backingEffect.CurrentTechnique = vsmTechnique;
                        break;
                    case ShadowMapTechniques.Pcf2x2:
                        backingEffect.CurrentTechnique = pcf2x2Technique;
                        InitializePcfKernel(2);
                        break;
                    case ShadowMapTechniques.Pcf3x3:
                        backingEffect.CurrentTechnique = pcf3x3Technique;
                        InitializePcfKernel(3);
                        break;
                    default:
                        backingEffect.CurrentTechnique = classicTechnique;
                        break;
                }

                currentPass = backingEffect.CurrentTechnique.Passes[0];
            }
        }

        public ShadowSceneEffect(Effect backingEffect)
        {
            if (backingEffect == null) throw new ArgumentNullException("backingEffect");

            this.backingEffect = backingEffect;

            world = backingEffect.Parameters["World"];
            view = backingEffect.Parameters["View"];
            projection = backingEffect.Parameters["Projection"];

            depthBias = backingEffect.Parameters["DepthBias"];
            splitCount = backingEffect.Parameters["SplitCount"];
            splitDistances = backingEffect.Parameters["SplitDistances"];
            splitLightViewProjections = backingEffect.Parameters["SplitLightViewProjections"];

            shadowMaps = new EffectParameter[ShadowMapSettings.MaxSplitCount];
            for (int i = 0; i < shadowMaps.Length; i++)
                shadowMaps[i] = backingEffect.Parameters["ShadowMap" + i];

            pcfOffsetsParameter = backingEffect.Parameters["PcfOffsets"];

            classicTechnique = backingEffect.Techniques["Classic"];
            pcf2x2Technique = backingEffect.Techniques["Pcf2x2"];
            pcf3x3Technique = backingEffect.Techniques["Pcf3x3"];
            vsmTechnique = backingEffect.Techniques["Vsm"];

            Technique = DefaultShadowMapTechnique;
        }

        // I/F
        public void Apply()
        {
            currentPass.Apply();
        }

        //====================================================================
        // PCF specific

        void InitializePcfKernel(int kernelSize)
        {
            var texelSize = 1.0f / (float) ShadowMapSize;

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
