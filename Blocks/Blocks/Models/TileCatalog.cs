﻿#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Collections;
using Willcraftia.Xna.Framework.Content;
using Willcraftia.Xna.Framework.IO;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class TileCatalog : KeyedList<byte, Tile>, IAsset
    {
        public const int TextureSize = 256;

        public const int TileSize = 16;

        public const int TileLineWidth = 16;

        const float inverseTextureSize = 1 / 256f;

        const float inverseTileSize = 1 / 16f;

        const float inverseTileLineWidth = 1 / 16f;
        
        Color[] colorBuffer;

        // I/F
        public IResource Resource { get; set; }

        public string Name { get; set; }

        public Texture2D TileMap { get; private set; }

        public Texture2D DiffuseColorMap { get; private set; }

        public Texture2D EmissiveColorMap { get; private set; }

        public Texture2D SpecularColorMap { get; private set; }

        public TileCatalog(GraphicsDevice graphicsDevice, int capacity)
            : base(capacity)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");

            TileMap = CreateMap(graphicsDevice);
            DiffuseColorMap = CreateMap(graphicsDevice);
            EmissiveColorMap = CreateMap(graphicsDevice);
            SpecularColorMap = CreateMap(graphicsDevice);
        }

        //
        // TODO
        //
        // DrawMaps と ClearMaps は、後で効率を考えて修正すること。
        //

        public void DrawMaps()
        {
            foreach (var tile in this)
                DrawMaps(tile.Index);
        }

        public void DrawMaps(byte index)
        {
            var tile = this[index];

            Rectangle bounds;
            CalculateTileBounds(index, out bounds);

            EnsureColorBuffer();

            //------------------------
            // TileMap

            GetColorBuffer(tile.Texture);
            SetColorBuffer(TileMap, ref bounds);

            //------------------------
            // DiffuseColorMap

            var diffuseColor = tile.DiffuseColor;
            SetColorBuffer(DiffuseColorMap, ref bounds, ref diffuseColor);

            //------------------------
            // EmissiveColorMap

            var emissiveColor = tile.EmissiveColor;
            SetColorBuffer(EmissiveColorMap, ref bounds, ref emissiveColor);

            //------------------------
            // SpecularColorMap

            var specularColor = tile.SpecularColor;
            specularColor.A = (byte) (tile.SpecularPower * 255);
            SetColorBuffer(SpecularColorMap, ref bounds, ref specularColor);
        }

        public void ClearMaps()
        {
            for (byte i = 0; i < byte.MaxValue; i++)
                ClearMaps(i);
        }

        public void ClearMaps(byte index)
        {
            EnsureColorBuffer();
            ClearColorBuffer();

            Rectangle bounds;
            CalculateTileBounds(index, out bounds);

            SetColorBuffer(TileMap, ref bounds);
            SetColorBuffer(DiffuseColorMap, ref bounds);
            SetColorBuffer(EmissiveColorMap, ref bounds);
            SetColorBuffer(SpecularColorMap, ref bounds);
        }

        public void GetTexCoordOffset(byte index, out Vector2 offset)
        {
            offset = new Vector2
            {
                X = (index % TileLineWidth) * inverseTileLineWidth,
                Y = (index / TileLineWidth) * inverseTileLineWidth
            };
        }

        protected override byte GetKeyForItem(Tile item)
        {
            return item.Index;
        }

        protected override void SetOverride(int index, Tile item)
        {
            item.Catalog = this;

            base.SetOverride(index, item);
        }

        protected override void InsertOverride(int index, Tile item)
        {
            item.Catalog = this;

            base.InsertOverride(index, item);
        }

        protected override void RemoveAtOverride(int index)
        {
            this[index].Catalog = null;

            base.RemoveAtOverride(index);
        }

        Texture2D CreateMap(GraphicsDevice graphicsDevice)
        {
            return new Texture2D(graphicsDevice, TextureSize, TextureSize, false, SurfaceFormat.Color);
        }

        void CalculateTileBounds(byte index, out Rectangle bounds)
        {
            bounds = new Rectangle
            {
                X = (index % TileLineWidth) * TileSize,
                Y = (index / TileLineWidth) * TileSize,
                Width = TileSize,
                Height = TileSize
            };
        }

        void GetColorBuffer(Image2D image)
        {
            if (image == null || image.Texture == null) return;

            image.Texture.GetData(colorBuffer);
        }

        void SetColorBuffer(Texture2D texture, ref Rectangle bounds)
        {
            texture.SetData(0, bounds, colorBuffer, 0, colorBuffer.Length);
        }

        void SetColorBuffer(Texture2D texture, ref Rectangle bounds, ref Color color)
        {
            for (int i = 0; i < colorBuffer.Length; i++) colorBuffer[i] = color;
            SetColorBuffer(texture, ref bounds);
        }

        void EnsureColorBuffer()
        {
            if (colorBuffer == null) colorBuffer = new Color[TileSize * TileSize];
        }

        void ClearColorBuffer()
        {
            for (int i = 0; i < colorBuffer.Length; i++) colorBuffer[i] = Color.Black;
        }

        #region ToString

        public override string ToString()
        {
            return "[Uri:" + ((Resource != null) ? Resource.AbsoluteUri : string.Empty) + "]";
        }

        #endregion
    }
}
