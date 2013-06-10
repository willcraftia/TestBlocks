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

        string rootDirectory = "Chunks";

        public bool GetChunk(string regionKey, IntVector3 position, ChunkData data)
        {
            var storageContainer = StorageManager.RequiredCurrentStorageContainer;
            var filePath = ResolveFilePath(regionKey, position);

            bool result;
            if (!storageContainer.FileExists(filePath))
            {
                result = false;
            }
            else
            {
                using (var stream = storageContainer.OpenFile(filePath, FileMode.Open))
                using (var reader = new BinaryReader(stream))
                {
                    data.Read(reader);
                }

                result = true;
            }

            return result;
        }

        public void AddChunk(string regionKey, IntVector3 position, ChunkData data)
        {
            var storageContainer = StorageManager.RequiredCurrentStorageContainer;

            if (!storageContainer.DirectoryExists(rootDirectory))
                storageContainer.CreateDirectory(rootDirectory);

            var regionPath = ResolveRegionPath(regionKey);

            if (!storageContainer.DirectoryExists(regionPath))
                storageContainer.CreateDirectory(regionPath);

            var filePath = ResolveFilePath(regionKey, position);
            using (var stream = storageContainer.CreateFile(filePath))
            using (var writer = new BinaryWriter(stream))
            {
                data.Write(writer);
            }
        }

        public void DeleteChunk(string regionKey, IntVector3 position)
        {
            var storageContainer = StorageManager.RequiredCurrentStorageContainer;
            var filePath = ResolveFilePath(regionKey, position);

            storageContainer.DeleteFile(filePath);
        }

        public void ClearChunks(string regionKey)
        {
            var storageContainer = StorageManager.RequiredCurrentStorageContainer;
            var filePaths = GetChunkFilePaths(regionKey);

            foreach (var filePath in filePaths)
                storageContainer.DeleteFile(filePath);

            var regionPath = ResolveRegionPath(regionKey);

            storageContainer.DeleteDirectory(regionPath);
        }

        public void GetChunkBundle(Stream chunkBundleStream)
        {
            // TODO
            // バッファ用チャンクを引数で渡せば良いのでは？
            throw new NotImplementedException();

            //var storageContainer = StorageManager.RequiredCurrentStorageContainer;

            //var filePaths = GetChunkFilePaths();
            //if (filePaths.Length == 0) return;

            //// Chunk holder
            //var chunk = new Chunk();

            //using (var gzipStream = new GZipStream(chunkBundleStream, CompressionMode.Compress, CompressionLevel.Fastest))
            //using (var writer = new BinaryWriter(gzipStream))
            //{
            //    foreach (var filePath in filePaths)
            //    {
            //        using (var chunkStream = storageContainer.OpenFile(filePath, FileMode.Open))
            //        using (var chunkReader = new BinaryReader(chunkStream))
            //        {
            //            // read a chunk.
            //            chunk.Read(chunkReader);
            //        }

            //        // write this chunk to the chunk bundle.
            //        chunk.Write(writer);
            //    }
            //}
        }

        public void AddChunkBundle(Stream chunkBundleStream)
        {
            // TODO
            throw new NotImplementedException();

            //// Chunk holder
            //var chunk = new Chunk();

            //using (var gzipStream = new GZipStream(chunkBundleStream, CompressionMode.Decompress, CompressionLevel.Fastest))
            //using (var reader = new BinaryReader(gzipStream))
            //{
            //    while (-1 < reader.PeekChar())
            //    {
            //        // read a chunk from the chunk bundle.
            //        chunk.Read(reader);
            //        // add this chunk to the store.
            //        AddChunk(chunk);
            //    }
            //}
        }

        string[] GetChunkFilePaths(string regionKey)
        {
            var storageContainer = StorageManager.RequiredCurrentStorageContainer;
            return storageContainer.GetFileNames(rootDirectory + "/" + regionKey + "/*.bin");
        }

        string ResolveRegionPath(string regionKey)
        {
            return rootDirectory + "/" + regionKey + "/";
        }

        string ResolveFilePath(string regionKey, IntVector3 position)
        {
            return rootDirectory + "/" + regionKey + "/" + position.X + "_" + position.Y + "_" + position.Z + ".bin";
        }
    }
}
