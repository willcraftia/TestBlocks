#region Using

using System;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Blocks.Assets
{
    public sealed class Texture2DLoader : AssetLoaderBase
    {
        GraphicsDevice graphicsDevice;

        public Texture2DLoader(UriManager uriManager, GraphicsDevice graphicsDevice)
            : base(uriManager)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");

            this.graphicsDevice = graphicsDevice;
        }

        public override object Load(IUri uri)
        {
            using (var stream = uri.Open())
            {
                var result = Texture2D.FromStream(graphicsDevice, stream);
                result.Name = uri.AbsoluteUri;
                return result;
            }
        }

        public override void Unload(IUri uri, object asset)
        {
            var texture = asset as Texture2D;
            if (texture == null)
                throw new ArgumentException("Invalid asset type: " + asset.GetType());

            texture.Dispose();
        }

        public override void Save(IUri uri, object asset)
        {
            var texture = asset as Texture2D;
            if (texture == null)
                throw new ArgumentException("Invalid asset type: " + asset.GetType());

            using (var stream = uri.Create())
            {
                // PNG only.
                texture.SaveAsPng(stream, texture.Width, texture.Height);
            }
        }
    }
}
