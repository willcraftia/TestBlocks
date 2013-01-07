#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class EdgeSettings
    {
        public const float DefaultMapScale = 1;

        float mapScale = DefaultMapScale;

        /// <summary>
        /// 実スクリーンに対する法線深度マップのスケールを取得または設定します。
        /// </summary>
        public float MapScale
        {
            get { return mapScale; }
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException("value");

                mapScale = value;
            }
        }
    }
}
