#region Using

using System;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.IO;

#endregion

namespace Willcraftia.Xna.Blocks.Assets
{
    public sealed class Texture2DLoader : AssetLoaderBase
    {
        GraphicsDevice graphicsDevice;

        public Texture2DLoader(ResourceManager resourceManager, GraphicsDevice graphicsDevice)
            : base(resourceManager)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");

            this.graphicsDevice = graphicsDevice;
        }

        public override object Load(IResource resource)
        {
            using (var stream = resource.Open())
            {
                var result = Texture2D.FromStream(graphicsDevice, stream);
                result.Name = resource.AbsoluteUri;
                return result;
            }
        }

        public override void Unload(IResource resource, object asset)
        {
            var texture = asset as Texture2D;
            if (texture == null)
                throw new ArgumentException("Invalid asset type: " + asset.GetType());

            texture.Dispose();
        }

        public override void Save(IResource resource, object asset)
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
