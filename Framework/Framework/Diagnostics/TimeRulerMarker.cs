#region Using

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework.Diagnostics
{
    /// <summary>
    /// タイム ルーラの計測要素 (マーカ) を定義するクラスです。
    /// </summary>
    public sealed class TimeRulerMarker
    {
        /// <summary>
        /// タイム ルーラ。
        /// </summary>
        TimeRuler timeRuler;

        /// <summary>
        /// タイム ルーラに割り当てられた ID を取得します。
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// 名前を取得または設定します。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 関連付けるバーのインデックスを取得または設定します。
        /// </summary>
        public int BarIndex { get; set; }

        /// <summary>
        /// 色を取得または設定します。
        /// </summary>
        public Color Color { get; set; }

        public float MinTime { get; private set; }

        public float MaxTime { get; private set; }

        public float AverageTime { get; private set; }

        public float SnapMinTime { get; private set; }

        public float SnapMaxTime { get; private set; }

        public float SnapAverageTime { get; private set; }

        public int SnapSamples { get; private set; }

        public bool SnapInitialized { get; private set; }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        /// <param name="timeRuler">タイム ルーラ。</param>
        /// <param name="id">タイム ルーラに割り当てられた ID。</param>
        internal TimeRulerMarker(TimeRuler timeRuler, int id)
        {
            this.timeRuler = timeRuler;
            Id = id;
        }

        /// <summary>
        /// 計測を開始します。
        /// </summary>
        [Conditional("DEBUG"), Conditional("TRACE")]
        public void Begin()
        {
            timeRuler.Begin(this);
        }

        /// <summary>
        /// 計測を終了します。
        /// </summary>
        [Conditional("DEBUG"), Conditional("TRACE")]
        public void End()
        {
            timeRuler.End(this);
        }

        [Conditional("DEBUG"), Conditional("TRACE")]
        public void RecordFrame(float beginTime, float endTime)
        {
            float duration = endTime - beginTime;

            if (!SnapInitialized)
            {
                MinTime = duration;
                MaxTime = duration;
                AverageTime = duration;

                SnapInitialized = true;
            }
            else
            {
                MinTime = Math.Min(MinTime, duration);
                MaxTime = Math.Max(MaxTime, duration);
                AverageTime += duration;
                AverageTime *= 0.5f;

                if (timeRuler.LogSnapDuration <= ++SnapSamples)
                {
                    SnapMinTime = MinTime;
                    SnapMaxTime = MaxTime;
                    SnapAverageTime = AverageTime;
                    SnapSamples = 0;
                }
            }
        }
    }
}
