#region Using

using System;
using System.IO;

#endregion

namespace Willcraftia.Xna.Framework.IO
{
    public sealed class StorageResourceContainer : IResourceContainer
    {
        public const string Scheme = "storage";

        public static StorageResourceContainer Instance = new StorageResourceContainer();

        public bool ReadOnly
        {
            get { return false; }
        }

        StorageResourceContainer() { }

        public bool Exists(Uri uri)
        {
            var storageContainer = StorageManager.RequiredCurrentStorageContainer;
            return storageContainer.FileExists(uri.LocalPath);
        }

        public Stream Open(Uri uri)
        {
            var storageContainer = StorageManager.RequiredCurrentStorageContainer;
            return storageContainer.OpenFile(uri.LocalPath, FileMode.Open);
        }

        public Stream Create(Uri uri)
        {
            var storageContainer = StorageManager.RequiredCurrentStorageContainer;

            var directoryPath = Path.GetDirectoryName(uri.LocalPath);
            if (!storageContainer.DirectoryExists(directoryPath))
                storageContainer.CreateDirectory(directoryPath);

            return storageContainer.CreateFile(uri.LocalPath);
        }

        public void Delete(Uri uri)
        {
            var storageContainer = StorageManager.RequiredCurrentStorageContainer;
            storageContainer.DeleteFile(uri.LocalPath);
        }
    }
}
