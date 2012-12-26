#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class ChunkEffect
    {
        //====================================================================
        // Real Effect

        Effect backingEffect;

        //====================================================================
        // EffectParameter

        EffectParameter world;

        EffectParameter viewProjection;

        EffectParameter eyePosition;

        EffectParameter ambientLightColor;

        EffectParameter lightDirection;

        EffectParameter lightDiffuseColor;

        EffectParameter lightSpecularColor;

        EffectParameter fogEnabled;

        EffectParameter fogStart;

        EffectParameter fogEnd;

        EffectParameter fogColor;

        EffectParameter tileMap;

        EffectParameter diffuseMap;

        EffectParameter emissiveMap;

        EffectParameter specularMap;

        //====================================================================
        // EffectTechnique

        EffectTechnique defaultTechnique;

        EffectTechnique wireframeTechnique;

        //====================================================================
        // Cached pass

        EffectPass currentPass;

        public Matrix World
        {
            get { return world.GetValueMatrix(); }
            set { world.SetValue(value); }
        }

        public Matrix ViewProjection
        {
            get { return viewProjection.GetValueMatrix(); }
            set { viewProjection.SetValue(value); }
        }

        public Vector3 EyePosition
        {
            get { return eyePosition.GetValueVector3(); }
            set { eyePosition.SetValue(value); }
        }

        public Vector3 AmbientLightColor
        {
            get { return ambientLightColor.GetValueVector3(); }
            set { ambientLightColor.SetValue(value); }
        }

        public Vector3 LightDirection
        {
            get { return lightDirection.GetValueVector3(); }
            set { lightDirection.SetValue(value); }
        }

        public Vector3 LightDiffuseColor
        {
            get { return lightDiffuseColor.GetValueVector3(); }
            set { lightDiffuseColor.SetValue(value); }
        }

        public Vector3 LightSpecularColor
        {
            get { return lightSpecularColor.GetValueVector3(); }
            set { lightSpecularColor.SetValue(value); }
        }

        public bool FogEnabled
        {
            get { return fogEnabled.GetValueSingle() != 0; }
            set { fogEnabled.SetValue(value ? 1 : 0); }
        }

        public float FogStart
        {
            get { return fogStart.GetValueSingle(); }
            set { fogStart.SetValue(value); }
        }

        public float FogEnd
        {
            get { return fogEnd.GetValueSingle(); }
            set { fogEnd.SetValue(value); }
        }

        public Vector3 FogColor
        {
            get { return fogColor.GetValueVector3(); }
            set { fogColor.SetValue(value); }
        }

        public Texture2D TileMap
        {
            get { return tileMap.GetValueTexture2D(); }
            set { tileMap.SetValue(value); }
        }

        public Texture2D DiffuseMap
        {
            get { return diffuseMap.GetValueTexture2D(); }
            set { diffuseMap.SetValue(value); }
        }

        public Texture2D EmissiveMap
        {
            get { return emissiveMap.GetValueTexture2D(); }
            set { emissiveMap.SetValue(value); }
        }

        public Texture2D SpecularMap
        {
            get { return specularMap.GetValueTexture2D(); }
            set { specularMap.SetValue(value); }
        }

        public EffectTechnique CurrentTechnique
        {
            get { return backingEffect.CurrentTechnique; }
            set { backingEffect.CurrentTechnique = value; }
        }

        public ChunkEffect(Effect backingEffect)
        {
            if (backingEffect == null) throw new ArgumentNullException("backingEffect");

            this.backingEffect = backingEffect;

            CacheEffectParameters();
            CacheEffectTechniques();
        }

        public void Apply()
        {
            currentPass.Apply();
        }

        public void EnableDefaultTechnique()
        {
            backingEffect.CurrentTechnique = defaultTechnique;
            currentPass = defaultTechnique.Passes[0];
        }

        public void EnableWireframeTechnique()
        {
            backingEffect.CurrentTechnique = wireframeTechnique;
            currentPass = wireframeTechnique.Passes[0];
        }

        void CacheEffectParameters()
        {
            world = backingEffect.Parameters["World"];
            viewProjection = backingEffect.Parameters["ViewProjection"];

            eyePosition = backingEffect.Parameters["EyePosition"];

            ambientLightColor = backingEffect.Parameters["AmbientLightColor"];
            lightDirection = backingEffect.Parameters["LightDirection"];
            lightDiffuseColor = backingEffect.Parameters["LightDiffuseColor"];
            lightSpecularColor = backingEffect.Parameters["LightSpecularColor"];

            fogEnabled = backingEffect.Parameters["FogEnabled"];
            fogStart = backingEffect.Parameters["FogStart"];
            fogEnd = backingEffect.Parameters["FogEnd"];
            fogColor = backingEffect.Parameters["FogColor"];

            tileMap = backingEffect.Parameters["TileMap"];
            diffuseMap = backingEffect.Parameters["DiffuseMap"];
            emissiveMap = backingEffect.Parameters["EmissiveMap"];
            specularMap = backingEffect.Parameters["SpecularMap"];
        }

        void CacheEffectTechniques()
        {
            defaultTechnique = backingEffect.Techniques["Default"];
            wireframeTechnique = backingEffect.Techniques["Wireframe"];
        }
    }
}
