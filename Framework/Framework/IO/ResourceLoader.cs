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

            for (int i = 0; i < loaders.Count; i++)
            {
                var resource = loaders[i].LoadResource(uri);
                if (resource != null) return resource;
            }

            throw new InvalidOperationException("Resource loader not found: " + uri);
        }

        public abstract IResource LoadResource(string uri);
    }
}
