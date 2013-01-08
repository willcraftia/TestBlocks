//-----------------------------------------------------------------------------
// ParticleEffect.fx
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
// �J�����̃p�����[�^�[�B
float4x4 View;
float4x4 Projection;
float2 ViewportScale;

// ���݂̎��� (�b�P��)�B
float CurrentTime;

// �p�[�e�B�N���̃A�j���[�V�����\�����@���L�q����p�����[�^�[�B
float Duration;
float DurationRandomness;
float3 Gravity;
float EndVelocity;
float4 MinColor;
float4 MaxColor;

// �͈͂̍ŏ��l�ƍő�l���L�q���� float2 �p�����[�^�[�B
// ���ۂ̒l�́A�����_���Ȓl�� x �� y �̊Ԃ��Ԃ���悤��
// �e�p�[�e�B�N���ŕʁX�ɑI������܂��B
float2 RotateSpeed;
float2 StartSize;
float2 EndSize;

// �p�[�e�B�N�� �e�N�X�`���ƃT���v���[�B
texture Texture;
sampler Sampler = sampler_state
{
    Texture = (Texture);
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};

// ���_�V�F�[�_�[�̓��͍\���̂́A�p�[�e�B�N���̊J�n�ʒu�Ƒ��x�A
// �쐬�����A����уT�C�Y�Ɖ�]�ɉe����^���郉���_���Ȓl��
// �L�q���܂��B
struct VertexShaderInput
{
    float2 Corner : POSITION0;
    float3 Position : POSITION1;
    float3 Velocity : NORMAL0;
    float4 Random : COLOR0;
    float Time : TEXCOORD0;
};

// ���_�V�F�[�_�[�̏o�̓t�H�[�}�b�g�̓p�[�e�B�N���̈ʒu�ƐF�������܂��B
struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TextureCoordinate : COLOR1;
};

// �p�[�e�B�N���̈ʒu���v�Z���钸�_�V�F�[�_�[ �w���p�[�B
float4 ComputeParticlePosition(float3 position, float3 velocity,
                               float age, float normalizedAge)
{
    float startVelocity = length(velocity);

    // ���̃X�P�[�� �t�@�N�^�[���J�n���x�ɓK�p���āA�������ԏI������
    // �p�[�e�B�N���̈ړ����x������o���܂��B
    float endVelocity = startVelocity * EndVelocity;

    // �p�[�e�B�N���͈��̃A�N�Z�����[�V���� (�����x) �����̂ŁA
    // �J�n���x�� S �A�I�����x�� E �Ƃ���ƁA���� T �ɂ����鑬�x�� 
    // S + (E-S)*T �ƂȂ�܂��B�p�[�e�B�N���̈ʒu�́A0 �` T �͈̔͂ɂ�����
    // ���x�̍��v�ł��B�ʒu�𒼐ڌv�Z����ɂ́A���x�̎���ϕ�����K�v��
    // ����܂��BS + (E-S)*T �� T �Őϕ������ S*T + (E-S)*T*T/2 �ƂȂ�܂��B

    float velocityIntegral = startVelocity * normalizedAge + (endVelocity - startVelocity) * normalizedAge * normalizedAge / 2;

    position += normalize(velocity) * velocityIntegral * Duration;

    // �d�͂�K�p���܂��B
    position += Gravity * age * normalizedAge;

    // �J���� �r���[�Ǝˉe�ϊ���K�p���܂��B
    return mul(mul(float4(position, 1), View), Projection);
}

// �p�[�e�B�N���̃T�C�Y���v�Z���钸�_�V�F�[�_�[ �w���p�[�B
float ComputeParticleSize(float randomValue, float normalizedAge)
{
    // �����̒l��K�p���āA�e�p�[�e�B�N�����킸���ɈقȂ�T�C�Y�ɂ��܂��B
    float startSize = lerp(StartSize.x, StartSize.y, randomValue);
    float endSize = lerp(EndSize.x, EndSize.y, randomValue);

    // �p�[�e�B�N���̐���Ɋ�Â��Ď��ۂ̃T�C�Y���v�Z���܂��B
    float size = lerp(startSize, endSize, normalizedAge);

    // �X�N���[�����W�ɃT�C�Y���ˉe���܂��B
    return size * Projection._m11;
}

