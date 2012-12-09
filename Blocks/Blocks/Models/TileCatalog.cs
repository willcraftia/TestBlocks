#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class TileCatalog
    {
        public const int TextureSize = 256;

        public const int TileSize = 16;

        public const int TileLineWidth = 16;

        Color[] colorBuffer;

        public IUri Uri { get; set; }

        public string Name { get; set; }

        public TileCollection Tiles { get; private set; }

        public Texture2D TileMap { get; private set; }

        public Texture2D DiffuseColorMap { get; private set; }

        public Texture2D EmissiveColorMap { get; private set; }

        public Texture2D SpecularColorMap { get; private set; }

        public TileCatalog(GraphicsDevice graphicsDevice, int capacity)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");

            Tiles = new TileCollection(capacity);
            TileMap = new Texture2D(graphicsDevice, TextureSize, TextureSize, false, SurfaceFormat.Color);
            DiffuseColorMap = new Texture2D(graphicsDevice, TextureSize, TextureSize, false, SurfaceFormat.Color);
            EmissiveColorMap = new Texture2D(graphicsDevice, TextureSize, TextureSize, false, SurfaceFormat.Color);
            SpecularColorMap = new Texture2D(graphicsDevice, TextureSize, TextureSize, false, SurfaceFormat.Color);
        }

        //
        // TODO
        //
        // DrawMaps と ClearMaps は、後で効率を考えて修正すること。
        //

        public void DrawMaps()
        {
            foreach (var tile in Tiles)
                DrawMaps(tile.Index);
        }

        public void DrawMaps(byte index)
        {
            var tile = Tiles[index];

            //------------------------
            // Tile Texture

            if (colorBuffer == null)
                colorBuffer = new Color[TileSize * TileSize];
            tile.Texture.GetData<Color>(colorBuffer);

            //------------------------
            // TileMap

            var bounds = new Rectangle
            {
                X = index % TileLineWidth,
                Y = index / TileLineWidth,
                Width = TileSize,
                Height = TileSize
            };

            TileMap.SetData<Color>(0, bounds, colorBuffer, 0, colorBuffer.Length);

            //------------------------
            // DiffuseColorMap

            var diffuseColor = tile.DiffuseColor;
            for (int i = 0; i < colorBuffer.Length; i++)
                colorBuffer[i] = diffuseColor;
            DiffuseColorMap.SetData<Color>(0, bounds, colorBuffer, 0, colorBuffer.Length);

            //------------------------
            // EmissiveColorMap

            var emissiveColor = tile.EmissiveColor;
            for (int i = 0; i < colorBuffer.Length; i++)
                colorBuffer[i] = emissiveColor;
            EmissiveColorMap.SetData<Color>(0, bounds, colorBuffer, 0, colorBuffer.Length);

            //------------------------
            // SpecularColorMap

            var specularColor = tile.SpecularColor;
            specularColor.A = (byte) (tile.SpecularPower * 255);
            for (int i = 0; i < colorBuffer.Length; i++)
                colorBuffer[i] = specularColor;
            SpecularColorMap.SetData<Color>(0, bounds, colorBuffer, 0, colorBuffer.Length);
        }

        public void ClearMaps()
        {
            for (byte i = 0; i < byte.MaxValue; i++)
                ClearMaps(i);
        }

        public void ClearMaps(byte index)
        {
            if (colorBuffer == null)
                colorBuffer = new Color[TileSize * TileSize];

            for (int i = 0; i < colorBuffer.Length; i++)
                colorBuffer[i] = Color.Black;

            //------------------------
            // TileMap

            var bounds = new Rectangle
            {
                X = index % TileLineWidth,
                Y = index / TileLineWidth,
                Width = TileSize,
                Height = TileSize
            };

            TileMap.SetData<Color>(0, bounds, colorBuffer, 0, colorBuffer.Length);

            //------------------------
            // DiffuseColorMap

            DiffuseColorMap.SetData<Color>(0, bounds, colorBuffer, 0, colorBuffer.Length);

            //------------------------
            // EmissiveColorMap

            EmissiveColorMap.SetData<Color>(0, bounds, colorBuffer, 0, colorBuffer.Length);

            //------------------------
            // SpecularColorMap

            SpecularColorMap.SetData<Color>(0, bounds, colorBuffer, 0, colorBuffer.Length);
        }
    }
}
