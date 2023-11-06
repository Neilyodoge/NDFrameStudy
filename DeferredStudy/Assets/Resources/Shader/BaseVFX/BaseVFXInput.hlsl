#ifndef WATERINPUT
#define WATERINPUT

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
//#include "Packages/com.unity.render-pipelines.universal/Shaders/TerrainX/MainShader/DepthFogBilinearAdd.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/ParallaxMapping.hlsl"  // 视差相关函数

CBUFFER_START(UnityPerMaterial)
float _HardRimWidth;
float _DissolveX, _DissolveY, _DissolveSpeedX, _DissolveSpeedY, _MaskSpeedX, _MaskSpeedY, _CustomVertexAnimOffset;
float _FresnelWidth, _FresnelSideScale, _VertexAnimScale, _VertexAnimWidth, _UseInRole;
float _CustomBloomIntensity;
float _CustomBloomAlphaOffset;
float _Cutoff;
float _ParallaxIntensity;
float _DepthOffset;
float _CustomVFXColor; // 藏匿效果拿到的透明copy color会跟着场景走 // 现在是材质球手动控制关闭
float _BaseMapUVDir;
int4 _CopyColorBlend;
int _ParallaxMaxStep, _ParallaxMinStep;
int _DissolveType, _DistortionScreenUV;
int _InvertFresnel, _FresnelA;
int _VATexG;
int _Surface;
// Toggle
int _NoLOn;
int _UseCopyColorBlend;
int _SoftParticle, _Distortion, _Fresnel, _VertexAnim, _PolarCoordinates, _HardRim, _Parallax;
half4 _VATint1, _VATint2, _VATint3, _VertexAnimTiling;
half4 _BaseColor;
half4 _FresnelColor;
half4 _DissolveEdgeColor;
// ST
half4 _BaseMap_ST;
half4 _MaskTex_ST;
half4 _DissolveTex_ST;
half4 _DistortionTex_ST;
half4 _FresnelTex_ST;
half4 _VertexAnimTex_ST;
half4 _NoLTint;

half _NoLpos;
half _SoftValue, _HardRimIntensity;
half _OffsetSpeedX, _OffsetSpeedY;
half _SoftParticleFadeParamsNear, _SoftParticleFadeParamsFar, _SoftParticleFadeHeightMapIntensity, _SoftParticleFadeHeightMapScale;
half _DissolveIntensity, _DissolveEdgeWidth, _DissolveEdgeWidthSoft;
half _DistortionIntensity, _DistortionOpaque, _DistortionTransparents, _MainTexCustomDataON, _DissolveCustomData;
half _FresnelIntensity, _FresnelOffsetX, _FresnelOffsetY;
half _VertexAnimSpeedX, _VertexAnimSpeedY, _VertexAnimIntensity, _VertexAnimTint, _VertexAnimCustomData;
// 其他
half _PreAlphaMul;
// half _SceneOrCharacter, _NoFocusIntensity; // 自定义光照用的，动态调整颜色强度
CBUFFER_END
//half _SceneFocusIntensity; // 自定义光照用的，动态调整颜色强度
half _ParticleBloomIntensity;  // MRT用的，前面没声明
//half _CharacterFocusIntensity;


TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);
TEXTURE2D(_MaskTex);
SAMPLER(sampler_MaskTex);
TEXTURE2D(_DissolveTex);
SAMPLER(sampler_DissolveTex);
// TEXTURE2D(_CameraDepthTexture);
// SAMPLER(sampler_CameraDepthTexture);
TEXTURE2D(_DistortionTex);
SAMPLER(sampler_DistortionTex);
TEXTURE2D(_FresnelTex);
SAMPLER(sampler_FresnelTex);
TEXTURE2D(_CameraOpaqueTexture);
SAMPLER(sampler_CameraOpaqueTexture);
TEXTURE2D(_CameraTransparentsTexture);
SAMPLER(sampler_CameraTransparentsTexture);
TEXTURE2D(_VertexAnimTex);
SAMPLER(sampler_VertexAnimTex);
TEXTURE2D(_ParallaxTex);
SAMPLER(sampler_ParallaxTex);


#if defined(_SOFTPARTICLES_ON)  //软粒子
    // height : 用maintex作为高度图去丰富软粒子
    float SoftParticles(float near, float far, float4 projection, float height)
    {
        float fade = 1;
        // 代替 if 了
        near = max(0.0001, near);
        far = max(0.0001, far);
        float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, projection.xy / projection.w * _dynamicResScale), _ZBufferParams);
#if defined( SHADER_API_OPENGL ) || defined( SHADER_API_GLES ) || defined( SHADER_API_GLES3 ) || defined( SHADER_API_GLCORE )
        float thisZ = LinearEyeDepth(projection.z / projection.w * 0.5 + 0.5, _ZBufferParams);//YJY
