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

        Effect backingEffect;

        //====================================================================
        // EffectParameter

        EffectParameter worldViewProjection;

        EffectParameter sunDirection;

        EffectParameter sunDiffuseColor;

        EffectParameter sunThreshold;

        EffectParameter sunVisible;

        EffectParameter time;

        EffectParameter texture;

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

        public float Time
        {
            get { return time.GetValueSingle(); }
            set { time.SetValue(value); }
        }

        public Texture2D Texture
        {
            get { return texture.GetValueTexture2D(); }
            set { texture.SetValue(value); }
        }

        public SkySphereEffect(Effect backingEffect)
        {
            if (backingEffect == null) throw new ArgumentNullException("backingEffect");

            this.backingEffect = backingEffect;

            CacheEffectParameters();
            CacheEffectTechniques();
        }

        public void EnableDefaultTechnique()
        {
            backingEffect.CurrentTechnique = defaultTechnique;
            currentPass = defaultTechnique.Passes[0];
        }

        public void Apply()
        {
            currentPass.Apply();
        }
        
        void CacheEffectParameters()
        {
            worldViewProjection = backingEffect.Parameters["WorldViewProjection"];

            sunDirection = backingEffect.Parameters["SunDirection"];
            sunDiffuseColor = backingEffect.Parameters["SunDiffuseColor"];
            sunThreshold = backingEffect.Parameters["SunThreshold"];
            sunVisible = backingEffect.Parameters["SunVisible"];

            time = backingEffect.Parameters["Time"];

            texture = backingEffect.Parameters["Texture"];
        }

        void CacheEffectTechniques()
        {
            defaultTechnique = backingEffect.Techniques["Default"];
        }
    }
}
