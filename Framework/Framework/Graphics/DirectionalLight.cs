#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class DirectionalLight
    {
        Vector3 direction = Vector3.Up;

        Vector3 diffuseColor = Vector3.One;

        Vector3 specularColor = Vector3.One;

        Vector3 shadowColor = Vector3.Zero;

        bool enabled = true;

        public string Name { get; private set; }

        public Vector3 Direction
        {
            get { return direction; }
            set
            {
                if (value.LengthSquared() == 0) throw new ArgumentException("Invaid vector.");

                direction = value;
                direction.Normalize();
            }
        }

        public Vector3 DiffuseColor
        {
            get { return diffuseColor; }
            set
            {
                if (value.X < 0 || 1 < value.X ||
                    value.Y < 0 || 1 < value.Y ||
                    value.Z < 0 || 1 < value.Z)
                    throw new ArgumentException("Invalid vector.");

                diffuseColor = value;
            }
        }

        public Vector3 SpecularColor
        {
            get { return specularColor; }
            set
            {
                if (value.X < 0 || 1 < value.X ||
                    value.Y < 0 || 1 < value.Y ||
                    value.Z < 0 || 1 < value.Z)
                    throw new ArgumentException("Invalid vector.");

                specularColor = value;
            }
        }

        public Vector3 ShadowColor
        {
            get { return shadowColor; }
            set
            {
                if (value.X < 0 || 1 < value.X ||
                    value.Y < 0 || 1 < value.Y ||
                    value.Z < 0 || 1 < value.Z)
                    throw new ArgumentException("Invalid vector.");

                shadowColor = value;
            }
        }

        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        public DirectionalLight(string name)
        {
            if (name == null) throw new ArgumentNullException("name");

            Name = name;
        }
    }
}
