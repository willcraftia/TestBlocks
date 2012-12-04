#region Using

using System;
using System.IO;
using System.Collections.Generic;
using Willcraftia.Xna.Framework.IO;

#endregion

namespace Willcraftia.Xna.Framework.Serialization
{
    public sealed class ResourceManager
    {
        public static readonly ResourceManager Instance = new ResourceManager();

        public Dictionary<string, IResourceLoader> LoaderMap { get; private set; }

        ResourceManager()
        {
            LoaderMap = new Dictionary<string, IResourceLoader>();
        }

        public bool Exists(Uri uri)
        {
            return ResourceContainerManager.Instance.ResourceExists(uri);
        }

        public T Load<T>(Uri uri)
        {
            return (T) Load(uri, typeof(T));
        }

        public object Load(Uri uri, Type type)
        {
            if (uri == null) throw new ArgumentNullException("uri");
            if (type == null) throw new ArgumentNullException("type");

            using (var stream = ResourceContainerManager.Instance.OpenResource(uri))
            {
                return GetLoader(uri).Load(uri, stream, type);
            }
        }

        public void Save(Uri uri, object resource)
        {
            if (uri == null) throw new ArgumentNullException("uri");
            if (resource == null) throw new ArgumentNullException("resource");

            using (var stream = ResourceContainerManager.Instance.CreateResource(uri))
            {
                GetLoader(uri).Save(uri, stream, resource);
            }
        }

        public void Delete(Uri uri)
        {
            ResourceContainerManager.Instance.DeleteResource(uri);
        }

        IResourceLoader GetLoader(Uri uri)
        {
            var extension = Path.GetExtension(uri.LocalPath);

            IResourceLoader loader;
            if (string.IsNullOrEmpty(extension) || !LoaderMap.TryGetValue(extension, out loader))
            {
                throw new InvalidOperationException(string.Format("Resource loader not found: {0}", uri));
            }

            return loader;
        }
    }
}
