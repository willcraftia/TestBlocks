//-----------------------------------------------------------------------------
//
// Draw scene contains only shadow using VSM shadow technique.
//
//-----------------------------------------------------------------------------
#include "Common.fxh"
#include "Shadow.fxh"
#include "Pssm.fxh"

//-----------------------------------------------------------------------------
//
// Section: Pixel shader
//
//-----------------------------------------------------------------------------
float4 PS(VSOutput input) : COLOR0
{
    float distance = abs(input.LightingPosition[0].z);

    float splitIndex = 0;
    float shadow = 1;

    for (int i = 0; i < SplitCount; i++)
    {
        if (SplitDistances[i] <= distance && distance < SplitDistances[i + 1])
        {
            float4 lightingPosition = input.LightingPosition[i + 1];
            float2 shadowTexCoord = ProjectionToTexCoord(lightingPosition);
            shadow = TestVarianceShadowMap(
                ShadowMapSampler[i],
                shadowTexCoord,
                lightingPosition,
                DepthBias);
            splitIndex = i;
        }
    }
//    return float4(shadow, shadow, shadow, 1);
    splitIndex %= 3;
    float r = shadow;
    float g = (splitIndex == 1) ? shadow : 0;
    float b = (splitIndex == 2) ? shadow : 0;
    return float4(r, g, b, 1);
}

//-----------------------------------------------------------------------------
//
// Section: Technique
//
//-----------------------------------------------------------------------------
technique Default
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 PS();
    }
}
