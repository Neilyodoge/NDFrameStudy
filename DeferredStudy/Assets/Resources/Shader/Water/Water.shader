Shader "Neilyodog/Water"
{
    // 现在颜色混合有问题，混合不出很浅的颜色
    // 泡沫和深度没分开
    Properties
    { 
        _WaterAlpha("Alpha",range(0,1)) = 0.5
        _WaterColor("水颜色",color) = (1,1,1,1)
        _WaterDepthColor("深度颜色",color) = (1,1,1,1)
        _BumpTex("Normal", 2D) = "white"{}
        _BumpScale("法线强度",range(0,2)) = 1
        _NormalSpeed("法线速度",Vector) = (0,0,0,0)

        [Space(10)]
        [Header(Refection))]
        _RefectionIntensity("反射强度",range(0,1)) = 1
        _RefectionTex("反射贴图", Cube) = "white"{}

        [Space(10)]
        [Header(HighLight)]
        [HDR]_SpecularColor("高光颜色",color) = (1,1,1,1)
        _Specular("高光强度",float) = 1
        _Smoothness("高光范围",float) = 1

        [Space(10)]
        [Header(Distortion)]
        _DistortionTex("扭曲", 2D) = "white"{}
        _DistortionIntensity("扭曲强度",range(0,0.1)) = 1
        _DistortionSpeed("扭曲速度",Vector) = (0,0,0,0)

        [Space(10)]
        [Header(Caustic)]
        _CausticTex("焦散贴图", 2D) = "white"{}

        [Space(10)]
        [Header(Foam)]
        _FoamSpeed("泡沫速度/焦散速度",Vector) = (0,0,0,0)
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
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
            };
            struct Varyings
            {
                float4 PositionHCS  : SV_POSITION;
                float2 uv : TEXCOORD0;
                float fogCoord : TEXCOORD1;
                float3 PositionVS : TEXCOORD2;  // view Space
                float3 PositionWS : TEXCOORD3;
                float3 TW1:TEXCOORD4;   // Normal
                float3 TW2:TEXCOORD5;
                float3 TW3:TEXCOORD6;
            };            
            
            CBUFFER_START(UnityPerMaterial)
            float4 _WaterColor, _WaterDepthColor, _SpecularColor;
            float _DepthIntensity;
            half4 _FoamSpeed;
            half4 _FoamTex_ST,_DistortionTex_ST,_BumpTex_ST;
            half _FoamRange;
            half4 _FoamTint;
            half _DistortionIntensity;
            half4 _DistortionSpeed,_NormalSpeed;
            half _Specular, _Smoothness;
            float _BumpScale;
            half _WaterAlpha;
            half4 _CausticTex_ST;
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
                o.TW1 = float3(worldTangent.x,worldBinormal.x,worldNormal.x);
                o.TW2 = float3(worldTangent.y,worldBinormal.y,worldNormal.y);
                o.TW3 = float3(worldTangent.z,worldBinormal.z,worldNormal.z);

                o.uv = v.uv;
                o.fogCoord = ComputeFogFactor(o.PositionHCS.z);
                return o;
            }    
            float3 DecodeNormalWS(float3x3 TW,float4 normalTex)
            {
                float3 bumpWS = UnpackNormal(normalTex);                              //解包，也就是将法线从-1,1重新映射回0,1
                bumpWS.xy *= _BumpScale;
                bumpWS.z = sqrt(1.0 - saturate(dot(bumpWS.xy,bumpWS.xy)));
                bumpWS = mul(TW,bumpWS);             //将切线空间中的法线转换到世界空间中
                return bumpWS;
            }
            half4 frag(Varyings i) : SV_Target
            {
            // Normal
                float3x3 TW = float3x3(i.TW1.xyz,i.TW2.xyz,i.TW3.xyz);
                float2 normalUV = float2(frac(_NormalSpeed.x * _Time.y), frac(_NormalSpeed.y * _Time.y))        // 法线偏移
                                                + (i.uv * _BumpTex_ST.xy + _BumpTex_ST.zw);
                float4 normalTex = SAMPLE_TEXTURE2D(_BumpTex,sampler_BumpTex,normalUV);                         //对法线纹理采样
                float2 normalUV2 = float2(frac(_NormalSpeed.z * _Time.y), frac(_NormalSpeed.w * _Time.y))        
                                                + (i.uv * _BumpTex_ST.xy + _BumpTex_ST.zw);
                float4 normalTex2 = SAMPLE_TEXTURE2D(_BumpTex,sampler_BumpTex,normalUV2);                         
                float3 bumpWS = DecodeNormalWS(TW,normalTex);
                float3 bumpWS2 = DecodeNormalWS(TW,normalTex2);  
                bumpWS = (bumpWS + bumpWS2)/2;      // 混合不同方向的法线                                         
                
            // water depth
                float2 ScreenUV = i.PositionHCS.xy / _ScreenParams.xy;
                half depthTex = SAMPLE_TEXTURE2D(_CameraDepthTexture,sampler_CameraDepthTexture,ScreenUV).r;
                half depthScene = LinearEyeDepth(depthTex,_ZBufferParams);
                half depthWater = depthScene + i.PositionVS.z;  // 相当于把本来深度为0的位置为near改到水面了，变向加强对比度
                depthWater = saturate(pow(depthWater,_DepthIntensity));

            // 焦散
                half4 depthVS = 1; // 观察空间下深度坐标点
                depthVS.xy = i.PositionVS * depthScene / -i.PositionVS.z;
                depthVS.z = depthScene;
                half3 depthWS = mul(unity_CameraToWorld ,depthVS);
                float2 causticUV = float2(frac(_FoamSpeed.z * _Time.y), frac(_FoamSpeed.w * _Time.y))        
                                                + (depthWS.xz * _CausticTex_ST.xy + _CausticTex_ST.zw);
                half4 causticTex = SAMPLE_TEXTURE2D(_CausticTex,sampler_CausticTex,causticUV);      
                float2 causticUV2 = float2(frac(_FoamSpeed.z * _Time.y * 0.5), frac(-_FoamSpeed.w * _Time.y * 0.5))        
                                                + (depthWS.xz * _CausticTex_ST.xy + _CausticTex_ST.zw);
                half4 causticTex2 = SAMPLE_TEXTURE2D(_CausticTex,sampler_CausticTex,causticUV2.yx);    

                return causticTex * causticTex2;

            // Distortion
                float2 distortionUV = float2(frac(_DistortionSpeed.x * _Time.y), frac(_DistortionSpeed.y * _Time.y))
                                            + (i.uv * _DistortionTex_ST.xy + _DistortionTex_ST.zw);
                half distortionTex = SAMPLE_TEXTURE2D(_DistortionTex, sampler_DistortionTex, distortionUV).xy;
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
                half3 N = normalize(bumpWS);//waterNormal;// normalize(i.NormalWS);
                half3 L = light.direction;
                half3 V = normalize(_WorldSpaceCameraPos.xyz - i.PositionWS);
                half3 H = normalize(V + L);
                half NoH = saturate(dot(N,H));
                half4 specular = _SpecularColor * _Specular * pow(NoH,_Smoothness);
                // return half4(NoH.xxx,1);

            // Refection
                float3 refectionUV = reflect(N,-V);
                half4 refectionTex = SAMPLE_TEXTURECUBE(_RefectionTex,sampler_RefectionTex,refectionUV);
                // return refectionTex;
                ///////// 改写ref怎么叠加了
                
            // foam
                float2 foamUV = float2(frac(_FoamSpeed.x * _Time.y),frac(_FoamSpeed.x * _Time.y)) + (i.PositionWS.xz * _FoamTex_ST.xy + _FoamTex_ST.zw);
                half foamTex = SAMPLE_TEXTURE2D(_FoamTex,sampler_FoamTex,foamUV).r;
                half foam = smoothstep(0,foamTex.r * _FoamRange,depthWater);

            // color blend
                half4 Tint = lerp(_WaterDepthColor,_WaterColor,depthWater);     // 深水区浅水区颜色区分
                half4 addfoam = lerp(_FoamTint,Tint * camColorTex,foam);        // 叠加Foam,叠加扭曲
                half4 addHighLight = addfoam + specular;
                // return camColorTex;
                return addHighLight;
                // return half4(addHighLight.xyz,_WaterAlpha);
            }
            ENDHLSL
        }
    }
}
