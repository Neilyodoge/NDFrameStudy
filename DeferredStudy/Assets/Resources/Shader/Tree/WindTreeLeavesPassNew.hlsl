#ifndef WINDTREELEAVESPASS
#define WINDTREELEAVESPASS

/// 储存树叶的渲染PASS
#include "WindTreeLeavesInput.hlsl"
//* shadowPass里好像某个include会采样这张图造成重复定义
TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);

v2f vert(appdata v)
{
    v2f o = (v2f)0;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    o.uv = v.uv;
    o.normalWS = TransformObjectToWorldNormal(v.normalOS);
    o.baseNormal = TransformObjectToWorldNormal(v.tangentOS);

    float3 v_posWorld;
    VertexPositionInputs vertexInput = GetVertexPositionInputs(v.positionOS.xyz);
    v_posWorld = vertexInput.positionWS;

    //* wanghaoyu 计算垂直颜色变化,这里改成local的变化,场景里的树难免会有高有低
    float height = v.positionOS.y;// - objectPivot.y;
    o.treeParam = saturate((height - _TreeLerpRoot) / (_TreeLerpTop - _TreeLerpRoot) * _TreeLerpIntensity);

    float2 worldRotUV;
    float debug = 1;
    float windSpeed;
    float windSineOffset = 0;
    //* wanghaoyu
    #ifdef _VERTEXANIMTION
        // 增加风的影响
        v_posWorld.xyz = ApplyWind(v.color.r, v_posWorld.xyz, windSineOffset, worldRotUV, windSpeed, debug);
    #endif
    #if _DEBUGMODE
        o.debugWind = debug;
        o.VertexColor = v.color;
    #endif
    o.ambient.rgb = 0;
    #ifdef _LIGHTPROBE
        // 计算环境光
        o.ambient.rgb = SampleSH(o.normalWS);
    #endif
    // 环境光的A通道存入顶点色的R通道，作为模拟AO，但是对于海贼王项目来说可能不需要。
    o.ambient.a = v.color.r;
    o.positionWS = v_posWorld;
    o.positionHCS = mul(UNITY_MATRIX_VP, float4(v_posWorld, 1));
    return o;
}

struct LitFragmentOutput
{
    #if _MRTEnable
        float4 color0: SV_Target0;
        float4 color1: SV_Target1;
    #else
        float4 color0: SV_Target0;
    #endif
};

