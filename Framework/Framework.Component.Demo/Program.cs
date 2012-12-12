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
    public struct OwnerDefinition
    {
        public ComponentDefinition[] Definitions;
    }

    class Program
    {
        static void InitialzieAliases(ComponentContainer container)
        {
            container.RegisterAlias(typeof(Perlin));
            container.RegisterAlias(typeof(SumFractal));
            container.RegisterAlias(typeof(ScaleBias));
        }

        static void Main(string[] args)
        {
            var container = new ComponentContainer();
            InitialzieAliases(container);

            var perlinInfo = container.GetComponentInfo("Perlin");
            container.AddComponent("perlin", perlinInfo);
            container.SetPropertyValue("perlin", "Seed", 300);

            var sumFractalInfo = container.GetComponentInfo("SumFractal");
            container.AddComponent("sumFractal", sumFractalInfo);
            container.SetReferencedComponent("sumFractal", "Source", "perlin");

            var scaleBias = container.GetComponentInfo("ScaleBias");
            container.AddComponent("scaleBias", scaleBias);
            container.SetPropertyValue("scaleBias", "Scale", 0.5f);
            container.SetPropertyValue("scaleBias", "Bias", 0.5f);
            container.SetReferencedComponent("scaleBias", "Source", "sumFractal");

            ComponentDefinition[] definitions;
            container.GetDefinitions(out definitions);

            var newContainer = new ComponentContainer();
            InitialzieAliases(newContainer);
            newContainer.Initialize(definitions);

            var ownerDefinition = new OwnerDefinition
            {
                Definitions = definitions
            };

            var xmlSerializer = new XmlSerializerAdapter(typeof(OwnerDefinition));
            xmlSerializer.WriterSettings.Indent = true;
            xmlSerializer.WriterSettings.OmitXmlDeclaration = true;

            var resource = FileResourceLoader.Instance.LoadResource("file:///" + Directory.GetCurrentDirectory() + "/Noise.xml");
            using (var stream = resource.Create())
            {
                xmlSerializer.Serialize(stream, ownerDefinition);
            }

            OwnerDefinition deserializedOwnerDefinition;
            using (var stream = resource.Open())
            {
                deserializedOwnerDefinition = (OwnerDefinition) xmlSerializer.Deserialize(stream, null);
            }

            var deserializedContainer = new ComponentContainer();
            InitialzieAliases(deserializedContainer);
            deserializedContainer.Initialize(deserializedOwnerDefinition.Definitions);

            var noiseSource = container["scaleBias"] as INoiseSource;
            var signal = noiseSource.Sample(0.5f, 0.5f, 0.5f);

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();
        }
    }
}
