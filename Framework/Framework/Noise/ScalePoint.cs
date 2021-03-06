﻿#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Noise
{
    public sealed class ScalePoint : INoiseSource
    {
        float scaleX = 1;

        float scaleY = 1;

        float scaleZ = 1;

        public string Name { get; set; }

        public INoiseSource Source { get; set; }

        public float ScaleX
        {
            get { return scaleX; }
            set { scaleX = value; }
        }

        public float ScaleY
        {
            get { return scaleY; }
            set { scaleY = value; }
        }

        public float ScaleZ
        {
            get { return scaleZ; }
            set { scaleZ = value; }
        }

        // I/F
        public float Sample(float x, float y, float z)
        {
            return Source.Sample(x * scaleX, y * scaleY, z * scaleZ);
        }

        #region ToString

        public override string ToString()
        {
            return "[Name:" + (Name ?? string.Empty) + "]";
        }

        #endregion
    }
}
