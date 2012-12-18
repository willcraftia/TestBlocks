#region Using

using System;
using System.IO;
using System.Text;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Diagnostics;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Framework.IO.Compression;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class StorageChunkStore : IChunkStore
    {
        static readonly Logger logger = new Logger(typeof(StorageChunkStore).Name);

        IResource regionUri;

        string rootDirectory = "Chunks";

        string regionDirectory;

        public StorageChunkStore(IResource regionResource)
        {
            if (regionResource == null) throw new ArgumentNullException("regionResource");

            this.regionUri = regionResource;

            var b = new StringBuilder(rootDirectory);
            b.Append('/');
            foreach (var c in regionResource.AbsoluteUri)
            {
                switch (c)
                {
                    case ':':
                    case '/':
                    case '!':
                        b.Append('_');
                        break;
                    default:
                        b.Append(c);
                        break;
                }
            }
            regionDirectory = b.ToString();
        }

        public bool GetChunk(ref VectorI3 position, Chunk chunk)
        {
            logger.InfoBegin("GetChunk: {0}", position);

            var storageContainer = StorageManager.RequiredCurrentStorageContainer;
            var filePath = ResolveFilePath(position);

            bool result;
            if (!storageContainer.FileExists(filePath))
            {
                logger.Info("No chunk cache exists: {0}", filePath);
                result = false;
            }
            else
            {
                logger.Info("Chunk cache exists: {0}", filePath);

                using (var stream = storageContainer.OpenFile(filePath, FileMode.Open))
                using (var reader = new BinaryReader(stream))
                {
                    chunk.Read(reader);
                }

                result = true;
            }

            logger.InfoEnd("GetChunk: {0}", position);

            return result;
        }

        public void AddChunk(Chunk chunk)
        {
            logger.InfoBegin("AddChunk: {0}", chunk.Position);

            var storageContainer = StorageManager.RequiredCurrentStorageContainer;

            if (!storageContainer.DirectoryExists(rootDirectory))
                storageContainer.CreateDirectory(rootDirectory);

            if (!storageContainer.DirectoryExists(regionDirectory))
                storageContainer.CreateDirectory(regionDirectory);
            
            var filePath = ResolveFilePath(chunk.Position);

            logger.Info("Chunk cache: {0}", filePath);

            using (var stream = storageContainer.CreateFile(filePath))
            using (var writer = new BinaryWriter(stream))
            {
                chunk.Write(writer);
            }

            logger.InfoEnd("AddChunk: {0}", chunk.Position);
        }

        public void DeleteChunk(Chunk chunk)
        {
            var storageContainer = StorageManager.RequiredCurrentStorageContainer;
            var filePath = ResolveFilePath(chunk.Position);

            storageContainer.DeleteFile(filePath);
        }

        public void ClearChunks()
        {
            var storageContainer = StorageManager.RequiredCurrentStorageContainer;
            var filePaths = storageContainer.GetFileNames(regionDirectory + "/*.bin");

            foreach (var filePath in filePaths)
                storageContainer.DeleteFile(filePath);

            storageContainer.DeleteDirectory(regionDirectory);
        }

        public void GetChunkBundle(Stream chunkBundleStream, ref VectorI3 chunkSize)
        {
            var storageContainer = StorageManager.RequiredCurrentStorageContainer;

            var filePaths = GetChunkFilePaths();
            if (filePaths.Length == 0) return;

            // Chunk holder
            var chunk = new Chunk(chunkSize);

            using (var gzipStream = new GZipStream(chunkBundleStream, CompressionMode.Compress, CompressionLevel.Fastest))
            using (var writer = new BinaryWriter(gzipStream))
            {
                foreach (var filePath in filePaths)
                {
                    using (var chunkStream = storageContainer.OpenFile(filePath, FileMode.Open))
                    using (var chunkReader = new BinaryReader(chunkStream))
                    {
                        // read a chunk.
                        chunk.Read(chunkReader);
                    }

                    // write this chunk to the chunk bundle.
                    chunk.Write(writer);
                }
            }
        }

        public void AddChunkBundle(Stream chunkBundleStream, ref VectorI3 chunkSize)
        {
            // Chunk holder
            var chunk = new Chunk(chunkSize);

            using (var gzipStream = new GZipStream(chunkBundleStream, CompressionMode.Decompress, CompressionLevel.Fastest))
            using (var reader = new BinaryReader(gzipStream))
            {
                while (-1 < reader.PeekChar())
                {
                    // read a chunk from the chunk bundle.
                    chunk.Read(reader);
                    // add this chunk to the store.
                    AddChunk(chunk);
                }
            }
        }

        string[] GetChunkFilePaths()
        {
            var storageContainer = StorageManager.RequiredCurrentStorageContainer;
            return storageContainer.GetFileNames(regionDirectory + "/*.bin");
        }

        string ResolveFilePath(VectorI3 position)
        {
            return regionDirectory + "/" + position.X + "_" + position.Y + "_" + position.Z + ".bin";
        }
    }
}
