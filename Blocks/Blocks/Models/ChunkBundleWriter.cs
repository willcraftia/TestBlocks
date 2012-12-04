#region Using

using System;
using System.IO;
using Willcraftia.Xna.Framework.IO.Compression;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class ChunkBundleWriter : IDisposable
    {
        Stream stream;

        Stream gzipStream;

        BinaryWriter writer;

        public ChunkBundleWriter(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");

            this.stream = stream;

            gzipStream = new GZipStream(stream, CompressionMode.Compress, CompressionLevel.Fastest);
            writer = new BinaryWriter(gzipStream);
        }

        public void WriteChunk(Chunk chunk)
        {
            chunk.Write(writer);
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed;

        ~ChunkBundleWriter()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposed) return;

            writer.Dispose();
            gzipStream.Dispose();
            stream.Dispose();

            disposed = true;
        }

        #endregion
    }
}
