#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Serialization
{
    public sealed class SerializerManager
    {
        public static readonly SerializerManager Instance = new SerializerManager();

        public ISerializerRegistory Registory { get; set; }

        SerializerManager()
        {
            Registory = ExtensionSerializerRegistory.Instance;
        }

        public ISerializer GetSerializer<T>(IUri uri)
        {
            return GetSerializer(uri, typeof(T));
        }

        public ISerializer GetSerializer(IUri uri, Type type)
        {
            return Registory.ResolveSerializer(uri, type);
        }
    }
}
