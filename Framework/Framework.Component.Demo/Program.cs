#region Using

using System;
using System.IO;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Framework.Component;
using Willcraftia.Xna.Framework.Noise;
using Willcraftia.Xna.Framework.Serialization;

#endregion

namespace Willcraftia.Xna.Framework.Component.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            //================================================================
            //
            // Shared ComponentTypeRegistory
            //

            var typeRegistory = new AliasTypeRegistory();
            typeRegistory.SetTypeAlias(typeof(Perlin));
            typeRegistory.SetTypeAlias(typeof(SumFractal));
            typeRegistory.SetTypeAlias(typeof(ScaleBias));

            //================================================================
            //
            // NamedComponentFactory
            //

            var factory = new ComponentBundleFactory(typeRegistory);

            factory.AddComponent("perlin", "Perlin");
            factory.SetPropertyValue("perlin", "Seed", 300);

            factory.AddComponent("sumFractal", "SumFractal");
            factory.SetPropertyValue("sumFractal", "Source", "perlin");

            factory.AddComponent("scaleBias", "ScaleBias");
            factory.SetPropertyValue("scaleBias", "Scale", 0.5f);
            factory.SetPropertyValue("scaleBias", "Bias", 0.5f);
            factory.SetPropertyValue("scaleBias", "Source", "sumFractal");
            factory.Build();

            //================================================================
            //
            // Serialization
            //

            BundleDefinition definition;
            factory.GetDefinition(out definition);

            var xmlSerializer = new XmlSerializerAdapter(typeof(BundleDefinition));
            xmlSerializer.WriterSettings.Indent = true;
            xmlSerializer.WriterSettings.OmitXmlDeclaration = true;

            var resource = FileResourceLoader.Instance.LoadResource("file:///" + Directory.GetCurrentDirectory() + "/Noise.xml");
            using (var stream = resource.Create())
            {
                xmlSerializer.Serialize(stream, definition);
            }

            //================================================================
            //
            // Deserialization
            //

            BundleDefinition deserializedDefinition;
            using (var stream = resource.Open())
            {
                deserializedDefinition = (BundleDefinition) xmlSerializer.Deserialize(stream, null);
            }

            //================================================================
            //
            // Other NamedComponentFactory
            //

            var otherFactory = new ComponentBundleFactory(typeRegistory);
            otherFactory.Initialize(ref deserializedDefinition);
            otherFactory.Build();

            // just debug
            var noise = otherFactory["scaleBias"] as INoiseSource;
            var signal = noise.Sample(0.5f, 0.5f, 0.5f);

            //================================================================
            //
            // Exit
            //

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();
        }
    }
}
