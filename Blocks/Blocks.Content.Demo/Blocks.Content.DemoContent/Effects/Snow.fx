float4x4 Projection;
float4 Color;

texture Texture;
sampler TextureSampler = sampler_state
{
    Texture = <Texture>;
    MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};

void VS(
    float4 pos : POSITION, float2 uv : TEXCOORD0,
    out float4 Pos : POSITION, out float2 UV : TEXCOORD0)
{
    Pos = mul(pos, Projection);
    UV = uv;
}

float4 PS(float2 UV : TEXCOORD0) : COLOR
{
    return tex2D(TextureSampler, UV) * Color;
}

technique Default
{
    pass P0
    {
        VertexShader = compile vs_2_0 VS();
        PixelShader = compile ps_2_0 PS();
    }
}
