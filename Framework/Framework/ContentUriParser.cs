#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework
{
    public sealed class ContentUriParser : IUriParser
    {
        public static readonly ContentUriParser Instance = new ContentUriParser();

        const string prefix = "content:";

        ContentUriParser() { }

        // I/F
        public bool CanParse(string uri)
        {
            if (uri == null) throw new ArgumentNullException("uri");

            return uri.StartsWith(ContentUri.ContentScheme);
        }

        // I/F
        public IUri Parse(string uri)
        {
            if (uri == null) throw new ArgumentNullException("uri");
            if (uri.Length < prefix.Length) throw new ArgumentException("Invalid format: " + uri);

            var absolutePath = uri.Substring(prefix.Length);
            return new ContentUri { AbsoluteUri = uri, AbsolutePath = absolutePath };
        }
    }
}
