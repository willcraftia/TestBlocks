//=============================================================================
//
// 定数
//
//-----------------------------------------------------------------------------
float2 EdgeOffset;
float EdgeIntensity;

float NormalThreshold;
float DepthThreshold;

float NormalSensitivity;
float DepthSensitivity;

float3 EdgeColor;

texture SceneMap;
sampler SceneMapSampler : register(s0) = sampler_state
{
    Texture = <SceneMap>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = None;
};

texture NormalDepthMap;
sampler NormalDepthMapSampler = sampler_state
{
    Texture = <NormalDepthMap>;
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = None;
    AddressU = Clamp;
    AddressV = Clamp;
};

//=============================================================================
//
// ピクセル シェーダ
//
//-----------------------------------------------------------------------------
float4 PS(float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(SceneMapSampler, texCoord);

    float4 s = tex2D(NormalDepthMapSampler, texCoord);
    float4 s1 = tex2D(NormalDepthMapSampler, texCoord + float2(-1, -1) * EdgeOffset);
    float4 s2 = tex2D(NormalDepthMapSampler, texCoord + float2( 1,  1) * EdgeOffset);
    float4 s3 = tex2D(NormalDepthMapSampler, texCoord + float2(-1,  1) * EdgeOffset);
    float4 s4 = tex2D(NormalDepthMapSampler, texCoord + float2( 1, -1) * EdgeOffset);

    float4 deltaSample = abs(s1 - s2) + abs(s3 - s4);

    float deltaNormal = dot(deltaSample.xyz, 1);
    deltaNormal = saturate((deltaNormal - NormalThreshold) * NormalSensitivity);

    float deltaDepth = deltaSample.w;
    deltaDepth = saturate((deltaDepth - DepthThreshold) * DepthSensitivity);

    float amount = saturate(deltaNormal + deltaDepth);

    // XNA サンプルとは異なり、遠方に行く程に影響を少なくする。
    // これにより、ファー クリップ面での不正なエッジ描画が無くなる。
    amount *= EdgeIntensity * (1 - s.w);

    color.rgb = lerp(color.rgb, color.rgb * EdgeColor, saturate(amount));

    return color;
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
        PixelShader = compile ps_2_0 PS();
    }
}
