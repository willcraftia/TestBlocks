#region Using

using System;
using System.IO;
using Willcraftia.Xna.Framework.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

#endregion

namespace Willcraftia.Xna.Framework.Serialization.Xml.Demo
{
    public class Program
    {
        #region Demo Classes

        public interface IAnimal
        {
            string Name { get; set; }
        }

        public class Dog : IAnimal
        {
            public string Name { get; set; }

            public string DogProperty;

            public override string ToString()
            {
                return "Name=" + Name + ", CatProperty=" + DogProperty;
            }
        }

        public class Cat : IAnimal
        {
            public string Name { get; set; }

            public string CatProperty;

            public override string ToString()
            {
                return "Name=" + Name + ", CatProperty=" + CatProperty;
            }
        }

        public class AnimalManager : IXmlSerializable
        {
            public IAnimal[] Animals;
            
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();

            public AnimalManager()
            {
                namespaces.Add(string.Empty, string.Empty);
            }

            // I/F
            public XmlSchema GetSchema()
            {
                return null;
            }

            // I/F
            public void ReadXml(XmlReader reader)
            {
                reader.Read();
                reader.ReadStartElement("Animals");

                var list = new System.Collections.Generic.List<IAnimal>();

                while (reader.IsStartElement("Animal"))
                {
                    var typeString = reader.GetAttribute("Type");
                    reader.ReadStartElement("Animal");

                    var animalType = Type.GetType(typeString);

                    var animalSerializer = XmlResourceLoader.Instance.GetSerializer(animalType);
                    var animal = animalSerializer.Deserialize(reader) as IAnimal;

                    list.Add(animal);

                    reader.ReadEndElement();
                }

                if (list.Count != 0) Animals = list.ToArray();

                reader.ReadEndElement();
            }

            // I/F
            public void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement("Animals");

                if (Animals == null || Animals.Length == 0) return;

                foreach (var animal in Animals)
                {
                    writer.WriteStartElement("Animal");

                    var animalType = animal.GetType();
                    writer.WriteAttributeString("Type", animalType.FullName);

                    var animalSerializer = XmlResourceLoader.Instance.GetSerializer(animalType);
                    animalSerializer.Serialize(writer, animal, namespaces);

                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }

            public override string ToString()
            {
                var result = "{";
                if (Animals != null)
                {
                    for (int i = 0; i < Animals.Length; i++)
                    {
                        result += "{";
                        result += Animals[i];
                        result += "}";
                        if (i < Animals.Length - 2) result += ", ";
                    }
                }
                result += "}";
                return result.ToString();
            }
        }

        #endregion

        static void Main(string[] args)
        {
            //================================================================
            // Test polymorphic objects

            var animalManager = new AnimalManager();
            animalManager.Animals = new IAnimal[]
            {
                new Dog { Name = "Pochi", DogProperty = "Wan wan" },
                new Cat { Name = "Tama", CatProperty = "Nya nya" }
            };

            // Register JsonResourceLoader.
            ResourceManager.Instance.LoaderMap[".xml"] = XmlResourceLoader.Instance;

            // Output file path.
            var path = Path.Combine(Directory.GetCurrentDirectory(), "AnimalManager.xml");
            var uri = new Uri(path);

            // Save.
            ResourceManager.Instance.Save(uri, animalManager);
            Console.WriteLine("Save:");
            Console.WriteLine(uri.LocalPath);

            using (var stream = ResourceContainerManager.Instance.OpenResource(uri))
            using (var reader = new StreamReader(stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null) Console.WriteLine(line);
            }
            Console.WriteLine();

            // Load.
            var loadedObject = ResourceManager.Instance.Load<AnimalManager>(uri);
            Console.WriteLine("Load:");
            Console.WriteLine(loadedObject);
            Console.WriteLine();

            // Exit.
            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();
        }
    }
}
