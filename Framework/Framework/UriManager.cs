#region Using

using System;
using System.Collections.Generic;

#endregion

namespace Willcraftia.Xna.Framework
{
    public sealed class UriManager
    {
        #region RelativeUriKey

        struct RelativeUriKey : IEquatable<RelativeUriKey>
        {
            public string BaseUri;

            public string RelativeUri;

            #region Equatable

            public static bool operator ==(RelativeUriKey p1, RelativeUriKey p2)
            {
                return p1.Equals(p2);
            }

            public static bool operator !=(RelativeUriKey p1, RelativeUriKey p2)
            {
                return !p1.Equals(p2);
            }

            // I/F
            public bool Equals(RelativeUriKey other)
            {
                return BaseUri == other.BaseUri && RelativeUri == other.RelativeUri;
            }

            public override bool Equals(object obj)
            {
                if (obj == null || GetType() != obj.GetType()) return false;

                return Equals((RelativeUriKey) obj);
            }

            public override int GetHashCode()
            {
                return BaseUri.GetHashCode() ^ RelativeUri.GetHashCode();
            }

            #endregion

            #region ToString

            public override string ToString()
            {
                return "[" + BaseUri + ", " + RelativeUri + "]";
            }

            #endregion
        }

        #endregion

        Dictionary<string, IUri> cache = new Dictionary<string, IUri>();

        Dictionary<RelativeUriKey, IUri> relativeUriCache = new Dictionary<RelativeUriKey, IUri>();

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

            IUri uri;
            if (cache.TryGetValue(relativeUri, out uri))
                return uri;

            if (0 < relativeUri.IndexOf(':'))
                return Create(relativeUri);

            var relativeUriKey = new RelativeUriKey
            {
                BaseUri = baseUri,
                RelativeUri = relativeUri
            };

            if (relativeUriCache.TryGetValue(relativeUriKey, out uri))
                return uri;

            uri = Create(baseUri + relativeUri);
            relativeUriCache[relativeUriKey] = uri;
            return uri;
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
