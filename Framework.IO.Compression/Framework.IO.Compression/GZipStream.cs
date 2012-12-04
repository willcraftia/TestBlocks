#region Using

using System;
using System.IO;

#endregion

namespace Willcraftia.Xna.Framework.IO.Compression
{
    public sealed class GZipStream : Ionic.Zlib.GZipStream
    {
        public GZipStream(Stream stream, CompressionMode compressionMode, CompressionLevel compressionLevel)
            : base(stream, IonicUtil.ToIonicType(compressionMode), IonicUtil.ToIonicType(compressionLevel))
        {
        }
    }
}
