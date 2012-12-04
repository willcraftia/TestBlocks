#region Using

using System;
using System.IO;

#endregion

namespace Willcraftia.Xna.Framework.Serialization
{
    public interface IResourceLoader
    {
        object Load(Uri uri, Stream stream, Type type);

        void Save(Uri uri, Stream stream, object resource);
    }
}
