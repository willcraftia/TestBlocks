#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    /// <summary>
    /// Settings クラスには、パーティクル システムの外観の制御に使用する
    /// 調整可能なオプションがすべて記述されています。
    /// </summary>
    public class ParticleSettings
    {
        // 一度に表示可能なパーティクルの最大数。
        public int MaxParticles = 100;

        // パーティクルの存続期間。
        public TimeSpan Duration = TimeSpan.FromSeconds(1);

        // ゼロよりも大きい場合、他のパーティクルよりも存続期間の短いパーティクルが存在します。
        public float DurationRandomness = 0;

        // 作成元であるオブジェクトの速度に影響を受けるパーティクルの数を
        // 制御します。この動作は、爆発エフェクトとともに見ることができます。
        // その際、炎はソースの発射体と同じ方向への移動を続けます。一方、
        // 発射体のトレール パーティクルはこの値を極めて低く設定するので、
        // 発射体の速度による影響は少なくなります。
        public float EmitterVelocitySensitivity = 1;

        // 各パーティクルに与える X 軸と Z 軸の速度を制御する値の範囲。
        // 個々のパーティクルに対する値は、これらの制限の間でランダムに
        // 選択されます。
        public float MinHorizontalVelocity = 0;
        public float MaxHorizontalVelocity = 0;

        // 各パーティクルに与える Y 軸の速度を制御する値の範囲。
        // 個々のパーティクルに対する値は、これらの制限の間で
        // ランダムに選択されます。
        public float MinVerticalVelocity = 0;
        public float MaxVerticalVelocity = 0;

        // 重力エフェクトの方向と強さ。これは下方向だけでなく、任意の方向に
        // 向けることができます。発射エフェクトではこれを上方向に向けて炎が上昇するように
        // しています。また、煙の立ち上がりでは横方向にして風をシミュレートしています。
        public Vector3 Gravity = Vector3.Zero;

        // 存続期間中にパーティクルの速度がどのように変化するかを制御します。
        // 1 に設定すると、パーティクルは作成時と同じ速度を維持します。
        // 0 に設定すると、パーティクルは存続期間の終了時に完全に停止します。
        // 1 よりも大きい値では、パーティクルの速度は時間経過とともに上昇します。
        public float EndVelocity = 1;

        // パーティクルのカラーとアルファを制御する値の範囲。個々の
        // パーティクルに対する値は、これらの制限の間でランダムに選択されます。
        public Color MinColor = Color.White;
        public Color MaxColor = Color.White;

        // パーティクルの回転速度を制御する値の範囲。個々のパーティクルに対する値は、
        // これらの制限の間でランダムに選択されます。これら両方の値が 0 に設定されると、
        // パーティクル システムは回転をサポートしていない代替のシェーダー テクニックに
        // 自動的に切り替わるので、必要となる GPU パワーを大幅に削減できます。
        // これは、回転エフェクトが不要な場合はこれらの値を 0 のままにして、
        // パフォーマンスを向上させることができることを意味します。
        public float MinRotateSpeed = 0;
        public float MaxRotateSpeed = 0;

        // 初回作成時のパーティクルの大きさを制御する値の範囲。個々のパーティクルに対する値は、
        // これらの制限の間でランダムに選択されます。
        public float MinStartSize = 100;
        public float MaxStartSize = 100;

        // 存続期間終了時のパーティクルの大きさを制御する値の範囲。個々のパーティクルに
        // 対する値は、これらの制限の間でランダムに選択されます。
        public float MinEndSize = 100;
        public float MaxEndSize = 100;

        // アルファ ブレンディングの設定。
        public BlendState BlendState = BlendState.NonPremultiplied;
    }
}
