#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Willcraftia.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class SkyColorTable : IEnumerable<SkyColor>
    {
        public const int InitialCapacity = 10;

        List<SkyColor> entries = new List<SkyColor>(InitialCapacity);

        public int Count
        {
            get { return entries.Count; }
        }

        // I/F
        public IEnumerator<SkyColor> GetEnumerator()
        {
            return entries.GetEnumerator();
        }

        // I/F
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return entries.GetEnumerator();
        }

        public void AddColor(SkyColor skyColor)
        {
            var index = FindInsertionIndex(skyColor.Time);
            entries.Insert(index, skyColor);
        }

        public Vector3 GetColor(float time)
        {
            Vector3 result;
            GetColor(time, out result);
            return result;
        }

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
