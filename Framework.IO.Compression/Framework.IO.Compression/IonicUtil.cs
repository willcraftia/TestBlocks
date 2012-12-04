#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.IO.Compression
{
    internal static class IonicUtil
    {
        internal static Ionic.Zlib.CompressionMode ToIonicType(CompressionMode compressionMode)
        {
            switch (compressionMode)
            {
                case CompressionMode.Decompress:
                    return Ionic.Zlib.CompressionMode.Decompress;
                case CompressionMode.Compress:
                    return Ionic.Zlib.CompressionMode.Compress;
                default:
                    throw new InvalidOperationException();
            }
        }

        internal static Ionic.Zlib.CompressionLevel ToIonicType(CompressionLevel compressionLevel)
        {
            switch (compressionLevel)
            {
                case CompressionLevel.Optimal:
                    return Ionic.Zlib.CompressionLevel.BestCompression;
                case CompressionLevel.Fastest:
                    return Ionic.Zlib.CompressionLevel.BestSpeed;
                case CompressionLevel.NoCompression:
                    return Ionic.Zlib.CompressionLevel.None;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
