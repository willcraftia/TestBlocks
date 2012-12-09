#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework
{
    public sealed class StorageUriParser : IUriParser
    {
        public static readonly StorageUriParser Instance = new StorageUriParser();

        const string prefix = "storage:";

        StorageUriParser() { }

        // I/F
        public bool CanParse(string uri)
        {
            if (uri == null) throw new ArgumentNullException("uri");

            return uri.StartsWith(StorageUri.StorageScheme);
        }

        // I/F
        public IUri Parse(string uri)
        {
            if (uri == null) throw new ArgumentNullException("uri");
            if (uri.Length < prefix.Length) throw new ArgumentException("Invalid format: " + uri);

            var absolutePath = uri.Substring(prefix.Length);
            return new StorageUri { AbsoluteUri = uri, AbsolutePath = absolutePath };
        }
    }
}
