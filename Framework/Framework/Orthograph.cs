#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework
{
    public sealed class Orthograph : ProjectionBase
    {
        float left;

        float right;

        float bottom;

        float top;

        float zNearPlane;

        float zFarPlane;

        public float Left
        {
            get { return left; }
            set
            {
                if (left == value) return;

                left = value;
                MatrixDirty = true;
            }
        }

        public float Right
        {
            get { return right; }
            set
            {
                if (right == value) return;

                right = value;
                MatrixDirty = true;
            }
        }

        public float Bottom
        {
            get { return bottom; }
            set
            {
                if (bottom == value) return;

                bottom = value;
                MatrixDirty = true;
            }
        }

        public float Top
        {
            get { return left; }
            set
            {
                if (top == value) return;

                top = value;
                MatrixDirty = true;
            }
        }

        public float ZNearPlane
        {
            get { return zNearPlane; }
            set
            {
                if (zNearPlane == value) return;

                zNearPlane = value;
                MatrixDirty = true;
            }
        }

        public float ZFarPlane
        {
            get { return zFarPlane; }
            set
            {
                if (zFarPlane == value) return;

                zFarPlane = value;
                MatrixDirty = true;
            }
        }

        protected override void UpdateOverride()
        {
            Matrix.CreateOrthographicOffCenter(Left, Right, Bottom, Top, ZNearPlane, ZFarPlane, out Matrix);
        }
    }
}
