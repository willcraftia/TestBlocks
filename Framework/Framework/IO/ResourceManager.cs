#region Using

using System;
using System.Collections.Generic;

#endregion

namespace Willcraftia.Xna.Framework.IO
{
    public sealed class ResourceManager
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

        static readonly char[] delimiter = { '/', ':' };

        Dictionary<string, IResource> cache = new Dictionary<string, IResource>();

        Dictionary<RelativeUriKey, IResource> relativeUriCache = new Dictionary<RelativeUriKey, IResource>();

        public IResource Load(string uri)
        {
            if (uri == null) throw new ArgumentNullException("uri");

            IResource resource;
            if (cache.TryGetValue(uri, out resource))
                return resource;

            resource = ResourceLoader.Load(uri);
            cache[resource.AbsoluteUri] = resource;
            return resource;
        }

        public IResource Load(IResource baseResource, string relativeUri)
        {
            if (baseResource == null) throw new ArgumentNullException("baseResource");
            if (relativeUri == null) throw new ArgumentNullException("relativeUri");

            if (0 < relativeUri.IndexOf(':'))
                return Load(relativeUri);

            var relativeUriKey = new RelativeUriKey
            {
                BaseUri = baseResource.BaseUri,
                RelativeUri = relativeUri
            };

            IResource resource;
            if (relativeUriCache.TryGetValue(relativeUriKey, out resource))
                return resource;

            var uri = CombineUri(baseResource.BaseUri, "./" + relativeUri);
            resource = Load(uri);
            
            relativeUriCache[relativeUriKey] = resource;
            return resource;
        }

        public string CreateRelativeUri(IResource baseResource, IResource resource)
        {
            if (baseResource == null) throw new ArgumentNullException("baseResource");
            if (resource == null) throw new ArgumentNullException("resource");

            if (resource.AbsoluteUri.StartsWith(baseResource.BaseUri))
                return resource.AbsoluteUri.Substring(baseResource.BaseUri.Length);

            return resource.AbsoluteUri;
        }

        public void ClearCache()
        {
            cache.Clear();
            relativeUriCache.Clear();
        }

        string CombineUri(string baseUri, string relativeUri)
        {
            var baseLastIndex = baseUri.Length - 1;

            var relativeUriIndex = 0;
            if (relativeUri.StartsWith("./")) relativeUriIndex += 2;

            int relativeCursor = -1;
            while (0 <= (relativeCursor = relativeUri.IndexOf("../", relativeUriIndex)))
            {
                relativeUriIndex = relativeCursor + 3;

                baseLastIndex = baseUri.LastIndexOfAny(delimiter, baseLastIndex - 1);
                if (baseLastIndex < 0)
                    throw new ArgumentException(string.Format("Can not combine \"{0}\" and \"{1}\"", baseUri, relativeUri));
            }

            return baseUri.Substring(0, baseLastIndex + 1) + relativeUri.Substring(relativeUriIndex);
        }
    }
}
