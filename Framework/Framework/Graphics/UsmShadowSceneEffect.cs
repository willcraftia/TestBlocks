#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class UsmShadowSceneEffect : IEffectShadowScene
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
        
        EffectParameter lightViewProjection;
        
        EffectParameter shadowMap;

        //--------------------------------------------------------------------
        // PCF specific

        EffectParameter tapCountParameter;

        EffectParameter offsetsParameter;

        //====================================================================
        // EffectTechnique

        ShadowMapTechniques technique;

        EffectTechnique classicTechnique;

        EffectTechnique pcfTechnique;

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

        public Matrix LightViewProjection
        {
            get { return lightViewProjection.GetValueMatrix(); }
            set { lightViewProjection.SetValue(value); }
        }

        public Texture2D ShadowMap
        {
            get { return shadowMap.GetValueTexture2D(); }
            set { shadowMap.SetValue(value); }
        }

        //--------------------------------------------------------------------
        // PCF specific

        public int ShadowMapSize { get; set; }

        public int PcfKernelSize { get; set; }

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
                    case ShadowMapTechniques.Pcf:
                        backingEffect.CurrentTechnique = pcfTechnique;
                        break;
                    default:
                        backingEffect.CurrentTechnique = classicTechnique;
                        break;
                }

                currentPass = backingEffect.CurrentTechnique.Passes[0];
            }
        }

        public UsmShadowSceneEffect(Effect backingEffect)
        {
            if (backingEffect == null) throw new ArgumentNullException("backingEffect");

            this.backingEffect = backingEffect;

            world = backingEffect.Parameters["World"];
            view = backingEffect.Parameters["View"];
            projection = backingEffect.Parameters["Projection"];

            depthBias = backingEffect.Parameters["DepthBias"];
            lightViewProjection = backingEffect.Parameters["LightViewProjection"];

            shadowMap = backingEffect.Parameters["ShadowMap"];

            tapCountParameter = backingEffect.Parameters["TapCount"];
            offsetsParameter = backingEffect.Parameters["Offsets"];

            classicTechnique = backingEffect.Techniques["Classic"];
            pcfTechnique = backingEffect.Techniques["Pcf"];
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

        public void InitializePcfKernel()
        {
            var texelSize = 1.0f / (float) ShadowMapSize;

            int start;
            if (PcfKernelSize % 2 == 0)
            {
                start = -(PcfKernelSize / 2) + 1;
            }
            else
            {
                start = -(PcfKernelSize - 1) / 2;
            }
            var end = start + PcfKernelSize;

            var tapCount = PcfKernelSize * PcfKernelSize;
            var offsets = new Vector2[tapCount];

            int i = 0;
            for (int y = start; y < end; y++)
            {
                for (int x = start; x < end; x++)
                {
                    offsets[i++] = new Vector2(x, y) * texelSize;
                }
            }

            tapCountParameter.SetValue(tapCount);
            offsetsParameter.SetValue(offsets);
        }

        //
        //====================================================================
    }
}