half4 frag(v2f i): SV_Target
{
    //* wanghaoyu 说是移动端dither不放在前面的话，着色部分也会计算
    half4 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);
    float3 viewDir = SafeNormalize(GetCameraPositionWS() - i.positionWS.xyz);
    // float clipPart = 1 - abs(dot(i.baseNormal, viewDir));
    // // float3 posObj = TransformObjectToWorld(float3(0,0,0)); 整体dither
    // float dis = distance(float3(_WorldSpaceCameraPos.x, 0, _WorldSpaceCameraPos.z), float3(i.positionWS.x, 0, i.positionWS.z)/*posObj*/);
    // float clipValue = 0;
    // Unity_Dither(smoothstep(_DitherAmountMin, _DitherAmountMax, dis), i.positionHCS.xy, clipValue);
    // clip(min((baseColor.a * lerp((int)1,_CutIntensity,(int)_FlatClip) - clipPart), clipValue)); // min 是为了处理Dither ,前面*强度是为了不做除法

    UNITY_SETUP_INSTANCE_ID(i);
    
    // Lerp Color
    half3 tintColorTop = lerp(float3(1, 1, 1), _BaseColor.rgb, _BaseColor.a);
    half3 tintColorRoot = lerp(float3(1, 1, 1), _LerpColor.rgb, _LerpColor.a);
    half3 tintColor = lerp(tintColorRoot, tintColorTop, i.treeParam);
    half3 baseTexColor = baseColor.rgb * tintColor;
    baseColor.rgb = baseTexColor;

    // 屏蔽逐instancing差异
    /*
    float4 tintColor = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
    baseColor.rgb *= tintColor;

    float4 fade = UNITY_ACCESS_INSTANCED_PROP(Props, _FadeTime);
    UnityApplyDitherCrossFade(i.positionHCS.xy, fade);
    */

    float4 shadowCoord = TransformWorldToShadowCoord(i.positionWS.xyz);
    Light light = GetMainLight(shadowCoord);
    float shadowAtten = light.shadowAttenuation;
    half3 lightColor = light.color.rgb;
    // 白糖的秘籍。
    // 白糖这里思考的是，大气散射不会被存进probe中，手动算了个影响，sh该曝就曝吧
    float3 SHNOL = 0;
    #ifdef _LIGHTPROBE
        float vertexNdotL = max(0, dot(i.normalWS, _MainLightPosition.xyz));
        vertexNdotL = lerp(vertexNdotL * _SHDarkPart,vertexNdotL,shadowAtten);
        SHNOL = i.ambient.rgb + vertexNdotL* _MainLightColor.rgb ;//* _VertexLightIntensity;
    #endif
    
    half ao = saturate(i.ambient.a / _AORange);
    float3 L = normalize(light.direction);
    float3 V = normalize(_WorldSpaceCameraPos.xyz - i.positionWS.xyz);
    float3 H = normalize(V + L);
    float3 N = normalize(i.normalWS);
    float NoH = saturate(dot(N, H));
    float NoL = saturate(dot(N, L));
    float NoV = saturate(dot(N,V));
    float positiveNL = saturate((NoL - _ToonCutPos) /** _ToonCutSharpness*/);   // 项目不需要调整边缘软硬
    
    float NoLAndShadow = positiveNL * shadowAtten;
    float GrayPart = saturate(lerp(1, i.ambient.a, _FaceLightGrayScale));  // 亮部灰阶
    float subSurfaceTerm = 0;
    #ifdef _SUBSURFACE
        float VoL = saturate(-dot(L, viewDir));
        VoL = saturate(VoL * VoL * VoL);    // 让vol变得更线性，这样算透光的时候接近0的时候有更多灰阶
        subSurfaceTerm = saturate(VoL * saturate(NoL - _SubSurfaceScale)) * ao * _SubSurfaceGain;
    #endif
    GrayPart *= 1 - NoLAndShadow;                                           // 只想让这个叠加进 nol 小于0的部分
    float darkPart = min(NoLAndShadow + GrayPart * saturate(NoL * 0.5 + 0.5) * _FaceLightGrayIntensity * shadowAtten, 1);   // NoLAndShadow 人工提亮应该去掉
    //---反射部分---
    half refSide = saturate(max(max(NoV*2.5,(1-i.ambient.a)),NoL*0.5+0.5));// *2.5 是为了更好的剔除中间
    float refDis = length(_WorldSpaceCameraPos.xyz - i.positionWS.xyz); //反射有效距离
    float ratio = saturate(refDis / _refDis);
    ratio = ratio*ratio*ratio; //pow3
    float refPart = saturate(ratio * (1-pow(refSide,_refScale)));   // pow((1-refSide),_refScale) 用来控制边缘范围
    //---高光部分---
    float bNoH = dot(normalize(i.baseNormal + N),H);  // 用奇怪的混合方法达到了期望的效果
    float heightLightPart = pow(max(0,bNoH),_heightLightSmooth);
    heightLightPart = heightLightPart * NoLAndShadow * i.treeParam; // 这里不想高光对GrayPart有影响，所以要用 NoLAndShadow
    float3 heightLightTint = _heightLightColor.rgb; // 跟sh挂钩
    
    half4 f_finalColor = float4(baseColor.rgb, 1);                              // 固有色
    f_finalColor.rgb *= lightColor;                                             // 固有色 + 直射光色
    f_finalColor.rgb += lerp(0,heightLightTint,heightLightPart);                // 高光
    f_finalColor.rgb *= lerp(_DarkColor.rgb, _LightIntensity,darkPart);         // 固有色 + 暗部
    f_finalColor.rgb += baseTexColor * SHNOL * _SHIntensity;                    // 固有色 + 环境光
    f_finalColor.rgb *= lerp(_AOTint.rgb, 1, ao);                               // AO
    f_finalColor.rgb *= lerp(1,_SubSurfaceGain,subSurfaceTerm);                 // 透射
    f_finalColor.rgb += lerp(0,i.ambient.rgb/2 * _refIntensity,refPart);        // 固有色的暗部 + 反射光色
    

    half gray = 0.21 * f_finalColor.x + 0.72 * f_finalColor.y + 0.072 * f_finalColor.z;
    f_finalColor.rgb = lerp(float3(gray, gray, gray), f_finalColor.rgb, _saturate);     // 饱和度
    f_finalColor.a = baseColor.a;

    // HardRim
    //* wanghaoyu HardRim好像暂时不打算用了,要调整的话可以试试把深度图往光源的反方向推

    float hardRimMask = 0;
    float hardRim = 0;
    #if defined(_HARDRIM)
        float2 HardRimScreenUV = (i.positionHCS.xy / _ScreenParamsCompatible.xy - 0.5) * (1 + _HardRimWidth) + 0.5;
        float depthTex = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, HardRimScreenUV).r;
        hardRim = 1 - depthTex.xxx * max(0, _HardRimDistanceIntensity);
        hardRimMask = 1 - step(i.positionHCS.z * 100, _LodMask);
        float hardRimScale = saturate(hardRimMask * hardRim);
        f_finalColor.rgb = lerp(f_finalColor.rgb, _HardRimTint/*f_finalColor.rgb*/, hardRimScale);
        // f_finalColor.rgb += f_finalColor.rgb * max(0, hardRim) * hardRimMask * lerp(_HardRimIntensity2,_HardRimIntensity,NoL);
        // lod有具体米数么，如果有可以直接调_HardRimDistanceIntensity调到内个范围，主要这个确定了才好确定距离（现在想用step控制）怎么调整
        // hardrim是多远都有么，lod还没适配hardrim
    #endif

    // // 增加点光源
    // float4 shadowmask = float4(0, 0, 0, 0);
    // #ifdef _ADDITIONAL_LIGHTS
    //     uint pixelLightCount = GetAdditionalLightsCount();
    //     for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
    //     {
    //         //* pwrd majiao: 支持Shadowmask //
    //         Light light = GetAdditionalLight(lightIndex, i.positionWS.xyz, shadowmask);
    //         //* pwrd majiao //

    //         // 增加普通的Lambert 光照模型计算点光源
    //         float3 lightColor = light.color * light.distanceAttenuation * light.shadowAttenuation;
    //         f_finalColor.rgb += saturate(dot(normalize(i.normalWS), L)) * lightColor * baseColor.rgb;
    //     }
    // #endif

    //*	//在战斗时 会压暗场景 突出角色
    //f_finalColor.rgb *= _SceneFocusIntensity;
    
    // Debug 用
    #if _DEBUGMODE
        switch(_Debug)
        {
            case 0:
            f_finalColor.rgb = ao;
            break;
            case 1:
            f_finalColor.rgb = subSurfaceTerm;
            break;
            case 2:
            f_finalColor.rgb = darkPart;
            break;
            case 3:
            f_finalColor.rgb = lightColor;
            break;
            case 4:
            f_finalColor.rgb = i.ambient.rgb * _SHIntensity;
            break;
            case 5:
            f_finalColor.rgb = i.debugWind;
            break;
            case 6:
            f_finalColor.rgb = i.treeParam;
            break;
            case 7:
            f_finalColor.rgb = hardRimMask * hardRim;
            break;
            case 8:
            f_finalColor.rgb = i.VertexColor.rrr;
            break;
            case 9:
            f_finalColor.rgb = i.VertexColor.ggg;
            break;
            case 10:
            f_finalColor.rgb = i.VertexColor.bbb;
            break;
            case 11:
            f_finalColor.rgb = i.VertexColor.aaa;
            break;
            case 12:
            f_finalColor.rgb = lerp(0,i.ambient.rgb/2 * _refIntensity,refPart);//saturate(max(max(dot(viewDir,i.normalWS)*2.5,(1-i.ambient.a)*0.5),NoL*0.5+0.5));//
            break;
            case 13:
            f_finalColor.rgb = heightLightPart;//
            break;
        }
    #endif
    
    // LitFragmentOutput output = (LitFragmentOutput)0;
    
    // output.color0 = f_finalColor;
    
    // #if _MRTEnable
    //     //tree - bloom
    //     float expandBloomValue = 0;
    //     float bloomValue = _CustomBloomIntensity * 0.5;
    //     bloomValue *= _SceneBloomIntensity;
        
    //     output.color1 = float4(expandBloomValue, bloomValue, 0, saturate(f_finalColor.a + _CustomBloomAlphaOffset));
    //     //output.color1 = float4(0, 0, 0, 0);
    // #endif
    return f_finalColor;
}
v2fDepth vertDepth(appdataDepth v)
{
    v2fDepth o = (v2fDepth)0;
    o.uv = v.uv;
    o.baseNormal = TransformObjectToWorldNormal(v.tangentOS);

    float3 v_posWorld;
    VertexPositionInputs vertexInput = GetVertexPositionInputs(v.positionOS.xyz);
    v_posWorld = vertexInput.positionWS;

    float2 worldRotUV;
    float debug = 1;
    float windSpeed;
    float windSineOffset = 0;
    //* wanghaoyu
    #ifdef _VERTEXANIMTION
        // 增加风的影响
        v_posWorld.xyz = ApplyWind(v.color.r, v_posWorld.xyz, windSineOffset, worldRotUV, windSpeed, debug);
    #endif
    o.positionWS = v_posWorld;
    o.positionHCS = mul(UNITY_MATRIX_VP, float4(v_posWorld, 1));
    return o;
}
float4 fragDepth(v2fDepth i): SV_Target
{
    float4 col = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);
    float3 viewDir = SafeNormalize(GetCameraPositionWS() - i.positionWS.xyz);
    float clipPart = 1 - abs(dot(i.baseNormal, viewDir));
    float dis = distance(float3(_WorldSpaceCameraPos.x, 0, _WorldSpaceCameraPos.z), float3(i.positionWS.x, 0, i.positionWS.z)/*posObj*/);
    float clipValue = 0;
    Unity_Dither(smoothstep(_DitherAmountMin, _DitherAmountMax, dis), i.positionHCS.xy, clipValue);
    clip(min((col.a * lerp((int)1,_CutIntensity,(int)_FlatClip) - clipPart), clipValue));

    return 0;
}

#endif
