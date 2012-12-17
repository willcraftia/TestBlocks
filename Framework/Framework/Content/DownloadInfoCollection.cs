#if WINDOWS

#region Using

using System;
using System.Collections.ObjectModel;

#endregion

namespace Willcraftia.Xna.Framework.Content
{
    public sealed class DownloadInfoCollection : KeyedCollection<string, DownloadInfo>
    {
        protected override string GetKeyForItem(DownloadInfo item)
        {
            return item.Uri;
        }
    }
}

#endif