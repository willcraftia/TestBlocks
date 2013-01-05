//=============================================================================
// Variables
//-----------------------------------------------------------------------------
float4x4 World;
float4x4 LightViewProjection;

struct VSOutput
{
    float4 Position     : POSITION;
    float Depth         : TEXCOORD0;
};

//=============================================================================
// Vertex shader
//-----------------------------------------------------------------------------
VSOutput VS(float4 position : POSITION)
{
    VSOutput output = (VSOutput) 0;

    output.Position = mul(position, mul(World, LightViewProjection));
    output.Depth = output.Position.z / output.Position.w;

    return output;
}

//=============================================================================
// Pixel shader
//-----------------------------------------------------------------------------
float4 DefaultPS(VSOutput input) : COLOR0
{
    return float4(input.Depth, 0.0f, 0.0f, 0.0f);
}

float4 VsmPS(VSOutput input) : COLOR0
{
    return float4(input.Depth, input.Depth * input.Depth, 0.0f, 0.0f);
}

//=============================================================================
// Technique
//-----------------------------------------------------------------------------
technique Default
{
    pass P0
    {
//        CullMode = CW;
//        CullMode = CCW;
        CullMode = None;
        VertexShader = compile vs_2_0 VS();
        PixelShader = compile ps_2_0 DefaultPS();
    }
}

technique Vsm
{
    pass P0
    {
//        CullMode = CW;
//        CullMode = CCW;
        CullMode = None;
        VertexShader = compile vs_2_0 VS();
        PixelShader = compile ps_2_0 VsmPS();
    }
}
