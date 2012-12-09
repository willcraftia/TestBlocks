#region Using

using System;
using System.Collections.Generic;

#endregion

namespace Willcraftia.Xna.Framework
{
    public sealed class UriManager
    {
        Dictionary<string, IUri> cache = new Dictionary<string, IUri>();

        public IUri Create(string uriString)
        {
            if (uriString == null) throw new ArgumentNullException("uriString");

            var parser = UriParserRegistory.Instance.GetUriParser(uriString);
            var uri = parser.Parse(uriString);
            cache[uri.AbsoluteUri] = uri;
            return uri;
        }

        public IUri Create(string baseUri, string relativeUri)
        {
            if (baseUri == null) throw new ArgumentNullException("baseUri");
            if (relativeUri == null) throw new ArgumentNullException("relativeUri");

            if (0 < relativeUri.IndexOf(':'))
                return Create(relativeUri);

            return Create(baseUri + relativeUri);
        }

        public string CreateRelativeUri(string baseUri, IUri uri)
        {
            if (baseUri == null) throw new ArgumentNullException("baseUri");
            if (uri == null) throw new ArgumentNullException("uri");

            if (uri.AbsoluteUri.StartsWith(baseUri))
                return uri.AbsoluteUri.Substring(baseUri.Length);

            return uri.AbsoluteUri;
        }

        public void ClearCache()
        {
            cache.Clear();
        }
    }
}
