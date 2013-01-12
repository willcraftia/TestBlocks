#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Willcraftia.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    /// <summary>
    /// ある時間における色を管理するクラスです。
    /// 時間は 0 を 0 時、1 を 24 時として [0, 1] で管理されます。
    /// [0, 1] にある時間を指定すると、その時間よりも前の時間で定義された色、
    /// および、その次の時間で定義された色の間で線形補間した色を取得できます。
    /// この仕組から、0 時および 24 時には必ず色を設定しなければなりません。
    /// </summary>
    public sealed class TimeColorCollection : IEnumerable<TimeColor>
    {
        public const int InitialCapacity = 10;

        List<TimeColor> entries = new List<TimeColor>(InitialCapacity);

        /// <summary>
        /// 登録されている時間色の数を取得します。
        /// </summary>
        public int Count
        {
            get { return entries.Count; }
        }

        // I/F
        public IEnumerator<TimeColor> GetEnumerator()
        {
            return entries.GetEnumerator();
        }

        // I/F
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return entries.GetEnumerator();
        }

        /// <summary>
        /// 時間色を追加します。
        /// </summary>
        /// <param name="timeColor"></param>
        public void AddColor(TimeColor timeColor)
        {
            var index = FindInsertionIndex(timeColor.Time);
            entries.Insert(index, timeColor);
        }

        /// <summary>
        /// 指定の時間に対する色を取得します。
        /// </summary>
        /// <param name="time">時間 ([0, 1])。</param>
        /// <returns>色。</returns>
        public Vector3 GetColor(float time)
        {
            Vector3 result;
            GetColor(time, out result);
            return result;
        }

        /// <summary>
        /// 指定の時間に対する色を取得します。
        /// </summary>
        /// <param name="time">時間 ([0, 1])。</param>
        /// <param name="result">色。</param>
        public void GetColor(float time, out Vector3 result)
        {
            int baseIndex = 0;
            for (baseIndex = 0; baseIndex < entries.Count; baseIndex++)
            {
                if (time < entries[baseIndex].Time) break;
            }

            if (entries.Count <= baseIndex)
            {
                result = Color.CornflowerBlue.ToVector3();
                return;
            }

            var index0 = MathExtension.Clamp(baseIndex - 1, 0, entries.Count);
            var index1 = MathExtension.Clamp(baseIndex, 0, entries.Count);

            if (index0 == index1)
            {
                result = entries[index1].Color;
                return;
            }

            var color0 = entries[index0].Color;
            var color1 = entries[index1].Color;
            var time0 = entries[index0].Time;
            var time1 = entries[index1].Time;
            var amount = (time - time0) / (time1 - time0);
            Vector3.Lerp(ref color0, ref color1, amount, out result);
        }

        int FindInsertionIndex(float time)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (time < entries[i].Time) return i;
                if (entries[i].Time == time) throw new ArgumentException("Time duplicated.");
            }
            return entries.Count;
        }
    }
}
