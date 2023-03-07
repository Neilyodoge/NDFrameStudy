#ifndef WATERPASS
#define WATERPASS

#include "WaterInput.hlsl"

struct Attributes
{
    float4 PositionOS: POSITION;
    float2 uv: TEXCOORD0;
    float3 normalOS: NORMAL;
    float4 tangentOS: TANGENT;
    float4 Color : COLOR;
};
struct Varyings
{
    float4 PositionHCS: SV_POSITION;
    float4 Color : COLOR;
    float2 uv: TEXCOORD0;
    float fogCoord: TEXCOORD1;
    float3 PositionVS: TEXCOORD2;  // view Space
    float3 PositionWS: TEXCOORD3;
    float3 tangentWS: TEXCOORD4;
    float3 bitangentWS: TEXCOORD5;
    float3 normalWS: TEXCOORD6;
    float vecterAnim : TEXCOORD7;
    
};

Varyings vert(Attributes v)
{
    Varyings o = (Varyings)0;
    o.Color = v.Color;
    // 法线相关
    VertexNormalInputs normal = GetVertexNormalInputs(v.normalOS, v.tangentOS);
    o.tangentWS = normal.tangentWS;
    o.bitangentWS = normal.bitangentWS;
    o.normalWS = normal.normalWS;
    // 单独算下waterdepth，让边缘的地方顶点动画更弱
    float2 ScreenUV = TransformObjectToHClip(v.PositionOS.xyz).xy / _ScreenParams.xy;
    half depthTex = SAMPLE_TEXTURE2D_LOD(_CameraDepthTexture, sampler_CameraDepthTexture, ScreenUV,0).r;
    half depthScene = LinearEyeDepth(depthTex, _ZBufferParams);
    half depthWater = depthScene + TransformWorldToView(v.PositionOS.xyz).z;

    // 顶点动画部分
    o.PositionWS = TransformObjectToWorld(v.PositionOS.xyz);
    float2 animUV = float2(frac(_VertexAnimSpeed.x * _Time.y), frac(_VertexAnimSpeed.y * _Time.y))        
                    + (o.PositionWS.xz * _VertexAnim_ST.xy + _VertexAnim_ST.zw);
    half animTex = SAMPLE_TEXTURE2D_LOD(_VertexAnim, sampler_VertexAnim,animUV,0).r;
    float3 VertexAnim = GerstnerWave(_WaveA, o.PositionWS, o.tangentWS, o.bitangentWS); // 这里有个问题,法线是贴图前的
    VertexAnim += GerstnerWave(_WaveB, o.PositionWS, o.tangentWS, o.bitangentWS);
    VertexAnim = (VertexAnim + pow(animTex,6) * _VertexIntensity)/2;
    o.PositionWS += VertexAnim * v.Color.r;
    o.vecterAnim = VertexAnim;

    o.PositionVS = TransformWorldToView(o.PositionWS);
    o.PositionHCS = TransformWorldToHClip(o.PositionWS);

    o.uv = o.PositionWS.xz; //v.uv; 这有个小坑，ps部分的i.uv是世界xz
    o.fogCoord = ComputeFogFactor(o.PositionHCS.z);
    return o;
}

