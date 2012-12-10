#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.IO
{
    public sealed class FileResourceLoader : ResourceLoader
    {
        public static readonly FileResourceLoader Instance = new FileResourceLoader();

        const string prefix = "file:///";

        FileResourceLoader() { }

        public override IResource LoadResource(string uri)
        {
            if (!uri.StartsWith(prefix)) return null;

            var absolutePath = uri.Substring(prefix.Length);
            return new FileResource { AbsoluteUri = uri, AbsolutePath = absolutePath };
        }
    }
}
