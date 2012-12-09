#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class Tile
    {
        Vector3 diffuseColor;

        Vector3 emissiveColor;

        Vector3 specularColor;

        public byte Index { get; set; }

        public IUri Uri { get; set; }

        public string Name { get; set; }

        public Texture2D Texture { get; set; }

        public string TextureUri { get; set; }

        public bool Translucent { get; set; }

        public Color DiffuseColor
        {
            get { return new Color(diffuseColor); }
            set { diffuseColor = value.ToVector3(); }
        }

        public Color EmissiveColor
        {
            get { return new Color(emissiveColor); }
            set { emissiveColor = value.ToVector3(); }
        }

        public Color SpecularColor
        {
            get { return new Color(specularColor); }
            set { specularColor = value.ToVector3(); }
        }

        public byte SpecularPower { get; set; }
    }
}
