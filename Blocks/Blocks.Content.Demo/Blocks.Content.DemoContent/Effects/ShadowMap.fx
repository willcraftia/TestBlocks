//=============================================================================
// Variables
//-----------------------------------------------------------------------------
float4x4 World;
float4x4 LightViewProjection;

struct VSOutput
{
    float4 Position     : POSITION;
    float4 Depth        : TEXCOORD0;
};

//=============================================================================
// Vertex shader
//-----------------------------------------------------------------------------
VSOutput VS(float4 position : POSITION)
{
    VSOutput output = (VSOutput) 0;

    output.Position = mul(position, mul(World, LightViewProjection));
    output.Depth = output.Position;

    return output;
}

//=============================================================================
// Pixel shader
//-----------------------------------------------------------------------------
float4 DefaultPS(VSOutput input) : COLOR0
{
    float depth = input.Depth.z / input.Depth.w;
    return float4(depth, 0.0f, 0.0f, 0.0f);
}

float4 VsmPS(VSOutput input) : COLOR0
{
    float depth = input.Depth.z / input.Depth.w;
    return float4(depth, depth * depth, 0.0f, 0.0f);
}

//=============================================================================
// Technique
//-----------------------------------------------------------------------------
technique Default
{
    pass P0
    {
        CullMode = CW;
//        CullMode = CCW;
//        CullMode = None;
        VertexShader = compile vs_2_0 VS();
        PixelShader = compile ps_2_0 DefaultPS();
    }
}

technique Vsm
{
    pass P0
    {
        CullMode = CW;
//        CullMode = CCW;
//        CullMode = None;
        VertexShader = compile vs_2_0 VS();
        PixelShader = compile ps_2_0 VsmPS();
    }
}
