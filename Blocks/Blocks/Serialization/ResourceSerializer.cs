#region Using

using System;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Serialization;

#endregion

namespace Willcraftia.Xna.Blocks.Serialization
{
    public static class ResourceSerializer
    {
        public static T Deserialize<T>(IUri uri)
        {
            var serializer = SerializerManager.Instance.GetSerializer<T>(uri);
            using (var stream = uri.Open())
            {
                return serializer.Deserialize<T>(stream);
            }
        }

        public static void Serialize<T>(IUri uri, T resource)
        {
            var serializer = SerializerManager.Instance.GetSerializer<T>(uri);
            using (var stream = uri.Create())
            {
                serializer.Serialize(stream, resource);
            }
        }
    }
}
