#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Willcraftia.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Blocks.Content.Demo
{
    public sealed class FreeViewInput
    {
        const float twoPi = 2 * MathHelper.Pi;

        Vector3 position = Vector3.Zero;

        float yaw;

        float pitch;

        float roll;

        Matrix invertView;

        public int InitialMousePositionX { get; set; }

        public int InitialMousePositionY { get; set; }

        public float RotationVelocity { get; set; }

        public float MoveVelocity { get; set; }

        public float DashFactor { get; set; }

        public View View { get; set; }

        public FreeViewInput()
        {
            RotationVelocity = 0.3f;
            MoveVelocity = 30;
            DashFactor = 2;
        }

        public void Initialize(int initialMousePositionX, int initialMousePositionY)
        {
            InitialMousePositionX = initialMousePositionX;
            InitialMousePositionY = initialMousePositionY;
            ResetMousePosition();
        }

        public void Update(GameTime gameTime)
        {
            position = View.Position;
            Matrix.Invert(ref View.Matrix, out invertView);

            var deltaTime = (float) gameTime.ElapsedGameTime.TotalSeconds;

            var mouseState = Mouse.GetState();
            if (InitialMousePositionX != mouseState.X ||
                InitialMousePositionY != mouseState.Y)
            {
                // yaw
                var yawAmount = -(mouseState.X - InitialMousePositionX);
                Yaw(yawAmount * RotationVelocity * deltaTime);
                
                // pitch
                var pitchAmount = -(mouseState.Y - InitialMousePositionY);
                Pitch(pitchAmount * RotationVelocity * deltaTime);

                ResetMousePosition();
            }

            var moveDirection = Vector3.Zero;
            var keyboardState = Keyboard.GetState();
            var distance = MoveVelocity * deltaTime;

            // accelerate.
            if (keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift))
                distance *= DashFactor;

            if (keyboardState.IsKeyDown(Keys.W)) Move(distance);
            if (keyboardState.IsKeyDown(Keys.S)) Move(-distance);
            if (keyboardState.IsKeyDown(Keys.A)) Strafe(distance);
            if (keyboardState.IsKeyDown(Keys.D)) Strafe(-distance);
            if (keyboardState.IsKeyDown(Keys.Q)) MoveUp(distance);
            if (keyboardState.IsKeyDown(Keys.Z)) MoveUp(-distance);

            Matrix rotation;
            Matrix.CreateFromYawPitchRoll(yaw, pitch, roll, out rotation);

            View.Position = position;
            View.Direction = rotation.Forward;
            View.Up = rotation.Up;
        }

        public void Move(float distance)
        {
            if (distance == 0) return;

            var direction = invertView.Forward;
            position += direction * distance;
        }

        public void Strafe(float distance)
        {
            if (distance == 0) return;

            var direction = invertView.Left;
            position += direction * distance;
        }

        public void MoveUp(float distance)
        {
            if (distance == 0) return;

            var direction = invertView.Up;
            position += direction * distance;
        }

        public void Yaw(float amount)
        {
            if (amount == 0) return;

            yaw += amount;
            yaw %= MathHelper.TwoPi;
        }

        public void Pitch(float amount)
        {
            if (amount == 0) return;

            pitch += amount;
            pitch %= MathHelper.TwoPi;
        }

        public void Roll(float amount)
        {
            if (amount == 0) return;

            roll += amount;
            roll %= MathHelper.TwoPi;
        }

        void ResetMousePosition()
        {
            Mouse.SetPosition(InitialMousePositionX, InitialMousePositionY);
        }
    }
}
