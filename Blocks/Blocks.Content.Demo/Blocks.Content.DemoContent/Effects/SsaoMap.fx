//=============================================================================
//
// 変数宣言
//
//-----------------------------------------------------------------------------
float TotalStrength = 1.00;
float Strength = 1.0;
float RandomOffset = 18.0;
float Falloff = 0.000002;
float Radius = 1.0;
#define SAMPLES 10 // 10 位が丁度良いらしい
float invSamples = 1.0 / (float) SAMPLES;

#if SAMPLES == 8
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
#elif SAMPLES == 10
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
#elif SAMPLES == 12
const float3 SampleSphere[12] =
{
    float3(-0.13657719, 0.30651027, 0.16118456),
    float3(-0.14714938, 0.33245975, -0.113095455),
    float3(0.030659059, 0.27887347, -0.7332209),
    float3(0.009913514, -0.89884496, 0.07381549),
    float3(0.040318526, 0.40091, 0.6847858),
    float3(0.22311053, -0.3039437, -0.19340435),
    float3(0.36235332, 0.21894878, -0.05407306),
    float3(-0.15198798, -0.38409665, -0.46785462),
    float3(-0.013492276, -0.5345803, 0.11307949),
    float3(-0.4972847, 0.037064247, -0.4381323),
    float3(-0.024175806, -0.008928787, 0.17719103),
    float3(0.694014, -0.122672155, 0.33098832)
};
#elif SAMPLES == 16
const float3 SampleSphere[16] =
{
    float3(0.53812504, 0.18565957, -0.43192),
    float3(0.13790712, 0.24864247, 0.44301823),
    float3(0.33715037, 0.56794053, -0.005789503),
    float3(-0.6999805, -0.04511441, -0.0019965635),
    float3(0.06896307, -0.15983082, -0.85477847),
    float3(0.056099437, 0.006954967, -0.1843352),
    float3(-0.014653638, 0.14027752, 0.0762037),
    float3(0.010019933, -0.1924225, -0.034443386),
    float3(-0.35775623, -0.5301969, -0.43581226),
    float3(-0.3169221, 0.106360726, 0.015860917),
    float3(0.010350345, -0.58698344, 0.0046293875),
    float3(-0.08972908, -0.49408212, 0.3287904),
    float3(0.7119986, -0.0154690035, -0.09183723),
    float3(-0.053382345, 0.059675813, -0.5411899),
    float3(0.035267662, -0.063188605, 0.54602677),
    float3(-0.47761092, 0.2847911, -0.0271716)
};
#endif

texture NormalDepthMap;
sampler NormalDepthMapSampler : register(s0) = sampler_state
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
    float3 randomNormal = normalize((tex2D(RandomNormalMapSampler, texCoord * RandomOffset).xyz * 2.0) - 1.0f);

    float4 currentNormalDepth = tex2D(NormalDepthMapSampler, texCoord);
    float3 currentNormal = normalize(currentNormalDepth.xyz * 2.0f - 1.0f);
    float currentDepth = currentNormalDepth.w;

    float occlusion = 0;
    float adjustedRadius = Radius * (1 - currentDepth);

    for (int i = 0; i < SAMPLES; i++)
    {
        if (currentDepth < 0.999999f)
        {
            float3 ray = adjustedRadius * reflect(SampleSphere[i], randomNormal);
            float2 occluderUV = texCoord + sign(dot(ray, currentNormal)) * ray * float2(1.0f, -1.0f);

            float4 occluderNormalDepth = tex2D(NormalDepthMapSampler, occluderUV);
            float3 occluderNormal = normalize(occluderNormalDepth.xyz * 2.0f - 1.0f);
            float occluderDepth = occluderNormalDepth.w;

            float deltaDepth = currentDepth - occluderDepth;
            float dotNormals = dot(occluderNormal, currentNormal);
            float deltaNormal = 1.0 - dotNormals * dotNormals;
            occlusion += step(Falloff, deltaDepth) * deltaNormal * (1.0 - smoothstep(Falloff, Strength, deltaDepth));
        }
    }

    float ao = 1.0 - TotalStrength * occlusion * invSamples;
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
