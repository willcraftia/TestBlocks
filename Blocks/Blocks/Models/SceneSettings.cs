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

        Vector3 shadowColor = Vector3.Zero;

        Vector3 sunRotationAxis;

        Vector3 moonRotationAxis;

        float initialFogStartScale = 0.7f;

        float initialFogEndScale = 0.9f;

        float secondsPerDay = DefaultSecondsPerDay;

        float fixedSecondsPerDay;

        float halfDaySeconds;

        float inverseHalfDaySeconds;

        bool initialized;

        Vector3 sunDirection;

        Vector3 moonDirection;

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

        public TimeColorCollection AmbientLightColors { get; private set; }

        public bool InitialFogEnabled { get; set; }

        // FarPlaneDistance に対する割合
        public float InitialFogStartScale
        {
            get { return initialFogStartScale; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");

                initialFogStartScale = value;
            }
        }

        // FarPlaneDistance に対する割合
        public float InitialFogEndScale
        {
            get { return initialFogEndScale; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");

                initialFogEndScale = value;
            }
        }

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
                if (value < 0) throw new ArgumentOutOfRangeException("value");

                fixedSecondsPerDay = value;
            }
        }

        public float ElapsedSecondsPerDay { get; private set; }

        public Vector3 SunDirection
        {
            get { return sunDirection; }
        }

        public Vector3 MoonDirection
        {
            get { return moonDirection; }
        }

        public bool SunAboveHorizon
        {
            get { return 0 <= sunDirection.Y; }
        }

        public bool MoonAboveHorizon
        {
            get { return 0 <= moonDirection.Y; }
        }

        public Vector3 CurrentSkyColor { get; private set; }

        public Vector3 CurrentAmbientLightColor { get; private set; }

        public bool FogEnabled { get; set; }

        public float FogStartScale { get; set; }

        public float FogEndScale { get; set; }

        public SceneSettings()
        {
            Sunlight = new DirectionalLight("Sun");
            Sunlight.Direction = -DefaultMidnightSunDirection;

            Moonlight = new DirectionalLight("Moon");
            Moonlight.Direction = -DefaultMidnightMoonDirection;

            SkyColors = new TimeColorCollection();
            AmbientLightColors = new TimeColorCollection();
        }

        public void Initialize()
        {
            if (initialized) return;

            InitializeSunRotationAxis();
            InitializeMoonRotationAxis();

            FogEnabled = InitialFogEnabled;
            FogStartScale = initialFogStartScale;
            FogEndScale = initialFogEndScale;

            halfDaySeconds = secondsPerDay * 0.5f;
            inverseHalfDaySeconds = 1 / halfDaySeconds;

            initialized = true;
        }

        public void Update(GameTime gameTime)
        {
            //----------------------------------------------------------------
            // 0 時からの経過時間 (ゲーム内での一日の経過時間)

            if (!TimeStopped)
            {
                ElapsedSecondsPerDay = (float) gameTime.TotalGameTime.TotalSeconds % secondsPerDay;
            }
            else
            {
                ElapsedSecondsPerDay = fixedSecondsPerDay;
            }

            //----------------------------------------------------------------
            // 太陽と月

            UpdateSun();
            UpdateMoon();

            //----------------------------------------------------------------
            // 環境光

            UpdateAmbientLightColor();

            //----------------------------------------------------------------
            // 空の色

            UpdateSkyColor();
        }
        
        void UpdateSun()
        {
            // 0 時での太陽の位置を基点に、設定された軸の周囲で太陽を回転。

            var angle = (ElapsedSecondsPerDay / secondsPerDay) * MathHelper.TwoPi;
            Matrix transform;
            Matrix.CreateFromAxisAngle(ref sunRotationAxis, angle, out transform);
            Vector3.Transform(ref midnightSunDirection, ref transform, out sunDirection);
            sunDirection.Normalize();

            Sunlight.Direction = -sunDirection;
        }

        void UpdateMoon()
        {
            // 0 時での月の位置を基点に、設定された軸の周囲で月を回転。

            var angle = (ElapsedSecondsPerDay / secondsPerDay) * MathHelper.TwoPi;
            Matrix transform;
            Matrix.CreateFromAxisAngle(ref moonRotationAxis, angle, out transform);
            Vector3.Transform(ref midnightMoonDirection, ref transform, out moonDirection);
            moonDirection.Normalize();

            Moonlight.Direction = -moonDirection;
        }

        void UpdateAmbientLightColor()
        {
            // 一日の時間を [0, 1] へ変換。
            // 0 が 0 時、1 が 24 時。
            var elapsed = ElapsedSecondsPerDay / SecondsPerDay;

            CurrentAmbientLightColor = AmbientLightColors.GetColor(elapsed);
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
            // 0 時での太陽の位置から回転軸を算出。
            var right = Vector3.Cross(midnightSunDirection, Vector3.Up);
            sunRotationAxis = Vector3.Cross(right, midnightSunDirection);
        }

        void InitializeMoonRotationAxis()
        {
            // 0 時での月の位置から回転軸を算出。
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
