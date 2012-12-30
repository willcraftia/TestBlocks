#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class ShadowMapEffect
    {
        public const ShadowMapTechniques DefaultShadowMapTechnique = ShadowMapTechniques.Vsm;

        //====================================================================
        // Real Effect

        Effect backingEffect;

        //====================================================================
        // EffectParameter

        EffectParameter world;
        
        EffectParameter lightViewProjection;

        //====================================================================
        // EffectTechnique

        ShadowMapTechniques technique;

        EffectTechnique defaultTechnique;

        EffectTechnique vsmTechnique;

        //====================================================================
        // Cached pass

        EffectPass currentPass;

        public Matrix World
        {
            get { return world.GetValueMatrix(); }
            set { world.SetValue(value); }
        }

        public Matrix LightViewProjection
        {
            get { return lightViewProjection.GetValueMatrix(); }
            set { lightViewProjection.SetValue(value); }
        }

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
                    default:
                        backingEffect.CurrentTechnique = defaultTechnique;
                        break;
                }
                
                currentPass = backingEffect.CurrentTechnique.Passes[0];
            }
        }

        public ShadowMapEffect(Effect backingEffect)
        {
            if (backingEffect == null) throw new ArgumentNullException("backingEffect");

            this.backingEffect = backingEffect;

            world = backingEffect.Parameters["World"];
            lightViewProjection = backingEffect.Parameters["LightViewProjection"];

            defaultTechnique = backingEffect.Techniques["Default"];
            vsmTechnique = backingEffect.Techniques["Vsm"];

            Technique = DefaultShadowMapTechnique;
        }

        public void Apply()
        {
            currentPass.Apply();
        }
    }
}
