#region Using

using System;
using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Willcraftia.Xna.Framework.Serialization;

#endregion

namespace Willcraftia.Xna.Framework.Serialization.Json
{
    public sealed class JsonSerializerAdapter : ISerializer
    {
        public static readonly JsonSerializerAdapter Instance = new JsonSerializerAdapter();

        // I/F
        public bool CanDeserializeIntoExistingObject
        {
            get { return true; }
        }

        public JsonSerializer JsonSerializer { get; private set; }

        JsonSerializerAdapter()
        {
            JsonSerializer = new JsonSerializer();
            JsonSerializer.TypeNameHandling = TypeNameHandling.Auto;
        }

        // I/F
        public T Deserialize<T>(Stream stream)
        {
            return (T) Deserialize(stream, typeof(T), null);
        }

        // I/F
        public T Deserialize<T>(Stream stream, T existingInstance) where T : class
        {
            return Deserialize(stream, typeof(T), existingInstance) as T;
        }

        // I/F
        public object Deserialize(Stream stream, Type type, object existingInstance)
        {
            using (var reader = new StreamReader(stream))
            {
                try
                {
                    if (existingInstance == null)
                    {
                        return JsonSerializer.Deserialize(reader, type);
                    }
                    else
                    {
                        JsonSerializer.Populate(reader, existingInstance);
                        return existingInstance;
                    }
                }
                catch (JsonException e)
                {
                    throw new SerializationException("Error json deserializing: " + type, e);
                }
            }
        }

        // I/F
        public void Serialize(Stream stream, object resource)
        {
            using (var writer = new StreamWriter(stream))
            {
                try
                {
                    JsonSerializer.Serialize(writer, resource);
                }
                catch (JsonException e)
                {
                    throw new SerializationException("Error json serializing: " + resource.GetType(), e);
                }
            }
        }
    }
}
