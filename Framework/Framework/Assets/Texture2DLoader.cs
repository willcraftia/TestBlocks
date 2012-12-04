#region Using

using System;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.IO;

#endregion

namespace Willcraftia.Xna.Framework.Assets
{
    public sealed class Texture2DLoader : IAssetLoader
    {
        GraphicsDevice graphicsDevice;

        public Texture2DLoader(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");

            this.graphicsDevice = graphicsDevice;
        }

        public object Load(AssetManager assetManager, Uri uri)
        {
            using (var stream = ResourceContainerManager.Instance.OpenResource(uri))
            {
                var result = Texture2D.FromStream(graphicsDevice, stream);
                result.Name = uri.OriginalString;
                return result;
            }
        }

        public void Unload(AssetManager assetManager, Uri uri, object asset)
        {
            var texture = asset as Texture2D;
            if (texture == null)
                throw new ArgumentException(string.Format("The unexpected asset is specified: {0}", asset.GetType()));

            texture.Dispose();
        }

        public void Save(AssetManager assetManager, Uri uri, object asset)
        {
            var texture = asset as Texture2D;
            if (texture == null)
                throw new ArgumentException(string.Format("The unexpected asset is specified: {0}", asset.GetType()));

            using (var stream = ResourceContainerManager.Instance.CreateResource(uri))
            {
                // PNG format only.
                texture.SaveAsPng(stream, texture.Width, texture.Height);
            }
        }
    }
}
