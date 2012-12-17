#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.IO
{
    public sealed class WebResourceLoader : ResourceLoader
    {
        public static readonly WebResourceLoader Instance = new WebResourceLoader();

        const string httpPrefix = "http://";

        const string httpsPrefix = "https://";

        WebResourceLoader() { }

        public override IResource LoadResource(string uri)
        {
#if WINDOWS
            if (uri.StartsWith(httpPrefix))
            {
                var absolutePath = uri.Substring(httpPrefix.Length);
                return new WebResource
                {
                    Scheme = WebResource.HttpScheme,
                    AbsoluteUri = uri,
                    AbsolutePath = absolutePath
                };
            }
            else if (uri.StartsWith(httpsPrefix))
            {
                var absolutePath = uri.Substring(httpsPrefix.Length);
                return new WebResource
                {
                    Scheme = WebResource.HttpsScheme,
                    AbsoluteUri = uri,
                    AbsolutePath = absolutePath
                };
            }
            else
            {
                return null;
            }
#else
            return null;
#endif
        }
    }
}