#else 
        float thisZ = LinearEyeDepth(projection.z / projection.w, _ZBufferParams);
#endif
        //#if UNITY_REVERSED_Z
        fade = saturate(far * ((sceneZ - near) - thisZ));
        // #else
        //     fade = saturate(far * ((sceneZ - near) + thisZ));
        // #endif
        float HeighFade = saturate(far * ((sceneZ - _SoftParticleFadeHeightMapScale) - thisZ));
        float HeighScale = (1 - HeighFade) * fade;
        HeighScale *= HeighScale;
        float heightMap = 1 - pow(height, _SoftParticleFadeHeightMapIntensity);
        return saturate(sceneZ- thisZ);//saturate(fade * (1-HeighScale *heightMap));

    }
#endif
#if defined(_POLARUV)   // 极坐标函数
    float2 RectToPolar(float2 uv, float2 centerUV)
    {
        uv = uv - centerUV;
        float theta = atan2(uv.y, uv.x);    // atan()值域[-π/2, π/2]一般不用; atan2()值域[-π, π]
        float r = length(uv);
        return float2(theta, r);
    }
#endif
#if defined(_PARALLAX)
    //*why 着急结单子，还有很大优化空间
    //参考 GPU Gems 3 chapter 18
    // https://zhuanlan.zhihu.com/p/337399160
    // https://developer.nvidia.com/gpugems/gpugems3/part-iii-rendering/chapter-18-relaxed-cone-stepping-relief-mapping
    // 踩坑 https://developer.nvidia.com/gpugems/gpugems3/part-iii-rendering/chapter-18-relaxed-cone-stepping-relief-mapping
    float2 ParallaxRaymarching(float2 viewDir, float scale, float2 uv, float nov)
    {
        float2 uvOffset = 0;
        int numStep = (int)lerp(_ParallaxMaxStep, _ParallaxMinStep, saturate(nov));
        float stepSize = 1.0f / numStep;       // 计算步数
        float2 uvDelta = viewDir * (stepSize * scale);

        float stepHeight = 1 - 1e-4; // 继续以[0,1]作为取值范围，射线上的第一步高度始终为1
        float surfaceHeight = SAMPLE_TEXTURE2D_LOD(_ParallaxTex, sampler_ParallaxTex, uv, 0).g;

        float2 prevUVOffset = 0;
        float prevStepHeight = 0;
        float prevSurfaceHeight = 0;

        for (int i = 1; i < numStep && stepHeight > surfaceHeight; i++)
        {
            // 步数内，stepHeight > surfaceHeight 步进的mask
            prevUVOffset = uvOffset;
            prevStepHeight = stepHeight;
            prevSurfaceHeight = surfaceHeight;
            
            uvOffset -= uvDelta;        // 计算视差uv的偏移量
            stepHeight -= stepSize;     // 步数
            surfaceHeight = SAMPLE_TEXTURE2D_LOD(_ParallaxTex, sampler_ParallaxTex, uv + uvOffset, 0).g;  // 这里是为了每次计算出高度差

        }
        // 因为最后只会保留一个偏移的深度值，所以不会丢进for循环里
        float prevDifference = prevStepHeight - prevSurfaceHeight;
        float difference = surfaceHeight - stepHeight;
        float t = prevDifference / (prevDifference + difference);
        uvOffset = prevUVOffset -uvDelta * t;  //
        return uvOffset;
    }
#endif
float2 rotUV(float2 uv)
{
    float x = _BaseMapUVDir;
    uv = mul(uv - 0.5,float2x2(cos(x),-sin(x),
                                sin(x),cos(x))) + 0.5;
    return uv;
}

// MRT
struct FragmentOutputParticles
{
    #if _MRTEnable
        half4 color0: SV_Target0;
        half4 color1: SV_Target1;
    #else
        half4 color0: SV_Target0;
    #endif
};
struct appdata
{
    float4 PositionOS: POSITION;
    float3 normalOS: NORMAL;
    #if defined(_PARALLAX)
        float4 tangentOS: TANGENT;
    #endif
    float4 uv: TEXCOORD0;
    float4 vertexColor: COLOR;
    float4 CustomData1: TEXCOORD1;
    float4 CustomData2: TEXCOORD2;
};
struct v2f
{
    float4 uv: TEXCOORD0;
    float3 normalWS: NORMAL;
    float4 PositionCS: SV_POSITION;
    float4 vertexColor: COLOR;
    float4 CustomData1: TEXCOORD1;
    float4 CustomData2: TEXCOORD2;
    #if defined(_SOFTPARTICLES_ON)  // 软粒子
        float4 ScreenPos: TEXCOORD3;
    #endif
    #if defined(_FRESNEL_ON) || defined(_PARALLAX)
        float3 viewDirWS: TEXCOORD4;
    #endif
    #if defined(_PARALLAX)
        float3 tangentWS: TEXCOORD6;
    #endif
};
#endif