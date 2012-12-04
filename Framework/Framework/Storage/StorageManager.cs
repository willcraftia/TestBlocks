#region Using

using System;
using Microsoft.Xna.Framework.Storage;

#endregion

namespace Willcraftia.Xna.Framework.Storage
{
    public sealed class StorageManager
    {
        public static readonly StorageManager Instance = new StorageManager();

        StorageContainer storageContainer;

        public StorageContainer StorageContainer
        {
            get
            {
                if (storageContainer == null)
                    throw new InvalidOperationException("StorageContainer is not opened.");

                return storageContainer;
            }
        }

        StorageManager() { }

        public void SelectStorageContainer(string storageName)
        {
            if (string.IsNullOrEmpty(storageName))
                throw new ArgumentException("storageName must be not null or empty.");

            var showSelectorResult = StorageDevice.BeginShowSelector(null, null);
            showSelectorResult.AsyncWaitHandle.WaitOne();

            var storageDevice = StorageDevice.EndShowSelector(showSelectorResult);
            showSelectorResult.AsyncWaitHandle.Close();

            var openContainerResult = storageDevice.BeginOpenContainer(storageName, null, null);
            openContainerResult.AsyncWaitHandle.WaitOne();

            storageContainer = storageDevice.EndOpenContainer(openContainerResult);
            openContainerResult.AsyncWaitHandle.Close();
        }
    }
}
