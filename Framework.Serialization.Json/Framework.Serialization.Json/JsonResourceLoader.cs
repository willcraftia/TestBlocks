#region Using

using System;
using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Willcraftia.Xna.Framework.Serialization;

#endregion

namespace Willcraftia.Xna.Framework.Serialization.Json
{
    public sealed class JsonResourceLoader : IResourceLoader
    {
        public static readonly JsonResourceLoader Instance = new JsonResourceLoader();

        JsonSerializer jsonSerializer = new JsonSerializer();

        JsonResourceLoader() { }

        public object Load(Uri uri, Stream stream, Type type)
        {
            using (var reader = new StreamReader(stream))
            {
                try
                {
                    return jsonSerializer.Deserialize(reader, type);
                }
                catch (JsonSerializationException e)
                {
                    throw new SerializationException(
                        string.Format("Error json deserializing: uri={0}, type={1}", uri, type), e);
                }
            }
        }

        public void Save(Uri uri, Stream stream, object resource)
        {
            using (var writer = new StreamWriter(stream))
            {
                try
                {
                    jsonSerializer.Serialize(writer, resource);
                }
                catch (JsonSerializationException e)
                {
                    throw new SerializationException(
                        string.Format("Error json serializing: uri={0}, type={1}", uri, resource.GetType()), e);
                }
            }
        }
    }
}
