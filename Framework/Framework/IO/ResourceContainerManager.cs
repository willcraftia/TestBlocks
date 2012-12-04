#region Using

using System;
using System.Collections.Generic;
using System.IO;

#endregion

namespace Willcraftia.Xna.Framework.IO
{
    public sealed class ResourceContainerManager
    {
        public static readonly ResourceContainerManager Instance = new ResourceContainerManager();

        public Dictionary<string, IResourceContainer> ResourceContainerMap { get; private set; }

        ResourceContainerManager()
        {
            ResourceContainerMap = new Dictionary<string, IResourceContainer>(3);
            // Built-in resource containers.
            ResourceContainerMap[TitleResourceContainer.Scheme] = TitleResourceContainer.Instance;
            ResourceContainerMap[FileResourceContainer.Scheme] = FileResourceContainer.Instance;
            ResourceContainerMap[StorageResourceContainer.Scheme] = StorageResourceContainer.Instance;
        }

        public bool IsReadOnly(Uri uri)
        {
            if (uri == null) throw new ArgumentNullException("uri");

            return ResolveResourceContainer(uri.Scheme).ReadOnly;
        }

        public bool ResourceExists(Uri uri)
        {
            if (uri == null) throw new ArgumentNullException("uri");

            return ResolveResourceContainer(uri.Scheme).Exists(uri);
        }

        public Stream OpenResource(Uri uri)
        {
            if (uri == null) throw new ArgumentNullException("uri");

            return ResolveResourceContainer(uri.Scheme).Open(uri);
        }

        public Stream CreateResource(Uri uri)
        {
            if (uri == null) throw new ArgumentNullException("uri");

            return ResolveResourceContainer(uri.Scheme).Create(uri);
        }

        public void DeleteResource(Uri uri)
        {
            if (uri == null) throw new ArgumentNullException("uri");

            ResolveResourceContainer(uri.Scheme).Delete(uri);
        }

        IResourceContainer ResolveResourceContainer(string scheme)
        {
            IResourceContainer resourceContainer;
            if (ResourceContainerMap.TryGetValue(scheme, out resourceContainer))
                return resourceContainer;

            throw new ArgumentException(string.Format("No resource container for '{0}' exists.", scheme));
        }
    }
}
