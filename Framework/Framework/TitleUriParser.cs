#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework
{
    public sealed class TitleUriParser : IUriParser
    {
        public static readonly TitleUriParser Instance = new TitleUriParser();

        const string prefix = "title:";

        TitleUriParser() { }

        // I/F
        public bool CanParse(string uri)
        {
            if (uri == null) throw new ArgumentNullException("uri");

            return uri.StartsWith(TitleUri.TitleScheme);
        }

        // I/F
        public IUri Parse(string uri)
        {
            if (uri == null) throw new ArgumentNullException("uri");
            if (uri.Length < prefix.Length) throw new ArgumentException("Invalid format: " + uri);

            var absolutePath = uri.Substring(prefix.Length);
            return new TitleUri { AbsoluteUri = uri, AbsolutePath = absolutePath };
        }
    }
}
