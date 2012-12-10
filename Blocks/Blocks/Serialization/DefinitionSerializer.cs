#region Using

using System;
using System.Collections.Generic;
using System.IO;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Serialization;
using Willcraftia.Xna.Framework.Serialization.Json;

#endregion

namespace Willcraftia.Xna.Blocks.Serialization
{
    public sealed class DefinitionSerializer
    {
        Dictionary<string, ISerializer> serializerMap = new Dictionary<string, ISerializer>(2);

        public DefinitionSerializer(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");

            serializerMap[".json"] = new JsonSerializerAdapter(type);

            var xmlSerializer = new XmlSerializerAdapter(type);
            xmlSerializer.WriterSettings.OmitXmlDeclaration = true;
            serializerMap[".xml"] = xmlSerializer;
        }

        public object Deserialize(IUri uri)
        {
            var serializer = GetSerializer(uri.Extension);
            using (var stream = uri.Open())
            {
                return serializer.Deserialize(stream, null);
            }
        }

        public void Serialize(IUri uri, object resource)
        {
            var serializer = GetSerializer(uri.Extension);
            using (var stream = uri.Create())
            {
                serializer.Serialize(stream, resource);
            }
        }

        ISerializer GetSerializer(string extension)
        {
            ISerializer serializer;
            if (!serializerMap.TryGetValue(extension, out serializer))
                throw new InvalidOperationException("Unknown extension: " + extension);

            return serializer;
        }
    }
}
