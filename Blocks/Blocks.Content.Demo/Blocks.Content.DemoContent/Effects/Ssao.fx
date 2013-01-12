//=============================================================================
//
// 定数
//
//-----------------------------------------------------------------------------
float3 ShadowColor = float3(0.4, 0.4, 0.4);

texture SceneMap;
sampler2D SceneMapSampler : register(s0) = sampler_state
{
    Texture = <SceneMap>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = None;
};

texture SsaoMap;
sampler2D SsaoMapSampler = sampler_state
{
    Texture = <SsaoMap>;
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = None;
};

//=============================================================================
//
// ピクセル シェーダ
//
//-----------------------------------------------------------------------------
float4 PS(float2 texCoord : TEXCOORD0) : COLOR0
{
    float ao = tex2D(SsaoMapSampler, texCoord).x;
    float3 sceneColor = tex2D(SceneMapSampler, texCoord);

    float3 color = lerp(sceneColor * ShadowColor, sceneColor, ao);
    return float4(color, 1);
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
