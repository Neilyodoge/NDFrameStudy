Shader "Neilyodog/Water"
{
    Properties
    { 
        _WaterColor("水颜色",color) = (1,1,1,1)
        _WaterDepthColor("深度颜色",color) = (1,1,1,1)
        
        [Space(10)]
        [Header(HighLight)]
        _SpecularColor("高光颜色",color) = (1,1,1,1)
        _Specular("高光强度",float) = 1
        _Smoothness("高光范围",float) = 1

        [Space(10)]
        [Header(Distortion)]
        _DistortionTex("扭曲", 2D) = "white"{}
        _DistortionIntensity("扭曲强度",range(0,1)) = 1
        _DistortionSpeed("扭曲速度",Vector) = (0,0,0,0)

        [Space(10)]
        [Header(Foam)]
        _FoamSpeed("泡沫速度",Vector) = (0,0,0,0)
        _FoamTex("泡沫纹理",2D) = "white"{}
        _FoamTint("泡沫颜色",color) = (1,1,1,1)
        _FoamRange("泡沫范围",range(0,1)) = 1
        _DepthIntensity("深度强度",range(0,10)) = 1
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            HLSLPROGRAM

            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"  
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"   

            struct Attributes
            {
                float4 positionOS   : POSITION;  
                float2 uv : TEXCOORD0;         
                float3 normalOS : Normal;     
            };
            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float2 uv : TEXCOORD0;
                float fogCoord : TEXCOORD1;
                float3 positionVS : TEXCOORD2;  // view Space
                float3 positionWS : TEXCOORD3;
                float3 normalWS : TEXCOORD4;
            };            
            
            CBUFFER_START(UnityPerMaterial)
            float4 _WaterColor, _WaterDepthColor, _SpecularColor;
            float _DepthIntensity;
            half4 _FoamSpeed;
            half4 _FoamTex_ST,_DistortionTex_ST;
            half _FoamRange;
            half4 _FoamTint;
            half _DistortionIntensity;
            half4 _DistortionSpeed;
            half _Specular, _Smoothness;
            CBUFFER_END

            TEXTURE2D(_FoamTex);
            SAMPLER(sampler_FoamTex);
            TEXTURE2D(_DistortionTex);
            SAMPLER(sampler_DistortionTex);
            TEXTURE2D(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);
            TEXTURE2D(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);

            Varyings vert(Attributes v)
            {
                Varyings o = (Varyings)0;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.positionVS = TransformWorldToView(o.positionWS);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);

                o.uv = TRANSFORM_TEX(v.uv,_FoamTex);
                o.fogCoord = ComputeFogFactor(o.positionHCS.z);
                return o;
            }    
            half4 frag(Varyings i) : SV_Target
            {
            // water depth
                float2 ScreenUV = i.positionHCS.xy / _ScreenParams.xy;
                half depthTex = SAMPLE_TEXTURE2D(_CameraDepthTexture,sampler_CameraDepthTexture,ScreenUV).r;
                half depthScene = LinearEyeDepth(depthTex,_ZBufferParams);
                half depthWater = depthScene + i.positionVS.z;  // 相当于把本来深度为0的位置为near改到水面了，变向加强对比度
                depthWater = saturate(pow(depthWater,_DepthIntensity));

            // highLight
                // Blinn-Phong
                // Ks = 强度; Shininess = 范围
                // Specular = SpecularColor * Ks * pow(max(0,dot(N,H)), Shininess)
                Light light = GetMainLight();
                half3 N = i.normalWS;
                half3 L = light.direction;
                half3 V = _WorldSpaceCameraPos.xyz - i.positionWS;
                half3 H = V + L;
                half NoH = saturate(dot(N,H));
                return half4(NoH.xxx,1);
                

            // Refection

            // 焦散

            // Distortion
                float2 distortionUV = float2(frac(_DistortionSpeed.x * _Time.y), frac(_DistortionSpeed.y * _Time.y))
                                            + (i.uv * _DistortionTex_ST.xy + _DistortionTex_ST.zw);
                half2 distortionTex = SAMPLE_TEXTURE2D(_DistortionTex, sampler_DistortionTex, distortionUV).xy;
                float2 opaqueUV = ScreenUV + _DistortionIntensity * distortionTex;

                half depthDistortionTex = SAMPLE_TEXTURE2D(_CameraDepthTexture,sampler_CameraDepthTexture,opaqueUV).r;
                half depthDistortionScene = LinearEyeDepth(depthDistortionTex,_ZBufferParams);
                half depthDistortionWater = depthDistortionScene + i.positionVS.z;  // 以上三行是为了剔除水面上方扭曲用的
                if(depthDistortionWater<0)opaqueUV = ScreenUV;
                // depthDistortionWater = depthWater;
                // return half4(depthDistortionWater.xxx,1);

                half4 camColorTex = SAMPLE_TEXTURE2D(_CameraOpaqueTexture,sampler_CameraOpaqueTexture,opaqueUV);
                // return camColorTex;

            // foam
                float2 foamUV = float2(frac(_FoamSpeed.x * _Time.y),frac(_FoamSpeed.x * _Time.y)) + (i.positionWS.xz * _FoamTex_ST.xy + _FoamTex_ST.zw);
                half foamTex = SAMPLE_TEXTURE2D(_FoamTex,sampler_FoamTex,foamUV).r;
                half foam = smoothstep(0,foamTex.r * _FoamRange,depthWater);

            // color blend
                half4 Tint = lerp(_WaterDepthColor,_WaterColor,depthWater);     // 深水区浅水区颜色区分
                half4 addfoam = lerp(_FoamTint,Tint * camColorTex,foam);        // 叠加Foam,叠加扭曲
                return addfoam;
            }
            ENDHLSL
        }
    }
}
