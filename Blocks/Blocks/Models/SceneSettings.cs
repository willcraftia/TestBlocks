#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class SceneSettings
    {
        public const float DefaultSecondsPerDay = 10f;

        Vector3 midnightSunDirection = DefaultMidnightSunDirection;

        Vector3 middayAmbientLightColor;

        Vector3 midnightAmbientLightColor;

        Vector3 sunRotationAxis;

        float secondsPerDay = DefaultSecondsPerDay;

        float halfDaySeconds;

        float inverseHalfDaySeconds;

        bool initialized;

        Vector3 sunDirection;

        Vector3 sunlightDirection;

        Vector3 ambientLightColor;

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

        public Vector3 MiddayAmbientLightColor
        {
            get { return middayAmbientLightColor; }
            set { middayAmbientLightColor = value; }
        }

        public Vector3 MidnightAmbientLightColor
        {
            get { return midnightAmbientLightColor; }
            set { midnightAmbientLightColor = value; }
        }

        public Vector3 SunlightDiffuseColor { get; set; }

        public Vector3 SunlightSpecularColor { get; set; }

        public float SecondsPerDay
        {
            get { return secondsPerDay; }
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException("value");

                secondsPerDay = value;
            }
        }

        public float ElapsedSecondsPerDay { get; private set; }

        public Vector3 AmbientLightColor
        {
            get { return ambientLightColor; }
        }

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

            halfDaySeconds = secondsPerDay * 0.5f;
            inverseHalfDaySeconds = 1 / halfDaySeconds;

            initialized = true;
        }

        public void Update(GameTime gameTime)
        {
            ElapsedSecondsPerDay = (float) gameTime.TotalGameTime.TotalSeconds % secondsPerDay;

            var sunAngle = (ElapsedSecondsPerDay / secondsPerDay) * MathHelper.TwoPi;
            var sunTransform = Matrix.CreateFromAxisAngle(sunRotationAxis, sunAngle);
            sunDirection = Vector3.Transform(midnightSunDirection, sunTransform);
            sunDirection.Normalize();

            if (ElapsedSecondsPerDay < halfDaySeconds)
            {
                var amount = ElapsedSecondsPerDay * inverseHalfDaySeconds;
                Vector3.Lerp(ref midnightAmbientLightColor, ref middayAmbientLightColor, amount, out ambientLightColor);
            }
            else
            {
                var amount = (ElapsedSecondsPerDay - halfDaySeconds) * inverseHalfDaySeconds;
                Vector3.Lerp(ref middayAmbientLightColor, ref midnightAmbientLightColor, amount, out ambientLightColor);
            }

            sunlightDirection = -sunDirection;
        }

        void InitializeSunRotationAxis()
        {
            var right = Vector3.Cross(midnightSunDirection, Vector3.Up);
            sunRotationAxis = Vector3.Cross(right, midnightSunDirection);
        }
    }
}
