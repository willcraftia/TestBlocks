#if WINDOWS

#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Content
{
    public sealed class DownloadInfo : IEquatable<DownloadInfo>
    {
        public string Uri { get; private set; }

        public long LastModified { get; internal set; }

        public DownloadInfo(string uri)
        {
            if (uri == null) throw new ArgumentNullException("uri");

            Uri = uri;
            LastModified = DateTime.MinValue.Ticks;
        }

        #region Equatable

        public static bool operator ==(DownloadInfo p1, DownloadInfo p2)
        {
            return p1.Equals(p2);
        }

        public static bool operator !=(DownloadInfo p1, DownloadInfo p2)
        {
            return !p1.Equals(p2);
        }

        // I/F
        public bool Equals(DownloadInfo other)
        {
            return Uri == other.Uri;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;

            return Equals((DownloadInfo) obj);
        }

        public override int GetHashCode()
        {
            return Uri.GetHashCode();
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return "[Uri=" + Uri + ", LastModified=" + new DateTime(LastModified) + "]";
        }

        #endregion
    }
}

#endif