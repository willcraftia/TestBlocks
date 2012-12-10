#region Using

using System;
using System.IO;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework.IO
{
    public sealed class TitleResource : IResource, IEquatable<TitleResource>
    {
        public const string TitleScheme = "title";

        string extension;

        object extensionLock = new object();

        string baseUri;

        object baseUriLock = new object();

        // I/F
        public string AbsoluteUri { get; internal set; }

        // I/F
        public string Scheme
        {
            get { return TitleScheme; }
        }

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
                        baseUri = TitleScheme + ":" + basePath;
                    }
                    return baseUri;
                }
            }
        }

        internal TitleResource() { }

        // I/F
        public Stream Open()
        {
            return TitleContainer.OpenStream(AbsolutePath);
        }

        // I/F
        public Stream Create() { throw new NotSupportedException(); }

        // I/F
        public void Delete() { throw new NotSupportedException(); }

        #region Equatable

        public static bool operator ==(TitleResource p1, TitleResource p2)
        {
            return p1.Equals(p2);
        }

        public static bool operator !=(TitleResource p1, TitleResource p2)
        {
            return !p1.Equals(p2);
        }

        // I/F
        public bool Equals(TitleResource other)
        {
            return AbsoluteUri == other.AbsoluteUri;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;

            return Equals((TitleResource) obj);
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