// �p�[�e�B�N���̃J���[���v�Z���钸�_�V�F�[�_�[ �w���p�[�B
float4 ComputeParticleColor(float4 projectedPosition,
                            float randomValue, float normalizedAge)
{
    // �����̒l��K�p���āA�e�p�[�e�B�N�����킸���ɈقȂ�J���[�ɂ��܂��B
    float4 color = lerp(MinColor, MaxColor, randomValue);

    // �p�[�e�B�N���̐���Ɋ�Â��ăA���t�@�����������܂��B���̋Ȑ���
    // �n�[�h�R�[�f�B���O����Ă��܂��B�p�[�e�B�N���͏u���Ƀt�F�[�h�C�����A
    // �������ƃt�F�[�h�A�E�g���܂��B���̗l�q���m�F����ɂ́A�O���t�쐬��
    // �v���O������ x*(1-x)*(1-x) �� x=0 �` 1 �Ńv���b�g���Ă��������B
    // �X�P�[�����O �t�@�N�^�[�� 6.7 �͋Ȑ��𐳋K������̂ŁA�A���t�@�͍ŏI�I��
    // ���S�Ƀ\���b�h�ɂȂ�܂��B

    color.a *= normalizedAge * (1-normalizedAge) * (1-normalizedAge) * 6.7;

    return color;
}

// �p�[�e�B�N���̉�]���v�Z���钸�_�V�F�[�_�[ �w���p�[�B
float2x2 ComputeParticleRotation(float randomValue, float age)
{    
    // �����̒l��K�p���āA�e�p�[�e�B�N�����قȂ鑬�x�ŉ�]�����܂��B
    float rotateSpeed = lerp(RotateSpeed.x, RotateSpeed.y, randomValue);
    
    float rotation = rotateSpeed * age;

    // 2x2 ��]�s����v�Z���܂��B
    float c = cos(rotation);
    float s = sin(rotation);
    
    return float2x2(c, -s, s, c);
}

// �J�X�^���̒��_�V�F�[�_�[�ɂ���āAGPU ��ł��ׂẴp�[�e�B�N�����A�j���[�V�����\�����܂��B
VertexShaderOutput ParticleVertexShader(VertexShaderInput input)
{
    VertexShaderOutput output;

    // �p�[�e�B�N���̐�����v�Z���܂��B
    float age = CurrentTime - input.Time;

    // �����̒l��K�p���āA���ꂼ��̃p�[�e�B�N���̐�����قȂ�i�x�ɂ��܂��B
    age *= 1 + input.Random.x * DurationRandomness;

    // ����� 0 �` 1 �͈̔͂ɐ��K�����܂��B
    float normalizedAge = saturate(age / Duration);

    // �p�[�e�B�N���̈ʒu�A�T�C�Y�A�J���[�A����щ�]���v�Z���܂��B
    output.Position = ComputeParticlePosition(input.Position, input.Velocity,
                                              age, normalizedAge);

    float size = ComputeParticleSize(input.Random.y, normalizedAge);
    float2x2 rotation = ComputeParticleRotation(input.Random.w, age);

    output.Position.xy += mul(input.Corner, rotation) * size * ViewportScale;

    output.Color = ComputeParticleColor(output.Position, input.Random.z, normalizedAge);
    output.TextureCoordinate = (input.Corner + 1) / 2;

    return output;
}

// �p�[�e�B�N����`�悷��s�N�Z�� �V�F�[�_�[�B
float4 ParticlePixelShader(VertexShaderOutput input) : COLOR0
{
    return tex2D(Sampler, input.TextureCoordinate) * input.Color;
}

// �p�[�e�B�N����`�悷�邽�߂̃G�t�F�N�g �e�N�j�b�N�B
technique Particles
{
    pass P0
    {
        VertexShader = compile vs_2_0 ParticleVertexShader();
        PixelShader = compile ps_2_0 ParticlePixelShader();
    }
}
