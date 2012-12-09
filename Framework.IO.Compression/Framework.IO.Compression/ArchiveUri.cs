#region Using

using System;
using System.IO;

#endregion

namespace Willcraftia.Xna.Framework.IO.Compression
{
    public sealed class ArchiveUri : IUri, IEquatable<ArchiveUri>
    {
        public const string ArchiveScheme = "archive";

        string extension;

        object extensionLock = new object();

        string baseUri;

        object baseUriLock = new object();

        // I/F
        public string AbsoluteUri { get; internal set; }

        // I/F
        public string Scheme
        {
            get { return ArchiveScheme; }
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
                        extension = Path.GetExtension(EntryPath);
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
                        baseUri = ArchiveScheme + ":" + ZipUri + "!" + basePath;
                    }
                    return baseUri;
                }
            }
        }

        public IUri ZipUri { get; internal set; }

        public string EntryPath { get; internal set; }

        internal ArchiveUri() { }

        // I/F
        public Stream Open()
        {
            var archiveStream = ZipUri.Open();
            return new ZipEntryStream(archiveStream, EntryPath);
        }

        // I/F
        public Stream Create() { throw new NotSupportedException(); }

        // I/F
        public void Delete() { throw new NotSupportedException(); }

        #region Equatable

        public static bool operator ==(ArchiveUri p1, ArchiveUri p2)
        {
            return p1.Equals(p2);
        }

        public static bool operator !=(ArchiveUri p1, ArchiveUri p2)
        {
            return !p1.Equals(p2);
        }

        // I/F
        public bool Equals(ArchiveUri other)
        {
            return AbsoluteUri == other.AbsoluteUri;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;

            return Equals((ArchiveUri) obj);
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
