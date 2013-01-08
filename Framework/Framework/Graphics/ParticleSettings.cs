#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    /// <summary>
    /// パーティクル システムの外観の制御に使用する調整可能なオプションを表すクラスです。
    /// </summary>
    public sealed class ParticleSettings
    {
        /// <summary>
        /// パーティクル システム名。
        /// </summary>
        public string Name = string.Empty;

        /// <summary>
        /// 一度に表示可能なパーティクルの最大数。
        /// </summary>
        public int MaxParticles = 100;

        /// <summary>
        /// パーティクルの存続期間。
        /// </summary>
        public TimeSpan Duration = TimeSpan.FromSeconds(1);

        /// <summary>
        /// ゼロよりも大きい場合、他のパーティクルよりも存続期間の短いパーティクルが存在します。
        /// </summary>
        public float DurationRandomness = 0;

        /// <summary>
        /// 作成元であるオブジェクトの速度に影響を受けるパーティクルの数を制御します。
        /// この動作は、爆発エフェクトとともに見ることができます。
        /// その際、炎はソースの発射体と同じ方向への移動を続けます。
        /// 一方、発射体のトレール パーティクルはこの値を極めて低く設定するので、発射体の速度による影響は少なくなります。
        /// </summary>
        public float EmitterVelocitySensitivity = 1;

        /// <summary>
        /// 各パーティクルに与える X 軸と Z 軸の速度を制御する値の範囲の最小値。
        /// </summary>
        public float MinHorizontalVelocity = 0;

        /// <summary>
        /// 各パーティクルに与える X 軸と Z 軸の速度を制御する値の範囲の最大値。
        /// </summary>
        public float MaxHorizontalVelocity = 0;

        /// <summary>
        /// 各パーティクルに与える Y 軸の速度を制御する値の範囲の最小値。
        /// </summary>
        public float MinVerticalVelocity = 0;

        /// <summary>
        /// 各パーティクルに与える Y 軸の速度を制御する値の範囲の最大値。
        /// </summary>
        public float MaxVerticalVelocity = 0;

        /// <summary>
        /// 重力エフェクトの方向と強さ。
        /// </summary>
        public Vector3 Gravity = Vector3.Zero;

        /// <summary>
        /// 存続期間中にパーティクルの速度がどのように変化するかを制御します。
        /// 1 に設定すると、パーティクルは作成時と同じ速度を維持します。
        /// 0 に設定すると、パーティクルは存続期間の終了時に完全に停止します。
        /// 1 よりも大きい値では、パーティクルの速度は時間経過とともに上昇します。
        /// </summary>
        public float EndVelocity = 1;

        /// <summary>
        /// パーティクルのカラーとアルファを制御する値の範囲の最小値。
        /// </summary>
        public Color MinColor = Color.White;

        /// <summary>
        /// パーティクルのカラーとアルファを制御する値の範囲の最大値。
        /// </summary>
        public Color MaxColor = Color.White;

        /// <summary>
        /// パーティクルの回転速度を制御する値の範囲の最小値。
        /// </summary>
        public float MinRotateSpeed = 0;

        /// <summary>
        /// パーティクルの回転速度を制御する値の範囲の最大値。
        /// </summary>
        public float MaxRotateSpeed = 0;

        /// <summary>
        /// 初回作成時のパーティクルの大きさを制御する値の範囲の最小値。
        /// </summary>
        public float MinStartSize = 100;

        /// <summary>
        /// 初回作成時のパーティクルの大きさを制御する値の範囲の最大値。
        /// </summary>
        public float MaxStartSize = 100;

        /// <summary>
        /// 存続期間終了時のパーティクルの大きさを制御する値の範囲の最小値。
        /// </summary>
        public float MinEndSize = 100;

        /// <summary>
        /// 存続期間終了時のパーティクルの大きさを制御する値の範囲の最大値。
        /// </summary>
        public float MaxEndSize = 100;

        /// <summary>
        /// アルファ ブレンディングの設定。
        /// </summary>
        public BlendState BlendState = BlendState.NonPremultiplied;
    }
}
