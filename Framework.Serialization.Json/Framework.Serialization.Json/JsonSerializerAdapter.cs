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
        static readonly Logger logger = new Logger(typeof(JsonSerializerAdapter).Name);

        Type type;

        // I/F
        public bool CanDeserializeIntoExistingObject
        {
            get { return true; }
        }

#if WINDOWS
        public JsonSerializer JsonSerializer { get; private set; }
#endif

        public JsonSerializerAdapter(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");

            this.type = type;

#if WINDOWS
            JsonSerializer = new JsonSerializer();
            JsonSerializer.TypeNameHandling = TypeNameHandling.Auto;
#else
            logger.Error("Support only windows.");
#endif
        }

        // I/F
        public object Deserialize(Stream stream, object existingInstance)
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
