//=============================================================================
//
// �ϐ��錾
//
//-----------------------------------------------------------------------------
float4x4 World;
float4x4 View;
float4x4 Projection;

//=============================================================================
//
// �\���̐錾
//
//-----------------------------------------------------------------------------
struct VSInput
{
    float4 Position : POSITION;
};

struct VSOutput
{
    float4 Position     : POSITION;
    float4 PositionWVP  : TEXCOORD0;
};

//=============================================================================
//
// ���_�V�F�[�_
//
//-----------------------------------------------------------------------------
VSOutput VS(VSInput input)
{
    VSOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);

    output.Position = mul(viewPosition, Projection);
    output.PositionWVP = output.Position;

    return output;
}

//=============================================================================
//
// �s�N�Z�� �V�F�[�_
//
//-----------------------------------------------------------------------------
float4 PS(VSOutput input) : COLOR0
{
    float depth = input.PositionWVP.z / input.PositionWVP.w;
    return float4(depth, 0, 0, 0);
}

//=============================================================================
//
// �e�N�j�b�N
//
//-----------------------------------------------------------------------------
technique Default
{
    pass P0
    {
        FillMode = SOLID;
        CullMode = CCW;
        VertexShader = compile vs_2_0 VS();
        PixelShader = compile ps_2_0 PS();
    }
}
