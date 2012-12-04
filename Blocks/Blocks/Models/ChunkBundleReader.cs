#region Using

using System;
using System.IO;
using Willcraftia.Xna.Framework.IO.Compression;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class ChunkBundleReader : IDisposable
    {
        Stream stream;

        Stream gzipStream;

        BinaryReader reader;

        public ChunkBundleReader(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");

            this.stream = stream;

            gzipStream = new GZipStream(stream, CompressionMode.Decompress, CompressionLevel.Fastest);
            reader = new BinaryReader(gzipStream);
        }

        public bool ReadChunk(Chunk chunk)
        {
            if (-1 < reader.PeekChar())
                return false;

            chunk.Read(reader);
            return true;
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed;

        ~ChunkBundleReader()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposed) return;

            reader.Dispose();
            gzipStream.Dispose();
            stream.Dispose();

            disposed = true;
        }

        #endregion
    }
}
