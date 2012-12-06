#region Using

using System;
using System.Collections.Generic;
using System.IO;

#endregion

namespace Willcraftia.Xna.Framework.Serialization
{
    public sealed class ExtensionSerializerRegistory : ISerializerRegistory
    {
        public static readonly ExtensionSerializerRegistory Instance = new ExtensionSerializerRegistory();

        Dictionary<string, ISerializer> serializerMap;

        public ISerializer this[string extension]
        {
            get { return serializerMap[extension]; }
            set { serializerMap[extension] = value; }
        }

        ExtensionSerializerRegistory()
        {
            serializerMap = new Dictionary<string, ISerializer>();
        }

        // I/F
        public ISerializer ResolveSerializer(Uri uri, Type type)
        {
            var extension = Path.GetExtension(uri.LocalPath);

            ISerializer serializer;
            if (string.IsNullOrEmpty(extension) || !serializerMap.TryGetValue(extension, out serializer))
                throw new InvalidOperationException("Serializer not found: " + uri);

            return serializer;
        }

        public void Remove(string extension)
        {
            serializerMap.Remove(extension);
        }

        public void Clear()
        {
            serializerMap.Clear();
        }
    }
}
