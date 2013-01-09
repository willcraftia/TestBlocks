#region Using

using System;
using Willcraftia.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class GraphicsSettings
    {
        /// <summary>
        /// シャドウ処理が有効かどうかを示す値を取得または設定します。
        /// </summary>
        /// <value>
        /// true (シャドウ処理が有効)、false (それ以外の場合)。
        /// </value>
        public bool ShadowMapEnabled { get; set; }

        public ShadowMap.Settings ShadowMap { get; private set; }

        /// <summary>
        /// スクリーン スペース シャドウ マッピングが有効か否かを示す値を取得または設定します。
        /// </summary>
        /// <value>
        /// true (スクリーン スペース シャドウ マッピングが有効な場合)、false (それ以外の場合)。
        /// </value>
        public bool SssmEnabled { get; set; }

        public Sssm.Settings Sssm { get; private set; }

        /// <summary>
        /// スクリーン スペース アンビエント オクルージョンが有効か否かを示す値を取得または設定します。
        /// </summary>
        /// <value>
        /// true (スクリーン スペース アンビエント オクルージョンが有効な場合)、false (それ以外の場合)。
        /// </value>
        public bool SsaoEnabled { get; set; }

        public Ssao.Settings Ssao { get; private set; }

        /// <summary>
        /// エッジ強調が有効か否かを示す値を取得または設定します。
        /// </summary>
        /// <value>
        /// true (エッジ強調が有効な場合)、false (それ以外の場合)。
        /// </value>
        public bool EdgeEnabled { get; set; }

        public Edge.Settings Edge { get; private set; }

        /// <summary>
        /// ブルームが有効か否かを示す値を取得または設定します。
        /// </summary>
        /// <value>
        /// true (ブルームが有効な場合)、false (それ以外の場合)。
        /// </value>
        public bool BloomEnabled { get; set; }

        public Bloom.Settings Bloom { get; private set; }

        /// <summary>
        /// 被写界深度が有効か否かを示す値を取得または設定します。
        /// </summary>
        /// <value>
        /// true (被写界深度が有効な場合)、false (それ以外の場合)。
        /// </value>
        public bool DofEnabled { get; set; }

        public Dof.Settings Dof { get; private set; }

        /// <summary>
        /// カラー オーバラップが有効か否かを示す値を取得または設定します。
        /// </summary>
        /// <value>
        /// true (カラー オーバラップが有効な場合)、false (それ以外の場合)。
        /// </value>
        public bool ColorOverlapEnabled { get; set; }

        /// <summary>
        /// モノクロームが有効か否かを示す値を取得または設定します。
        /// </summary>
        /// <value>
        /// true (モノクロームが有効な場合)、false (それ以外の場合)。
        /// </value>
        public bool MonochromeEnabled { get; set; }

        /// <summary>
        /// レンズ フレアが有効か否かを示す値を取得または設定します。
        /// </summary>
        /// <value>
        /// true (レンズ フレアが有効な場合)、false (それ以外の場合)。
        /// </value>
        public bool LensFlareEnabled { get; set; }

        public GraphicsSettings()
        {
            ShadowMap = new ShadowMap.Settings();
            Sssm = new Sssm.Settings();
            Ssao = new Ssao.Settings();
            Edge = new Edge.Settings();
            Bloom = new Bloom.Settings();
            Dof = new Dof.Settings();
        }
    }
}
