#region Using

using System;
using Microsoft.Xna.Framework;
using Willcraftia.Xna.Framework.Content;
using Willcraftia.Xna.Framework.IO;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class BlocksSceneSettings : IAsset
    {
        public const float DefaultSecondsPerDay = 10f;

        Vector3 midnightSunDirection = DefaultMidnightSunDirection;

        Vector3 midnightMoonDirection = DefaultMidnightMoonDirection;

        Vector3 middayAmbientLightColor = new Vector3(0.6f);

        Vector3 midnightAmbientLightColor = new Vector3(0.1f);

        Vector3 sunRotationAxis;

        Vector3 moonRotationAxis;

        float secondsPerDay = DefaultSecondsPerDay;

        float halfDaySeconds;

        float inverseHalfDaySeconds;

        bool initialized;

        Vector3 sunDirection;

        Vector3 sunlightDirection;

        Vector3 moonDirection;

        Vector3 moonlightDirection;

        Vector3 ambientLightColor;

        Vector3 directionalLightDirection;

        Vector3 directionalLightDiffuseColor;

        Vector3 directionalLightSpecularColor;

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

        public Vector3 SunRotationAxis
        {
            get { return sunRotationAxis; }
        }

        public Vector3 MoonRotationAxis
        {
            get { return moonRotationAxis; }
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

        public Vector3 MoonlightDiffuseColor { get; set; }

        public Vector3 MoonlightSpecularColor { get; set; }

        public SkyColorTable ColorTable { get; private set; }

        public Vector3 SkyColor { get; private set; }

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

        public Vector3 MoonDirection
        {
            get { return moonDirection; }
        }

        public Vector3 MoonlightDirection
        {
            get { return moonlightDirection; }
        }

        public Vector3 DirectionalLightDirection
        {
            get { return directionalLightDirection; }
        }

        public Vector3 DirectionalLightDiffuseColor
        {
            get { return directionalLightDiffuseColor; }
        }

        public Vector3 DirectionalLightSpecularColor
        {
            get { return directionalLightSpecularColor; }
        }

        public bool SunVisible
        {
            get { return 0 <= sunDirection.Y; }
        }

        public bool MoonVisible
        {
            get { return 0 <= moonDirection.Y; }
        }

        public BlocksSceneSettings()
        {
            ColorTable = new SkyColorTable();
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
            ElapsedSecondsPerDay = (float) gameTime.TotalGameTime.TotalSeconds % secondsPerDay;

            UpdateSunDirection();
            UpdateMoonDirection();

            UpdateAmbientLightColor();
            UpdateDirectionalLight();

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

        void UpdateSunDirection()
        {
            var angle = (ElapsedSecondsPerDay / secondsPerDay) * MathHelper.TwoPi;
            var transform = Matrix.CreateFromAxisAngle(sunRotationAxis, angle);
            sunDirection = Vector3.Transform(midnightSunDirection, transform);
            sunDirection.Normalize();

            sunlightDirection = -sunDirection;
        }

        void UpdateMoonDirection()
        {
            var angle = (ElapsedSecondsPerDay / secondsPerDay) * MathHelper.TwoPi;
            var transform = Matrix.CreateFromAxisAngle(moonRotationAxis, angle);
            moonDirection = Vector3.Transform(midnightMoonDirection, transform);
            moonDirection.Normalize();

            moonlightDirection = -moonDirection;
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

        void UpdateDirectionalLight()
        {
            if (SunVisible)
            {
                directionalLightDirection = sunlightDirection;
                directionalLightDiffuseColor = SunlightDiffuseColor;
                directionalLightSpecularColor = SunlightSpecularColor;
            }
            else if (MoonVisible)
            {
                directionalLightDirection = moonlightDirection;
                directionalLightDiffuseColor = MoonlightDiffuseColor;
                directionalLightSpecularColor = MoonlightSpecularColor;
            }
            else
            {
                // TODO
                directionalLightDirection = Vector3.Up;
                directionalLightDiffuseColor = new Vector3(0.1f);
                directionalLightSpecularColor = Vector3.Zero;
            }
        }

        void UpdateSkyColor()
        {
            // 一日の時間を [0, 1] へ変換。
            // 0 が 0 時、1 が 24 時。
            var elapsed = ElapsedSecondsPerDay / SecondsPerDay;

            SkyColor = ColorTable.GetColor(elapsed);
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
