#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class ShadowSettings
    {
        //--------------------------------------------------------------------
        // Shadow Map

        public const int DefaultShadowMapSize = 512;
        
        public const SurfaceFormat DefaultShadowMapFormat = SurfaceFormat.Vector2;
        
        public const ShadowTests DefaultShadowTest = ShadowTests.Vsm;
        
        public const float DefaultShadowNearPlaneDistance = 0.1f;
        
        public const float DefaultShadowFarPlaneDistance = 500.0f;

        //--------------------------------------------------------------------
        // Shadow Scene Map

        public const float DefaultDepthBias = 0.005f;

        //--------------------------------------------------------------------
        // Light Frustum

        public const LightFrustumShape DefaultLightFrustumShape = LightFrustumShape.Pssm;

        public const float DefaultBackwardLightVolumeRadius = 10.0f;

        //--------------------------------------------------------------------
        // Fields

        public bool Enabled = false;

        public ShadowTests Test = DefaultShadowTest;

        public int Size = DefaultShadowMapSize;

        public SurfaceFormat Format = DefaultShadowMapFormat;

        public float DepthBias = DefaultDepthBias;

        public float Fov = MathHelper.PiOver4;

        public float AspectRatio = 1.0f;

        public float NearPlaneDistance = DefaultShadowNearPlaneDistance;

        public float FarPlaneDistance = DefaultShadowFarPlaneDistance;

        public float BackwardLightVolumeRadius = DefaultBackwardLightVolumeRadius;

        public LightFrustumShape Shape = DefaultLightFrustumShape;
    }
}
