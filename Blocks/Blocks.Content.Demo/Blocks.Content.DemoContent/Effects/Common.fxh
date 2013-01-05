//=============================================================================
//
// ���ʒ�`
//
//=============================================================================
#if !defined (COMMON_FXH)
#define COMMON_FXH

//-----------------------------------------------------------------------------
//
// �ˉe��ԍ��W����e�N�X�`�����W�֕ϊ��B
//
//-----------------------------------------------------------------------------
float2 ProjectionToTexCoord(float4 position)
{
    return position.xy / position.w * float2(0.5f, -0.5f) + float2(0.5f, 0.5f);
}

//-----------------------------------------------------------------------------
//
// �����t�H�O�̃t�@�N�^���v�Z�B
//
//-----------------------------------------------------------------------------
float CalculateFogFactor(float fogStart, float fogEnd, float distance, float fogEnabled)
{
    return saturate((distance - fogStart) / (fogEnd - fogStart)) * fogEnabled;
}

#endif // !defined (COMMON_FXH)
