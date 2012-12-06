#region Using

using System;
using Microsoft.Xna.Framework.Storage;

#endregion

namespace Willcraftia.Xna.Framework
{
    public sealed class StorageManager
    {
        public static StorageContainer CurrentStorageContainer { get; set; }

        public static StorageContainer RequiredCurrentStorageContainer
        {
            get
            {
                if (CurrentStorageContainer == null)
                    throw new InvalidOperationException("Storage container not selected.");
                return CurrentStorageContainer;
            }
        }

        public static void SelectStorageContainer(string storageName)
        {
            if (string.IsNullOrEmpty(storageName))
                throw new ArgumentException("storageName must be not null or empty.");

            var showSelectorResult = StorageDevice.BeginShowSelector(null, null);
            showSelectorResult.AsyncWaitHandle.WaitOne();

            var storageDevice = StorageDevice.EndShowSelector(showSelectorResult);
            showSelectorResult.AsyncWaitHandle.Close();

            var openContainerResult = storageDevice.BeginOpenContainer(storageName, null, null);
            openContainerResult.AsyncWaitHandle.WaitOne();

            CurrentStorageContainer = storageDevice.EndOpenContainer(openContainerResult);
            openContainerResult.AsyncWaitHandle.Close();
        }
    }
}
