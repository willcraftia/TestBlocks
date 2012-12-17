#region Using

using System;
using System.IO;

#if WINDOWS
using System.Net;
#endif

#endregion

namespace Willcraftia.Xna.Framework.IO
{
    public sealed class WebResource : IResource, IEquatable<WebResource>
    {
        public const string HttpScheme = "http";

        public const string HttpsScheme = "https";

        string extension;

        object extensionLock = new object();

        string baseUri;

        object baseUriLock = new object();

        // I/F
        public string AbsoluteUri { get; internal set; }

        // I/F
        public string Scheme { get; internal set; }

        // I/F
        public string AbsolutePath { get; internal set; }

        // I/F
        public string Extension
        {
            get
            {
                lock (extensionLock)
                {
                    if (extension == null)
                        extension = Path.GetExtension(AbsolutePath);
                    return extension;
                }
            }
        }

        // I/F
        public bool ReadOnly
        {
            get { return true; }
        }

        // I/F
        public string BaseUri
        {
            get
            {
                lock (baseUriLock)
                {
                    if (baseUri == null)
                    {
                        var lastSlash = AbsolutePath.LastIndexOf('/');
                        if (lastSlash < 0) lastSlash = 0;
                        var basePath = AbsolutePath.Substring(0, lastSlash + 1);
                        baseUri = Scheme + "://" + basePath;
                    }
                    return baseUri;
                }
            }
        }

        public StorageResource LocalResource { get; internal set; }

        internal WebResource() { }

        // I/F
        public Stream Open()
        {
#if WINDOWS
            var request = WebRequest.Create(AbsoluteUri);
            var response = request.GetResponse();

            return response.GetResponseStream();
#else
            throw new NotSupportedException();
#endif
        }

        // I/F
        public Stream Create()
        {
            throw new NotSupportedException();
        }

        // I/F
        public void Delete()
        {
            throw new NotSupportedException();
        }

        #region Equatable

        // I/F
        public bool Equals(WebResource other)
        {
            return AbsoluteUri == other.AbsoluteUri;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;

            return Equals((WebResource) obj);
        }

        public override int GetHashCode()
        {
            return AbsoluteUri.GetHashCode();
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return AbsoluteUri;
        }

        #endregion
    }
}
