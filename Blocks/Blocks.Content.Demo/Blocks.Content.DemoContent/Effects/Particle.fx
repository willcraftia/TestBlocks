//-----------------------------------------------------------------------------
// ParticleEffect.fx
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
// カメラのパラメーター。
float4x4 View;
float4x4 Projection;
float2 ViewportScale;

// 現在の時刻 (秒単位)。
float CurrentTime;

// パーティクルのアニメーション表示方法を記述するパラメーター。
float Duration;
float DurationRandomness;
float3 Gravity;
float EndVelocity;
float4 MinColor;
float4 MaxColor;

// 範囲の最小値と最大値を記述する float2 パラメーター。
// 実際の値は、ランダムな値で x と y の間を補間するように
// 各パーティクルで別々に選択されます。
float2 RotateSpeed;
float2 StartSize;
float2 EndSize;

// パーティクル テクスチャとサンプラー。
texture Texture;
sampler Sampler = sampler_state
{
    Texture = (Texture);
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};

// 頂点シェーダーの入力構造体は、パーティクルの開始位置と速度、
// 作成時刻、およびサイズと回転に影響を与えるランダムな値を
// 記述します。
struct VertexShaderInput
{
    float2 Corner : POSITION0;
    float3 Position : POSITION1;
    float3 Velocity : NORMAL0;
    float4 Random : COLOR0;
    float Time : TEXCOORD0;
};

// 頂点シェーダーの出力フォーマットはパーティクルの位置と色を示します。
struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TextureCoordinate : COLOR1;
};

// パーティクルの位置を計算する頂点シェーダー ヘルパー。
float4 ComputeParticlePosition(float3 position, float3 velocity,
                               float age, float normalizedAge)
{
    float startVelocity = length(velocity);

    // 一定のスケール ファクターを開始速度に適用して、存続期間終了時の
    // パーティクルの移動速度を割り出します。
    float endVelocity = startVelocity * EndVelocity;

    // パーティクルは一定のアクセラレーション (加速度) を持つので、
    // 開始速度を S 、終了速度を E とすると、時刻 T における速度は 
    // S + (E-S)*T となります。パーティクルの位置は、0 ～ T の範囲における
    // 速度の合計です。位置を直接計算するには、速度の式を積分する必要が
    // あります。S + (E-S)*T を T で積分すると S*T + (E-S)*T*T/2 となります。

    float velocityIntegral = startVelocity * normalizedAge + (endVelocity - startVelocity) * normalizedAge * normalizedAge / 2;

    position += normalize(velocity) * velocityIntegral * Duration;

    // 重力を適用します。
    position += Gravity * age * normalizedAge;

    // カメラ ビューと射影変換を適用します。
    return mul(mul(float4(position, 1), View), Projection);
}

// パーティクルのサイズを計算する頂点シェーダー ヘルパー。
float ComputeParticleSize(float randomValue, float normalizedAge)
{
    // 乱数の値を適用して、各パーティクルをわずかに異なるサイズにします。
    float startSize = lerp(StartSize.x, StartSize.y, randomValue);
    float endSize = lerp(EndSize.x, EndSize.y, randomValue);

    // パーティクルの世代に基づいて実際のサイズを計算します。
    float size = lerp(startSize, endSize, normalizedAge);

    // スクリーン座標にサイズを射影します。
    return size * Projection._m11;
}

// パーティクルのカラーを計算する頂点シェーダー ヘルパー。
float4 ComputeParticleColor(float4 projectedPosition,
                            float randomValue, float normalizedAge)
{
    // 乱数の値を適用して、各パーティクルをわずかに異なるカラーにします。
    float4 color = lerp(MinColor, MaxColor, randomValue);

    // パーティクルの世代に基づいてアルファを減衰させます。この曲線は
    // ハードコーディングされています。パーティクルは瞬時にフェードインし、
    // ゆっくりとフェードアウトします。この様子を確認するには、グラフ作成の
    // プログラムで x*(1-x)*(1-x) を x=0 ～ 1 でプロットしてください。
    // スケーリング ファクターの 6.7 は曲線を正規化するので、アルファは最終的に
    // 完全にソリッドになります。

    color.a *= normalizedAge * (1-normalizedAge) * (1-normalizedAge) * 6.7;

    return color;
}

// パーティクルの回転を計算する頂点シェーダー ヘルパー。
float2x2 ComputeParticleRotation(float randomValue, float age)
{    
    // 乱数の値を適用して、各パーティクルを異なる速度で回転させます。
    float rotateSpeed = lerp(RotateSpeed.x, RotateSpeed.y, randomValue);
    
    float rotation = rotateSpeed * age;

    // 2x2 回転行列を計算します。
    float c = cos(rotation);
    float s = sin(rotation);
    
    return float2x2(c, -s, s, c);
}

// カスタムの頂点シェーダーによって、GPU 上ですべてのパーティクルをアニメーション表示します。
VertexShaderOutput ParticleVertexShader(VertexShaderInput input)
{
    VertexShaderOutput output;

    // パーティクルの世代を計算します。
    float age = CurrentTime - input.Time;

    // 乱数の値を適用して、それぞれのパーティクルの世代を異なる進度にします。
    age *= 1 + input.Random.x * DurationRandomness;

    // 世代を 0 ～ 1 の範囲に正規化します。
    float normalizedAge = saturate(age / Duration);

    // パーティクルの位置、サイズ、カラー、および回転を計算します。
    output.Position = ComputeParticlePosition(input.Position, input.Velocity,
                                              age, normalizedAge);

    float size = ComputeParticleSize(input.Random.y, normalizedAge);
    float2x2 rotation = ComputeParticleRotation(input.Random.w, age);

    output.Position.xy += mul(input.Corner, rotation) * size * ViewportScale;

    output.Color = ComputeParticleColor(output.Position, input.Random.z, normalizedAge);
    output.TextureCoordinate = (input.Corner + 1) / 2;

    return output;
}

// パーティクルを描画するピクセル シェーダー。
float4 ParticlePixelShader(VertexShaderOutput input) : COLOR0
{
    return tex2D(Sampler, input.TextureCoordinate) * input.Color;
}

// パーティクルを描画するためのエフェクト テクニック。
technique Particles
{
    pass P0
    {
        VertexShader = compile vs_2_0 ParticleVertexShader();
        PixelShader = compile ps_2_0 ParticlePixelShader();
    }
}
