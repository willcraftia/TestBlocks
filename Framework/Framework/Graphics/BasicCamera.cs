#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class BasicCamera : ICamera
    {
        /// <summary>
        /// デフォルトの焦点距離。
        /// </summary>
        public const float DefaultFocusDistance = 3.0f;

        /// <summary>
        /// デフォルトの焦点範囲。
        /// </summary>
        public const float DefaultFocusRange = 100.0f;

        public string Name { get; private set; }

        public View View { get; private set; }

        public PerspectiveFov Projection { get; private set; }

        public BoundingFrustum Frustum { get; private set; }

        public float FocusDistance { get; set; }

        public float FocusRange { get; set; }

        public BasicCamera(string name)
        {
            if (name == null) throw new ArgumentNullException("name");

            Name = name;
            
            View = new View();
            Projection = new PerspectiveFov();
            Frustum = new BoundingFrustum(Matrix.Identity);

            FocusDistance = DefaultFocusDistance;
            FocusRange = DefaultFocusRange;
        }

        public void Update()
        {
            View.Update();
            Projection.Update();

            Matrix viewProjection;
            Matrix.Multiply(ref View.Matrix, ref Projection.Matrix, out viewProjection);
            Frustum.Matrix = viewProjection;
        }
    }
}
