#region Using

using System;
using System.IO;
using Newtonsoft.Json;
using Willcraftia.Xna.Framework.IO;

#endregion

namespace Willcraftia.Xna.Framework.Serialization.Json.Demo
{
    class Program
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

        public class AnimalManager
        {
            public IAnimal[] Animals;

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

            // Test settings.
            JsonResourceLoader.Instance.Serializer.Formatting = Formatting.Indented;

            // Register JsonResourceLoader.
            ResourceManager.Instance.LoaderMap[".json"] = JsonResourceLoader.Instance;
            
            // Output file path.
            var path = Path.Combine(Directory.GetCurrentDirectory(), "AnimalManager.json");
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
