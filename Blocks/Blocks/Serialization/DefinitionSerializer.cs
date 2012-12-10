#region Using

using System;
using System.Collections.Generic;
using System.IO;
using Willcraftia.Xna.Framework.IO;
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

        public object Deserialize(IResource resource)
        {
            var serializer = GetSerializer(resource.Extension);
            using (var stream = resource.Open())
            {
                return serializer.Deserialize(stream, null);
            }
        }

        public void Serialize(IResource resource, object instance)
        {
            var serializer = GetSerializer(resource.Extension);
            using (var stream = resource.Create())
            {
                serializer.Serialize(stream, instance);
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
