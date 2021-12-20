Shader "Neilyodog/Water"
{
    // Neilyodog 2021.12.13
    // 混合部分有一些问题
    // 1.透明度控制并不是很理想
    // 2.泡沫和深度没分开
    // cube反射暂时没网上叠
    // fresnel 特性没写

    // 深度的时候多少会有一丢丢误差

    // receive 得写GUI
    Properties
    {
       
        _WaterAlpha ("Alpha", range(0, 1)) = 0.5    // 没用
        _WaterColor ("水颜色", color) = (1, 1, 1, 1)
        _WaterDepthColor ("边缘深度颜色", color) = (1, 1, 1, 1)
        _WaterDepthWSColor ("深度颜色", color) = (1, 1, 1, 1)
        _DepthForCol ("深度颜色范围", float) = 1   

        _BumpTex ("Normal", 2D) = "white" { }
        _BumpScale ("法线强度", range(0, 2)) = 1
        _NormalSpeed ("法线速度", Vector) = (0, 0, 0, 0)

        [Space(10)]
        [Header(fresnel)]
        _fresnelScale("菲尼尔范围",float) = 5   
        _fresnelColor("菲尼尔颜色",color) = (1,1,1,1)

        [Space(10)]
        [Header(Shadow)]
        _ShadowColor ("阴影颜色", color) = (1, 1, 1, 1)

        [Space(10)]
        [Header(Refection))]
        _RefectionIntensity ("反射强度", range(0, 1)) = 1
        _RefectionTex ("反射贴图", Cube) = "white" { }

        [Space(10)]
        [Header(HighLight)]
        [HDR]_SpecularColor ("高光颜色", color) = (1, 1, 1, 1)
        _Specular ("高光强度", float) = 1
        _Smoothness ("高光范围", float) = 1

        [Space(10)]
        [Header(Distortion)]
        _DistortionTex ("扭曲", 2D) = "white" { }
        _DistortionIntensity ("扭曲强度", range(0, 0.3)) = 1
        _DistortionSpeed ("扭曲速度", Vector) = (0, 0, 0, 0)

        [Space(10)]
        [Header(Caustic)]
        _CausticTex ("焦散贴图", 2D) = "white" { }
        _CausticIntensity ("焦散强度", float) = 1
        _CausticScale ("焦散范围", float) = 1
        _CausticFacade ("焦散立面", range(0, 0.5)) = 0.15

        [Space(10)]
        [Header(Foam)]
        _FoamSpeed ("泡沫速度/焦散速度", Vector) = (0, 0, 0, 0)
        _FoamTex ("泡沫纹理", 2D) = "white" { }
        _FoamTint ("泡沫颜色", color) = (1, 1, 1, 1)
        _FoamRange ("泡沫范围", range(0, 1)) = 1
        _DepthIntensity ("深度强度", range(0, 10)) = 1
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
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
            // receive shadow
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT // 软阴影

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            struct Attributes
            {
                float4 PositionOS: POSITION;
                float2 uv: TEXCOORD0;
                float3 normalOS: NORMAL;
                float4 tangentOS: TANGENT;
            };
            struct Varyings
            {
                float4 PositionHCS: SV_POSITION;
                float2 uv: TEXCOORD0;
                float fogCoord: TEXCOORD1;
                float3 PositionVS: TEXCOORD2;  // view Space
                float3 PositionWS: TEXCOORD3;
                float3 TW1: TEXCOORD4;   // Normal
                float3 TW2: TEXCOORD5;
                float3 TW3: TEXCOORD6;
            };
            
            
            CBUFFER_START(UnityPerMaterial)
            float4 _WaterColor, _WaterDepthColor, _SpecularColor;
            float _DepthIntensity;
            half4 _FoamSpeed;
            half4 _FoamTex_ST, _DistortionTex_ST, _BumpTex_ST;
            half _FoamRange;
            half4 _FoamTint;
            half _DistortionIntensity;
            half4 _DistortionSpeed, _NormalSpeed;
            half _Specular, _Smoothness;
            float _BumpScale;
            half _WaterAlpha;
            half4 _CausticTex_ST;
            half _CausticIntensity;
            half4 _ShadowColor;
            half _CausticScale;
            half _CausticFacade;
            half _DepthForCol;
            half4 _WaterDepthWSColor;
            half _fresnelScale;
            half4 _fresnelColor;
            CBUFFER_END

            TEXTURE2D(_BumpTex);    SAMPLER(sampler_BumpTex);
            TEXTURE2D(_FoamTex);    SAMPLER(sampler_FoamTex);
            TEXTURE2D(_DistortionTex);          SAMPLER(sampler_DistortionTex);
            TEXTURE2D(_CameraDepthTexture);     SAMPLER(sampler_CameraDepthTexture);
            TEXTURE2D(_CameraOpaqueTexture);    SAMPLER(sampler_CameraOpaqueTexture);
            TEXTURECUBE(_RefectionTex);         SAMPLER(sampler_RefectionTex);
            TEXTURE2D(_CausticTex);             SAMPLER(sampler_CausticTex);

            Varyings vert(Attributes v)
            {
                Varyings o = (Varyings)0;
                o.PositionHCS = TransformObjectToHClip(v.PositionOS.xyz);
                o.PositionWS = TransformObjectToWorld(v.PositionOS.xyz);
                o.PositionVS = TransformWorldToView(o.PositionWS);
                // o.NormalWS = TransformObjectToWorldNormal(v.NormalOS);

                VertexNormalInputs normal = GetVertexNormalInputs(v.normalOS, v.tangentOS);
                float3 worldNormal = normal.normalWS;
                float3 worldTangent = normal.tangentWS;
                float3 worldBinormal = normal.bitangentWS;
                o.TW1 = float3(worldTangent.x, worldBinormal.x, worldNormal.x);
                o.TW2 = float3(worldTangent.y, worldBinormal.y, worldNormal.y);
                o.TW3 = float3(worldTangent.z, worldBinormal.z, worldNormal.z);

                o.uv = v.uv;
                o.fogCoord = ComputeFogFactor(o.PositionHCS.z);
                return o;
            }
            float3 DecodeNormalWS(float3x3 TW, float4 normalTex)
            {
                float3 bumpWS = UnpackNormal(normalTex);                              //解包，也就是将法线从-1,1重新映射回0,1
                bumpWS.xy *= _BumpScale;
                bumpWS.z = sqrt(1.0 - saturate(dot(bumpWS.xy, bumpWS.xy)));
                bumpWS = mul(TW, bumpWS);             //将切线空间中的法线转换到世界空间中
                return bumpWS;
            }
            
            half4 frag(Varyings i): SV_Target
            {
            // Normal
                float3x3 TW = float3x3(i.TW1.xyz, i.TW2.xyz, i.TW3.xyz);
                float2 normalUV = float2(frac(_NormalSpeed.x * _Time.y), frac(_NormalSpeed.y * _Time.y))        // 法线偏移
                + (i.uv * _BumpTex_ST.xy + _BumpTex_ST.zw);
                float4 normalTex = SAMPLE_TEXTURE2D(_BumpTex, sampler_BumpTex, normalUV);                         //对法线纹理采样
                float2 normalUV2 = float2(frac(_NormalSpeed.z * _Time.y), frac(_NormalSpeed.w * _Time.y))
                + (i.uv * _BumpTex_ST.xy + _BumpTex_ST.zw);
                float4 normalTex2 = SAMPLE_TEXTURE2D(_BumpTex, sampler_BumpTex, normalUV2);
                float3 bumpWS = DecodeNormalWS(TW, normalTex);
                float3 bumpWS2 = DecodeNormalWS(TW, normalTex2);
                bumpWS = (bumpWS + bumpWS2) / 2;      // 混合不同方向的法线
                
                
            // water depth
                float2 ScreenUV = i.PositionHCS.xy / _ScreenParams.xy ;
                half depthTex = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, ScreenUV).r;
                half depthScene = LinearEyeDepth(depthTex, _ZBufferParams);
                half depthWater = depthScene + i.PositionVS.z;  // 相当于把本来深度为0的位置为near改到水面了，变向加强对比度
                depthWater = saturate(abs(depthWater) * _DepthIntensity);

                
            // Distortion
                float2 distortionUV = float2(frac(_DistortionSpeed.x * _Time.y), frac(_DistortionSpeed.y * _Time.y))
                                    + (i.uv * _DistortionTex_ST.xy + _DistortionTex_ST.zw);
                half2 distortionTex = SAMPLE_TEXTURE2D(_DistortionTex, sampler_DistortionTex, distortionUV).xy;
                float2 opaqueUV = ScreenUV + _DistortionIntensity * distortionTex;
                half depthDistortionTex = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, opaqueUV).r;
                half depthDistortionScene = LinearEyeDepth(depthDistortionTex, _ZBufferParams);
                half depthDistortionWater = depthDistortionScene + i.PositionVS.z;  // 以上三行是为了剔除水面上方扭曲用的
            // 扭曲这里边缘有点问题
                half depthMask = 1-step(depthDistortionWater,0.1);
                if (depthDistortionWater < 0.01)
                {
                    opaqueUV = ScreenUV;
                    //depthDistortionWater = half4(lerp(depthDistortionWater.xxx,depthWater,depthMask),1);;  // 为了后面让深度部分也扭曲
                }
                half4 camColorTex = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, opaqueUV);

            // 焦散
                half4 depthVS = 1; // 观察空间下深度坐标点
                depthVS.xy = i.PositionVS.xy * depthDistortionScene / - i.PositionVS.z; // 用 depthDistortionScene 来让焦散扭曲
                depthVS.z = depthScene;
                half4 depthWS = mul(unity_CameraToWorld, depthVS);
                float2 causticUV = float2(frac(_FoamSpeed.z * _Time.y), frac(_FoamSpeed.w * _Time.y))
                + (depthWS.xz * _CausticTex_ST.xy + _CausticTex_ST.zw) + (depthWS.y * _CausticFacade);
                half4 causticTex = SAMPLE_TEXTURE2D(_CausticTex, sampler_CausticTex, causticUV);
                float2 causticUV2 = float2(frac(_FoamSpeed.z * _Time.y * 0.5), frac(-_FoamSpeed.w * _Time.y * 0.5))
                + (depthWS.xz * _CausticTex_ST.xy + _CausticTex_ST.zw) + (depthWS.y * _CausticFacade);
                half4 causticTex2 = SAMPLE_TEXTURE2D(_CausticTex, sampler_CausticTex, causticUV2.yx);
                half4 finalCaustic = min(causticTex, causticTex2); //causticTex * causticTex2;
                // return causticTex;

            // highLight
                float2 distortionUV2 = float2(frac(_DistortionSpeed.z * _Time.y), frac(_DistortionSpeed.w * _Time.y))
                + (i.uv * _DistortionTex_ST.xy + _DistortionTex_ST.zw);
                half distortionTex2 = SAMPLE_TEXTURE2D(_DistortionTex, sampler_DistortionTex, distortionUV2.yx).x;
                half waterNormal = max(distortionTex.x, distortionTex2);
                // Blinn-Phong
                // Ks = 强度; Shininess = 范围
                // Specular = SpecularColor * Ks * pow(max(0,dot(N,H)), Shininess)
                float3 shadowDistortion = i.PositionWS;
                shadowDistortion.xz = shadowDistortion.xz + _DistortionIntensity * distortionTex;
                float4 shadowCoord = TransformWorldToShadowCoord(shadowDistortion);         // shadow采样uv
                
                Light light = GetMainLight(shadowCoord);  // 加入阴影范围
                // return half4(light.shadowAttenuation.xxx,1);
                half3 N = normalize(bumpWS);    //waterNormal;// normalize(i.NormalWS);
                half3 L = light.direction;
                half3 V = normalize(_WorldSpaceCameraPos.xyz - i.PositionWS);
                half3 H = normalize(V + L);
                half NoH = saturate(dot(N, H));
                half4 specular = _SpecularColor * max(0, _Specular) * pow(NoH, _Smoothness);

            // Refection
                float3 refectionUV = reflect(N, -V);
                half4 refectionTex = SAMPLE_TEXTURECUBE(_RefectionTex, sampler_RefectionTex, refectionUV);
                ///////// 改写ref怎么叠加了
                
            // foam
                float2 foamUV = float2(frac(_FoamSpeed.x * _Time.y), frac(_FoamSpeed.x * _Time.y)) + (i.PositionWS.xz * _FoamTex_ST.xy + _FoamTex_ST.zw);
                half foamTex = SAMPLE_TEXTURE2D(_FoamTex, sampler_FoamTex, foamUV).r;
                half foam = smoothstep(0, foamTex.r * _FoamRange, depthWater);

            // fresnel
                half NoV = saturate(dot(N,V));
                half3 fresnelTint = (1-NoV) * _fresnelScale * _fresnelColor.rgb;
                // return 1-NoV;

            // color blend
                // depthWater 扭曲
                half4 Tint = lerp(_WaterDepthColor, _WaterColor, saturate(depthDistortionWater));                     // 深水区浅水区颜色区分
                Tint = lerp(camColorTex, Tint, _WaterAlpha);                                                          // 叠加扭曲
                Tint.rgb = lerp( Tint.rgb,_WaterDepthWSColor.rgb, saturate(depthDistortionWater 
                            + _DepthForCol) * _WaterDepthWSColor.a * depthMask);                                      // 根据深度给染色不同效果
                Tint.rgb = lerp(Tint.rgb, _FoamTint.rgb, (1 - foam) * _FoamTint.a);                                   // 叠加Foam,a控制强度
                Tint += lerp(specular * _ShadowColor.a, specular, light.shadowAttenuation);                           // 加入高光部分,根据阴影的a来判断是否剔除掉高光部分
                Tint.rgb += finalCaustic.rgb * saturate(1 - pow(depthWater, _CausticScale)) * _CausticIntensity;      // 加入焦散
                Tint.rgb = lerp(_ShadowColor.rgb * Tint.rgb, Tint.rgb, saturate(light.shadowAttenuation));            // 叠加接受阴影
                Tint.rgb += lerp(0,fresnelTint,_fresnelColor.a);                                                      // 叠加菲尼尔
                Tint.rgb = MixFog(Tint.rgb, i.fogCoord);                                                              // 混合Fog
                return saturate(Tint);
            }
            ENDHLSL

        }
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM

            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature _ALPHATEST_ON

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL

        }
    }
}
