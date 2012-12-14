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
    public sealed class DemoComponent
    {
        public DemoComponent Child { get; set; }

        public string StringProperty { get; set; }

        [PropertyIgnored]
        public string IgnoredProperty { get; set; }

        public void SetComponentBundleFactory(ComponentFactory factory)
        {
        }

        public void SetComponentName(string componentName)
        {
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            //================================================================
            //
            // Shared ComponentTypeRegistory
            //

            var typeRegistory = new AliasTypeRegistory();
            typeRegistory.SetTypeAlias(typeof(SCurve5));
            typeRegistory.SetTypeAlias(typeof(Perlin));
            typeRegistory.SetTypeAlias(typeof(SumFractal));
            typeRegistory.SetTypeAlias(typeof(ScaleBias));

            //================================================================
            //
            // NamedComponentFactory
            //

            var factory = new ComponentFactory(typeRegistory);

            factory.AddComponent("scurve5", "SCurve5");
            factory.AddComponent("perlin", "Perlin");
            factory.SetPropertyValue("perlin", "Seed", 300);
            factory.SetPropertyValue("perlin", "FadeCurve", "scurve5");

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

            var otherFactory = new ComponentFactory(typeRegistory);
            otherFactory.AddBundleDefinition(ref deserializedDefinition);
            otherFactory.Build();

            // just debug
            var noise = otherFactory["scaleBias"] as INoiseSource;
            var signal = noise.Sample(0.5f, 0.5f, 0.5f);

            //================================================================
            //
            // DemoComponent
            //

            var demoFactory = new ComponentFactory();
            demoFactory.AddComponent("rootComponent", typeof(DemoComponent).AssemblyQualifiedName);
            demoFactory.SetPropertyValue("rootComponent", "StringProperty", "rootValue");
            demoFactory.SetComponentReference("rootComponent", "Child", "childComponent");
            demoFactory.AddComponent("childComponent", typeof(DemoComponent).AssemblyQualifiedName);
            demoFactory.SetPropertyValue("childComponent", "StringProperty", "childValue");
            demoFactory.Build();

            //================================================================
            //
            // Exit
            //

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();
        }
    }
}
