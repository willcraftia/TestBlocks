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

        public Dictionary<string, ISerializer> SerializerMap { get; private set; }

        ExtensionSerializerRegistory()
        {
            SerializerMap = new Dictionary<string, ISerializer>();
        }

        // I/F
        public ISerializer ResolveSerializer(Uri uri, Type type)
        {
            var extension = Path.GetExtension(uri.LocalPath);

            ISerializer serializer;
            if (string.IsNullOrEmpty(extension) || !SerializerMap.TryGetValue(extension, out serializer))
                throw new InvalidOperationException("Serializer not found: " + uri);

            return serializer;
        }
    }
}
