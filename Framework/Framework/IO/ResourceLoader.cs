#region Using

using System;
using System.Collections.Generic;

#endregion

namespace Willcraftia.Xna.Framework.IO
{
    public abstract class ResourceLoader
    {
        static List<ResourceLoader> loaders = new List<ResourceLoader>();

        public static void Register(ResourceLoader loader)
        {
            if (!loaders.Contains(loader)) loaders.Add(loader);
        }

        public static IResource Load(string uri)
        {
            if (uri == null) throw new ArgumentNullException("uri");

            foreach (var loader in loaders)
            {
                var resource = loader.LoadResource(uri);
                if (resource != null) return resource;
            }

            throw new InvalidOperationException("Resource loader not found: " + uri);
        }

        public abstract IResource LoadResource(string uri);
    }
}
