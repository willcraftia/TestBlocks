#region Using

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

#endregion

namespace Willcraftia.Xna.Framework.Serialization.Xml
{
    public sealed class XmlResourceLoader : IResourceLoader
    {
        public static readonly XmlResourceLoader Instance = new XmlResourceLoader();

        Dictionary<Type, XmlSerializer> cache = new Dictionary<Type, XmlSerializer>();
        
        public XmlWriterSettings WriterSettings { get; private set; }

        public XmlReaderSettings ReaderSettings { get; private set; }

        XmlResourceLoader()
        {
            WriterSettings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "\t",
                Encoding = Encoding.UTF8,
                OmitXmlDeclaration = true
            };
            ReaderSettings = new XmlReaderSettings
            {
                IgnoreComments = true,
                IgnoreProcessingInstructions = true,
                IgnoreWhitespace = true
            };
        }

        public object Load(Uri uri, Stream stream, Type type)
        {
            var serializer = GetSerializer(type);

            using (var reader = CreateXmlReader(stream))
            {
                return serializer.Deserialize(reader);
            }
        }

        public void Save(Uri uri, Stream stream, object resource)
        {
            var serializer = GetSerializer(resource.GetType());

            using (var writer = CreateXmlWriter(stream))
            {
                serializer.Serialize(writer, resource);
            }
        }

        public XmlSerializer GetSerializer(Type type)
        {
            lock (cache)
            {
                XmlSerializer result;
                if (!cache.TryGetValue(type, out result))
                {
                    result = new XmlSerializer(type);
                    cache[type] = result;
                }
                return result;
            }
        }

        XmlWriter CreateXmlWriter(Stream stream)
        {
            return XmlWriter.Create(stream, WriterSettings);
        }

        XmlReader CreateXmlReader(Stream stream)
        {
            return XmlReader.Create(stream, ReaderSettings);
        }
    }
}
