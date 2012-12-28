#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public enum ShadowTests
    {
        /// <summary>
        /// クラシックな検査。
        /// </summary>
        Classic,

        /// <summary>
        /// VSM (Variant Shadow Mapping)。
        /// </summary>
        Vsm,

        /// <summary>
        /// PCF (Percentage Closer Filtering)。
        /// </summary>
        Pcf
    }
}
