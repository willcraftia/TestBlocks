//=============================================================================
//
// 変数宣言
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
    AddressU = Clamp;
    AddressV = Clamp;
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

    float4 n = tex2D(NormalDepthMapSampler, texCoord);
    float4 n1 = tex2D(NormalDepthMapSampler, texCoord + float2(-1, -1) * EdgeOffset);
    float4 n2 = tex2D(NormalDepthMapSampler, texCoord + float2( 1,  1) * EdgeOffset);
    float4 n3 = tex2D(NormalDepthMapSampler, texCoord + float2(-1,  1) * EdgeOffset);
    float4 n4 = tex2D(NormalDepthMapSampler, texCoord + float2( 1, -1) * EdgeOffset);

    float4 deltaDiagonal = abs(n1 - n2) + abs(n3 - n4);

    float deltaNormal = dot(deltaDiagonal.xyz, 1);
    float deltaDepth = deltaDiagonal.w;

    deltaNormal = saturate((deltaNormal - NormalThreshold) * NormalSensitivity);
    deltaDepth = saturate((deltaDepth - DepthThreshold) * DepthSensitivity);

    float edgeAmount = saturate(deltaNormal + deltaDepth) * EdgeIntensity;
    edgeAmount *= (1 + log(n.w));

    color.rgb = lerp(color.rgb, color.rgb * EdgeColor, edgeAmount);

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
