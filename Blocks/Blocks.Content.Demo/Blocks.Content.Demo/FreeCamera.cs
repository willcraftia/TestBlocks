#region Using

using System;
using Microsoft.Xna.Framework;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Blocks.Content.Demo
{
    public sealed class FreeCamera : ICamera
    {
        Vector3 position;

        Vector3 forward;

        Vector3 up;

        public string Name { get; private set; }

        public View View { get; private set; }

        public PerspectiveFov Projection { get; private set; }

        public BoundingFrustum Frustum { get; private set; }

        public FreeView FreeView { get; private set; }

        public Vector3 Position
        {
            get { return position; }
        }

        public Vector3 Forward
        {
            get { return forward; }
        }

        public Vector3 Up
        {
            get { return up; }
        }

        public FreeCamera(string name)
        {
            if (name == null) throw new ArgumentNullException("name");

            Name = name;
            
            FreeView = new FreeView();
            View = FreeView;
            Projection = new PerspectiveFov();
            Frustum = new BoundingFrustum(Matrix.Identity);
        }

        public void Update()
        {
            View.Update();
            Projection.Update();

            Matrix viewProjection;
            Matrix.Multiply(ref View.Matrix, ref Projection.Matrix, out viewProjection);
            Frustum.Matrix = viewProjection;

            position = FreeView.Position;
            forward = FreeView.Forward;
            up = FreeView.Up;
        }
    }
}
