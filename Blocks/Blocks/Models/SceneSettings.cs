#region Using

using System;
using Microsoft.Xna.Framework;
using Willcraftia.Xna.Framework.Content;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class SceneSettings : IAsset
    {
        public const float DefaultSecondsPerDay = 10f;

        Vector3 midnightSunDirection = DefaultMidnightSunDirection;

        Vector3 midnightMoonDirection = DefaultMidnightMoonDirection;

        Vector3 middayAmbientLightColor = new Vector3(0.6f);

        Vector3 midnightAmbientLightColor = new Vector3(0.1f);

        Vector3 shadowColor = Vector3.Zero;

        Vector3 sunRotationAxis;

        Vector3 moonRotationAxis;

        float secondsPerDay = DefaultSecondsPerDay;

        float fixedSecondsPerDay;

        float halfDaySeconds;

        float inverseHalfDaySeconds;

        bool initialized;

        Vector3 sunDirection;

        Vector3 moonDirection;

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

        public static Vector3 DefaultMidnightMoonDirection
        {
            get
            {
                var direction = new Vector3(0, 1, 1);
                direction.Normalize();
                return direction;
            }
        }

        // I/F
        public IResource Resource { get; set; }

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

        public Vector3 MidnightMoonDirection
        {
            get { return midnightMoonDirection; }
            set
            {
                if (value.LengthSquared() == 0) throw new ArgumentException("value");

                midnightMoonDirection = value;
                midnightMoonDirection.Normalize();
            }
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

        public Vector3 ShadowColor
        {
            get { return shadowColor; }
            set { shadowColor = value; }
        }

        public Vector3 SunRotationAxis
        {
            get { return sunRotationAxis; }
        }

        public Vector3 MoonRotationAxis
        {
            get { return moonRotationAxis; }
        }

        public DirectionalLight Sunlight { get; private set; }

        public DirectionalLight Moonlight { get; private set; }

        public TimeColorCollection SkyColors { get; private set; }

        public Vector3 CurrentSkyColor { get; private set; }

        public float SecondsPerDay
        {
            get { return secondsPerDay; }
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException("value");

                secondsPerDay = value;
            }
        }

        public bool TimeStopped { get; set; }

        public float FixedSecondsPerDay
        {
            get { return fixedSecondsPerDay; }
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException("value");

                fixedSecondsPerDay = value;
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

        public Vector3 MoonDirection
        {
            get { return moonDirection; }
        }

        public bool SunVisible
        {
            get { return 0 <= sunDirection.Y; }
        }

        public bool MoonVisible
        {
            get { return 0 <= moonDirection.Y; }
        }

        public SceneSettings()
        {
            Sunlight = new DirectionalLight("Sun");
            Sunlight.Direction = -DefaultMidnightSunDirection;

            Moonlight = new DirectionalLight("Moon");
            Moonlight.Direction = -DefaultMidnightMoonDirection;

            SkyColors = new TimeColorCollection();
        }

        public void Initialize()
        {
            if (initialized) return;

            InitializeSunRotationAxis();
            InitializeMoonRotationAxis();

            halfDaySeconds = secondsPerDay * 0.5f;
            inverseHalfDaySeconds = 1 / halfDaySeconds;

            initialized = true;
        }

        public void Update(GameTime gameTime)
        {
            if (!TimeStopped)
            {
                ElapsedSecondsPerDay = (float) gameTime.TotalGameTime.TotalSeconds % secondsPerDay;
            }
            else
            {
                ElapsedSecondsPerDay = fixedSecondsPerDay;
            }

            UpdateSun();
            UpdateMoon();

            UpdateAmbientLightColor();

            UpdateSkyColor();
        }

        public void CalculateSkyColor(ref Vector3 middaySkyColor, ref Vector3 midnightSkyColor, out Vector3 resul)
        {
            if (ElapsedSecondsPerDay < halfDaySeconds)
            {
                var amount = ElapsedSecondsPerDay * inverseHalfDaySeconds;
                Vector3.Lerp(ref midnightSkyColor, ref middaySkyColor, amount, out resul);
            }
            else
            {
                var amount = (ElapsedSecondsPerDay - halfDaySeconds) * inverseHalfDaySeconds;
                Vector3.Lerp(ref middaySkyColor, ref midnightSkyColor, amount, out resul);
            }
        }

        void UpdateSun()
        {
            var angle = (ElapsedSecondsPerDay / secondsPerDay) * MathHelper.TwoPi;
            Matrix transform;
            Matrix.CreateFromAxisAngle(ref sunRotationAxis, angle, out transform);
            Vector3.Transform(ref midnightSunDirection, ref transform, out sunDirection);
            sunDirection.Normalize();

            Sunlight.Direction = -sunDirection;
        }

        void UpdateMoon()
        {
            var angle = (ElapsedSecondsPerDay / secondsPerDay) * MathHelper.TwoPi;
            Matrix transform;
            Matrix.CreateFromAxisAngle(ref moonRotationAxis, angle, out transform);
            Vector3.Transform(ref midnightMoonDirection, ref transform, out moonDirection);
            moonDirection.Normalize();

            Moonlight.Direction = -moonDirection;
        }

        void UpdateAmbientLightColor()
        {
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
        }

        void UpdateSkyColor()
        {
            // 一日の時間を [0, 1] へ変換。
            // 0 が 0 時、1 が 24 時。
            var elapsed = ElapsedSecondsPerDay / SecondsPerDay;

            CurrentSkyColor = SkyColors.GetColor(elapsed);
        }

        void InitializeSunRotationAxis()
        {
            var right = Vector3.Cross(midnightSunDirection, Vector3.Up);
            sunRotationAxis = Vector3.Cross(right, midnightSunDirection);
        }

        void InitializeMoonRotationAxis()
        {
            var right = Vector3.Cross(midnightMoonDirection, Vector3.Up);
            moonRotationAxis = Vector3.Cross(right, midnightMoonDirection);
        }

        #region ToString

        public override string ToString()
        {
            return "[Uri:" + ((Resource != null) ? Resource.AbsoluteUri : string.Empty) + "]";
        }

        #endregion
    }
}
