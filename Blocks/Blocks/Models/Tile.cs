﻿#region Using

using System;
using Microsoft.Xna.Framework;
using Willcraftia.Xna.Framework.Content;
using Willcraftia.Xna.Framework.IO;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class Tile : IAsset
    {
        public const int Size = 16;

        public const byte EmptyIndex = 0;

        Vector3 diffuseColor;

        Vector3 emissiveColor;

        Vector3 specularColor;

        // I/F
        public IResource Resource { get; set; }

        public byte Index { get; set; }

        public TileCatalog Catalog { get; internal set; }

        public string Name { get; set; }

        public Image2D Texture { get; set; }

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

        public void GetTexCoordOffset(out Vector2 offset)
        {
            if (Catalog == null)
            {
                offset = Vector2.Zero;
            }
            else
            {
                Catalog.GetTexCoordOffset(Index, out offset);
            }
        }

        #region ToString

        public override string ToString()
        {
            return "[Uri:" + ((Resource != null) ? Resource.AbsoluteUri : string.Empty) + ", Index:" + Index + "]";
        }

        #endregion
    }
}