half4 frag(Varyings i): SV_Target
{
    // Normal (法线贴图不能反转uv.y来进行采样,因为法线贴图的方向是固定的)
    float3x3 T2W = float3x3(i.tangentWS.xyz, i.bitangentWS.xyz, i.normalWS.xyz);
    float2 normalUV = float2(frac(_NormalSpeed.x * _Time.y), frac(_NormalSpeed.y * _Time.y))        // 法线偏移
                    + (i.uv * _BumpTex_ST.xy + _BumpTex_ST.zw);
    float4 normalTex = SAMPLE_TEXTURE2D(_BumpTex, sampler_BumpTex, normalUV);                         //对法线纹理采样
    float2 normalUV2 = float2(frac(_NormalSpeed.z * _Time.y), frac(_NormalSpeed.w * _Time.y))
                    + (float2(i.uv.x,-i.uv.y) * _DetailBumpTex_ST.xy + _DetailBumpTex_ST.zw);
    float4 detailnormalTex = SAMPLE_TEXTURE2D(_DetailBumpTex, sampler_DetailBumpTex, normalUV2);
    float3 bumpWS = TransformTangentToWorldNormal(T2W, normalTex,_WaterBumpScale);
    float3 detailbumpWS = TransformTangentToWorldNormal(T2W, detailnormalTex,_DetailBumpScale);
    bumpWS = UNDNormal(bumpWS,detailbumpWS);    // 混合不同方向的法线
    float flatNormalDistance = 1-i.PositionHCS.w * _flatNormal;
    bumpWS = lerp(float3(0,1,0),bumpWS,flatNormalDistance);
    
    // water depth  (这里的两个深度都是近处为0)
    float2 ScreenUV = i.PositionHCS.xy / _ScreenParams.xy ;
    half depthTex = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, ScreenUV).r;
    half depthScene = LinearEyeDepth(depthTex, _ZBufferParams);
    half depthWater = depthScene + i.PositionVS.z;  // 相当于把本来深度为0的位置为near改到水面了，变向加强对比度
    
    // Distortion
    float2 distortionUV = float2(frac(_DistortionSpeed.x * _Time.y), frac(_DistortionSpeed.y * _Time.y))
    + (i.uv * _DistortionTex_ST.xy + _DistortionTex_ST.zw);
    half2 distortionTex = SAMPLE_TEXTURE2D(_DistortionTex, sampler_DistortionTex, distortionUV).xy * i.Color.r; // 削弱岸边扭曲
    float2 opaqueUV = ScreenUV + _DistortionIntensity * distortionTex;
    half depthDistortionTex = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, opaqueUV).r;
    half depthDistortionScene = LinearEyeDepth(depthDistortionTex, _ZBufferParams);
    half depthDistortionWater = depthDistortionScene + i.PositionVS.z;  // 以上三行是为了剔除水面上方扭曲用的
    //half depthMask = step(depthDistortionWater, 0.1);
    // 这里实际上就是根据深度去决定用Copy
    if (depthDistortionWater < 0)
    {
        opaqueUV = ScreenUV;
        depthDistortionWater = depthWater;  // 为了后面让深度部分也扭曲
    }
    half4 camColorTex = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, opaqueUV);
    
    // 焦散
    half4 depthVS = 1; 
    depthVS.xy = i.PositionVS.xy * depthDistortionScene / - i.PositionVS.z; // 用 depthDistortionScene 来让焦散扭曲
    depthVS.z = depthDistortionScene;
    half4 depthWS = mul(unity_CameraToWorld, depthVS);
    // 立面 depthWS.y 很trick的做法，相当于根据深度做采样偏移，凑出来一个坐标系
    float2 causticUV = float2(frac(_FoamSpeed.z * _Time.y), frac(_FoamSpeed.w * _Time.y))
                    + (depthWS.xz * _CausticTex_ST.xy + _CausticTex_ST.zw) + (depthWS.y * _CausticFacade);  
    half4 causticTex = SAMPLE_TEXTURE2D(_CausticTex, sampler_CausticTex, causticUV);
    float2 causticUV2 = float2(frac(_FoamSpeed.z * _Time.y * 0.5), frac(-_FoamSpeed.w * _Time.y * 0.5))
                    + (depthWS.xz * _CausticTex_ST.xy + _CausticTex_ST.zw) + (depthWS.y * _CausticFacade);
    half4 causticTex2 = SAMPLE_TEXTURE2D(_CausticTex, sampler_CausticTex, causticUV2.yx);
    half4 finalCaustic = min(causticTex, causticTex2); //causticTex * causticTex2;
    finalCaustic.rgb = finalCaustic.rgb * saturate(1 - pow(depthDistortionWater, _CausticScale)) * _CausticIntensity;
    // return half4(saturate(depthWS.y * _CausticFacade),0,0,1);

    // 高光 & 卡渲高光
    // float2 distortionUV2 = float2(frac(_DistortionSpeed.z * _Time.y), frac(_DistortionSpeed.w * _Time.y))
    // + (i.uv * _DistortionTex_ST.xy + _DistortionTex_ST.zw);
    float2 distortionUV2 = float2(_DistortionSpeed.z * _Time.y, _DistortionSpeed.w * _Time.y)
                            + (i.uv * _DistortionTex_ST.xy + _DistortionTex_ST.zw);
    half distortionTex2 = 0;
    #if _NOTILING                        
        distortionTex2 = noTiling(TEXTURE2D_ARGS(_DistortionTex, sampler_DistortionTex), distortionUV2, _DistortionTex_ST, 3.14); 
    #else
        distortionTex2 = SAMPLE_TEXTURE2D(_DistortionTex, sampler_DistortionTex,distortionUV2);
    #endif  //_NOTILING
    // Blinn-Phong HightLight
    // Ks = 强度; Shininess = 范围
    // Specular = SpecularColor * Ks * pow(max(0,dot(N,H)), Shininess)
    float3 shadowDistortion = i.PositionWS;
    shadowDistortion.xz = shadowDistortion.xz + _DistortionIntensity * distortionTex;
    float4 shadowCoord = TransformWorldToShadowCoord(shadowDistortion);         // shadow采样uv
    Light light = GetMainLight(shadowCoord);  // 加入阴影范围
    float shadowPart = saturate(light.shadowAttenuation);
    float3 N = bumpWS;    //waterNormal;// normalize(i.NormalWS);
    float3 vertexN = normalize(i.normalWS);
    float3 L = light.direction;
    float3 customLDir = normalize(_CustSunPos - i.PositionWS);  // 自定义sun Dir
    L = lerp(L,customLDir,_CustomSunPosON);
    half3 V = normalize(_WorldSpaceCameraPos.xyz - i.PositionWS);
    half3 H = normalize(V + L);
    half NoH = saturate(dot(N, H));
    half LoH = saturate(dot(L, H));
    float NoL = saturate(dot(N, L));
    float3 SH = SampleSH(N);
    // fresnel
    half NoV = saturate(dot(N, V)) * _fresnelScale;
    half fresnelPart = 1 - NoV * NoV * NoV;             // 三次方过度更线性
    fresnelPart = saturate(fresnelPart-(1-shadowPart) * (1-_ShadowColor.a));
    half4 specular = _SpecularColor /** max(0, _Specular)*/ * pow(max(0,NoH), _HeightScale);
    half waterNormal = 1-max(step(abs(bumpWS.r),0.3),step(abs(bumpWS.g),0.3));
    // 卡渲高光
    half4 ToonSpecular = (1-step(NoL,_CartoonSpecularScale)) * _CartoonSpecular;
    ToonSpecular = ToonSpecular * i.vecterAnim;   // 这里跟顶点动画强度做了个运算
    //混合两种高光
    specular = max(ToonSpecular,specular);

    // Refection
    float3 refectionUV = reflect(N, -V);
    half4 refectionTex = SAMPLE_TEXTURECUBE(_RefectionTex, sampler_RefectionTex, refectionUV);
    ///////// ref叠加没写呢
    
    // foam
    float2 foamUV = float2(_FoamSpeed.x * _Time.y, _FoamSpeed.x * _Time.y) + (i.PositionWS.xz * _FoamTex_ST.xy + _FoamTex_ST.zw);
    half foamTex = 0;
    #if _NOTILING 
        foamTex = noTiling(TEXTURE2D_ARGS(_FoamTex, sampler_FoamTex), foamUV, _DistortionTex_ST, 3.14) ; 
    #else
        foamTex = SAMPLE_TEXTURE2D(_FoamTex, sampler_FoamTex,foamUV);
    #endif
    half foam = smoothstep(0, foamTex.r * _FoamRange, depthWater);
    // waterSide
    float waterSide = depthWater * ((i.PositionWS.y - _FoamHeight) * sin((_Time.y * _FoamSpeed.y) * 10)/10+1);
    waterSide = smoothstep(0,_FoamSide,waterSide) * max((1-i.vecterAnim),depthDistortionWater);

    // color blend
    half4 Tint = half4(0,0,0,1);
    Tint.rgb = lerp(_WaterSideColor.rgb, _WaterColor.rgb, saturate(depthDistortionWater));                        // 深水区浅水区颜色区分,这里不限制的话return会有问题
    Tint.rgb = lerp(Tint.rgb,_WaterDepthWSColor, saturate(depthDistortionWater + _DepthForCol) * _WaterDepthWSColor.a); // 根据深度给染色不同效果,前面这样混合效果最好
    half3 NoLColorBlend = 0; // init
    UNITY_BRANCH
    if(_UseRamp < 0.5)  // 区分用ramp混还是用前面的颜色混
        NoLColorBlend = lerp(_WaterDepthWSColor.rgb * _WaterDepthWSColor.a,_WaterSideColor, NoL);   // 根据NoL混合颜色
    else
        NoLColorBlend = SAMPLE_TEXTURE2D(_ramp,sampler_ramp,float2(NoL * 0.5 +0.5,0.1));            // 半兰伯特过度自然，ramp更可控
    float3 outputBlend = (Tint.rgb+NoLColorBlend)/2;                                                        // 根据NoL混色
    Tint.rgb = lerp(Tint.rgb,outputBlend,_UseBlend);                                                        
    Tint.rgb = lerp(camColorTex, Tint, _WaterAlpha);                                                        // 扭曲部分颜色强度
    Tint.rgb = lerp(Tint.rgb, _FoamTint.rgb, (1 - foam) * _FoamTint.a);                                     // 叠加Foam,a控制强度
    Tint.rgb += lerp(specular * _ShadowColor.a, specular, shadowPart);                                      // 加入高光部分,_ShadowColor.a控制“阴影内高光”强度
    Tint.rgb += lerp(finalCaustic.rgb * _ShadowColor.a,finalCaustic.rgb,shadowPart);                        // 加入焦散。焦散不能放在camColorTex一起是因为camColorTex占比很低
    Tint.rgb = lerp(_ShadowColor.rgb * Tint.rgb, Tint.rgb, shadowPart);                                     // 叠加接受阴影
    Tint.rgb = lerp(Tint.rgb,max(Tint.rgb, _fresnelColor.rgb * SH * _fresnelColor.a), fresnelPart);         // 叠加菲尼尔
    Tint.rgb *= lerp(1,SH,_SHIntensity);                                                                    // 混合SH，这种算法更适应特别暗的环境
    Tint.rgb = MixFog(Tint.rgb, i.fogCoord);                                                                // 混合Fog
    #if _WATERSIDE
        Tint.rgb = lerp(camColorTex * _WaterSideTint.rgb,Tint,saturate(smoothstep(0,2,waterSide)));             // 岸边,还是跟camColorTex lerp
        float damp = smoothstep(0,2-1.9,waterSide) * (1-smoothstep(0,2,waterSide));                             // 算出潮湿部分
        Tint.rgb = lerp(Tint.rgb,Tint.rgb*_WaterSideTint,damp);                                                 // 潮湿部分叠加
        Tint.a *= smoothstep(0,_DampSide,waterSide);                                                            // 叠加透明，让最边缘的地方软一些
    #endif 

    #if _DEBUGMODE
        switch(_Debug)
        {
            case 0: // 岸边透明部分
            Tint.rgb = damp;
            break;
            case 1: // 水23阶深度
            Tint.rgb = saturate(depthDistortionWater + _DepthForCol);
            break;
            case 2: // SH
            Tint.rgb = SH;
            break;
            case 3: // 菲尼尔部分
            Tint.rgb = fresnelPart;
            break;
            case 4: // 阴影部分
            Tint.rgb = shadowPart;
            break;
            case 5: // 顶点动画
            Tint.rgb = i.vecterAnim;
            break;
            case 6: // 高光部分
            Tint.rgb =  specular;
            break;
            case 7: // 法线平整距离
            Tint.rgb = 1-i.PositionHCS.w * _flatNormal;
            break;
            case 8: // 顶点色
            Tint.rgb = i.Color.rgb;
            break;
        }
    #endif

    
    return saturate(Tint);
}

#endif