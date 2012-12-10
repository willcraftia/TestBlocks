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

            resource = Load(baseResource.BaseUri + relativeUri);
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
    }
}
