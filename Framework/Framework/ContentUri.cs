#region Using

using System;
using System.IO;

#endregion

namespace Willcraftia.Xna.Framework
{
    public sealed class ContentUri : IUri, IEquatable<ContentUri>
    {
        public const string ContentScheme = "content";

        string baseUri;

        object baseUriLock = new object();

        // I/F
        public string AbsoluteUri { get; internal set; }

        // I/F
        public string Scheme
        {
            get { return ContentScheme; }
        }

        // I/F
        public string AbsolutePath { get; internal set; }

        // I/F
        public string Extension
        {
            get { return string.Empty; }
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
                        baseUri = ContentScheme + ":" + basePath;
                    }
                    return baseUri;
                }
            }
        }

        internal ContentUri() { }

        // I/F
        public Stream Open() { throw new NotSupportedException(); }

        // I/F
        public Stream Create() { throw new NotSupportedException(); }

        // I/F
        public void Delete() { throw new NotSupportedException(); }

        #region Equatable

        public static bool operator ==(ContentUri p1, ContentUri p2)
        {
            return p1.Equals(p2);
        }

        public static bool operator !=(ContentUri p1, ContentUri p2)
        {
            return !p1.Equals(p2);
        }

        // I/F
        public bool Equals(ContentUri other)
        {
            return AbsoluteUri == other.AbsoluteUri;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;

            return Equals((ContentUri) obj);
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
