#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class SceneSettings
    {
        /// <summary>
        /// 影設定。
        /// </summary>
        public ShadowSettings Shadow { get; private set; }

        public SceneSettings()
        {
            Shadow = new ShadowSettings();
        }
    }
}
