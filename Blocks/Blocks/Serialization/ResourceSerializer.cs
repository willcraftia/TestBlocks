#region Using

using System;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Framework.Serialization;

#endregion

namespace Willcraftia.Xna.Blocks.Serialization
{
    public static class ResourceSerializer
    {
        public static T Deserialize<T>(Uri uri)
        {
            var serializer = SerializerManager.Instance.GetSerializer<T>(uri);
            using (var stream = ResourceContainerManager.Instance.Open(uri))
            {
                return serializer.Deserialize<T>(stream);
            }
        }

        public static void Serialize<T>(Uri uri, T resource)
        {
            var serializer = SerializerManager.Instance.GetSerializer<T>(uri);
            using (var stream = ResourceContainerManager.Instance.Create(uri))
            {
                serializer.Serialize(stream, resource);
            }
        }
    }
}
