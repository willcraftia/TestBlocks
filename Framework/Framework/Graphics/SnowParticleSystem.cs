#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class SnowParticleSystem : ParticleSystem
    {
        public SnowParticleSystem(Effect effect, Texture2D texture)
            : base(effect, texture)
        {
        }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.MaxParticles = 4000;
            settings.Duration = TimeSpan.FromSeconds(5);
            settings.DurationRandomness = 0;
            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 0;
            settings.MinVerticalVelocity = -10;
            settings.MaxVerticalVelocity = -10;
            settings.Gravity = Vector3.Down;
            settings.EndVelocity = 1;
            settings.MinColor = Color.White;
            settings.MaxColor = Color.White;
            settings.MinRotateSpeed = 0;
            settings.MaxRotateSpeed = 0;
            settings.MinStartSize = 0.5f;
            settings.MaxStartSize = 0.5f;
            settings.MinEndSize = 0.2f;
            settings.MaxEndSize = 0.2f;

            // 加算ブレンディングを使用します。
            settings.BlendState = BlendState.AlphaBlend;
        }
    }
}
