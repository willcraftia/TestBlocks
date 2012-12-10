#region Using

using System;
using System.Collections.ObjectModel;

#endregion

namespace Willcraftia.Xna.Framework
{
    public sealed class UriParserRegistory : Collection<IUriParser>
    {
        public static readonly UriParserRegistory Instance = new UriParserRegistory();

        UriParserRegistory()
        {
            Add(ContentUriParser.Instance);
            Add(TitleUriParser.Instance);
            Add(StorageUriParser.Instance);
#if WINDOWS
            Add(FileUriParser.Instance);
#endif
        }

        public IUriParser GetUriParser(string uri)
        {
            foreach (var parser in this)
            {
                if (parser.CanParse(uri)) return parser;
            }

            throw new InvalidOperationException("Parser not found: " + uri);
        }
    }
}
