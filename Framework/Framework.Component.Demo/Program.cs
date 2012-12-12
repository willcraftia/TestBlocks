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

            var typeRegistory = new ComponentTypeRegistory();
            typeRegistory.SetAlias(typeof(Perlin));
            typeRegistory.SetAlias(typeof(SumFractal));
            typeRegistory.SetAlias(typeof(ScaleBias));

            //================================================================
            //
            // ComponentContainer
            //

            var container = new ComponentFactory(typeRegistory);

            container.AddComponent("perlin", "Perlin");
            container.SetPropertyValue("perlin", "Seed", 300);

            container.AddComponent("sumFractal", "SumFractal");
            container.SetComponentReference("sumFractal", "Source", "perlin");

            container.AddComponent("scaleBias", "ScaleBias");
            container.SetPropertyValue("scaleBias", "Scale", 0.5f);
            container.SetPropertyValue("scaleBias", "Bias", 0.5f);
            container.SetComponentReference("scaleBias", "Source", "sumFractal");

            //================================================================
            //
            // Serialization
            //

            ComponentBundleDefinition definition;
            container.GetDefinition(out definition);

            var xmlSerializer = new XmlSerializerAdapter(typeof(ComponentBundleDefinition));
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

            ComponentBundleDefinition deserializedDefinition;
            using (var stream = resource.Open())
            {
                deserializedDefinition = (ComponentBundleDefinition) xmlSerializer.Deserialize(stream, null);
            }

            //================================================================
            //
            // Other ComponentContainer
            //

            var otherContainer = new ComponentFactory(typeRegistory);
            otherContainer.Initialize(ref deserializedDefinition);

            // just debug
            var noise = otherContainer["scaleBias"] as INoiseSource;
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
