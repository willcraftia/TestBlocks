#region Using

using System;
using Microsoft.Xna.Framework;
using Willcraftia.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Blocks.Content.Demo
{
    public sealed class FreeView : ViewBase
    {
        Vector3 position = Vector3.Zero;

        Vector3 forward = Matrix.Identity.Forward;

        Vector3 up = Matrix.Identity.Up;

        public Vector3 Position
        {
            get { return position; }
            set
            {
                if (position == value) return;

                position = value;
                MatrixDirty = true;
            }
        }

        public Vector3 Forward
        {
            get { return forward; }
            set
            {
                if (forward == value) return;

                forward = value;
                MatrixDirty = true;
            }
        }

        public Vector3 Up
        {
            get { return up; }
            set
            {
                if (up == value) return;

                up = value;
                MatrixDirty = true;
            }
        }

        protected override void UpdateOverride()
        {
            var target = position + forward;
            Matrix.CreateLookAt(ref position, ref target, ref up, out Matrix);
        }
    }
}
