#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.IO
{
    public sealed class ContentResourceLoader : ResourceLoader
    {
        public static readonly ContentResourceLoader Instance = new ContentResourceLoader();

        const string prefix = "content:";
        
        ContentResourceLoader() { }

        public override IResource LoadResource(string uri)
        {
            if (!uri.StartsWith(prefix)) return null;

            var absolutePath = uri.Substring(prefix.Length);
            return new ContentResource { AbsoluteUri = uri, AbsolutePath = absolutePath };
        }
    }
}
