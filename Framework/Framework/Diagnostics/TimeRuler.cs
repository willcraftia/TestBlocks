#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Diagnostics
{
    /// <summary>
    /// タイム ルーラを表示するクラスです。
    /// </summary>
    public sealed class TimeRuler : DrawableGameComponent
    {
        #region MarkerLog

        /// <summary>
        /// マーカの計測記録を表す構造体です。
        /// </summary>
        struct MarkerLog
        {
            /// <summary>
            /// ID。
            /// </summary>
            public int Id;

            /// <summary>
            /// 開始時間。
            /// </summary>
            public float BeginTime;

            /// <summary>
            /// 終了時間。
            /// </summary>
            public float EndTime;

            /// <summary>
            /// 色。
            /// </summary>
            public Color Color;
        }

        #endregion

        #region Bar

        /// <summary>
        /// バーを表すクラスです。
        /// </summary>
        class Bar
        {
            /// <summary>
            /// MarkerLogs のリスト。
            /// </summary>
            public MarkerLog[] MarkerLogs = new MarkerLog[MaxBarMarkerCount];

            /// <summary>
            /// 有効になっている MarkerLogs の数。
            /// </summary>
            public int MarkerLogCount;

            /// <summary>
            /// ネストしている MarkerLog のインデックスのリスト。
            /// </summary>
            public int[] MarkerLogIndexStack = new int[MaxBarMerkerNestCount];

            /// <summary>
            /// 有効になっている MarkerLogIndexStack の数。
            /// </summary>
            public int MarkerLogIndexStackCount;
        }

        #endregion

        #region FrameLog

        /// <summary>
        /// フレームごとの記録を表すクラスです。
        /// </summary>
        class FrameLog
        {
            /// <summary>
            /// バーのリスト。
            /// </summary>
            public List<Bar> Bars = new List<Bar>();

            /// <summary>
            /// インスタンスを生成します。
            /// </summary>
            public FrameLog()
            {
                for (int i = 0; i < MaxBarCount; i++) Bars.Add(new Bar());
            }
        }

        #endregion

        /// <summary>
        /// 利用可能なバーの最大数。
        /// </summary>
        public const int MaxBarCount = 10;

        /// <summary>
        /// バーに関連付けられるマーカの総数。
        /// </summary>
        public const int MaxBarMarkerCount = 256;

        /// <summary>
        /// バーでネストできるマーカの総数。
        /// </summary>
        public const int MaxBarMerkerNestCount = 32;

        /// <summary>
        /// Stopwatch。
        /// </summary>
        Stopwatch stopwatch = Stopwatch.StartNew();

        /// <summary>
        /// マーカに割り当てる ID のカウンタ。
        /// </summary>
        int markerIdCounter;

        /// <summary>
        /// ID をキーにマーカを値として関連付けるディクショナリ。
        /// </summary>
        Dictionary<int, TimeRulerMarker> markers = new Dictionary<int, TimeRulerMarker>();

        /// <summary>
        /// 前回のフレームの記録。
        /// </summary>
        FrameLog previousFrameLog = new FrameLog();

        /// <summary>
        /// 現在のフレームの記録。
        /// </summary>
        FrameLog currentFrameLog = new FrameLog();

        /// <summary>
        /// 塗り潰し用テクスチャ。
        /// </summary>
        Texture2D fillTexture;

        /// <summary>
        /// 表示領域の幅 (LoadContent メソッドで自動的に決定)。
        /// </summary>
        int width;

        /// <summary>
        /// 表示領域の X 座標 (LoadContent メソッドで自動的に決定)。
        /// </summary>
        int offsetX;

        /// <summary>
        /// 表示領域の Bottom ラインの Y 座標 (LoadContent メソッドで自動的に決定)。
        /// </summary>
        int offsetY;

        /// <summary>
        /// 計測の基準となったフレーム数 (処理速度に応じて変化)。
        /// </summary>
        int sampleFrames;

        /// <summary>
        /// 処理の遅延の発生、あるいは、遅延の解消の度合いを表す値。
        /// </summary>
        int frameAdjust;

        /// <summary>
        /// SpriteBatch。
        /// </summary>
        SpriteBatch spriteBatch;

        /// <summary>
        /// バーを表示するかどうかを示す値を取得または設定します。
        /// </summary>
        /// <value>
        /// true (バーを表示する場合)、false (それ以外の場合)。
        /// </value>
        public bool BarVisible { get; set; }

        /// <summary>
        /// バーの垂直方向の余白を取得または設定します。
        /// </summary>
        public int BarPadding { get; set; }

        /// <summary>
        /// 対象とする FPS を取得または設定します。
        /// </summary>
        public int Fps { get; set; }

        /// <summary>
        /// 最大の基準フレーム数を取得または設定します。
        /// </summary>
        public int MaxSampleFrames { get; set; }

        /// <summary>
        /// 処理の遅延の発生、あるいは、遅延の解消について、
        /// 何回それらを検出したら基準フレーム数を調整するかを示す値を取得または設定します。
        /// </summary>
        public int AutoAdjustDelay { get; set; }

        /// <summary>
        /// 基準フレーム数を取得または設定します。
        /// </summary>
        public int TargetSampleFrames { get; set; }

        /// <summary>
        /// 背景色を取得または設定します。
        /// </summary>
        public Color BackgroundColor { get; set; }

        /// <summary>
        /// ミリ秒単位のグリッド色を取得または設定します。
        /// </summary>
        public Color MillisecondGridColor { get; set; }

        /// <summary>
        /// フレーム単位のグリッド色を取得または設定します。
        /// </summary>
        public Color FrameGridColor { get; set; }

        /// <summary>
        /// バーの高さを取得または設定します。
        /// </summary>
        public int BarHeight { get; set; }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        /// <param name="game">Game。</param>
        public TimeRuler(Game game)
            : base(game)
        {
            BackgroundColor = Color.Black * 0.5f;
            MillisecondGridColor = Color.Gray;
            FrameGridColor = Color.White;
            BarHeight = 4;
            BarVisible = true;
            BarPadding = 1;
            Fps = 60;
            MaxSampleFrames = 4;
            AutoAdjustDelay = 30;
            TargetSampleFrames = 1;
        }

        public TimeRulerMarker CreateMarker()
        {
            int id = ++markerIdCounter;

            // マーカを登録。
            var marker = new TimeRulerMarker(this, id);
            markers[id] = marker;

            return marker;
        }

        public void ReleaseMarker(TimeRulerMarker marker)
        {
            if (marker == null) throw new ArgumentNullException("marker");

            // 登録を解除。
            markers.Remove(marker.Id);
        }

        [Conditional("DEBUG"), Conditional("TRACE")]
        public void StartFrame()
        {
            // currentFrameLog の同期をとるために自身をロック。
            lock (this)
            {
                // previousFrameLog と currentFrameLog を入れ替え。
                var tempFrameLog = previousFrameLog;
                previousFrameLog = currentFrameLog;
                currentFrameLog = tempFrameLog;

                // フレーム終了時間を取得。
                var endFrameTime = (float) stopwatch.Elapsed.TotalMilliseconds;

                for (int barIndex = 0; barIndex < previousFrameLog.Bars.Count; barIndex++)
                {
                    var previousBar = previousFrameLog.Bars[barIndex];
                    var currentBar = currentFrameLog.Bars[barIndex];

                    currentBar.MarkerLogCount = 0;
                    currentBar.MarkerLogIndexStackCount = 0;

                    // End が呼ばれないままの MarkerLog を終了させ、新フレームで継続。
                    for (int stackIndex = 0; stackIndex < previousBar.MarkerLogIndexStackCount; stackIndex++)
                    {
                        int markerIndex = previousBar.MarkerLogIndexStack[stackIndex];

                        // 終了時間を設定。
                        previousBar.MarkerLogs[markerIndex].EndTime = endFrameTime;

                        // 新フレームへ継続。
                        currentBar.MarkerLogIndexStack[stackIndex] = stackIndex;
                        currentBar.MarkerLogIndexStackCount++;

                        currentBar.MarkerLogs[stackIndex].Id = previousBar.MarkerLogs[markerIndex].Id;
                        currentBar.MarkerLogs[stackIndex].BeginTime = 0;
                        currentBar.MarkerLogs[stackIndex].EndTime = float.NaN;
                        currentBar.MarkerLogs[stackIndex].Color = previousBar.MarkerLogs[markerIndex].Color;
                        currentBar.MarkerLogCount++;
                    }
                }

                // このフレームでの時間計測をリセット。
                stopwatch.Reset();
                stopwatch.Start();
            }
        }

        public override void Draw(GameTime gameTime)
        {
            // 表示の高さ。
            int height = 0;

            // 最大の終了時間。
            float maxEndTime = 0;

            // 表示対象となるバーの数に応じて、表示サイズと位置を決定。
            foreach (var bar in previousFrameLog.Bars)
            {
                // バーが可視で、マーカを持つならば表示対象。
                if (BarVisible && bar.MarkerLogCount != 0)
                {
                    // バーの高さと余白の分だけ表示の高さを積み上げ。
                    height += BarHeight + BarPadding * 2;

                    // 最後に Begin した MarkerLog の終了時間と比較して最大のものを取得。
                    maxEndTime = Math.Max(maxEndTime, bar.MarkerLogs[bar.MarkerLogCount - 1].EndTime);
                }
            }

            // 1 フレームにかかる基準時間 (ミリ秒) を計算。
            float frameSpan = 1000f / (float) Fps;
            // 基準フレーム数にかかる基準時間を計算。
            float sampleFrameSpan = (float) sampleFrames * frameSpan;

            if (sampleFrameSpan < maxEndTime)
            {
                // 基準時間よりも遅れているならば、遅延発生をカウントアップ。
                frameAdjust = Math.Max(0, frameAdjust) + 1;
            }
            else
            {
                // 基準時間から遅れていないならば、遅延解消をカウントダウン。
                frameAdjust = Math.Min(0, frameAdjust) - 1;
            }

            // 指定回数を越える遅延発生や遅延解消があるならば、基準フレーム数を増減。
            if (AutoAdjustDelay < Math.Abs(frameAdjust))
            {
                sampleFrames = Math.Min(MaxSampleFrames, sampleFrames);
                sampleFrames = Math.Max(TargetSampleFrames, (int) (maxEndTime / frameSpan) + 1);

                // 基準フレーム数を調整したのでカウンタをリセット。
                frameAdjust = 0;
            }

            // 1 ミリ秒を表すピクセル数を計算。
            float pixelPerMillisecond = (float) width / sampleFrameSpan;

            // 描画開始位置: Y 座標。
            int startY = offsetY - height;

            spriteBatch.Begin();

            if (BarVisible)
            {
                int y = startY;

                // 背景領域を描画。
                spriteBatch.Draw(fillTexture, new Rectangle(offsetX, y, width, height), BackgroundColor);

                // 各バーを描画。
                var barRectangle = new Rectangle();
                barRectangle.Height = BarHeight;
                foreach (var bar in previousFrameLog.Bars)
                {
                    // MarkerLog がないバーはスキップ。
                    if (bar.MarkerLogCount == 0) continue;

                    barRectangle.Y = y + BarPadding;

                    for (int i = 0; i < bar.MarkerLogCount; i++)
                    {
                        int barStartX = (int) (offsetX + bar.MarkerLogs[i].BeginTime * pixelPerMillisecond);
                        int barEndX = (int) (offsetX + bar.MarkerLogs[i].EndTime * pixelPerMillisecond);
                        barRectangle.X = barStartX;
                        barRectangle.Width = Math.Max(barEndX - barStartX, 1);

                        spriteBatch.Draw(fillTexture, barRectangle, bar.MarkerLogs[i].Color);
                    }

                    y += BarHeight + BarPadding;
                }

                // ミリ秒単位のグリッドを描画。
                var msGridRectangle = new Rectangle(offsetX, startY, 1, height);
                for (float ms = 1.0f; ms < sampleFrameSpan; ms += 1.0f)
                {
                    msGridRectangle.X = (int) (offsetX + ms * pixelPerMillisecond);
                    spriteBatch.Draw(fillTexture, msGridRectangle, MillisecondGridColor);
                }

                // フレーム単位のグリッドを描画。
                var frameGridRectangle = new Rectangle(offsetX, startY, 1, height);
                for (int i = 0; i < sampleFrames; i++)
                {
                    frameGridRectangle.X = (int) (offsetX + frameSpan * (float) i * pixelPerMillisecond);
                    spriteBatch.Draw(fillTexture, frameGridRectangle, FrameGridColor);
                }
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        protected override void LoadContent()
        {
            var titleSafeArea = GraphicsDevice.Viewport.TitleSafeArea;

            // 表示幅を TitleSafeArea から余白を考慮した値に設定。
            width = titleSafeArea.Width - 16;

            // 表示位置を計算。
            // 高さはバーの数で変動するため、Bottom 合わせで固定し、Bottom ラインをベースに描画時に調整。
            var layout = new DebugLayout()
            {
                ContainerBounds = titleSafeArea,
                Width = width,
                Height = 0,
                HorizontalMargin = 8,
                VerticalMargin = 8,
                HorizontalAlignment = DebugHorizontalAlignment.Center,
                VerticalAlignment = DebugVerticalAlignment.Bottom
            };
            layout.Arrange();

            offsetX = layout.ArrangedBounds.X;
            offsetY = layout.ArrangedBounds.Y;

            spriteBatch = new SpriteBatch(GraphicsDevice);
            fillTexture = Texture2DHelper.CreateFillTexture(GraphicsDevice);

            base.LoadContent();
        }

        protected override void UnloadContent()
        {
            if (spriteBatch != null) spriteBatch.Dispose();
            if (fillTexture != null) fillTexture.Dispose();

            base.UnloadContent();
        }

        /// <summary>
        /// 指定されたマーカに対する計測を開始させます。
        /// </summary>
        /// <param name="marker">計測を開始させるマーカ。</param>
        internal void Begin(TimeRulerMarker marker)
        {
            // currentFrameLog の同期をとるために自身をロック。
            lock (this)
            {
                // 対象のバーを取得。
                var bar = currentFrameLog.Bars[marker.BarIndex];

                // MarkerLog を追加。
                bar.MarkerLogs[bar.MarkerLogCount].Id = marker.Id;
                bar.MarkerLogs[bar.MarkerLogCount].BeginTime = (float) stopwatch.Elapsed.TotalMilliseconds;
                bar.MarkerLogs[bar.MarkerLogCount].EndTime = float.NaN;
                bar.MarkerLogs[bar.MarkerLogCount].Color = marker.Color;

                // MarkerLog のインデックスを呼び出しスタックに追加。
                bar.MarkerLogIndexStack[bar.MarkerLogIndexStackCount++] = bar.MarkerLogCount;

                // MarkerLog 数を増やす。
                bar.MarkerLogCount++;
            }
        }

        /// <summary>
        /// 指定されたマーカに対する計測を終了させます。
        /// </summary>
        /// <param name="marker">計測を終了させるマーカ。</param>
        internal void End(TimeRulerMarker marker)
        {
            // currentFrameLog の同期をとるために自身をロック。
            lock (this)
            {
                // 対象のバーを取得。
                var bar = currentFrameLog.Bars[marker.BarIndex];

                // 呼び出しスタックから POP して MarkerLog のインデックスを取得。
                var markerLogIndex = bar.MarkerLogIndexStack[--bar.MarkerLogIndexStackCount];

                // インデックスの示す MarkerLog と ID が一致しないならば不正な順序での呼び出し。
                if (bar.MarkerLogs[markerLogIndex].Id != marker.Id)
                    throw new InvalidOperationException("Invalid Begin/End call sequence.");

                // 終了時間を記録。
                bar.MarkerLogs[markerLogIndex].EndTime = (float) stopwatch.Elapsed.TotalMilliseconds;
            }
        }
    }
}
