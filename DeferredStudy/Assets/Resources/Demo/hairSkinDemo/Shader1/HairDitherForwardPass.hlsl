#ifndef HAIR_DITHER_PASS_INCLUDED
#define HAIR_DITHER_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "HairLighting.hlsl"

struct Attributes
{
    float4 positionOS    : POSITION;
    float3 normalOS      : NORMAL;
    float4 tangentOS     : TANGENT;
    float2 texcoord      : TEXCOORD0;
    float2 lightmapUV    : TEXCOORD1;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float2 uv                       : TEXCOORD0;
    DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);

    float3 posWS                    : TEXCOORD2;    // xyz: posWS

    float4 normal                   : TEXCOORD3;    // xyz: normal, w: viewDir.x
    float4 tangent                  : TEXCOORD4;    // xyz: tangent, w: viewDir.y
    float4 bitangent                : TEXCOORD5;    // xyz: bitangent, w: viewDir.z

    half4 fogFactorAndVertexLight   : TEXCOORD6; // x: fogFactor, yzw: vertex light

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    float4 shadowCoord              : TEXCOORD7;
#endif

    float4 positionCS               : SV_POSITION;
    float4 screenPos                : TEXCOORD8;
    float2 uv2                      : TEXCOORD9;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

///////////////////////////////////////////////////////////////////////////////
//                  Vertex and Fragment functions                            //
///////////////////////////////////////////////////////////////////////////////

// Used in Standard (Simple Lighting) shader
Varyings LitPassVertexSimple(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
    half3 viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
    half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);
    half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);

    output.uv = input.texcoord;
    output.posWS.xyz = vertexInput.positionWS;
    output.positionCS = vertexInput.positionCS;

    output.normal = half4(normalInput.normalWS, viewDirWS.x);
    output.tangent = half4(normalInput.tangentWS, viewDirWS.y);
    output.bitangent = half4(normalInput.bitangentWS, viewDirWS.z);

    OUTPUT_LIGHTMAP_UV(input.lightmapUV, unity_LightmapST, output.lightmapUV);
    OUTPUT_SH(output.normal.xyz, output.vertexSH);

    output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    output.shadowCoord = GetShadowCoord(vertexInput);
#endif

    output.screenPos = ComputeScreenPos(output.positionCS);
    output.uv2 = input.lightmapUV;
    return output;
}

void GetDepthOffset(float2 screenUV, half depthSample, inout float deviceDepth, out float depthOffset)
{
    //get dithered depth offset
    //TODO: 没找到ue那边PDO传进来是怎么转的单位，我默然是厘米吧
    half randOffset = Dither(1, screenUV + frac(_Time))- 0.5;
    depthOffset = _PixelDepthOffset*(randOffset + depthSample);
    
    //get depth offset
    float vsDpeth = LinearEyeDepth(deviceDepth, _ZBufferParams);
    depthOffset = max(vsDpeth, vsDpeth + depthOffset/100) - vsDpeth;
    vsDpeth = vsDpeth + depthOffset;
    deviceDepth = LinearDepthToZBuffer(vsDpeth, _ZBufferParams);
}

float3 transformCoordinats(float3 v)
{
    float3 res = v;
    res.r = v.b;
    res.g = v.r;
    res.b = v.g;
    return res;
}
half4 LitPassFragmentSimple(Varyings input, out float depth : SV_Depth) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    //init base data
    float2 uv = input.uv;
    float2 screenUV = input.screenPos.xy/input.screenPos.w;
    float3 viewDir = normalize(input.posWS - _WorldSpaceCameraPos);
    half nov = dot(input.normal, viewDir);

    //sample clipping maps
    half alpha = SAMPLE_TEXTURE2D_BIAS(_OpacityMap, sampler_OpacityMap, uv, -1).r  * _Test.r;
    half depthSample = SAMPLE_TEXTURE2D(_DepthMap, sampler_DepthMap, uv);

    //apply edge mask
    float edgeMask = CheapContrast(abs(nov), _EdgeMaskContrast);
    edgeMask = lerp(_EdgeMaskMin, 1, edgeMask);
    float contrastDepth = depthSample * depthSample;
    edgeMask = lerp(contrastDepth, 1, edgeMask);
    alpha *= edgeMask;

    //do dither alpha
    //AlphaDither(alpha, screenUV+ frac(_Time.w));
    clip(alpha-0.5);

    //get depth dist conversion
    //TODO: 不知道为啥ue没做这个
    //TODO: 可以把这个弄到VS做优化
    float dist2depth, depth2dist;
    GetDistDepthConversion(viewDir, dist2depth, depth2dist);

    //do depth offset
    float deviceDepth = input.positionCS.z;
    float depthOffset;
    GetDepthOffset(screenUV, depthSample, deviceDepth, depthOffset);
    input.positionCS.z = deviceDepth;
    input.screenPos.z = deviceDepth;
    input.posWS += viewDir * depthOffset * depth2dist;

    //sample base maps
    half idSample = SAMPLE_TEXTURE2D_BIAS(_IdMap, sampler_IdMap, uv, -1).r;
    float3 baseColorSample =  SAMPLE_TEXTURE2D_BIAS(_ColorMap, sampler_ColorMap, uv, -1);
    float3 rootSample =  SAMPLE_TEXTURE2D_BIAS(_RootMap, sampler_RootMap, uv, -1);

    //get base color
    float3 rootTipColor = lerp(_RootColor, _TipColor, rootSample);
#if defined _ENABLE_ROOTMAP2
    float2 uv2 = input.uv2;
    float4 root2Sample =  SAMPLE_TEXTURE2D_BIAS(_RootMap2, sampler_RootMap2, uv2, -1);
    rootTipColor = lerp(rootTipColor, _Tip2Color, root2Sample);
    //return root2Sample;
#endif
    float3 baseColor = baseColorSample * rootTipColor * _Brightness;
    
    //get light info
    float4 shadowCoord = TransformWorldToShadowCoord(input.posWS);
    Light mainLight = GetMainLight(shadowCoord, input.posWS,  half4(1, 1, 1, 1));

    //get bitangent TS
    float3 bitangentTS = _Tangent + lerp(_TangentA, _TangentB, idSample);
    bitangentTS = normalize(bitangentTS);

    //准备材质数据
    half roughnessBSq = _Roughness * _Roughness;
    float3 bitangentWS = TransformTangentToWorld(bitangentTS,
        half3x3(input.tangent.xyz, input.bitangent.xyz, input.normal.xyz));
    if(nov>0)
    {
        bitangentWS = -bitangentWS;
    }

    //准备光照数据
    float3 lightColor = mainLight.distanceAttenuation *  mainLight.color * mainLight.shadowAttenuation;
    float3 lightDirWS = -mainLight.direction;
    
    //计算直接光
    float3 S = lightColor * HairShading(baseColor, lightDirWS, viewDir, bitangentWS, saturate(mainLight.shadowAttenuation), roughnessBSq, 0, 1);

    half4 color = 0;
    depth = input.positionCS.z;
    color.rgb = (S);
    return color;
}

#endif
