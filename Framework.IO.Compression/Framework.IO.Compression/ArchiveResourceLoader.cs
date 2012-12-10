#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.IO.Compression
{
    public sealed class ArchiveResourceLoader : ResourceLoader
    {
        public static readonly ArchiveResourceLoader Instance = new ArchiveResourceLoader();

        const string prefix = "title:";

        ArchiveResourceLoader() { }

        public override IResource LoadResource(string uri)
        {
            if (!uri.StartsWith(prefix)) return null;

            var absolutePath = uri.Substring(prefix.Length);

            var elements = absolutePath.Split('!');
            if (elements.Length != 2) throw new ArgumentException("Invalid format: " + uri);

            return new ArchiveResource
            {
                AbsoluteUri = uri,
                AbsolutePath = absolutePath,
                ZipResource = ResourceLoader.Load(elements[0]),
                EntryPath = elements[1]
            };
        }
    }
}
