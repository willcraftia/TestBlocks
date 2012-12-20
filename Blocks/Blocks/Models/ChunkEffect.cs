#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class ChunkEffect
    {
        EffectParameter worldViewProjection;

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

        public Effect BackingEffect { get; private set; }

        public Matrix WorldViewProjection
        {
            get { return worldViewProjection.GetValueMatrix(); }
            set { worldViewProjection.SetValue(value); }
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

        public EffectTechnique DefaultTequnique { get; private set; }

        public ChunkEffect(Effect backingEffect)
        {
            if (backingEffect == null) throw new ArgumentNullException("backingEffect");

            BackingEffect = backingEffect;

            CacheEffectParameters();
            CacheEffectTequniques();
        }

        void CacheEffectParameters()
        {
            worldViewProjection = BackingEffect.Parameters["WorldViewProjection"];

            eyePosition = BackingEffect.Parameters["EyePosition"];

            ambientLightColor = BackingEffect.Parameters["AmbientLightColor"];
            lightDirection = BackingEffect.Parameters["LightDirection"];
            lightDiffuseColor = BackingEffect.Parameters["LightDiffuseColor"];
            lightSpecularColor = BackingEffect.Parameters["LightSpecularColor"];

            fogEnabled = BackingEffect.Parameters["FogEnabled"];
            fogStart = BackingEffect.Parameters["FogStart"];
            fogEnd = BackingEffect.Parameters["FogEnd"];
            fogColor = BackingEffect.Parameters["FogColor"];

            tileMap = BackingEffect.Parameters["TileMap"];
            diffuseMap = BackingEffect.Parameters["DiffuseMap"];
            emissiveMap = BackingEffect.Parameters["EmissiveMap"];
            specularMap = BackingEffect.Parameters["SpecularMap"];
        }

        void CacheEffectTequniques()
        {
            DefaultTequnique = BackingEffect.Techniques["Default"];
        }
    }
}
