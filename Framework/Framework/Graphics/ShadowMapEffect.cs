#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class ShadowMapEffect : Effect
    {
        EffectParameter world;
        
        EffectParameter lightViewProjection;

        EffectTechnique defaultTechnique;

        EffectTechnique vsmTechnique;

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

        public ShadowMapEffect(Effect cloneSource)
            : base(cloneSource)
        {
            world = Parameters["World"];
            lightViewProjection = Parameters["LightViewProjection"];
            defaultTechnique = Techniques["Default"];
            vsmTechnique = Techniques["Vsm"];
        }

        public void EnableTechnique(ShadowMapTechniques technique)
        {
            if (technique == ShadowMapTechniques.Vsm)
            {
                CurrentTechnique = vsmTechnique;
            }
            else
            {
                CurrentTechnique = defaultTechnique;
            }
        }
    }
}
