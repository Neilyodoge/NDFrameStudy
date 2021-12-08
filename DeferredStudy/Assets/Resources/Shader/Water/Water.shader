Shader "Neilyodog/Water"
{
    Properties
    { 
        _WaterColor("水颜色",color) = (1,1,1,1)
        _WaterDepthColor("深度颜色",color) = (1,1,1,1)
        _BumpMap("Normal Map", 2D) = "bump" {}
        _NormalScale("法线强度",float) = 1
        
        [Space(10)]
        [Header(HighLight)]
        [HDR]_SpecularColor("高光颜色",color) = (1,1,1,1)
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
                float4 PositionOS   : POSITION;  
                float2 uv : TEXCOORD0;         
                float4 NormalOS : Normal;     
                float4 TangentOS : TANGENT;
            };
            struct Varyings
            {
                float4 PositionHCS  : SV_POSITION;
                float2 uv : TEXCOORD0;
                float fogCoord : TEXCOORD1;
                float3 PositionVS : TEXCOORD2;  // view Space
                float3 PositionWS : TEXCOORD3;
                float3 NormalWS : NORMAL;
                float3 TangentWS : TANGENT;
                float3 BTangentWS : TEXCOORD4;
            };            
            
            CBUFFER_START(UnityPerMaterial)
            float4 _WaterColor, _WaterDepthColor, _SpecularColor;
            float _DepthIntensity;
            half4 _FoamSpeed;
            half4 _FoamTex_ST,_DistortionTex_ST,_NormalMap_ST;
            half _FoamRange;
            half4 _FoamTint;
            half _DistortionIntensity;
            half4 _DistortionSpeed;
            half _Specular, _Smoothness;
            half _NormalScale;
            CBUFFER_END

            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);
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
                o.PositionHCS = TransformObjectToHClip(v.PositionOS.xyz);
                o.PositionWS = TransformObjectToWorld(v.PositionOS.xyz);
                o.PositionVS = TransformWorldToView(o.PositionWS);
                o.NormalWS = TransformObjectToWorldNormal(v.NormalOS);

                VertexNormalInputs normalInput = GetVertexNormalInputs(v.NormalOS, v.TangentOS);
                o.NormalWS = normalInput.normalWS;       // viewDir 存进法线的w
                o.TangentWS = normalInput.tangentWS;
                o.BTangentWS = normalInput.bitangentWS;

                o.uv = TRANSFORM_TEX(v.uv,_FoamTex);
                o.fogCoord = ComputeFogFactor(o.PositionHCS.z);
                return o;
            }    
            half4 frag(Varyings i) : SV_Target
            {
            // Normal
                float3x3 T2W = {
                    i.TangentWS,
                    i.BTangentWS,
                    i.NormalWS
                };
                float2 normalUV = i.uv * _NormalMap_ST.xy + _NormalMap_ST.zw;
                real4 normalTex = SAMPLE_TEXTURE2D(_NormalMap,sampler_NormalMap,normalUV);
                normalTex = saturate(normalTex);
                float3 normalTS = UnpackNormalScale(normalTex,_NormalScale);
                normalTS.z = pow((1 - normalTS.x * normalTS.x - normalTS.y * normalTS.y), 0.5);
                float3 norWS = mul(normalTS,T2W);
                

            // water depth
                float2 ScreenUV = i.PositionHCS.xy / _ScreenParams.xy;
                half depthTex = SAMPLE_TEXTURE2D(_CameraDepthTexture,sampler_CameraDepthTexture,ScreenUV).r;
                half depthScene = LinearEyeDepth(depthTex,_ZBufferParams);
                half depthWater = depthScene + i.PositionVS.z;  // 相当于把本来深度为0的位置为near改到水面了，变向加强对比度
                depthWater = saturate(pow(depthWater,_DepthIntensity));

            // Refection

            // 焦散

            // Distortion
                float2 distortionUV = float2(frac(_DistortionSpeed.x * _Time.y), frac(_DistortionSpeed.y * _Time.y))
                                            + (i.uv * _DistortionTex_ST.xy + _DistortionTex_ST.zw);
                half distortionTex = SAMPLE_TEXTURE2D(_DistortionTex, sampler_DistortionTex, distortionUV);
                float2 opaqueUV = ScreenUV + _DistortionIntensity * distortionTex;

                half depthDistortionTex = SAMPLE_TEXTURE2D(_CameraDepthTexture,sampler_CameraDepthTexture,opaqueUV).r;
                half depthDistortionScene = LinearEyeDepth(depthDistortionTex,_ZBufferParams);
                half depthDistortionWater = depthDistortionScene + i.PositionVS.z;  // 以上三行是为了剔除水面上方扭曲用的
                if(depthDistortionWater<0)opaqueUV = ScreenUV;
                // depthDistortionWater = depthWater;
                // return half4(depthDistortionWater.xxx,1);

                half4 camColorTex = SAMPLE_TEXTURE2D(_CameraOpaqueTexture,sampler_CameraOpaqueTexture,opaqueUV);
                // return camColorTex;

            // highLight
                float2 distortionUV2 = float2(frac(_DistortionSpeed.z * _Time.y), frac(_DistortionSpeed.w * _Time.y))
                                                + (i.uv * _DistortionTex_ST.xy + _DistortionTex_ST.zw);
                half distortionTex2 = SAMPLE_TEXTURE2D(_DistortionTex, sampler_DistortionTex, distortionUV2.yx);
                half waterNormal = max(distortionTex, distortionTex2);
                // Blinn-Phong
                // Ks = 强度; Shininess = 范围
                // Specular = SpecularColor * Ks * pow(max(0,dot(N,H)), Shininess)
                Light light = GetMainLight();
                half3 N = normalize(norWS);//waterNormal;// normalize(i.NormalWS);
                
                half3 L = light.direction;
                return half4(saturate(dot(N,L)).xxx,1);
                half3 V = normalize(_WorldSpaceCameraPos.xyz - i.PositionWS);
                half3 H = normalize(V + L);
                half NoH = saturate(dot(N,H));
                half3 specular = _SpecularColor * _Specular * pow(saturate(dot(norWS,L)),_Smoothness);
               // return half4(specular.xxx,1);
                

            // foam
                float2 foamUV = float2(frac(_FoamSpeed.x * _Time.y),frac(_FoamSpeed.x * _Time.y)) + (i.PositionWS.xz * _FoamTex_ST.xy + _FoamTex_ST.zw);
                half foamTex = SAMPLE_TEXTURE2D(_FoamTex,sampler_FoamTex,foamUV).r;
                half foam = smoothstep(0,foamTex.r * _FoamRange,depthWater);

            // color blend
                half4 Tint = lerp(_WaterDepthColor,_WaterColor,depthWater);     // 深水区浅水区颜色区分
                half4 addfoam = lerp(_FoamTint,Tint * camColorTex,foam);        // 叠加Foam,叠加扭曲
                half3 addHighLight = addfoam.xyz + specular;
                return half4(addHighLight,addfoam.a);
            }
            ENDHLSL
        }
    }
}
