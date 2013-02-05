#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    using XnaDirectionalLight = Microsoft.Xna.Framework.Graphics.DirectionalLight;

    /// <summary>
    /// チャンクの描画に使用するエフェクトです。
    /// </summary>
    /// <remarks>
    /// チャンクの描画では、指向性光源を用いません。
    /// このため、指向性光源を設定するためのプロパティは機能せず、
    /// 指向性光源の方向に依存する拡散色 (diffuse) および反射色 (specular) を
    /// 指定することもできません。
    /// 指向性光源に対応しない理由は、
    /// 光が到達し得ない空間における期待しないライティングを避けることにあります。
    /// 例えば、洞窟のような空間では、
    /// シェーダ内での指向性光源によるライティングをすべきではありません。
    /// なお、光源の方向に依存しない環境光 (ambient) および放射色 (emissive) は利用します。
    /// </remarks>
    public sealed class ChunkEffect : Effect, IEffectMatrices, IEffectEye, IEffectLights, IEffectFog, IEffectShadowMap
    {
        readonly XnaDirectionalLight nullDirectionalLight = new XnaDirectionalLight(null, null, null, null);

        Texture2D[] shadowMapBuffer = new Texture2D[ShadowMap.Settings.MaxSplitCount];

        //====================================================================
        // パラメータのキャッシュ

        EffectParameter world;

        EffectParameter view;

        EffectParameter projection;

        EffectParameter eyePosition;

        EffectParameter ambientLightColor;

        EffectParameter fogEnabled;

        EffectParameter fogStart;

        EffectParameter fogEnd;

        EffectParameter fogColor;

        EffectParameter tileMap;

        EffectParameter emissiveMap;

        //--------------------------------------------------------------------
        // シャドウ マップ用

        EffectParameter shadowMapSize;

        EffectParameter shadowMapDepthBias;

        EffectParameter shadowMapCount;

        EffectParameter shadowMapDistances;

        EffectParameter shadowMapLightViewProjections;

        EffectParameter[] shadowMaps;

        //====================================================================
        // テクニックのキャッシュ

        EffectTechnique defaultTechnique;

        EffectTechnique noFogTechnique;

        EffectTechnique occlusionQueryTechnique;

        EffectTechnique wireframeTechnique;

        EffectTechnique basicShadowTechnique;

        EffectTechnique basicShadowNoFogTechnique;

        EffectTechnique pcf2x2ShadowTechnique;

        EffectTechnique pcf2x2ShadowNoFogTechnique;

        EffectTechnique pcf3x3ShadowTechnique;

        EffectTechnique pcf3x3ShadowNoFogTechnique;

        EffectTechnique vsmShadowTechnique;

        EffectTechnique vsmShadowNoFogTechnique;

        //--------------------------------------------------------------------
        // IEffectMatrices

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

        //--------------------------------------------------------------------
        // IEffectEye

        public Vector3 EyePosition
        {
            get { return eyePosition.GetValueVector3(); }
            set { eyePosition.SetValue(value); }
        }

        //--------------------------------------------------------------------
        // IEffectLights

        public Vector3 AmbientLightColor
        {
            get { return ambientLightColor.GetValueVector3(); }
            set { ambientLightColor.SetValue(value); }
        }

        public XnaDirectionalLight DirectionalLight0
        {
            get { return nullDirectionalLight; }
        }

        public XnaDirectionalLight DirectionalLight1
        {
            get { return nullDirectionalLight; }
        }

        public XnaDirectionalLight DirectionalLight2
        {
            get { return nullDirectionalLight; }
        }

        public void EnableDefaultLighting()
        {
            throw new NotSupportedException();
        }

        public bool LightingEnabled { get; set; }

        //--------------------------------------------------------------------
        // IEffectFog

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

        //--------------------------------------------------------------------
        // IEffectShadowMap

        public bool ShadowMapEnabled { get; set; }

        // PCF の場合、PCF テクニックを設定する前に必ず設定していなければならない。
        public int ShadowMapSize
        {
            get { return shadowMapSize.GetValueInt32(); }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException("value");

                shadowMapSize.SetValue(value);
            }
        }

        public ShadowMap.Techniques ShadowMapTechnique { get; set; }

        public float ShadowMapDepthBias
        {
            get { return shadowMapDepthBias.GetValueSingle(); }
            set { shadowMapDepthBias.SetValue(value); }
        }

        public int ShadowMapCount
        {
            get { return shadowMapCount.GetValueInt32(); }
            set { shadowMapCount.SetValue(value); }
        }

        public float[] ShadowMapDistances
        {
            get { return shadowMapDistances.GetValueSingleArray(ShadowMap.Settings.MaxSplitCount); }
            set { shadowMapDistances.SetValue(value); }
        }

        public Matrix[] ShadowMapLightViewProjections
        {
            get { return shadowMapLightViewProjections.GetValueMatrixArray(ShadowMap.Settings.MaxSplitCount); }
            set { shadowMapLightViewProjections.SetValue(value); }
        }

        public Texture2D[] ShadowMaps
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
        // その他プロパティ

        public Texture2D TileMap
        {
            get { return tileMap.GetValueTexture2D(); }
            set { tileMap.SetValue(value); }
        }

        public Texture2D EmissiveMap
        {
            get { return emissiveMap.GetValueTexture2D(); }
            set { emissiveMap.SetValue(value); }
        }

        public bool WireframeEnabled { get; set; }

        public ChunkEffect(Effect cloneSource)
            : base(cloneSource)
        {
            LightingEnabled = true;

            CacheEffectParameters();
            CacheEffectTechniques();
        }

        public void EnableOcclusionQueryTechnique()
        {
            CurrentTechnique = occlusionQueryTechnique;
        }

        public void ResolveCurrentTechnique()
        {
            if (WireframeEnabled)
            {
                CurrentTechnique = wireframeTechnique;
            }
            else
            {
                if (!ShadowMapEnabled)
                {
                    CurrentTechnique = FogEnabled ? defaultTechnique : noFogTechnique;
                }
                else
                {
                    switch (ShadowMapTechnique)
                    {
                        case ShadowMap.Techniques.Basic:
                            CurrentTechnique = FogEnabled ? basicShadowTechnique : basicShadowNoFogTechnique;
                            break;
                        case ShadowMap.Techniques.Pcf2x2:
                            if (CurrentTechnique != pcf2x2ShadowTechnique)
                            {
                                CurrentTechnique = FogEnabled ? pcf2x2ShadowTechnique : pcf2x2ShadowNoFogTechnique;
                                InitializePcfKernel(2);
                            }
                            break;
                        case ShadowMap.Techniques.Pcf3x3:
                            if (CurrentTechnique != pcf3x3ShadowTechnique)
                            {
                                CurrentTechnique = FogEnabled ? pcf3x3ShadowTechnique : pcf3x3ShadowNoFogTechnique;
                                InitializePcfKernel(3);
                            }
                            break;
                        case ShadowMap.Techniques.Vsm:
                            CurrentTechnique = FogEnabled ? vsmShadowTechnique : vsmShadowNoFogTechnique;
                            break;
                    }
                }
            }
        }

        void CacheEffectParameters()
        {
            world = Parameters["World"];
            view = Parameters["View"];
            projection = Parameters["Projection"];

            eyePosition = Parameters["EyePosition"];

            ambientLightColor = Parameters["AmbientLightColor"];

            fogEnabled = Parameters["FogEnabled"];
            fogStart = Parameters["FogStart"];
            fogEnd = Parameters["FogEnd"];
            fogColor = Parameters["FogColor"];

            shadowMapDepthBias = Parameters["ShadowMapDepthBias"];
            shadowMapCount = Parameters["ShadowMapCount"];
            shadowMapDistances = Parameters["ShadowMapDistances"];
            shadowMapLightViewProjections = Parameters["ShadowMapLightViewProjections"];

            shadowMaps = new EffectParameter[ShadowMap.Settings.MaxSplitCount];
            for (int i = 0; i < shadowMaps.Length; i++)
                shadowMaps[i] = Parameters["ShadowMap" + i];

            shadowMapSize = Parameters["ShadowMapSize"];

            tileMap = Parameters["TileMap"];
            emissiveMap = Parameters["EmissiveMap"];
        }

        void CacheEffectTechniques()
        {
            defaultTechnique = Techniques["Default"];
            noFogTechnique = Techniques["NoFog"];
            occlusionQueryTechnique = Techniques["OcclusionQuery"];
            wireframeTechnique = Techniques["Wireframe"];

            basicShadowTechnique = Techniques["BasicShadow"];
            basicShadowNoFogTechnique = Techniques["BasicShadowNoFog"];
            pcf2x2ShadowTechnique = Techniques["Pcf2x2Shadow"];
            pcf2x2ShadowNoFogTechnique = Techniques["Pcf2x2ShadowNoFog"];
            pcf3x3ShadowTechnique = Techniques["Pcf3x3Shadow"];
            pcf3x3ShadowNoFogTechnique = Techniques["Pcf3x3ShadowNoFog"];
            vsmShadowTechnique = Techniques["VsmShadow"];
            vsmShadowNoFogTechnique = Techniques["VsmShadowNoFog"];
        }

        //====================================================================
        // PCF specific

        void InitializePcfKernel(int kernelSize)
        {
            var texelSize = 1 / shadowMapSize.GetValueSingle();

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

            Parameters["PcfOffsets"].SetValue(offsets);
        }

        //
        //====================================================================
    }
}
