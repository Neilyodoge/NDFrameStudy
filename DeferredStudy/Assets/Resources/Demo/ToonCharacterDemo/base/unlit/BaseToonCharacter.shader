Shader "BaseToon/Character"
{
    Properties
    { 
        [Toggle(_HAIR)] _is_hair("头发材质",int) = 0
        [Toggle(_FACE)] _is_face("面部材质",int) = 0
        _MainTex("主贴图",2D) = "white"{}
        _MainColor("主颜色",color) = (1,1,1,1)
        _Brightness("亮度",range(0.5,2)) = 1
       
        _ShadowColor("阴影颜色",color) = (0,0,0,1)
        _NoLSmooth("半影锐利度",range(0,1)) = 1 // 半影 = 明暗交界线

        _WarmSideColor("暖边颜色",color) =  (1,1,1,1)
        _NoLWarmSide("半影暖边范围",range(0,0.5)) = 0
        [Header(Ramp)]
        _RampMap("Ramp",2D) = "white"{}
        _RampRange("RampRange",range(0,1)) = 1
        _FaceSdf("Sdf图",2D) = "white"{}    // r: sdf左右范围Mask; g: sdf左右贴图
        _FaceShadowBlur("Sdf半影锐利度",range(0,1)) = 0
        _FaceShadowWarmBlur("Sdf暖边范围",range(0,1)) = 0.2

        _MaskTex("_MaskTex",2D) = "black"{}
        _AnisoColor("Aniso颜色",color) = (1,1,1,1)
        _AnisoHairFresnelPow("Aniso范围",range(0,10)) = 5
        _AnisoHairFresnelIntensity("Aniso强度",range(0,5)) = 1

        [HideInInspector] _FaceUpDir("_FaceUpDir",vector) = (1,1,1,1)   // TODO:zheli 
        [HideInInspector] _FaceFrontDir("_FaceFrontDir",vector) = (1,1,1,1)
        [HideInInspector] _FaceRightDir("_FaceRightDir",vector) = (1,1,1,1)

        _Debug("debug",float) = 1
    }
    SubShader
    {
        Tags {"Queue"="Geometry" "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        Pass
        {
            HLSLPROGRAM
            
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma shader_feature_local _HAIR
            #pragma shader_feature_local _FACE

            #define PI 3.1415

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"  
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"   

            struct Attributes
            {
                float4 positionOS   : POSITION;  
                float3 NormalOS : NORMAL;  
                float2 uv : TEXCOORD0;            
                
            };
            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float3 NormalWS : NORMAL;
                float2 uv : TEXCOORD0;
                float fogCoord : TEXCOORD1;
                float3 positionWS   : TEXCOORD2;
                
            };            
            
            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST,_RampMap_ST;
            float4 _MainColor,_ShadowColor,_WarmSideColor,_AnisoColor;
            float4 _FaceUpDir,_FaceFrontDir,_FaceRightDir;
            float _FaceShadowBlur;
            float _FaceShadowWarmBlur;
            float _RampRange;
            float _ReceiveShadowOffset;
            float _NoLSmooth;
            float _NoLWarmSide;
            float _AnisoHairFresnelPow;
            float _AnisoHairFresnelIntensity;

            half _Brightness;
            float _Debug;
            int _is_hair;
            CBUFFER_END

            TEXTURE2D(_MainTex);    SAMPLER(sampler_MainTex);
            TEXTURE2D(_RampMap);    SAMPLER(sampler_RampMap);
            TEXTURE2D(_MaskTex);    SAMPLER(sampler_MaskTex);
            TEXTURE2D(_FaceSdf);    SAMPLER(sampler_FaceSdf); 

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = TRANSFORM_TEX(v.uv,_MainTex);
                o.fogCoord = ComputeFogFactor(o.positionHCS.z);
                o.NormalWS = TransformObjectToWorldNormal(v.NormalOS);
                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                return o;
            }    
            half4 frag(Varyings i) : SV_Target
            {
                // TODO:ALPHA TEST here
                // baseProp
                float4 shadowCoord = TransformWorldToShadowCoord(i.positionWS.xyz);
                Light light = GetMainLight(shadowCoord); 
                half3 lightColor = light.color;
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv) * _MainColor * half4(lightColor.rgb,1);
                half4 shadowColor = texColor * _ShadowColor;
                float shadowPart = light.shadowAttenuation;

                // 向量
                float3 l = normalize(light.direction);
                float3 v = normalize(GetCameraPositionWS() - i.positionWS.xyz);
                float3 n = normalize(i.NormalWS);

                float nol = saturate(dot(n,l));
                float nov = saturate(dot(n,v));
                
                float halfNol = saturate(nol * 0.5 + 0.5);
                float smoothNol = smoothstep(0,_NoLSmooth,nol); // 防止溢出
                float warmSideNol = smoothstep(0,_NoLSmooth,nol - _NoLWarmSide);
                
                

                // aniso
                float anisoPart = 0;
                #if defined(_HAIR)
                    half anisoHairTex = SAMPLE_TEXTURE2D(_MaskTex,sampler_MaskTex,i.uv).r;
                    float anisoHairFrensnel = pow(1-nol,_AnisoHairFresnelPow) * _AnisoHairFresnelIntensity;
                    float anisoHair = saturate(1-anisoHairFrensnel) * anisoHairTex * smoothNol; // 只有在 nol 亮部才有效果
                    anisoPart = anisoHair;
                #endif

                // faceSdf
                // TODO: sdf图的下巴需要全部处理成阴影
                // TODO: 上下sdf
                // TODO: 模型拆分眼球和脸部
                #if defined(_FACE)
                    // face sdf Vector
                    float3 lightProjectDir = normalize(l - _FaceUpDir * dot(l,_FaceUpDir));
                    float LPoF = dot(_FaceFrontDir,lightProjectDir);
                    float LPoFAcos = acos(LPoF) / PI;
                    float dotR = dot(_FaceRightDir.xz,l.xz);    // 判断左右 可以放在xz
                    // face sdf Calculate
                    float faceSdfSign = saturate(sign(dotR) * 0.5 + 0.5); // right=0 left=1
                    float2 faceSdfUV = float2(lerp(i.uv.x,1-i.uv.x,faceSdfSign),i.uv.y);    // 这里根据符号来判断左右脸
                    float faceSdfTex = SAMPLE_TEXTURE2D(_FaceSdf,sampler_FaceSdf,faceSdfUV).g;
                    float faceShadow = smoothstep(LPoFAcos-_FaceShadowBlur, LPoFAcos+_FaceShadowBlur, faceSdfTex);
                    float faceShadowWarmSide = smoothstep(LPoFAcos-_FaceShadowBlur-_FaceShadowWarmBlur, LPoFAcos+_FaceShadowBlur+_FaceShadowWarmBlur, faceSdfTex);
                    float faceSdfMask = SAMPLE_TEXTURE2D(_FaceSdf,sampler_FaceSdf,i.uv).r;
                    smoothNol = faceShadow * faceSdfMask;
                    warmSideNol = faceShadowWarmSide * faceSdfMask;
                #endif

                // ramp
                float rampSide = step(smoothNol,0.95);
                half4 ShadowRamp1 = SAMPLE_TEXTURE2D(_RampMap, sampler_RampMap, float2(halfNol * rampSide, _RampRange));
                half4 rampPart = ShadowRamp1 * texColor;

                // finalColor
                // TODO:暖边颜色根据sun走
                half4 finalColor = lerp(shadowColor,lerp(_WarmSideColor*texColor,texColor,warmSideNol),smoothNol);   // 颜色混合 //lerp(暗部颜色，lerp(亮部和暖边颜色)，nol)
                finalColor += anisoPart * _AnisoColor;    // 混合各向异性部分
                
                // Color Grading
                finalColor.rgb *= _Brightness;

                return finalColor;
            }
            ENDHLSL
        }
    }
}