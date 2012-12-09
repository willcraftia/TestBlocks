#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework
{
    public sealed class FileUriParser : IUriParser
    {
        public static readonly FileUriParser Instance = new FileUriParser();

        const string prefix = "file:///";

        FileUriParser() { }

        // I/F
        public bool CanParse(string uri)
        {
            if (uri == null) throw new ArgumentNullException("uri");

            return uri.StartsWith(FileUri.FileScheme);
        }

        // I/F
        public IUri Parse(string uri)
        {
            if (uri == null) throw new ArgumentNullException("uri");
            if (uri.Length < prefix.Length) throw new ArgumentException("Invalid format: " + uri);

            var absolutePath = uri.Substring(prefix.Length);
            return new FileUri { AbsoluteUri = uri, AbsolutePath = absolutePath };
        }
    }
}
