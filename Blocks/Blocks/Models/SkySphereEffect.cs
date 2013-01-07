#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class SkySphereEffect
    {
        //====================================================================
        // Real Effect

        Effect effect;

        //====================================================================
        // EffectParameter

        EffectParameter worldViewProjection;

        EffectParameter skyColor;

        EffectParameter sunDirection;

        EffectParameter sunDiffuseColor;

        EffectParameter sunThreshold;

        EffectParameter sunVisible;

        //====================================================================
        // EffectTechnique

        EffectTechnique defaultTechnique;

        //====================================================================
        // Cached pass

        EffectPass currentPass;

        public Matrix WorldViewProjection
        {
            get { return worldViewProjection.GetValueMatrix(); }
            set { worldViewProjection.SetValue(value); }
        }

        public Vector3 SkyColor
        {
            get { return skyColor.GetValueVector3(); }
            set { skyColor.SetValue(value); }
        }

        public Vector3 SunDirection
        {
            get { return sunDirection.GetValueVector3(); }
            set { sunDirection.SetValue(value); }
        }

        public Vector3 SunDiffuseColor
        {
            get { return sunDiffuseColor.GetValueVector3(); }
            set { sunDiffuseColor.SetValue(value); }
        }

        public float SunThreshold
        {
            get { return sunThreshold.GetValueSingle(); }
            set { sunThreshold.SetValue(value); }
        }

        public bool SunVisible
        {
            get { return sunVisible.GetValueSingle() != 0; }
            set { sunVisible.SetValue(value ? 1 : 0); }
        }

        public SkySphereEffect(Effect effect)
        {
            if (effect == null) throw new ArgumentNullException("effect");

            this.effect = effect;

            worldViewProjection = effect.Parameters["WorldViewProjection"];
            skyColor = effect.Parameters["SkyColor"];
            sunDirection = effect.Parameters["SunDirection"];
            sunDiffuseColor = effect.Parameters["SunDiffuseColor"];
            sunThreshold = effect.Parameters["SunThreshold"];
            sunVisible = effect.Parameters["SunVisible"];
            defaultTechnique = effect.Techniques["Default"];

            currentPass = effect.CurrentTechnique.Passes[0];
        }

        public void Apply()
        {
            currentPass.Apply();
        }
    }
}
