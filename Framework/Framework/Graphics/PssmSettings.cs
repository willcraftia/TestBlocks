#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class PssmSettings
    {
        public const int MinSplitCount = 1;

        public const int MaxSplitCount = 7;

        public const int DefaultSplitCount = 3;

        public const float DefaultSplitLambda = 0.7f;

        int splitCount = DefaultSplitCount;

        float splitLambda = DefaultSplitLambda;

        /// <summary>
        /// 分割数。
        /// </summary>
        public int SplitCount
        {
            get { return splitCount; }
            set
            {
                if (value < MinSplitCount || MaxSplitCount < value) throw new ArgumentOutOfRangeException("value");

                splitCount = value;
            }
        }

        /// <summary>
        /// 分割ラムダ値。
        /// </summary>
        public float SplitLambda
        {
            get { return splitLambda; }
            set { splitLambda = value; }
        }
    }
}
