#region Using

using System;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Content;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Blocks.Models;

#endregion

namespace Willcraftia.Xna.Blocks.Content
{
    public sealed class Image2DLoader : IAssetLoader
    {
        GraphicsDevice graphicsDevice;

        public Image2DLoader(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");

            this.graphicsDevice = graphicsDevice;
        }

        // I/F
        public object Load(IResource resource)
        {
            return new Image2D
            {
                Texture = LoadTexture(resource)
            };
        }

        // I/F
        public void Save(IResource resource, object asset)
        {
            var image = asset as Image2D;

            SaveTexture(resource, image.Texture);
        }

        Texture2D LoadTexture(IResource resource)
        {
            using (var stream = resource.Open())
            {
                var result = Texture2D.FromStream(graphicsDevice, stream);
                result.Name = resource.AbsoluteUri;
                return result;
            }
        }

        void SaveTexture(IResource resource, Texture2D texture)
        {
            if (texture == null) return;

            using (var stream = resource.Create())
            {
                // PNG only.
                texture.SaveAsPng(stream, texture.Width, texture.Height);
                texture.Name = resource.AbsoluteUri;
            }
        }
    }
}
