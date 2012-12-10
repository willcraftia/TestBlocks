#region Using

using System;
using System.IO;
using Newtonsoft.Json;

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

            // Serializer.
            var serializer = new JsonSerializerAdapter(typeof(AnimalManager));
            serializer.JsonSerializer.Formatting = Formatting.Indented;
            
            // Output file path.
            var path = Path.Combine(Directory.GetCurrentDirectory(), "AnimalManager.json");

            // Serialize.
            using (var stream = File.Create(path))
            {
                serializer.Serialize(stream, animalManager);
            }
            Console.WriteLine("Serialize:");
            Console.WriteLine(path);

            using (var stream = File.Open(path, FileMode.Open))
            using (var reader = new StreamReader(stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null) Console.WriteLine(line);
            }
            Console.WriteLine();

            // Deserialize.
            AnimalManager loadedObject;
            using (var stream = File.Open(path, FileMode.Open))
            {
                loadedObject = serializer.Deserialize(stream, null) as AnimalManager;
            }
            Console.WriteLine("Deserialize:");
            Console.WriteLine(loadedObject);
            Console.WriteLine();

            // Exit.
            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();
        }
    }
}
