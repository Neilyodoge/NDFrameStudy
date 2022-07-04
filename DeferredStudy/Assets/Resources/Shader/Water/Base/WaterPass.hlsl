#ifndef WATERPASS
#define WATERPASS

#include "WaterInput.hlsl"

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
    half depthMask = 1 - step(depthDistortionWater, 0.1);
    if (depthDistortionWater < 0.01)
    {
        opaqueUV = ScreenUV;
        depthDistortionWater = depthWater;  // 为了后面让深度部分也扭曲

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
    finalCaustic.rgb = finalCaustic.rgb * saturate(1 - pow(depthWater, _CausticScale)) * _CausticIntensity;

    // highLight
    // ToonHightLight
    // float2 distortionUV2 = float2(frac(_DistortionSpeed.z * _Time.y), frac(_DistortionSpeed.w * _Time.y))
    // + (i.uv * _DistortionTex_ST.xy + _DistortionTex_ST.zw);
     float2 distortionUV2 = float2(_DistortionSpeed.z * _Time.y, _DistortionSpeed.w * _Time.y)
    + (i.uv * _DistortionTex_ST.xy + _DistortionTex_ST.zw);
    half distortionTex2 = TexNoTileY(TEXTURE2D_ARGS(_DistortionTex, sampler_DistortionTex), distortionUV2, _DistortionTex_ST, 3.14); 
    half waterNormal = min(distortionTex.x, distortionTex2);
    half4 ToonSpecular = smoothstep(0.55, 0.65, waterNormal);   // 这里随便算了一下，可控一些可以参数开出来
    ToonSpecular = ToonSpecular * _CartoonSpecular;
    // Blinn-Phong HightLight
    // Ks = 强度; Shininess = 范围
    // Specular = SpecularColor * Ks * pow(max(0,dot(N,H)), Shininess)
    float3 shadowDistortion = i.PositionWS;
    shadowDistortion.xz = shadowDistortion.xz + _DistortionIntensity * distortionTex;
    float4 shadowCoord = TransformWorldToShadowCoord(shadowDistortion);         // shadow采样uv
    Light light = GetMainLight(shadowCoord);  // 加入阴影范围
    half3 N = normalize(bumpWS);    //waterNormal;// normalize(i.NormalWS);
    half3 L = light.direction;
    half3 V = normalize(_WorldSpaceCameraPos.xyz - i.PositionWS);
    half3 H = normalize(V + L);
    half NoH = saturate(dot(N, H));
    half4 specular = _SpecularColor /** max(0, _Specular)*/ * pow(NoH, _Smoothness);
    //混合两种高光
    specular = max(ToonSpecular,specular);

    // Refection
    float3 refectionUV = reflect(N, -V);
    half4 refectionTex = SAMPLE_TEXTURECUBE(_RefectionTex, sampler_RefectionTex, refectionUV);
    ///////// ref叠加没写呢
    
    // foam
    float2 foamUV = float2(_FoamSpeed.x * _Time.y, _FoamSpeed.x * _Time.y) + (i.PositionWS.xz * _FoamTex_ST.xy + _FoamTex_ST.zw);
    half foamTex = TexNoTileY(TEXTURE2D_ARGS(_FoamTex, sampler_FoamTex), foamUV, _DistortionTex_ST, 3.14); 
    half foam = smoothstep(0, foamTex.r * _FoamRange, depthWater);

    // fresnel
    half NoV = saturate(dot(N, V));
    half3 fresnelTint = (1 - NoV * NoV * NoV) * _fresnelScale * _fresnelColor.rgb;    // 三次方更线性

    // color blend
    // depthWater 扭曲
    camColorTex = lerp(finalCaustic, camColorTex, depthMask);                                             // 尝试去掉扭曲产生的边缘噪声
    half4 Tint = lerp(_WaterDepthColor, _WaterColor, saturate(depthDistortionWater));                     // 深水区浅水区颜色区分,这里不限制的话return会有问题
    Tint = lerp(camColorTex, Tint, _WaterAlpha);                                                          // 叠加扭曲
    Tint.rgb = lerp(Tint.rgb, _WaterDepthWSColor.rgb, saturate(depthDistortionWater
    + _DepthForCol) * _WaterDepthWSColor.a);                                                  // 根据深度给染色不同效果
    // return depthDistortionWater;
    Tint.rgb = lerp(Tint.rgb, _FoamTint.rgb, (1 - foam) * _FoamTint.a);                                   // 叠加Foam,a控制强度
    Tint += lerp(specular * _ShadowColor.a, specular, light.shadowAttenuation);                           // 加入高光部分,根据阴影的a来判断是否剔除掉高光部分
    Tint.rgb += finalCaustic.rgb;                                                                         // 加入焦散
    Tint.rgb = lerp(_ShadowColor.rgb * Tint.rgb, Tint.rgb, saturate(light.shadowAttenuation));            // 叠加接受阴影
    Tint.rgb += lerp(0, fresnelTint, _fresnelColor.a);                                                      // 叠加菲尼尔
    Tint.rgb = MixFog(Tint.rgb, i.fogCoord);                                                              // 混合Fog
    // return half4(distortionTex2.rrr,1);
    return saturate(Tint);
    // return half4(specular.rrr, 1);
}

#endif