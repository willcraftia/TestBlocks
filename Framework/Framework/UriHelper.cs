#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework
{
    public static class UriHelper
    {
        public static bool IsAbsoluteUri(string uriString)
        {
            if (uriString == null) throw new ArgumentNullException("uriString");

            return 0 < uriString.IndexOf(':');
        }

        public static string ResolveAbsoluteUri(Uri baseUri, string uriString)
        {
            if (baseUri == null) throw new ArgumentNullException("baseUri");
            if (!baseUri.IsAbsoluteUri) throw new ArgumentException("Uri not absolute: " + baseUri);
            if (uriString == null) throw new ArgumentNullException("uriString");

            return IsAbsoluteUri(uriString) ? uriString : baseUri.AbsoluteUri + uriString;
        }
    }
}
