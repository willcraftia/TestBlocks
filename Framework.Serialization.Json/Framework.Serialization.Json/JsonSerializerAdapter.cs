#region Using

using System;
using System.IO;
using System.Runtime.Serialization;
#if WINDOWS
using Newtonsoft.Json;
#endif
using Willcraftia.Xna.Framework.Diagnostics;
using Willcraftia.Xna.Framework.Serialization;

#endregion

namespace Willcraftia.Xna.Framework.Serialization.Json
{
    public sealed class JsonSerializerAdapter : ISerializer
    {
        public static readonly JsonSerializerAdapter Instance = new JsonSerializerAdapter();

        static readonly Logger logger = new Logger(typeof(JsonSerializerAdapter).Name);

        // I/F
        public bool CanDeserializeIntoExistingObject
        {
            get { return true; }
        }

#if WINDOWS
        public JsonSerializer JsonSerializer { get; private set; }
#endif

        JsonSerializerAdapter()
        {
#if WINDOWS
            JsonSerializer = new JsonSerializer();
            JsonSerializer.TypeNameHandling = TypeNameHandling.Auto;
#else
            logger.Error("Support only windows.");
#endif
        }

        // I/F
        public T Deserialize<T>(Stream stream)
        {
#if WINDOWS
            return (T) Deserialize(stream, typeof(T), null);
#else
            throw new NotSupportedException("Support only windows.");
#endif
        }

        // I/F
        public T Deserialize<T>(Stream stream, T existingInstance) where T : class
        {
#if WINDOWS
            return Deserialize(stream, typeof(T), existingInstance) as T;
#else
            throw new NotSupportedException("Support only windows.");
#endif
        }

        // I/F
        public object Deserialize(Stream stream, Type type, object existingInstance)
        {
#if WINDOWS
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
#else
            throw new NotSupportedException("Support only windows.");
#endif
        }

        // I/F
        public void Serialize(Stream stream, object resource)
        {
#if WINDOWS
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
#else
            throw new NotSupportedException("Support only windows.");
#endif
        }
    }
}
