#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class SceneManagerSettings
    {
        /// <summary>
        /// 影設定。
        /// </summary>
        public ShadowSettings Shadow { get; private set; }

        public DofSettings Dof { get; private set; }

        public EdgeSettings Edge { get; private set; }

        public SceneManagerSettings()
        {
            Shadow = new ShadowSettings();
            Dof = new DofSettings();
            Edge = new EdgeSettings();
        }
    }
}
