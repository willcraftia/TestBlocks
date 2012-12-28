//-----------------------------------------------------------------------------
//
// Parallel-split Shadow Map Common Definitions
//
//-----------------------------------------------------------------------------
#if !defined (PSSM_FXH)
#define PSSM_FXH

//-----------------------------------------------------------------------------
// Section:  Fields
//-----------------------------------------------------------------------------
uniform const float4x4 World;
uniform const float4x4 View;
uniform const float4x4 Projection;

uniform const float DepthBias = 0.001f;

#define MAX_SPLIT_COUNT 3

uniform const int SplitCount = MAX_SPLIT_COUNT;
uniform const float SplitDistances[MAX_SPLIT_COUNT + 1];
uniform const float4x4 SplitViewProjections[MAX_SPLIT_COUNT];

uniform const texture ShadowMap0;
#if MAX_SPLIT_COUNT > 1
uniform const texture ShadowMap1;
#endif
#if MAX_SPLIT_COUNT > 2
uniform const texture ShadowMap2;
#endif
#if MAX_SPLIT_COUNT > 3
uniform const texture ShadowMap3;
#endif
#if MAX_SPLIT_COUNT > 4
uniform const texture ShadowMap4;
#endif
#if MAX_SPLIT_COUNT > 5
uniform const texture ShadowMap5;
#endif
#if MAX_SPLIT_COUNT > 6
uniform const texture ShadowMap6;
#endif

uniform const sampler ShadowMapSampler[MAX_SPLIT_COUNT] =
{
    sampler_state
    {
        Texture = <ShadowMap0>;
        MinFilter = Point;
        MagFilter = Point;
        MipFilter = None;
    },
#if MAX_SPLIT_COUNT > 1
    sampler_state
    {
        Texture = <ShadowMap1>;
        MinFilter = Point;
        MagFilter = Point;
        MipFilter = None;
    },
#endif
#if MAX_SPLIT_COUNT > 2
    sampler_state
    {
        Texture = <ShadowMap2>;
        MinFilter = Point;
        MagFilter = Point;
        MipFilter = None;
    },
#endif
#if MAX_SPLIT_COUNT > 3
    sampler_state
    {
        Texture = <ShadowMap3>;
        MinFilter = Point;
        MagFilter = Point;
        MipFilter = None;
    },
#endif
#if MAX_SPLIT_COUNT > 4
    sampler_state
    {
        Texture = <ShadowMap4>;
        MinFilter = Point;
        MagFilter = Point;
        MipFilter = None;
    },
#endif
#if MAX_SPLIT_COUNT > 5
    sampler_state
    {
        Texture = <ShadowMap5>;
        MinFilter = Point;
        MagFilter = Point;
        MipFilter = None;
    },
#endif
#if MAX_SPLIT_COUNT > 6
    sampler_state
    {
        Texture = <ShadowMap6>;
        MinFilter = Point;
        MagFilter = Point;
        MipFilter = None;
    },
#endif
};

//-----------------------------------------------------------------------------
//
// Section: Vertex shader
//
//-----------------------------------------------------------------------------
struct VSInput
{
    float4 Position : POSITION;
};

struct VSOutput
{
    float4 Position : POSITION;
    float4 LightingPosition[MAX_SPLIT_COUNT + 1] : TEXCOORD0;
};

VSOutput VS(VSInput input)
{
    VSOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);

    output.Position = mul(viewPosition, Projection);

    output.LightingPosition[0] = viewPosition;
    for (int i = 0; i < SplitCount; i++)
    {
        output.LightingPosition[i + 1] = mul(worldPosition, SplitViewProjections[i]);
    }

    return output;
}

#endif // !defined (PSSM_FXH)
