#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework
{
    /// <summary>
    /// 4 ビットの値を三次元配列として管理するクラスです。
    /// </summary>
    public sealed class HalfByteArray3
    {
        /// <summary>
        /// 要素を管理する配列。
        /// </summary>
        /// <remarks>
        /// 設定するバイト値は、上位 4 ビットと下位 4 ビットで異なるインデクスの値を参照します。
        /// </remarks>
        byte[] array;

        /// <summary>
        /// X 次元の要素の数。
        /// </summary>
        int lengthX;

        /// <summary>
        /// Y 次元の要素の数。
        /// </summary>
        int lengthY;

        /// <summary>
        /// Z 次元の要素の数。
        /// </summary>
        int lengthZ;

        /// <summary>
        /// 配列の半分のサイズ。
        /// </summary>
        int halfSize;

        /// <summary>
        /// X 次元の要素の数を取得します。
        /// </summary>
        public int LengthX
        {
            get { return lengthX; }
        }

        /// <summary>
        /// Y 次元の要素の数を取得します。
        /// </summary>
        public int LengthY
        {
            get { return lengthY; }
        }

        /// <summary>
        /// Z 次元の要素の数を取得します。
        /// </summary>
        public int LengthZ
        {
            get { return lengthZ; }
        }

        /// <summary>
        /// 値を取得または設定します。
        /// </summary>
        /// <param name="x">X 次元のインデックス。</param>
        /// <param name="y">Y 次元のインデックス。</param>
        /// <param name="z">Z 次元のインデックス。</param>
        /// <returns>4 ビットのみ有効なバイト値。</returns>
        public byte this[int x, int y, int z]
        {
            get
            {
                if (x < 0 || lengthX <= x) throw new ArgumentOutOfRangeException("x");
                if (y < 0 || lengthY <= y) throw new ArgumentOutOfRangeException("y");
                if (z < 0 || lengthZ <= z) throw new ArgumentOutOfRangeException("z");

                var index = x + (y * lengthX) + (z * lengthY * lengthX);

                if (index < halfSize)
                {
                    return (byte) (array[index] & 0x0F);
                }
                else
                {
                    index %= halfSize;
                    return (byte) (array[index] >> 4);
                }
            }
            set
            {
                if (x < 0 || lengthX <= x) throw new ArgumentOutOfRangeException("x");
                if (y < 0 || lengthY <= y) throw new ArgumentOutOfRangeException("y");
                if (z < 0 || lengthZ <= z) throw new ArgumentOutOfRangeException("z");
                if (0x0F < value) throw new ArgumentOutOfRangeException("value");

                var index = x + (y * lengthX) + (z * lengthY * lengthX);

                if (index < halfSize)
                {
                    array[index] = (byte) ((array[index] & 0xF0) | value);
                }
                else
                {
                    index %= halfSize;
                    array[index] = (byte) ((value << 4) | (array[index] & 0x0F));
                }
            }
        }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        /// <param name="lengthX">X 次元の要素の数。</param>
        /// <param name="lengthY">Y 次元の要素の数。</param>
        /// <param name="lengthZ">Z 次元の要素の数。</param>
        public HalfByteArray3(int lengthX, int lengthY, int lengthZ)
        {
            this.lengthX = lengthX;
            this.lengthY = lengthY;
            this.lengthZ = lengthZ;

            var size = lengthX * lengthY * lengthZ;
            halfSize = size / 2;

            array = new byte[halfSize];
        }

        /// <summary>
        /// 全ての要素を 0 でリセットします。
        /// </summary>
        public void Clear()
        {
            Array.Clear(array, 0, array.Length);
        }

        public void Fill(byte value)
        {
            if (0x0F < value) throw new ArgumentOutOfRangeException("value");

            byte composition = (byte) ((value << 4) | value);
            for (int i = 0; i < array.Length; i++)
                array[i] = composition;
        }
    }
}
