#ifndef UNIVERSAL_SHADOW_CASTER_PASS_INCLUDED
#define UNIVERSAL_SHADOW_CASTER_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

float3 _LightDirection;

struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float2 texcoord     : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float2 uv           : TEXCOORD0;
    float4 positionCS   : SV_POSITION;
    float4 screenPos    : TEXCOORD1;
    float3 positionWS    : TEXCOORD2;
};

float4 GetShadowPositionHClip(Attributes input)
{
    float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
    float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

    float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));

    #if UNITY_REVERSED_Z
    positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
    #else
    positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
    #endif

    return positionCS;
}

Varyings ShadowPassVertex(Attributes input)
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);

    output.uv = input.texcoord;
    output.positionCS = GetShadowPositionHClip(input);
    
    output.screenPos = ComputeScreenPos(output.positionCS);
    output.positionWS = TransformObjectToWorld(input.positionOS);
    return output;
}

half4 ShadowPassFragment(Varyings input) : SV_TARGET
{
    // float3 screenUV = input.screenPos.xyz;
    // screenUV.xy = screenUV.xy/input.screenPos.w;
    // screenUV.z*=100;
    // //screenUV.z = LinearEyeDepth(screenUV.z, _ZBufferParams);
    //
    // clip(Dither_WS(alpha, screenUV));
    // //AlphaDither(alpha, input.screenPos.xy/input.screenPos.w);//+frac(_Time.y));

    half alpha = SAMPLE_TEXTURE2D_BIAS(_OpacityMap, sampler_OpacityMap, input.uv, -1).r  * _Test.r;

    //AlphaDither(alpha, input.positionCS.z*1000 + input.screenPos.xy/input.screenPos.w);
    //TODO: 暂时先这样顶着，得具体看UE到底怎么写的，现在背面阴影还是会漏光
    clip(alpha - random(input.positionWS));
    
    return 0;
}

#endif
