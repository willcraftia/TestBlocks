#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class SceneSettings
    {
        public const float DefaultTimeScale = 0.05f;

        Vector3 midnightSunDirection = DefaultMidnightSunDirection;

        Vector3 sunRotationAxis;

        float timeScale = DefaultTimeScale;

        bool initialized;

        Vector3 sunDirection;

        Vector3 sunlightDirection;

        public static Vector3 DefaultMidnightSunDirection
        {
            get
            {
                var direction = new Vector3(0, -1, 1);
                direction.Normalize();
                return direction;
            }
        }

        public bool EarthRotationEnabled { get; set; }

        public Vector3 MidnightSunDirection
        {
            get { return midnightSunDirection; }
            set
            {
                if (value.LengthSquared() == 0) throw new ArgumentException("value");

                midnightSunDirection = value;
                midnightSunDirection.Normalize();
            }
        }

        public Vector3 SunRotationAxis
        {
            get { return sunRotationAxis; }
        }

        public Vector3 AmbientLightColor { get; set; }

        public Vector3 SunlightDiffuseColor { get; set; }

        public Vector3 SunlightSpecularColor { get; set; }

        public float TimeScale
        {
            get { return timeScale; }
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException("value");

                timeScale = value;
            }
        }

        public float Time { get; private set; }

        public Vector3 SunDirection
        {
            get { return sunDirection; }
        }

        public Vector3 SunlightDirection
        {
            get { return sunlightDirection; }
        }

        public void Initialize()
        {
            if (initialized) return;

            InitializeSunRotationAxis();

            initialized = true;
        }

        public void Update(GameTime gameTime)
        {
            Time = (float) gameTime.TotalGameTime.TotalSeconds * timeScale;

            var sunTransform = Matrix.CreateFromAxisAngle(sunRotationAxis, MathHelper.TwoPi * Time);
            sunDirection = Vector3.Transform(midnightSunDirection, sunTransform);
            sunDirection.Normalize();

            sunlightDirection = -sunDirection;
        }

        void InitializeSunRotationAxis()
        {
            var right = Vector3.Cross(midnightSunDirection, Vector3.Up);
            sunRotationAxis = Vector3.Cross(right, midnightSunDirection);
        }
    }
}
