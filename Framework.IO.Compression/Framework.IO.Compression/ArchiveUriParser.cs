#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.IO.Compression
{
    public sealed class ArchiveUriParser : IUriParser
    {
        public static readonly ArchiveUriParser Instance = new ArchiveUriParser();

        const string prefix = "title:";

        ArchiveUriParser() { }

        // I/F
        public bool CanParse(string uri)
        {
            if (uri == null) throw new ArgumentNullException("uri");

            return uri.StartsWith(ArchiveUri.ArchiveScheme);
        }

        // I/F
        public IUri Parse(string uri)
        {
            if (uri == null) throw new ArgumentNullException("uri");
            if (uri.Length < prefix.Length) throw new ArgumentException("Invalid format: " + uri);

            var absolutePath = uri.Substring(prefix.Length);

            var elements = absolutePath.Split('!');
            if (elements.Length != 2) throw new ArgumentException("Invalid format: " + uri);

            var zipUriString = elements[0];
            var zipUriParser = UriParserRegistory.Instance.GetUriParser(zipUriString);
            var zipUri = zipUriParser.Parse(zipUriString);

            var entryPath = elements[1];

            return new ArchiveUri
            {
                AbsoluteUri = uri,
                AbsolutePath = absolutePath,
                ZipUri = zipUri,
                EntryPath = entryPath
            };
        }
    }
}
