#region Using

using System;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Assets;
using Willcraftia.Xna.Framework.IO;

#endregion

namespace Willcraftia.Xna.Blocks.Assets
{
    public sealed class Texture2DLoader : IAssetLoader
    {
        GraphicsDevice graphicsDevice;

        public Texture2DLoader(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");

            this.graphicsDevice = graphicsDevice;
        }

        // I/F
        public object Load(IResource resource)
        {
            using (var stream = resource.Open())
            {
                var result = Texture2D.FromStream(graphicsDevice, stream);
                result.Name = resource.AbsoluteUri;
                return result;
            }
        }

        // I/F
        public void Save(IResource resource, object asset)
        {
            var texture = asset as Texture2D;
            if (texture == null)
                throw new ArgumentException("Invalid asset type: " + asset.GetType());

            using (var stream = resource.Create())
            {
                // PNG only.
                texture.SaveAsPng(stream, texture.Width, texture.Height);
            }
        }
    }
}
