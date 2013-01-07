#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Graphics;

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

        EffectParameter view;

        EffectParameter projection;

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

        //--------------------------------------------------------------------
        // Shadow Maps

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

        EffectTechnique defaultTechnique;

        EffectTechnique wireframeTechnique;

        EffectTechnique classicShadowTechnique;

        EffectTechnique pcf2x2ShadowTechnique;

        EffectTechnique pcf3x3ShadowTechnique;

        EffectTechnique vsmShadowTechnique;

        //====================================================================
        // Cached pass

        EffectPass currentPass;

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
            get { return splitDistances.GetValueSingleArray(ShadowMap.Settings.MaxSplitCount); }
            set { splitDistances.SetValue(value); }
        }

        public Matrix[] SplitLightViewProjections
        {
            get { return splitLightViewProjections.GetValueMatrixArray(ShadowMap.Settings.MaxSplitCount); }
            set { splitLightViewProjections.SetValue(value); }
        }

        Texture2D[] shadowMapBuffer = new Texture2D[ShadowMap.Settings.MaxSplitCount];

        public Texture2D[] SplitShadowMaps
        {
            get
            {
                for (int i = 0; i < ShadowMap.Settings.MaxSplitCount; i++)
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
            currentPass = backingEffect.CurrentTechnique.Passes[0];
        }

        public void EnableWireframeTechnique()
        {
            backingEffect.CurrentTechnique = wireframeTechnique;
            currentPass = backingEffect.CurrentTechnique.Passes[0];
        }

        public void EnableShadowTechnique(ShadowMap.Techniques shadowMapTechnique)
        {
            switch (shadowMapTechnique)
            {
                case ShadowMap.Techniques.Classic:
                    backingEffect.CurrentTechnique = classicShadowTechnique;
                    break;
                case ShadowMap.Techniques.Pcf2x2:
                    if (backingEffect.CurrentTechnique != pcf2x2ShadowTechnique)
                    {
                        backingEffect.CurrentTechnique = pcf2x2ShadowTechnique;
                        InitializePcfKernel(2);
                    }
                    break;
                case ShadowMap.Techniques.Pcf3x3:
                    if (backingEffect.CurrentTechnique != pcf3x3ShadowTechnique)
                    {
                        backingEffect.CurrentTechnique = pcf3x3ShadowTechnique;
                        InitializePcfKernel(3);
                    }
                    break;
                case ShadowMap.Techniques.Vsm:
                    backingEffect.CurrentTechnique = vsmShadowTechnique;
                    break;
            }
            currentPass = backingEffect.CurrentTechnique.Passes[0];
        }

        void CacheEffectParameters()
        {
            world = backingEffect.Parameters["World"];
            view = backingEffect.Parameters["View"];
            projection = backingEffect.Parameters["Projection"];

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

            depthBias = backingEffect.Parameters["DepthBias"];
            splitCount = backingEffect.Parameters["SplitCount"];
            splitDistances = backingEffect.Parameters["SplitDistances"];
            splitLightViewProjections = backingEffect.Parameters["SplitLightViewProjections"];

            shadowMaps = new EffectParameter[ShadowMap.Settings.MaxSplitCount];
            for (int i = 0; i < shadowMaps.Length; i++)
                shadowMaps[i] = backingEffect.Parameters["ShadowMap" + i];

            shadowMapSize = backingEffect.Parameters["ShadowMapSize"];
            shadowMapTexelSize = backingEffect.Parameters["ShadowMapTexelSize"];

            pcfOffsetsParameter = backingEffect.Parameters["PcfOffsets"];
        }

        void CacheEffectTechniques()
        {
            defaultTechnique = backingEffect.Techniques["Default"];
            wireframeTechnique = backingEffect.Techniques["Wireframe"];

            classicShadowTechnique = backingEffect.Techniques["ClassicShadow"];
            pcf2x2ShadowTechnique = backingEffect.Techniques["Pcf2x2Shadow"];
            pcf3x3ShadowTechnique = backingEffect.Techniques["Pcf3x3Shadow"];
            vsmShadowTechnique = backingEffect.Techniques["VsmShadow"];

            currentPass = defaultTechnique.Passes[0];
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
