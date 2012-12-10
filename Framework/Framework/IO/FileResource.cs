#region Using

using System;
using System.IO;

#endregion

namespace Willcraftia.Xna.Framework.IO
{
    public sealed class FileResource : IResource, IEquatable<FileResource>
    {
        public const string FileScheme = "file";
     
        string extension;

        object extensionLock = new object();

        string baseUri;

        object baseUriLock = new object();

        // I/F
        public string AbsoluteUri { get; internal set; }

        // I/F
        public string Scheme
        {
            get { return FileScheme; }
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
                        baseUri = FileScheme + ":///" + basePath;
                    }
                    return baseUri;
                }
            }
        }

        // I/F
        public bool ReadOnly
        {
            get
            {
#if XBOX
                return true;
#else
                var attributes = File.GetAttributes(AbsolutePath);
                return (attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;
#endif
            }
        }

        internal FileResource() { }

        // I/F
        public Stream Open()
        {
            return File.Open(AbsolutePath, FileMode.Open);
        }

        // I/F
        public Stream Create()
        {
            return File.Create(AbsolutePath);
        }

        // I/F
        public void Delete()
        {
            File.Delete(AbsolutePath);
        }

        #region Equatable

        public static bool operator ==(FileResource p1, FileResource p2)
        {
            return p1.Equals(p2);
        }

        public static bool operator !=(FileResource p1, FileResource p2)
        {
            return !p1.Equals(p2);
        }

        // I/F
        public bool Equals(FileResource other)
        {
            return AbsoluteUri == other.AbsoluteUri;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;

            return Equals((FileResource) obj);
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
