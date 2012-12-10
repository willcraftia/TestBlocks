#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.IO
{
    public sealed class StorageResourceLoader : ResourceLoader
    {
        public static readonly StorageResourceLoader Instance = new StorageResourceLoader();

        const string prefix = "storage:";

        StorageResourceLoader() { }

        public override IResource LoadResource(string uri)
        {
            if (!uri.StartsWith(prefix)) return null;

            var absolutePath = uri.Substring(prefix.Length);
            return new StorageResource { AbsoluteUri = uri, AbsolutePath = absolutePath };
        }
    }
}
