#region Using

using System;
using Willcraftia.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class GraphicsSettings
    {
        public ShadowMap.Settings ShadowMap { get; private set; }

        public Sssm.Settings Sssm { get; private set; }

        public Ssao.Settings Ssao { get; private set; }

        public Edge.Settings Edge { get; private set; }

        public Bloom.Settings Bloom { get; private set; }

        public Dof.Settings Dof { get; private set; }

        public GraphicsSettings()
        {
            ShadowMap = new ShadowMap.Settings();
            Sssm = new Sssm.Settings();
            Ssao = new Ssao.Settings();
            Edge = new Edge.Settings();
            Bloom = new Bloom.Settings();
            Dof = new Dof.Settings();
        }
    }
}
