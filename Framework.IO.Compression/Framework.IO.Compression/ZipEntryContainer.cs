#region Using

using System;
using System.IO;
using Ionic.Zip;

#endregion

namespace Willcraftia.Xna.Framework.IO.Compression
{
    public sealed class ZipEntryContainer : IResourceContainer
    {
        public static readonly ZipEntryContainer Instance = new ZipEntryContainer();

        public const string Scheme = "archive";

        ZipEntryContainer() { }

        public bool ReadOnly
        {
            get { return true; }
        }

        public bool Exists(Uri uri)
        {
            if (uri == null) throw new ArgumentNullException("uri");

            Uri archiveUri;
            string entryPath;
            ParseUri(uri, out archiveUri, out entryPath);

            if (!ResourceContainerManager.Instance.ResourceExists(archiveUri))
                return false;

            using (var stream = ResourceContainerManager.Instance.OpenResource(archiveUri))
            using (var zipFile = ZipFile.Read(stream))
            {
                return zipFile.ContainsEntry(entryPath);
            }
        }

        public Stream Open(Uri uri)
        {
            if (uri == null) throw new ArgumentNullException("uri");

            Uri archiveUri;
            string entryPath;
            ParseUri(uri, out archiveUri, out entryPath);

            var stream = ResourceContainerManager.Instance.OpenResource(archiveUri);
            return new ZipEntryStream(stream, entryPath);
        }

        public Stream Create(Uri uri)
        {
            throw new NotSupportedException();
        }

        public void Delete(Uri uri)
        {
            throw new NotSupportedException();
        }

        void ParseUri(Uri uri, out Uri archiveUri, out string entryPath)
        {
            var archivePartLength = uri.LocalPath.IndexOf('!');
            if (archivePartLength < 0)
                throw new ArgumentException(
                    string.Format("The specified URI '[0}' is invalid as the archive scheme.", uri));

            var archivePart = uri.LocalPath.Substring(0, archivePartLength);
            if (uri.LocalPath.Length <= archivePartLength + 2)
                throw new ArgumentException(
                    string.Format("The specified URI '[0}' is invalid as the archive scheme.", uri));

            archiveUri = new Uri(archivePart);
            entryPath = uri.LocalPath.Substring(archivePartLength + 2);
        }
    }
}
