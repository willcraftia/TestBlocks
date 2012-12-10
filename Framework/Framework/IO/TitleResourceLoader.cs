#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.IO
{
    public sealed class TitleResourceLoader : ResourceLoader
    {
        public static readonly TitleResourceLoader Instance = new TitleResourceLoader();

        const string prefix = "title:";

        TitleResourceLoader() { }

        public override IResource LoadResource(string uri)
        {
            if (!uri.StartsWith(prefix)) return null;

            var absolutePath = uri.Substring(prefix.Length);
            return new TitleResource { AbsoluteUri = uri, AbsolutePath = absolutePath };
        }
    }
}
