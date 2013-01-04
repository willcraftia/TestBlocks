#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class LightFrustumSettings
    {
        public const LightFrustumTypes DefaultType = LightFrustumTypes.Pssm;

        /// <summary>
        /// ライト カメラの種類。
        /// </summary>
        public LightFrustumTypes Type = DefaultType;

        /// <summary>
        /// PSSM 設定。
        /// </summary>
        public PssmSettings Pssm { get; private set; }

        public LightFrustumSettings()
        {
            Pssm = new PssmSettings();
        }
    }
}
