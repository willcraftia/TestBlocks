#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Willcraftia.Xna.Framework.Diagnostics;

#endregion

namespace Willcraftia.Xna.Framework.Serialization
{
    public sealed class XmlSerializerAdapter : ISerializer
    {
        public static readonly XmlSerializerAdapter Instance = new XmlSerializerAdapter();

        static readonly Logger logger = new Logger(typeof(XmlSerializerAdapter).Name);

        XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();

        Dictionary<Type, XmlSerializer> cache = new Dictionary<Type, XmlSerializer>();

        // I/F
        public bool CanDeserializeIntoExistingObject
        {
            get { return false; }
        }
        
        public XmlWriterSettings WriterSettings { get; private set; }

        public XmlReaderSettings ReaderSettings { get; private set; }

        XmlSerializerAdapter()
        {
            namespaces.Add(string.Empty, string.Empty);

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
            var serializer = GetXmlSerializer(type);

            using (var reader = CreateXmlReader(stream))
            {
                return serializer.Deserialize(reader);
            }
        }

        // I/F
        public void Serialize(Stream stream, object resource)
        {
            var serializer = GetXmlSerializer(resource.GetType());

            using (var writer = CreateXmlWriter(stream))
            {
                serializer.Serialize(writer, resource, namespaces);
            }
        }

        public XmlSerializer GetXmlSerializer(Type type)
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
