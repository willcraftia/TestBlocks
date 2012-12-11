#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Noise.Definitions.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var project = new NoiseProject();

            var perlinInfo = project.CreateNoiseSourceInfo("Perlin");
            project.Add("perlin", perlinInfo);
            project.SetParameter("perlin", "Seed", 300);

            var sumFractalInfo = project.CreateNoiseSourceInfo("SumFractal");
            project.Add("sumFractal", sumFractalInfo);
            project.SetParameter("sumFractal", "Hurst", 0.5f);
            project.SetParameter("sumFractal", "Frequency", 0.8f);
            project.SetParameter("sumFractal", "OctaveCount", 8);
            project.SetReference("sumFractal", "Source", "perlin");

            var scaleBias = project.CreateNoiseSourceInfo("ScaleBias");
            project.Add("scaleBias", scaleBias);
            project.SetParameter("scaleBias", "Scale", 0.5f);
            project.SetParameter("scaleBias", "Bias", 0.5f);
            project.SetReference("scaleBias", "Source", "sumFractal");

            project.RootName = "scaleBias";

            var noiseSource = project.RootSource;

            NoiseProjectDefinition projectDefinition;
            project.ToDefinition(out projectDefinition);

            var newProject = new NoiseProject();
            newProject.SetDefinition(ref projectDefinition);

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();
        }
    }
}
