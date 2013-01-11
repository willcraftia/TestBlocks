#include "Common.fxh"

//=============================================================================
//
// 変数宣言
//
//-----------------------------------------------------------------------------
float TotalStrength = 1;
float Strength = 1;
float RandomOffset = 18;
float Falloff = 0.00001f;
float Radius = 1;

#define SAMPLE_COUNT 10
float invSamples = 1 / (float) SAMPLE_COUNT;

#if SAMPLE_COUNT == 8
const float3 SampleSphere[8] =
{
    float3(0.24710192, 0.6445882, 0.033550154),
    float3(0.00991752, -0.21947019, 0.7196721),
    float3(0.25109035, -0.1787317, -0.011580509),
    float3(-0.08781511, 0.44514698, 0.56647956),
    float3(-0.011737816, -0.0643377, 0.16030222),
    float3(0.035941467, 0.04990871, -0.46533614),
    float3(-0.058801126, 0.7347013, -0.25399926),
    float3(-0.24799341, -0.022052078, -0.13399573)
};
#elif SAMPLE_COUNT == 10
const float3 SampleSphere[10] =
{
    float3(-0.010735935, 0.01647018, 0.0062425877),
    float3(-0.06533369, 0.3647007, -0.13746321),
    float3(-0.6539235, -0.016726388, -0.53000957),
    float3(0.40958285, 0.0052428036, -0.5591124),
    float3(-0.1465366, 0.09899267, 0.15571679),
    float3(-0.44122112, -0.5458797, 0.04912532),
    float3(0.03755566, -0.10961345, -0.33040273),
    float3(0.019100213, 0.29652783, 0.066237666),
    float3(0.8765323, 0.011236004, 0.28265962),
    float3(0.29264435, -0.40794238, 0.15964167)
};
#endif

texture NormalDepthMap;
sampler NormalDepthMapSampler = sampler_state
{
    Texture = <NormalDepthMap>;
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = None;
};

texture RandomNormalMap;
sampler RandomNormalMapSampler = sampler_state
{
    Texture = <RandomNormalMap>;
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = None;
    AddressU = Wrap;
    AddressV = Wrap;
};

//=============================================================================
//
// 頂点シェーダ
//
//-----------------------------------------------------------------------------
void VS(inout float4 position : POSITION0, inout float2 texCoord : TEXCOORD0)
{
}

//=============================================================================
//
// ピクセル シェーダ
//
//-----------------------------------------------------------------------------
float4 PS(float2 texCoord : TEXCOORD0) : COLOR0
{
    // ランダムなレイを算出するための法線。
    float3 randomNormal = DecodeNormal(tex2D(RandomNormalMapSampler, texCoord * RandomOffset).xyz);

    // 現在対象とする位置での法線と深度。
    float4 normalDepth = tex2D(NormalDepthMapSampler, texCoord);
    float3 normal = DecodeNormal(normalDepth.xyz);
    float depth = normalDepth.w;

    // 遠方である程にサンプリングの半径を小さくする。
    float adjustedRadius = Radius * (1 - depth);

    // 最背面は処理しない。
    // 最背面の法線はシーンの法線ではなく、それらを用いて演算を行うと、
    // 最背面に対して誤った閉塞情報を出力してしまう。
    // また、演算を省く意味もある。
    float occlusion = 0;
    if (depth < 0.999999f)
    {
        for (int i = 0; i < SAMPLE_COUNT; i++)
        {
            // サンプルの座標を決定するためのレイ。
            float3 ray = adjustedRadius * reflect(SampleSphere[i], randomNormal);

            // サンプルの座標。
            float2 occluderTexCoord = texCoord + sign(dot(ray, normal)) * ray;

            // サンプルの法線と深度。
            float4 occluderNormalDepth = tex2D(NormalDepthMapSampler, occluderTexCoord);
            float3 occluderNormal = DecodeNormal(occluderNormalDepth.xyz);
            float occluderDepth = occluderNormalDepth.w;

            // 深度差。
            // deltaDepth < 0 は、サンプルがより奥にある状態。
            float deltaDepth = depth - occluderDepth;

            // 法線のなす角を算出。
            float dotNormals = dot(occluderNormal, normal);

            // 法線のなす角が並行ではない程に影響が大きくなるようにする。
            float deltaNormal = 1 - (dotNormals * 0.5 + 0.5);

            // サンプルが奥にある場合は凸状態であり、
            // step により法線の影響を 0 にしてしまう。
            //
            // TODO
            // 必ずしもそうではないのでは？
            deltaNormal *= step(Falloff, deltaDepth);

            // [Falloff, Strength] の間で深度差による影響の度合いを変える。
            // より深度差が小さい程に影響が大きく、より深度差が大きい程に影響が小さい。
//            deltaNormal *= (1 - smoothstep(Falloff, Strength, deltaDepth));

            // 法線の影響の度合いを足す。
            occlusion += deltaNormal;
        }

        occlusion *= invSamples * TotalStrength;
    }

    float ao = 1 - occlusion;
    return float4(ao, 0, 0, 0);
}

//=============================================================================
//
// テクニック
//
//-----------------------------------------------------------------------------
technique Default
{
    pass P0
    {
        // ps_3_0 を使うので vs_3_0 を明示。
        // これを明示しなければ実行時エラーとなる。
        VertexShader = compile vs_3_0 VS();
        PixelShader  = compile ps_3_0 PS();
    }
}
