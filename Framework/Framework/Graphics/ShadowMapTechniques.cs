#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public enum ShadowMapTechniques
    {
        /// <summary>
        /// クラシック。
        /// </summary>
        Classic,

        /// <summary>
        /// VSM (Variant Shadow Mapping)。
        /// </summary>
        Vsm,

        /// <summary>
        /// PCF (Percentage Closer Filtering) 2x2 カーネル。
        /// </summary>
        Pcf2x2,

        /// <summary>
        /// PCF (Percentage Closer Filtering) 3x3 カーネル。
        /// </summary>
        Pcf3x3
    }
}
