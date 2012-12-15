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
            typeRegistory.SetTypeDefinitionName(typeof(SCurve5));
            typeRegistory.SetTypeDefinitionName(typeof(Perlin));
            typeRegistory.SetTypeDefinitionName(typeof(SumFractal));
            typeRegistory.SetTypeDefinitionName(typeof(ScaleBias));

            //================================================================
            //
            // ComponentInfoManager
            //

            var componentInfoManager = new ComponentInfoManager(typeRegistory);

            //================================================================
            //
            // ComponentBundleBuilder
            //

            var myNoise = new ScaleBias
            {
                Scale = 0.5f,
                Bias = 0.5f,
                Source = new SumFractal
                {
                    Source = new Perlin
                    {
                        Seed = 300,
                        FadeCurve = new SCurve5()
                    }
                }
            };

            var builder = new ComponentBundleBuilder(componentInfoManager);
            builder.Add("scaleBias", myNoise);

            //================================================================
            //
            // Serialization
            //

            ComponentBundleDefinition definition;
            builder.BuildDefinition(out definition);

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
            // Exit
            //

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();
        }
    }
}
