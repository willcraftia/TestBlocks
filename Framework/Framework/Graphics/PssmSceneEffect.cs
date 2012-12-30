#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class PssmSceneEffect : Effect, IEffectMatrices
    {
        EffectParameter projection;

        EffectParameter view;

        EffectParameter world;

        EffectParameter depthBias;
        
        EffectParameter splitCount;
        
        EffectParameter splitDistances;
        
        EffectParameter splitViewProjections;
        
        EffectParameter[] shadowMaps;

        EffectTechnique classicTechnique;

        EffectTechnique pcfTechnique;

        EffectTechnique vsmTechnique;

        //--------------------------------------------------------------------
        // PCF specific

        EffectParameter tapCountParameter;

        EffectParameter offsetsParameter;

        public int ShadowMapSize { get; set; }

        public int PcfKernelSize { get; set; }

        //
        //--------------------------------------------------------------------

        // I/F
        public Matrix Projection
        {
            get { return projection.GetValueMatrix(); }
            set { projection.SetValue(value); }
        }

        // I/F
        public Matrix View
        {
            get { return view.GetValueMatrix(); }
            set { view.SetValue(value); }
        }

        // I/F
        public Matrix World
        {
            get { return world.GetValueMatrix(); }
            set { world.SetValue(value); }
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
            get { return splitDistances.GetValueSingleArray(PssmSettings.MaxSplitCount); }
            set { splitDistances.SetValue(value); }
        }

        public Matrix[] SplitViewProjections
        {
            get { return splitViewProjections.GetValueMatrixArray(PssmSettings.MaxSplitCount); }
            set { splitViewProjections.SetValue(value); }
        }

        public void SetShadowMaps(MultiRenderTargets renderTargets)
        {
            for (int i = 0; i < renderTargets.Count; i++)
                shadowMaps[i].SetValue(renderTargets[i]);
        }

        public PssmSceneEffect(Effect cloneSource)
            : base(cloneSource)
        {
            world = Parameters["World"];
            view = Parameters["View"];
            projection = Parameters["Projection"];

            depthBias = Parameters["DepthBias"];
            splitCount = Parameters["SplitCount"];
            splitDistances = Parameters["SplitDistances"];
            splitViewProjections = Parameters["SplitViewProjections"];

            shadowMaps = new EffectParameter[PssmSettings.MaxSplitCount];
            for (int i = 0; i < shadowMaps.Length; i++)
                shadowMaps[i] = Parameters["ShadowMap" + i];

            tapCountParameter = Parameters["TapCount"];
            offsetsParameter = Parameters["Offsets"];

            classicTechnique = Techniques["Classic"];
            pcfTechnique = Techniques["Pcf"];
            vsmTechnique = Techniques["Vsm"];
        }

        public void EnableTechnique(ShadowMapTechniques technique)
        {
            switch (technique)
            {
                case ShadowMapTechniques.Classic:
                    CurrentTechnique = classicTechnique;
                    break;
                case ShadowMapTechniques.Pcf:
                    CurrentTechnique = pcfTechnique;
                    break;
                case ShadowMapTechniques.Vsm:
                    CurrentTechnique = vsmTechnique;
                    break;
            }
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
