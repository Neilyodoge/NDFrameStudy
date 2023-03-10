Shader "BaseToon/Character"
{
    Properties
    { 
        [Toggle(_HAIR)] _is_hair("头发材质",int) = 0
        [Toggle(_FACE)] _is_hair("面部材质",int) = 0
        _MainTex("主贴图",2D) = "white"{}
        _MainColor("主颜色",color) = (1,1,1,1)
        _Brightness("亮度",range(0.5,2)) = 1
       
        _ShadowColor("阴影颜色",color) = (0,0,0,1)
        _NoLSmooth("半影锐利度",range(0,1)) = 1 // 半影 = 明暗交界线

        _WarmSideColor("暖边颜色",color) =  (1,1,1,1)
        _NoLWarmSide("半影暖边范围",range(0,0.5)) = 0
        
        _RampMap("Ramp",2D) = "white"{}
        _RampRange("RampRange",range(0,1)) = 1

        _MaskTex("_MaskTex",2D) = "black"{}
        _AnisoColor("Aniso颜色",color) = (1,1,1,1)
        _AnisoHairFresnelPow("Aniso范围",range(0,10)) = 5
        _AnisoHairFresnelIntensity("Aniso强度",range(0,5)) = 1


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

                // ramp
                float rampSide = step(smoothNol,0.95);
                half4 ShadowRamp1 = SAMPLE_TEXTURE2D(_RampMap, sampler_RampMap, float2(halfNol * rampSide, _RampRange));
                half4 rampPart = ShadowRamp1 * texColor;

                // finalColor
                // TODO:暖边颜色根据sun走
                half4 finalColor = lerp(shadowColor,lerp(_WarmSideColor,texColor,warmSideNol),smoothNol);   // 颜色混合 //lerp(暗部颜色，lerp(暖边颜色)，nol)
                finalColor += anisoPart * _AnisoColor;    // 混合各向异性部分
                
                // Color Grading
                finalColor.rgb *= _Brightness;

                return finalColor;
            }
            ENDHLSL
        }
    }
}