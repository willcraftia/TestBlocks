#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class LightView : ViewBase
    {
        Vector3 direction;

        Vector3 position = Vector3.Zero;

        Vector3 up = Vector3.Up;

        public Vector3 Direction
        {
            get { return direction; }
            set
            {
                if (direction == value) return;

                direction = value;
                MatrixDirty = true;
            }
        }

        protected override void UpdateOverride()
        {
            Matrix.CreateLookAt(ref position, ref direction, ref up, out Matrix);
        }
    }
}
