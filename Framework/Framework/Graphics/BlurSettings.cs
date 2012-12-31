#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class BlurSettings
    {
        public const bool DefaultEnabled = false;

        public const int DefaultRadius = 1;

        public const float DefaultAmount = 1;

        bool enabled = DefaultEnabled;

        int radius = DefaultRadius;

        float amount = DefaultAmount;

        /// <summary>
        /// 有効かどうかを示す値を取得または設定します。
        /// </summary>
        /// <value>
        /// true (ブラーが有効な場合)、false (それ以外の場合)。
        /// </value>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        /// <summary>
        /// 適用半径を取得または設定します。
        /// </summary>
        public int Radius
        {
            get { return radius; }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException("value");

                radius = value;
            }
        }

        /// <summary>
        /// 適用量を取得または設定します。
        /// </summary>
        public float Amount
        {
            get { return amount; }
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException("value");

                amount = value;
            }
        }
    }
}
