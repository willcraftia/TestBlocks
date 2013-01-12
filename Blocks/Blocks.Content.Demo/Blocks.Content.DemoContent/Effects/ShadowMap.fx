//=============================================================================
//
// �萔
//
//-----------------------------------------------------------------------------
float4x4 World;
float4x4 LightViewProjection;

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
VSOutput VS(float4 position : POSITION)
{
    VSOutput output;

    output.Position = mul(position, mul(World, LightViewProjection));
    output.PositionWVP = output.Position;

    return output;
}

//=============================================================================
//
// �s�N�Z�� �V�F�[�_
//
//-----------------------------------------------------------------------------
float4 DefaultPS(VSOutput input) : COLOR0
{
    float depth = input.PositionWVP.z / input.PositionWVP.w;
    return float4(depth, 0.0f, 0.0f, 0.0f);
}

float4 VsmPS(VSOutput input) : COLOR0
{
    float depth = input.PositionWVP.z / input.PositionWVP.w;
    return float4(depth, depth * depth, 0.0f, 0.0f);
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
        // CW �łُ͈�Ƀs�[�^�[�p�����ۂ��������Ă��܂����߁A
        // CCW �ɂ��Ă���B
        // CCW �͐[�x�o�C�A�X��K�v�Ƃ��邪�A�s�[�^�[�p�����ۂ����������y�B
        //
        // http://msdn.microsoft.com/ja-jp/library/ee416324(VS.85).aspx
        // �w�w�ʂƑS�ʁx�Q�ƁB
        //
        CullMode = CCW;
        VertexShader = compile vs_2_0 VS();
        PixelShader = compile ps_2_0 DefaultPS();
    }
}

technique Vsm
{
    pass P0
    {
        CullMode = CCW;
        VertexShader = compile vs_2_0 VS();
        PixelShader = compile ps_2_0 VsmPS();
    }
}
