#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Blocks.Serialization
{
    public struct ParticleSettingsDefinition
    {
        public string Name;

        public int MaxParticles;

        public double Duration;

        public float DurationRandomness;

        public float EmitterVelocitySensitivity;

        public float MinHorizontalVelocity;

        public float MaxHorizontalVelocity;

        public float MinVerticalVelocity;

        public float MaxVerticalVelocity;

        public Vector3 Gravity;

        public float EndVelocity;

        public Vector4 MinColor;

        public Vector4 MaxColor;

        public float MinRotateSpeed;

        public float MaxRotateSpeed;

        public float MinStartSize;

        public float MaxStartSize;

        public float MinEndSize;

        public float MaxEndSize;

        public string Texture;

        public BlendState BlendState;
    }
}
