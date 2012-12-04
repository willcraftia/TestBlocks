#region Using

using System;
using System.IO;
using Willcraftia.Xna.Framework.Storage;

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
            return StorageManager.Instance.StorageContainer.FileExists(uri.LocalPath);
        }

        public Stream Open(Uri uri)
        {
            return StorageManager.Instance.StorageContainer.OpenFile(uri.LocalPath, FileMode.Open);
        }

        public Stream Create(Uri uri)
        {
            var directoryPath = Path.GetDirectoryName(uri.LocalPath);
            if (!StorageManager.Instance.StorageContainer.DirectoryExists(directoryPath))
                StorageManager.Instance.StorageContainer.CreateDirectory(directoryPath);

            return StorageManager.Instance.StorageContainer.CreateFile(uri.LocalPath);
        }

        public void Delete(Uri uri)
        {
            StorageManager.Instance.StorageContainer.DeleteFile(uri.LocalPath);
        }
    }
}
