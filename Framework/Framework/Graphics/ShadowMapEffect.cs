#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class ShadowMapEffect : Effect, IEffectMatrices
    {
        public const ShadowMapTechniques DefaultShadowMapTechnique = ShadowMapTechniques.Classic;

        //====================================================================
        // EffectParameter

        EffectParameter world;
        
        EffectParameter lightViewProjection;

        //====================================================================
        // EffectTechnique

        ShadowMapTechniques shadowMapTechnique;

        EffectTechnique defaultTechnique;

        EffectTechnique vsmTechnique;

        // I/F
        public Matrix Projection
        {
            get { return Matrix.Identity; }
            set { }
        }

        // I/F
        public Matrix View
        {
            get { return Matrix.Identity; }
            set { }
        }

        // I/F
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

        public ShadowMapTechniques ShadowMapTechnique
        {
            get { return shadowMapTechnique; }
            set
            {
                shadowMapTechnique = value;

                switch (shadowMapTechnique)
                {
                    case ShadowMapTechniques.Vsm:
                        CurrentTechnique = vsmTechnique;
                        break;
                    default:
                        CurrentTechnique = defaultTechnique;
                        break;
                }
            }
        }

        public ShadowMapEffect(Effect cloneSource)
            : base(cloneSource)
        {
            world = Parameters["World"];
            lightViewProjection = Parameters["LightViewProjection"];

            defaultTechnique = Techniques["Default"];
            vsmTechnique = Techniques["Vsm"];

            ShadowMapTechnique = DefaultShadowMapTechnique;
        }
    }
}
