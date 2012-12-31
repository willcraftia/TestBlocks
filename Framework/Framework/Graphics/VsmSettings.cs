#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class VsmSettings
    {
        /// <summary>
        /// ブラー設定を取得します。
        /// </summary>
        public BlurSettings Blur { get; private set; }

        public VsmSettings()
        {
            Blur = new BlurSettings();
        }
    }
}
