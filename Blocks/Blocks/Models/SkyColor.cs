﻿#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class SkyColor
    {
        public float Time { get; set; }

        public Vector3 Color { get; set; }

        public SkyColor(float time, Vector3 color)
        {
            Time = time;
            Color = color;
        }

        #region ToString

        public override string ToString()
        {
            return "[Time: " + Time + ", Color: " + Color + "]";
        }

        #endregion
    }
}
