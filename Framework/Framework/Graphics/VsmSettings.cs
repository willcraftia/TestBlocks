#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class VsmSettings
    {
        public const bool DefaultBlurEnabled = true;

        public const int DefaultBlurRadius = 1;

        public const float DefaultBlurAmount = 1;

        bool blurEnabled = DefaultBlurEnabled;

        int blurRadius = DefaultBlurRadius;

        float blurAmount = DefaultBlurAmount;

        /// <summary>
        /// ブラーが有効かどうか。
        /// true (ブラーが有効な場合)、false (それ以外の場合)。
        /// </summary>
        public bool BlurEnabled
        {
            get { return blurEnabled; }
            set { blurEnabled = value; }
        }

        /// <summary>
        /// ブラー適用半径。
        /// </summary>
        public int BlurRadius
        {
            get { return blurRadius; }
            set { blurRadius = value; }
        }

        /// <summary>
        /// ブラー適用量。
        /// </summary>
        public float BlurAmount
        {
            get { return blurAmount; }
            set { blurAmount = value; }
        }
    }
}
