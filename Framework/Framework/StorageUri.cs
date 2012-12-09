#region Using

using System;
using System.IO;

#endregion

namespace Willcraftia.Xna.Framework
{
    public sealed class StorageUri : IUri, IEquatable<StorageUri>
    {
        public const string StorageScheme = "storage";

        string extension;

        object extensionLock = new object();

        string baseUri;

        object baseUriLock = new object();

        // I/F
        public string AbsoluteUri { get; internal set; }

        // I/F
        public string Scheme
        {
            get { return StorageScheme; }
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
            get { return false; }
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
                        baseUri = StorageScheme + ":" + basePath;
                    }
                    return baseUri;
                }
            }
        }

        internal StorageUri() { }

        // I/F
        public Stream Open()
        {
            var storageContainer = StorageManager.RequiredCurrentStorageContainer;
            return storageContainer.OpenFile(AbsolutePath, FileMode.Open);
        }

        // I/F
        public Stream Create()
        {
            var storageContainer = StorageManager.RequiredCurrentStorageContainer;

            var directoryPath = Path.GetDirectoryName(AbsolutePath);
            if (!storageContainer.DirectoryExists(directoryPath))
                storageContainer.CreateDirectory(directoryPath);

            return storageContainer.CreateFile(AbsolutePath);
        }

        // I/F
        public void Delete()
        {
            var storageContainer = StorageManager.RequiredCurrentStorageContainer;
            storageContainer.DeleteFile(AbsolutePath);
        }

        #region Equatable

        public static bool operator ==(StorageUri p1, StorageUri p2)
        {
            return p1.Equals(p2);
        }

        public static bool operator !=(StorageUri p1, StorageUri p2)
        {
            return !p1.Equals(p2);
        }

        // I/F
        public bool Equals(StorageUri other)
        {
            return AbsoluteUri == other.AbsoluteUri;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;

            return Equals((StorageUri) obj);
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
