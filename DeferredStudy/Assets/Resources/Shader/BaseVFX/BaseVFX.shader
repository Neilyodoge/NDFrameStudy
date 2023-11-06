//* wanghaoyu 
// CustomData1 溶解是x 扭曲是y .zw 控制MainTex的Offset速度。现在这里不是很好调，Curve比较敏感，可以吧shader中的速度调成0.1之后在调
// CustomData2 x在顶点动画染色的情况下是染色范围; x在开启Custom2控制offset时候控制offset
// HardRim,开fresnl跟他的颜色走，不开跟强度走。功能不是很完善，要运行游戏具体看效果
Shader "Particles/BaseVFX"
{
    Properties
    {
        // 开关&参数控制
        [Toggle]_UseInRole("角色使用",int) = 0   // 会受到 _CharacterFocusIntensity 影响
        _CopyColorBlend("用来控制CopyColorTrans和MainTex的混合方式",vector) = (0,0,0,0)
        _BaseMapUVDir("用来控制MainTex的UV方向",float) = 0
        _UseBaseMapUVDir("用来控制贴图旋转的",float) = 0
        // 用来存开关参数的。这里的参数不用写进Input
        [HideInInspector]_UseCopyColorBlend("",int) = 0

        [HideInInspector]_MainTex ("", 2D) = "white" {}
        // 基础属性
        // [Enum(UnityEngine.Rendering.BlendOp)]_blendop("blendop", Float) = 0.0
        [Toggle(_ALPHATEST_ON)] _AlphaTest("Use Clip",float) = 0
        _Cutoff("Clip",range(0,1)) = 0.5
        // 基础属性
        _NoLOn("_NoLOn",int) = 0    // nol着色开关
        _NoLpos("_NoLpos",range(-1,1)) = 0
        _NoLTint("_NoLTint",color) = (0,0,0,1)
        
        [MainTexture]_BaseMap ("MainTex", 2D) = "white" {}
        _OffsetSpeedX("SpeedX", float) = 0
        _OffsetSpeedY("SpeedY", float) = 0
        [HDR]_BaseColor ("MainColor", Color) = (1, 1, 1, 1)
        _MaskTex ("MaskTex", 2D) = "white" {}
        _MaskSpeedX ("MaskSpeedX", float) = 0
        _MaskSpeedY ("MaskSpeedY", float) = 0
        // 一些效果
        // soft Dissolve
        _DissolveTex("_DissolveTex", 2D) = "white"{}
        _SoftValue ("Soft Value", range(0,1)) = 1
        _DissolveX ("DissolveX", float) = 0
        _DissolveY ("DissolveY", float) = 0
        _DissolveSpeedX ("DissolveSpeedX", float) = 0
        _DissolveSpeedY ("DissolveSpeedY", float) = 0
        // hard Dissolve
        [HDR]_DissolveEdgeColor("_DissolveEdgeColor", Color) = (1,1,1,1)
        _DissolveEdgeWidth("_DissolveEdgeWidth", Float) = 0.2
        _DissolveEdgeWidthSoft("_DissolveEdgeWidth", range(0,0.2)) = 0  // 主要就是为了不影响硬溶解
        _DissolveIntensity("_DissolveIntensity", Range(0, 3)) = 1
        // SoftParticle
        [Toggle(_SOFTPARTICLES_ON)] _SoftParticle("Use SoftParticle",float) = 0
        _SoftParticleFadeParamsNear("SoftParticleNear", float) = 0
        _SoftParticleFadeParamsFar("SoftParticleFar", float) = 1
        _SoftParticleFadeHeightMapIntensity("SoftParticleFadeHeightMapIntensity",range(0,5)) = 1
        _SoftParticleFadeHeightMapScale("SoftParticleFadeHeightMapScale",range(0,2)) = 0
        // Distortion 扭曲
        [Toggle(_DISTORTION_ON)] _Distortion("Use Distortion",float) = 0
        _DistortionScreenUV("屏幕UV采样扭曲图",int) = 0
        _DistortionTex("DistortionTex", 2D) = "white"{}
        _DistortionIntensity("DistortionIntensity",range(0,1)) = 0
        // Fresnel
        [Toggle(_FRESNEL_ON)] _Fresnel("Use Fresnel",float) = 0
        _FresnelA("_FresnelA",int) = 0
        _InvertFresnel("InvertFresnel",int) = 0
        [HDR]_FresnelColor("FresnelColor",color) = (1,1,1,1)
        _FresnelTex("FresnelTex",2D) = "white" {}
        _FresnelWidth("fresnel边缘软硬",range(0,50)) = 1
        _FresnelSideScale("FresnelSideScale",range(0,5)) = 5
        _FresnelIntensity("FresnelIntensity",range(0,1)) = 1
        _FresnelOffsetX("FresnelOffsetX",float) = 0
        _FresnelOffsetY("FresnelOffsetX",float) = 0
        // VertexAnim
        [Toggle(_VERTEXANIM)] _VertexAnim("Use VertexAnim",float) = 0
        _VATexG("VATexG",int) = 0 // 贴图只有G通道生效
        _VertexAnimTex("VertexAnimTex", 2D) = "white"{}     // r：顶点动画noise  g：顶点动画mask  b:染色noise
        _CustomVertexAnimOffset("CustomVertexAnimOffset", float) = 0  // customData2.xy
        _VertexAnimSpeedX("VertexAnimSpeedX", float) = 0
        _VertexAnimSpeedY("VertexAnimSpeedX", float) = 0
        _VertexAnimTiling("VertexTiling",Vector) = (1,1,1,1)    // xy: r通道Tiling zw: b通道TIling
        _VertexAnimIntensity("VertexAnimIntensity", range(0,20)) = 0
        _VertexAnimScale("VertexAnimScale", range(-0.1,1)) = 0
        _VertexAnimWidth("VertexAnimWidth", range(-0.5,1)) = 0
        [HDR]_VATint1 ("VATint1", Color) = (1, 1, 1, 1)
        [HDR]_VATint2 ("VATint2", Color) = (1, 1, 1, 1)
        [HDR]_VATint3 ("VATint3", Color) = (1, 1, 1, 1)

        [Toggle(_POLARUV)] _PolarCoordinates("极坐标开启",float) = 0

        // 硬边光
        [Toggle(_HARDRIM)] _HardRim("Use HardRim",float) = 0
        _HardRimWidth("硬边光宽度",range(0,0.1)) = 0
        _HardRimIntensity("硬边光强度",range(0,5)) = 1
        // 视差
        [Toggle(_PARALLAX)]_Parallax("Use Parallax",float) = 0
        _ParallaxTex("视差贴图",2D) = "white"{}
        _ParallaxIntensity("视差强度",range(0,0.2)) = 0
        _ParallaxMaxStep("视差最大步数",range(1,30)) = 15
        _ParallaxMinStep("视差最小步数",range(1,20)) = 5
        // MRT
        _CustomBloomIntensity("_CustomBloomIntensity", Range(0, 2)) = 1.0
        _CustomBloomAlphaOffset("_CustomBloomAlphaOffset", Range(-1, 1)) = 0

        [Enum(UnityEngine.Rendering.CompareFunction)]_StencilComp ("Stencil Comparison", Float) = 8
        [IntRange]_StencilWriteMask ("Stencil Write Mask", Range(0,255)) = 255
        [IntRange]_StencilReadMask ("Stencil Read Mask", Range(0,255)) = 255
        [IntRange]_Stencil ("Stencil ID", Range(0,255)) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilPass ("Stencil Pass", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilFail ("Stencil Fail", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilZFail ("Stencil ZFail", Float) = 0

        // 用在Editor里的
        [HideInInspector] _DissolveCustomData("Dissolve CustomData",float) = 0
        [HideInInspector] _DistortionOpaque("Distortion Opaque",float) = 0  // 扭曲不透明RT
        [HideInInspector] _DistortionTransparents("Distortion Opaque",float) = 0   // 扭曲透明RT
        [HideInInspector] _MainTexCustomDataON("MainTex CustomData ON",float) = 0
        [HideInInspector] _VertexAnimTint("VertexAnimTint",float) = 0 // 配合顶点动画染色
        [HideInInspector] _VertexAnimCustomData("Vertex Anim CustomData",float) = 0
        [HideInInspector] _CustomVFXColor("_CustomVFXColor",float) = 1

        [HideInInspector] _DepthOffset("__DepthOffset",float) = 0
        [HideInInspector] _Surface("__surface", Float) = 0.0
        [HideInInspector] _Blend("__blend", Float) = 0.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        _CustomZwrite("_CustomZwrite",float) = 0
        [HideInInspector] [Enum(Off, 0, On, 1)] _ZWrite("__zw", Float) = 1.0
        [HideInInspector] [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("__zt", Float) = 4.0
        [HideInInspector] _Cull("__cull", Float) = 2.0
        [HideInInspector] _AlphaClip("__clip", Float) = 0.0
        [HideInInspector] _QueueOffset("_Queue", Range(-100,100)) = 0
        [HideInInspector] _PreAlphaMul("PreAlphaMul",range(0,1)) = 0
        [HideInInspector] _Feature_Qpaque("__Feature_Qpaque",range(0,1)) = 0
        [HideInInspector] _Feature_Transparent("__Feature_Transparent",range(0,1)) = 0
        [HideInInspector] _DissolveType("_DissolveType",float) = 0

        [HideInInspector] _SceneOrCharacter("_SceneOrCharacter",Float) = 0
        [HideInInspector] _NoFocusIntensity("_NoFocusIntensity",Float) = 0
        [HideInInspector] _HitFactor("_HitFactor",Float) = 0
        [HideInInspector] _DepthAddFog("_DepthAddFog", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent"  "RenderPipeline" = "UniversalRenderPipeline" "IgnoreProjector" = "True" "Queue" = "Transparent" "PreviewType"="Plane"}
        
        Pass
        {
            Tags {"PreviewType"="Plane"}
            Blend[_SrcBlend][_DstBlend]
            ZTest[_ZTest]
            ZWrite[_ZWrite]
            Cull[_Cull]
            Offset[_DepthOffset],[_DepthOffset]
            //ColorMask [_ColorMask]
            Stencil
            {
                Ref [_Stencil]
                Comp [_StencilComp]
                ReadMask [_StencilReadMask]
                WriteMask [_StencilWriteMask]
                Pass [_StencilPass]
                Fail [_StencilFail]
                ZFail [_StencilZFail]
            }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "BaseVFXInput.hlsl"
            // #pragma enable_d3d11_debug_symbols
            #pragma multi_compile _ _MRTEnable
            #pragma shader_feature_local _DEPTH_FOG_ADD_ON
            #pragma shader_feature_local _SOFTPARTICLES_ON
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local _DISTORTION_ON
            #pragma shader_feature_local _FRESNEL_ON 
            #pragma shader_feature_local _DISSOLVE
            #pragma shader_feature_local _VERTEXANIM
            #pragma shader_feature_local _POLARUV
            #pragma shader_feature_local _HARDRIM
            #pragma shader_feature_local _PARALLAX
            
            v2f vert(appdata v)
            {
                v2f o;
                // 顶点动画
                #if defined(_VERTEXANIM)
                    float2 VAUV = float2(frac(_VertexAnimSpeedX * _Time.y), frac(_VertexAnimSpeedY * _Time.y)) 
                                + (v.uv.xy * _VertexAnimTiling.xy)
                                + float2(lerp(0,v.CustomData2.x,_CustomVertexAnimOffset),0); // Custom2.x 控制offset
                    float2 VAMaskUV = v.uv.xy * _VertexAnimTex_ST.xy + _VertexAnimTex_ST.zw;
                    float VertexAnimTex = SAMPLE_TEXTURE2D_LOD(_VertexAnimTex,sampler_VertexAnimTex,VAUV,0).x;    
                    float VertexAnimTexMask = lerp(1,SAMPLE_TEXTURE2D_LOD(_VertexAnimTex,sampler_VertexAnimTex,VAMaskUV,0).y,_VATexG);      // y用来做mask,_VATexG做开关
                    v.PositionOS.xyz += v.normalOS * (_VertexAnimIntensity/10/*这里是为了方便控制*/) * VertexAnimTex * VertexAnimTexMask;
                #endif
                o.PositionCS = TransformObjectToHClip(v.PositionOS.xyz);
                o.uv = v.uv;
                o.vertexColor = v.vertexColor;
                o.CustomData1 = v.CustomData1; 
                o.CustomData2 = v.CustomData2; 
                o.normalWS = normalize(TransformObjectToWorldNormal(v.normalOS));
                #if defined(_SOFTPARTICLES_ON)  // 软粒子
                    o.ScreenPos = ComputeScreenPos(o.PositionCS);
                #endif
                #if defined(_FRESNEL_ON)  || defined(_PARALLAX)  // 逐顶点的
                    half3 PositionWS = TransformObjectToWorld(v.PositionOS.xyz);
                    o.viewDirWS = normalize(_WorldSpaceCameraPos.xyz - PositionWS);
                #endif
                #if defined(_PARALLAX)
                    VertexNormalInputs normal = GetVertexNormalInputs(v.normalOS, v.tangentOS);
                    o.tangentWS = normal.tangentWS;
                #endif
                return o;
            }
            // 用MRT就从这个结构体里输出了
            half4 frag(v2f i): SV_Target
            {                
                Light light = GetMainLight();
                float nov = 0;
                float nol = dot(i.normalWS,light.direction);
                #if defined(_FRESNEL_ON)  || defined(_PARALLAX)
                nov = max(0.001,dot(i.normalWS,i.viewDirWS));
                #endif
                float4 col = 0;
                float2 UV = 0;
                #if defined(_POLARUV)   // 极坐标
                    float2 thetaR = RectToPolar(i.uv.xy, float2(0.5, 0.5));
                    UV =  float2((thetaR.x / 3.141593 * 0.5 + 0.5 + frac(_OffsetSpeedX * _Time.y)) * _BaseMap_ST.x + _BaseMap_ST.z,    // θ映射到[0, 1]
                                                        (thetaR.y + frac(_OffsetSpeedY * _Time.y)) * _BaseMap_ST.y + _BaseMap_ST.w);      // r随时间流动
                #else                   // UV 偏移
                    UV = float2(frac(_OffsetSpeedX * _Time.y), frac(_OffsetSpeedY * _Time.y))
                                 + (i.uv.xy * _BaseMap_ST.xy + _BaseMap_ST.zw);
                #endif 
                // 这里对UV的方向进行调整
                UV = rotUV(UV);// 根据_BaseMapUVDir.z判断 invertXY
                // 这里是CustomData.zw 用来动态控制偏移速度
                UV += lerp(0,i.CustomData1.zw,_MainTexCustomDataON);       
                float2 MaskUV = float2(frac(_MaskSpeedX * _Time.y), frac(_MaskSpeedY * _Time.y))
                                         + i.uv.xy * _MaskTex_ST.xy + _MaskTex_ST.zw;
                // Mask P5特效的需求是Mask贴图只有在透明情况下有用，且只有a通道生效
                float MaskColor = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, MaskUV).a;
                float2 parallaxOffset = 0;
                #if defined(_PARALLAX)  // 视差部分
                    float3 viewTS = GetViewDirectionTangentSpace(float4(i.tangentWS, 0), i.normalWS.xyz, i.viewDirWS);
                    viewTS = normalize(viewTS);
                    parallaxOffset = ParallaxRaymarching(viewTS.xy / viewTS.z, _ParallaxIntensity, UV, 1 - nov * nov);
                    UV += parallaxOffset;
                #endif
                // 如果用扭曲效果就不用采样MainTex了
                //#if !defined(_DISTORTION_ON)    
                    // base color = 贴图 * 颜色 * 顶点色
                    col = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, UV) * i.vertexColor * _BaseColor;
                    // nol 二分着色
                    col.rgb = lerp(col.rgb,col.rgb * _NoLTint,step(saturate((nol*0.5+0.5) - _NoLpos),0.5) * _NoLOn);
                    UNITY_BRANCH
                    if (_VertexAnimTint > 0.5)
                    {
                        float2 VAUV = float2(frac(_VertexAnimSpeedX * _Time.y), frac(_VertexAnimSpeedY * _Time.y)) 
                                + (i.uv.xy * _VertexAnimTiling.zw);
                        float VertexAnimTex = SAMPLE_TEXTURE2D(_VertexAnimTex,sampler_VertexAnimTex,VAUV).z;    // b 通道控制分层染色
                        half4 VA1 = lerp(_VATint1,_VATint2,step(VertexAnimTex,lerp(_VertexAnimScale, i.CustomData2.x, _VertexAnimCustomData))); 
                        half4 VA2 = lerp(VA1,_VATint3,step(VertexAnimTex,(lerp(_VertexAnimScale, i.CustomData2.x, _VertexAnimCustomData) - _VertexAnimWidth))); // customData2.x 控制顶点动画范围
                        col = VA2 * i.vertexColor;
                    }
                //#endif
                // 扭曲效果（要放在fresnel上面，不然就和fresnel冲突了） 
                #if defined(_DISTORTION_ON)
                    // 扭曲贴图的极坐标
                    float2 DistortionUV = 0;
                    float2 DistortionScreenUVOn = lerp(i.uv.xy,i.PositionCS.xy,_DistortionScreenUV);  // 目前不会影响极坐标
                    #if defined(_POLARUV)
                        thetaR = RectToPolar(i.uv.xy, float2(0.5, 0.5));
                        DistortionUV = float2((thetaR.x / 3.141593 * 0.5 + 0.5 + frac(_DissolveX * _Time.y)) * _DistortionTex_ST.x + _DistortionTex_ST.z, // θ映射到[0, 1]
                                                                    (thetaR.y + frac(_DissolveY * _Time.y)) * _DistortionTex_ST.y + _DistortionTex_ST.w);      // r随时间流动
                    #else
                        DistortionUV = float2(frac(_DissolveX * _Time.y), frac(_DissolveY * _Time.y))
                                            + (DistortionScreenUVOn * _DistortionTex_ST.xy + _DistortionTex_ST.zw);
                    #endif
                    float2 DistortionTex = SAMPLE_TEXTURE2D(_DistortionTex, sampler_DistortionTex, DistortionUV).xy;    // 虽然这张图是满足2通道的
                    // 根据 Distortion UV 重新采样 MainTex  // 不影响背景的默认扭曲
                    float4 DisMainTex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, 
                                        ((UV * _BaseMap_ST.xy + _BaseMap_ST.zw) + DistortionTex * _DistortionIntensity))  //* wanghaoyu bug 这里有点小问题,强度会根据uv有影响
                                        * i.vertexColor * _BaseColor;
                    // 扭曲不透明和透明，这里的alpha会计算扭曲贴图的alpha
                    float2 screen = i.PositionCS.xy / _ScreenParamsCompatible.xy;
                    float2 DistortionScreenUV = saturate(screen + DistortionTex * _DistortionIntensity * i.CustomData1.y * _dynamicResScale);
                    float4 copyColorDis = SAMPLE_TEXTURE2D(_CameraOpaqueTexture,sampler_CameraOpaqueTexture,clamp(DistortionScreenUV,0,_dynamicResScale - (_ScreenParamsCompatible.zw -1)));
                    float4 copyTransColorDis = SAMPLE_TEXTURE2D(_CameraTransparentsTexture,sampler_CameraTransparentsTexture,clamp(DistortionScreenUV,0,_dynamicResScale - (_ScreenParamsCompatible.zw -1)));
                    float4 CameraColorTex = (copyColorDis * (int)_DistortionOpaque) + (copyTransColorDis * (int)_DistortionTransparents);
                    // 三种扭曲 最终效果
                    col = lerp(DisMainTex,CameraColorTex * i.vertexColor,_DistortionOpaque+_DistortionTransparents); 
                    // 混合算法，这里的混合算法只算RGB，因为算A会小时
                    half3 CopyColorBlendMul = CameraColorTex.rgb * DisMainTex.rgb;  
                    half3 CopyColorBlendAdd = CameraColorTex.rgb + DisMainTex.rgb;
                    col.rgb = lerp(col.rgb,CopyColorBlendMul,(int)_CopyColorBlend.x);    // Mul
                    col.rgb = lerp(col.rgb,CopyColorBlendAdd,(int)_CopyColorBlend.y);    // Add
                #endif
                // 扭曲也要适配 MaskTex
                    col.a *= MaskColor;
                // 硬边光 (因为用到了深度图根据实际游戏运行效果调试)
                #if defined(_HARDRIM)
                    float2 HardRimScreenUV = (i.PositionCS.xy / _ScreenParamsCompatible.xy - 0.5) * (1 + _HardRimWidth) + 0.5;  // _HardRimWidth = 0.05
                    float depthTex = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, HardRimScreenUV * _dynamicResScale).r;
                    depthTex = step(depthTex,0.01); // 因为距离近所以step的值小
                    half3 hardRimCol = 0;
                    #if defined(_FRESNEL_ON)    // 如果开了fresnel就跟他的颜色走
                        hardRimCol = depthTex * _FresnelColor.rgb;
                    #else
                        hardRimCol = depthTex * _HardRimIntensity;
                    #endif 
                    #if !defined(_FRESNEL_ON)
                        col.rgb = lerp(col.rgb,col.rgb * _HardRimIntensity,depthTex);//1 - step(depthTex22,_HardRimWidth);//lerp(col.rgb, _HardRimTint, hardRimScale);
                    #endif
                #endif
                // 菲尼尔
                #if defined(_FRESNEL_ON)
                    // fresnelTex 用来定制化边光颜色。但非fresnel部分也会生效
                    float2 FresnelUV = float2(frac(_FresnelOffsetX * _Time.y),frac(_FresnelOffsetY * _Time.y)) 
                                        + (i.uv.xy * _FresnelTex_ST.xy + _FresnelTex_ST.zw);
                    float4 FresnelTex = SAMPLE_TEXTURE2D(_FresnelTex, sampler_FresnelTex, FresnelUV);
                    float Fresnel = pow(nov,_FresnelSideScale);
                    Fresnel = lerp(1-Fresnel,Fresnel,_InvertFresnel);   // 反转fresnel需求
                    Fresnel = pow(Fresnel,_FresnelWidth);
                    float4 FinalFresnelCol = Fresnel * _FresnelColor * lerp(half4(FresnelTex.rgb,1),FresnelTex,_FresnelA);// 默认fresnel贴图的A只在开启
                    // 这里只*了贴图的R通道
                    float FresnelLerp = _FresnelIntensity * Fresnel * FresnelTex.r;
                    float FresnelA = 0;
                    #if defined(_HARDRIM)
                        col.rgb = lerp(col,max(FinalFresnelCol.rgb,hardRimCol),FresnelLerp);
                    #else
                        col.rgb = lerp(col.rgb,FinalFresnelCol.rgb,FresnelLerp);
                    #endif
                    //* 龙卷风需求单独处理alpha
                    FresnelA = lerp(col.a,FinalFresnelCol.a,FresnelLerp);
                    col.a = lerp(col.a,FresnelA,_FresnelA);
                #endif
                
                // 为了避免多个变体(仅在软硬溶解情况下，再加互斥功能还得走变体)用了lerp，根据_DissolveType决定使用方式
                #if defined(_DISSOLVE)
                    half _cutoff = 1-col.a;
                    float2 NoiseUV = float2(frac(_DissolveSpeedX * _Time.y),frac(_DissolveSpeedY * _Time.y)) 
                                        + (i.uv.xy * _DissolveTex_ST.xy + _DissolveTex_ST.zw);
                    float Noise = SAMPLE_TEXTURE2D(_DissolveTex, sampler_DissolveTex, NoiseUV).r;
                // 软溶解
                    float softDissolveA = col.a * saturate(smoothstep(0, _SoftValue, Noise-lerp(_cutoff,i.CustomData1.x,_DissolveCustomData) + (1 - _DissolveIntensity)));   // dissolve控制的是溶解强度，用custom控制
                    float softDissolveSide = col.a * saturate(smoothstep(_DissolveEdgeWidthSoft, _SoftValue + _DissolveEdgeWidthSoft, Noise-lerp(_cutoff,i.CustomData1.x,_DissolveCustomData) + (1 - _DissolveIntensity)));
                    //softDissolveA = softDissolveA - softDissolveSide;
                    col.rgb = lerp(col.rgb,_DissolveEdgeColor.rgb,softDissolveA - softDissolveSide);
                    col.a = lerp(col.a,softDissolveA,_DissolveType);
                // 硬溶解
                    _cutoff = lerp(0, _cutoff + _DissolveEdgeWidth, _cutoff);
                    half Edge = lerp(smoothstep(_cutoff + _DissolveEdgeWidth, _cutoff,
                                                clamp(Noise, _DissolveEdgeWidth, 1)), 
                                                0, _DissolveEdgeWidth == 0);
                    float3 hardDissolveRGB = col.rgb + lerp(0, _DissolveEdgeColor.rgb * Edge, _DissolveIntensity);
                    col.rgb = lerp(hardDissolveRGB,col.rgb,_DissolveType);
                    float hardDissolveClip = Noise - lerp(_cutoff,i.CustomData1.x,_DissolveCustomData) + (1 - _DissolveIntensity); // 硬溶解也用custom1的x来通过曲线控制溶解
                    clip(lerp(hardDissolveClip,1,_DissolveType));
                #endif

                // 透明度测试
                #if defined(_ALPHATEST_ON)
                    clip(col.a - _Cutoff);
                #endif
                // 软粒子
                #if _SOFTPARTICLES_ON
                    float4 screenPos = i.ScreenPos;
                    float heightMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, UV).r;
                    col.a *= SoftParticles(_SoftParticleFadeParamsNear, _SoftParticleFadeParamsFar, screenPos,heightMap);
                #endif
                // 透明度预乘，换成lerp了
                col.rgb = lerp(col.rgb, col.rgb * col.a, _PreAlphaMul);

                //half3 oldColor = col.rgb;
                //col.rgb *= lerp(_SceneFocusIntensity.xxx, lerp(1,_CharacterFocusIntensity,_UseInRole), _SceneOrCharacter);
                //col.rgb = lerp(col.rgb, oldColor, _NoFocusIntensity); // 这不和里
                // col.rgb *= _NoFocusIntensity; // 这就合理了
            // #if defined(_DEPTH_FOG_ADD_ON)
            //     AddDepthFogBilinear(col, i.PositionCS.xy / _ScreenParamsCompatible.xy);
            // #endif
                
                // FragmentOutputParticles output = (FragmentOutputParticles)0;    // 初始化MRT输出值
                // col.rgb = clamp(col.rgb,0,100); // 防止溢出
                // output.color0 = col;   // 第一张RT（默认输出值）
                // // return col;
                // #if _MRTEnable
                //     // 第二张RT输出值
                //     //* 之前让天宇那边把默认强度映射到了[0,2],Role好用，但vfx一直没效果。为了在multiply模式下正确这边先改回原来的
                //     output.color1 = half4(0, _CustomBloomIntensity * _ParticleBloomIntensity * lerp(1,col.a,_Surface), 0,0);
                // #endif

                return col;//output;
            }
            ENDHLSL
        }
    }
    CustomEditor "BaseVFXGUI"
}
