#region Using

using System;
using System.IO;
using Ionic.Zip;

#endregion

namespace Willcraftia.Xna.Framework.IO.Compression
{
    public sealed class ZipEntryStream : Stream
    {
        Stream archiveStream;

        ZipFile zipFile;

        Stream resourceStream;

        public override bool CanRead
        {
            get { return resourceStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return resourceStream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return resourceStream.CanWrite; }
        }

        public override long Length
        {
            get { return resourceStream.Length; }
        }

        public override long Position
        {
            get { return resourceStream.Position; }
            set { resourceStream.Position = value; }
        }

        public ZipEntryStream(Stream archiveStream, string entryPath)
        {
            if (archiveStream == null) throw new ArgumentNullException("archiveStream");
            if (entryPath == null) throw new ArgumentNullException("entryPath");

            this.archiveStream = archiveStream;

            zipFile = ZipFile.Read(archiveStream);
            var zipEntry = zipFile[entryPath];
            if (zipEntry == null)
                throw new IOException(string.Format("The entry '{0}' can not be found.", entryPath));

            resourceStream = zipEntry.OpenReader();
        }

        public override void Flush()
        {
            resourceStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return resourceStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return resourceStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            resourceStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            resourceStream.Write(buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                resourceStream.Dispose();
                zipFile.Dispose();
                archiveStream.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
