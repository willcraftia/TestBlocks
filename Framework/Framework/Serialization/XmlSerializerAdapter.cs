#region Using

using System;
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
        static readonly Logger logger = new Logger(typeof(XmlSerializerAdapter).Name);

        XmlSerializer xmlSerializer;

        XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();

        // I/F
        public bool CanDeserializeIntoExistingObject
        {
            get { return false; }
        }
        
        public XmlWriterSettings WriterSettings { get; private set; }

        public XmlReaderSettings ReaderSettings { get; private set; }

        public XmlSerializerAdapter(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");

            xmlSerializer = new XmlSerializer(type);

            namespaces.Add(string.Empty, string.Empty);

            WriterSettings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8
            };
            ReaderSettings = new XmlReaderSettings
            {
                IgnoreComments = true,
                IgnoreProcessingInstructions = true,
                IgnoreWhitespace = true
            };
        }

        // I/F
        public object Deserialize(Stream stream, object existingInstance)
        {
            using (var reader = XmlReader.Create(stream, ReaderSettings))
            {
                return xmlSerializer.Deserialize(reader);
            }
        }

        // I/F
        public void Serialize(Stream stream, object instance)
        {
            using (var writer = XmlWriter.Create(stream, WriterSettings))
            {
                xmlSerializer.Serialize(writer, instance, namespaces);
            }
        }
    }
}
