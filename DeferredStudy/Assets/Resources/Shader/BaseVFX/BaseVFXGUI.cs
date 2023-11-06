using System.Net.Sockets;
using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEditor.Rendering.Universal.ShaderGUI;
using UnityEditor.Rendering.Universal;
using System.Collections.Generic;
using UnityEditorInternal;
using System.Linq;

public class BaseVFXGUI : ShaderGUI
{
    #region 枚举部分
    public enum SurfaceType
    {
        Opaque,
        Transparent
    }

    public enum BlendMode
    {
        Alpha,   // Old school alpha-blending mode, "菲尼尔" does not affect amount of transparency
        Premultiply, // Physically plausible transparency mode, implemented as alpha pre-multiply
        Additive,
        Multiply
    }
    public enum RenderFace
    {
        Front = 2,
        Back = 1,
        Both = 0
    }

    public enum FeatureO    // 互斥功能_不透明
    {
        Off,
        HardDissolve
    }

    public enum FeatureT    // 互斥功能_透明
    {
        Off,
        SoftDissolve,
        HardDissolve
    }
    public enum TexUVDir   // 修改贴图uv方向
    {
        NoChange,   // 不变
        rot90,      // 转90*
        rot270,     // 反转y
    }
    public enum ColorMask0   // 带有0的Colormask
    {
        ColorMaskRGBA,
        ColorMaskR,
        ColorMaskG,
        ColorMaskB,
        ColorMaskA,
        ColorMask0
    }
    public enum CopyTransColorBlend // 混合copy透明color内张图 和 MainTex 的方式
    {
        NoBlend,
        mul,
        add,
        blank1,
        blank2
    }
    #endregion

    // OnGuI 接收的两个参数 ：
    MaterialEditor materialEditor;//当前材质面板
    MaterialProperty[] materialProperty;//当前shader的properties
    Material targetMat;//绘制对象材质球
    // List of renderers using this material in the scene, used for validating vertex streams
    List<ParticleSystemRenderer> m_RenderersUsingThisMaterial = new List<ParticleSystemRenderer>();

    // 可折叠组名称
    private bool m_StencilOptions = EditorPrefs.GetBool("", false);
    private bool m_UniversalOptions = true;
    private bool m_BaseAttribute = true;
    private bool m_Advanced = true;
    private static ReorderableList vertexStreamList;
    // 单纯的开关
    //private bool ColorMask0 = false;

    // 最终计算部分
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        this.materialEditor = materialEditor;
        this.materialProperty = properties;
        this.targetMat = materialEditor.target as Material; // 当前材质球

        SetupMaterialBlendMode(targetMat);  // 混合模式部分
        show();
        //DoVertexStreamsArea(targetMat, m_RenderersUsingThisMaterial);
    }
    void show()
    {
        #region Shader属性
        // Shader属性（声明之后直接赋值了）
        MaterialProperty _BaseMap = FindProperty("_BaseMap", materialProperty, false);
        if (_BaseMap == null) _BaseMap = FindProperty("_MainTex", materialProperty);  // 兼容UI版本
        MaterialProperty _NoLOn = FindProperty("_NoLOn", materialProperty);
        MaterialProperty _NoLpos = FindProperty("_NoLpos", materialProperty);
        MaterialProperty _NoLTint = FindProperty("_NoLTint", materialProperty);
        MaterialProperty _BaseColor = FindProperty("_BaseColor", materialProperty);
        MaterialProperty _PolarCoordinates = FindProperty("_PolarCoordinates", materialProperty);
        MaterialProperty _DistortionScreenUV = FindProperty("_DistortionScreenUV", materialProperty);
        MaterialProperty _OffsetSpeedX = FindProperty("_OffsetSpeedX", materialProperty);
        MaterialProperty _OffsetSpeedY = FindProperty("_OffsetSpeedY", materialProperty);
        MaterialProperty _MaskTex = FindProperty("_MaskTex", materialProperty);
        MaterialProperty _DissolveTex = FindProperty("_DissolveTex", materialProperty);
        MaterialProperty _SoftValue = FindProperty("_SoftValue", materialProperty);
        MaterialProperty _DissolveX = FindProperty("_DissolveX", materialProperty);
        MaterialProperty _DissolveY = FindProperty("_DissolveY", materialProperty);
        MaterialProperty _DissolveEdgeColor = FindProperty("_DissolveEdgeColor", materialProperty);
        MaterialProperty _DissolveEdgeWidth = FindProperty("_DissolveEdgeWidth", materialProperty);
        MaterialProperty _DissolveEdgeWidthSoft = FindProperty("_DissolveEdgeWidthSoft", materialProperty);
        MaterialProperty _DissolveIntensity = FindProperty("_DissolveIntensity", materialProperty);
        MaterialProperty _DistortionTex = FindProperty("_DistortionTex", materialProperty);
        MaterialProperty _SoftParticle = FindProperty("_SoftParticle", materialProperty);
        MaterialProperty _SoftParticleFadeParamsNear = FindProperty("_SoftParticleFadeParamsNear", materialProperty);
        MaterialProperty _SoftParticleFadeParamsFar = FindProperty("_SoftParticleFadeParamsFar", materialProperty);
        MaterialProperty _SoftParticleFadeHeightMapIntensity = FindProperty("_SoftParticleFadeHeightMapIntensity", materialProperty);
        MaterialProperty _SoftParticleFadeHeightMapScale = FindProperty("_SoftParticleFadeHeightMapScale", materialProperty);
        MaterialProperty _VertexAnimTex = FindProperty("_VertexAnimTex", materialProperty);
        MaterialProperty _CustomVertexAnimOffset = FindProperty("_CustomVertexAnimOffset", materialProperty);
        MaterialProperty _VertexAnimSpeedX = FindProperty("_VertexAnimSpeedX", materialProperty);
        MaterialProperty _VertexAnimSpeedY = FindProperty("_VertexAnimSpeedY", materialProperty);
        MaterialProperty _VertexAnimIntensity = FindProperty("_VertexAnimIntensity", materialProperty);
        MaterialProperty _VertexAnim = FindProperty("_VertexAnim", materialProperty);
        MaterialProperty _VertexAnimTiling = FindProperty("_VertexAnimTiling", materialProperty);
        MaterialProperty _VertexAnimTint = FindProperty("_VertexAnimTint", materialProperty);
        MaterialProperty _VATexG = FindProperty("_VATexG", materialProperty);
        MaterialProperty _VertexAnimCustomData = FindProperty("_VertexAnimCustomData", materialProperty);
        MaterialProperty _VertexAnimScale = FindProperty("_VertexAnimScale", materialProperty);
        MaterialProperty _VertexAnimWidth = FindProperty("_VertexAnimWidth", materialProperty);
        MaterialProperty _VATint1 = FindProperty("_VATint1", materialProperty);
        MaterialProperty _VATint2 = FindProperty("_VATint2", materialProperty);
        MaterialProperty _VATint3 = FindProperty("_VATint3", materialProperty);
        MaterialProperty _Distortion = FindProperty("_Distortion", materialProperty);
        MaterialProperty _DistortionIntensity = FindProperty("_DistortionIntensity", materialProperty);
        MaterialProperty _HardRimWidth = FindProperty("_HardRimWidth", materialProperty);
        MaterialProperty _HardRimIntensity = FindProperty("_HardRimIntensity", materialProperty);
        MaterialProperty _HardRim = FindProperty("_HardRim", materialProperty);
        MaterialProperty _Fresnel = FindProperty("_Fresnel", materialProperty);
        MaterialProperty _FresnelA = FindProperty("_FresnelA", materialProperty);
        MaterialProperty _Parallax = FindProperty("_Parallax", materialProperty);
        MaterialProperty _ParallaxTex = FindProperty("_ParallaxTex", materialProperty);
        MaterialProperty _ParallaxIntensity = FindProperty("_ParallaxIntensity", materialProperty);
        MaterialProperty _ParallaxMaxStep = FindProperty("_ParallaxMaxStep", materialProperty);
        MaterialProperty _ParallaxMinStep = FindProperty("_ParallaxMinStep", materialProperty);
        MaterialProperty _FresnelColor = FindProperty("_FresnelColor", materialProperty);
        MaterialProperty _FresnelSideScale = FindProperty("_FresnelSideScale", materialProperty);
        MaterialProperty _FresnelWidth = FindProperty("_FresnelWidth", materialProperty);
        MaterialProperty _FresnelIntensity = FindProperty("_FresnelIntensity", materialProperty);
        MaterialProperty _FresnelTex = FindProperty("_FresnelTex", materialProperty);
        MaterialProperty _InvertFresnel = FindProperty("_InvertFresnel", materialProperty);
        MaterialProperty _FresnelOffsetX = FindProperty("_FresnelOffsetX", materialProperty);
        MaterialProperty _FresnelOffsetY = FindProperty("_FresnelOffsetY", materialProperty);
        MaterialProperty _DissolveSpeedX = FindProperty("_DissolveSpeedX", materialProperty);
        MaterialProperty _DissolveSpeedY = FindProperty("_DissolveSpeedY", materialProperty);
        MaterialProperty _MaskSpeedX = FindProperty("_MaskSpeedX", materialProperty);
        MaterialProperty _MaskSpeedY = FindProperty("_MaskSpeedY", materialProperty);
        MaterialProperty _DistortionOpaque = FindProperty("_DistortionOpaque", materialProperty);
        MaterialProperty _DistortionTransparents = FindProperty("_DistortionTransparents", materialProperty);
        MaterialProperty _DissolveCustomData = FindProperty("_DissolveCustomData", materialProperty);
        MaterialProperty _MainTexCustomDataON = FindProperty("_MainTexCustomDataON", materialProperty);
        // MRT 用
        MaterialProperty _CustomBloomIntensity = FindProperty("_CustomBloomIntensity", materialProperty);
        MaterialProperty _CustomBloomAlphaOffset = FindProperty("_CustomBloomAlphaOffset", materialProperty);
        // 一些基础的
        MaterialProperty _UseInRole = FindProperty("_UseInRole", materialProperty);
        MaterialProperty _DepthOffset = FindProperty("_DepthOffset", materialProperty);
        MaterialProperty surfaceTypeProp = FindProperty("_Surface", materialProperty);
        MaterialProperty blendModeProp = FindProperty("_Blend", materialProperty);
        MaterialProperty cullingProp = FindProperty("_Cull", materialProperty);
        MaterialProperty alphaClipProp = FindProperty("_AlphaClip", materialProperty);
        MaterialProperty alphaCutoffProp = FindProperty("_Cutoff", materialProperty);
        MaterialProperty queueOffsetProp = FindProperty("_QueueOffset", materialProperty);
        MaterialProperty _Feature_Qpaque = FindProperty("_Feature_Qpaque", materialProperty);
        MaterialProperty _Feature_Transparent = FindProperty("_Feature_Transparent", materialProperty);
        MaterialProperty _StencilComp = FindProperty("_StencilComp", materialProperty);
        MaterialProperty _StencilWriteMask = FindProperty("_StencilWriteMask", materialProperty);
        MaterialProperty _StencilReadMask = FindProperty("_StencilReadMask", materialProperty);
        MaterialProperty _Stencil = FindProperty("_Stencil", materialProperty);
        MaterialProperty _StencilPass = FindProperty("_StencilPass", materialProperty);
        MaterialProperty _StencilFail = FindProperty("_StencilFail", materialProperty);
        MaterialProperty _StencilZFail = FindProperty("_StencilZFail", materialProperty);
        //MaterialProperty _ColorMask = FindProperty("_ColorMask", materialProperty);
        MaterialProperty _ZTest = FindProperty("_ZTest", materialProperty);
        MaterialProperty _CustomZwrite = FindProperty("_CustomZwrite", materialProperty);
        MaterialProperty _ZWrite = FindProperty("_ZWrite", materialProperty);
        MaterialProperty _UseBaseMapUVDir = FindProperty("_UseBaseMapUVDir", materialProperty);
        //MaterialProperty _UseColorMask = FindProperty("_UseColorMask", materialProperty);
        MaterialProperty _UseCopyColorBlend = FindProperty("_UseCopyColorBlend", materialProperty);
        // Fog
        MaterialProperty m_depthAddFogMaterialProperty = FindProperty("_DepthAddFog", materialProperty);
        #endregion

        #region GUI名称
        // 一些基础的
        GUIContent surfaceType = new GUIContent("SurfaceType");
        GUIContent blendingMode = new GUIContent("Blending Mode");
        GUIContent cullingText = new GUIContent("Render Face");
        GUIContent alphaClipText = new GUIContent("Alpha Clipping");
        GUIContent alphaClipThresholdText = new GUIContent("Alpha Threshold");
        GUIContent queueSlider = new GUIContent("Queue Offset");
        GUIContent feature = new GUIContent("其他功能");
        GUIContent depthFog = new GUIContent("深度雾");
        #endregion

        GUILayout.Label("BaseVFX");
        materialEditor.ShaderProperty(_UseInRole, "角色特效");
        #region 通用设置
        // 折叠组
        m_UniversalOptions = EditorGUILayout.BeginFoldoutHeaderGroup(m_UniversalOptions, "通用设置");
        if (m_UniversalOptions)
        {
            // BlendMode Part
            DoPopup(surfaceType, surfaceTypeProp, Enum.GetNames(typeof(SurfaceType)));
            if ((SurfaceType)targetMat.GetFloat("_Surface") == SurfaceType.Transparent)
                DoPopup(blendingMode, blendModeProp, Enum.GetNames(typeof(BlendMode)));

            // Render Face Part
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = cullingProp.hasMixedValue; // 这个没懂有啥用
            var culling = (RenderFace)cullingProp.floatValue;
            culling = (RenderFace)EditorGUILayout.EnumPopup(cullingText, culling);
            if (EditorGUI.EndChangeCheck())
            {
                materialEditor.RegisterPropertyChangeUndo(cullingText.text);
                cullingProp.floatValue = (int)culling;  // 原文强转成Float，不知道为啥。这里改回int
            }
            EditorGUI.showMixedValue = false;

            // Alpha Clip Part
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = alphaClipProp.hasMixedValue;
            var alphaClipEnabled = EditorGUILayout.Toggle(alphaClipText, alphaClipProp.floatValue == 1);
            if (EditorGUI.EndChangeCheck())
                alphaClipProp.floatValue = alphaClipEnabled ? 1 : 0;
            EditorGUI.showMixedValue = false;

            if (alphaClipProp.floatValue == 1)
                materialEditor.ShaderProperty(alphaCutoffProp, alphaClipThresholdText, 1);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        #endregion

        #region 渲染设置
        m_StencilOptions = EditorGUILayout.BeginFoldoutHeaderGroup(m_StencilOptions, "渲染设置");
        if (m_StencilOptions)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = _CustomZwrite.hasMixedValue;
            var CustomZwriteON = EditorGUILayout.Toggle("自定义ZWrite", _CustomZwrite.floatValue == 1);
            if (EditorGUI.EndChangeCheck())
                _CustomZwrite.floatValue = CustomZwriteON ? 1 : 0;
            if (_CustomZwrite.floatValue == 1)
            {
                targetMat.SetInt("_CustomZwrite", 1);
                materialEditor.ShaderProperty(_ZWrite, "ZWrite");
            }
            else
            {
                targetMat.SetInt("_CustomZwrite", 0);
            }
            EditorGUI.EndChangeCheck();

            materialEditor.ShaderProperty(_DepthOffset, "DepthOffset");
            materialEditor.ShaderProperty(_ZTest, "ZTest");
            // 重新封了个Colormask
            //#region ColorMask
            //DoPopup(new GUIContent("ColorMask"), _UseColorMask, Enum.GetNames(typeof(ColorMask0)));
            //if ((ColorMask0)targetMat.GetFloat("_UseColorMask") == ColorMask0.ColorMaskRGBA)
            //{
            //    targetMat.SetInt("_ColorMask", 15);
            //}
            //else if ((ColorMask0)targetMat.GetFloat("_UseColorMask") == ColorMask0.ColorMaskR)
            //{
            //    targetMat.SetInt("_ColorMask", 8);
            //}
            //else if ((ColorMask0)targetMat.GetFloat("_UseColorMask") == ColorMask0.ColorMaskG)
            //{
            //    targetMat.SetInt("_ColorMask", 4);
            //}
            //else if ((ColorMask0)targetMat.GetFloat("_UseColorMask") == ColorMask0.ColorMaskB)
            //{
            //    targetMat.SetInt("_ColorMask", 2);
            //}
            //else if ((ColorMask0)targetMat.GetFloat("_UseColorMask") == ColorMask0.ColorMaskA)
            //{
            //    targetMat.SetInt("_ColorMask", 1);
            //}
            //else if ((ColorMask0)targetMat.GetFloat("_UseColorMask") == ColorMask0.ColorMask0)
            //{
            //    targetMat.SetInt("_ColorMask", 0);
            //}// colorMask End
            //#endregion
            EditorGUILayout.Space(20);
            materialEditor.ShaderProperty(_Stencil, "Stencil ID");
            materialEditor.ShaderProperty(_StencilComp, "Stencil Comparison");
            //materialEditor.ShaderProperty(_StencilReadMask, "Stencil Read Mask");
            //materialEditor.ShaderProperty(_StencilWriteMask, "Stencil Write Mask");
            materialEditor.ShaderProperty(_StencilPass, "Stencil Pass");
            materialEditor.ShaderProperty(_StencilFail, "Stencil Fail");
            materialEditor.ShaderProperty(_StencilZFail, "Stencil ZFail");
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorPrefs.SetBool("", m_StencilOptions);
        #endregion

        #region 基础属性
        EditorGUILayout.Space(20); // 与上文隔开一段距离
        m_BaseAttribute = EditorGUILayout.BeginFoldoutHeaderGroup(m_BaseAttribute, "基础属性");
        if (m_BaseAttribute)
        {
            // 只有在开顶点动画的时候关闭 MainTex
            if (targetMat.GetFloat("_VertexAnimTint") < 0.5)
            {
                EditorGUI.BeginChangeCheck();   // 极坐标start
                EditorGUI.showMixedValue = _PolarCoordinates.hasMixedValue;
                var PolarCoordinatesON = EditorGUILayout.Toggle("极坐标", _PolarCoordinates.floatValue == 1);
                if (EditorGUI.EndChangeCheck())
                    _PolarCoordinates.floatValue = PolarCoordinatesON ? 1 : 0;
                if (_PolarCoordinates.floatValue == 1)
                {
                    targetMat.EnableKeyword("_POLARUV");
                }
                else
                {
                    targetMat.DisableKeyword("_POLARUV");
                }
                EditorGUI.EndChangeCheck(); // 极坐标end
                EditorGUI.BeginChangeCheck();   // NOL start
                EditorGUI.showMixedValue = _NoLOn.hasMixedValue;
                var _USENOL = EditorGUILayout.Toggle("二分着色", _NoLOn.floatValue == 1);
                if (EditorGUI.EndChangeCheck()) _NoLOn.floatValue = _USENOL ? 1 : 0;
                if (_NoLOn.floatValue == 1)
                {
                    EditorGUI.indentLevel++;
                    materialEditor.ShaderProperty(_NoLpos, "明暗交界线位置");
                    materialEditor.ShaderProperty(_NoLTint, "NoL暗部颜色");
                    EditorGUI.indentLevel--;
                }
                EditorGUI.EndChangeCheck(); // NoL end
                if (_BaseMap != null)   // 修改BaseMap的UV方向 start
                {
                    #region 修改BaseMap的UV方向
                    DoPopup(new GUIContent("特效贴图UV方向"), _UseBaseMapUVDir, Enum.GetNames(typeof(TexUVDir)));
                    if ((TexUVDir)targetMat.GetFloat("_UseBaseMapUVDir") == TexUVDir.NoChange)
                    {
                        targetMat.SetFloat("_BaseMapUVDir", 0f);
                    }
                    else if ((TexUVDir)targetMat.GetFloat("_UseBaseMapUVDir") == TexUVDir.rot90)
                    {
                        targetMat.SetFloat("_BaseMapUVDir", Mathf.PI / 2f);
                    }
                    else if ((TexUVDir)targetMat.GetFloat("_UseBaseMapUVDir") == TexUVDir.rot270)
                    {
                        targetMat.SetFloat("_BaseMapUVDir", Mathf.PI / 2f * 3f);
                    }
                    #endregion
                }                       // 修改BaseMap的UV方向 end
                materialEditor.ShaderProperty(_BaseMap, "特效贴图");
                EditorGUI.indentLevel++;
                materialEditor.ShaderProperty(_BaseColor, "特效贴图颜色");
                // CustomData.zw start
                EditorGUI.BeginChangeCheck();
                EditorGUI.showMixedValue = _MainTexCustomDataON.hasMixedValue;
                var MainTexCustomDataON = EditorGUILayout.Toggle("启用customData1.zw 控制流动一次", _MainTexCustomDataON.floatValue == 1);
                if (EditorGUI.EndChangeCheck())
                    _MainTexCustomDataON.floatValue = MainTexCustomDataON ? 1 : 0;
                if (_MainTexCustomDataON.floatValue == 1)
                {
                    targetMat.SetFloat("_MainTexCustomDataON", 1);
                }
                else
                {
                    targetMat.SetFloat("_MainTexCustomDataON", 0);
                }
                EditorGUI.EndChangeCheck();
                // CustomData.zw end
                BaseShaderGUI.TwoFloatSingleLine(new GUIContent("特效图偏移"), _OffsetSpeedX, new GUIContent("X速度"), _OffsetSpeedY, new GUIContent("Y速度"), materialEditor);
                EditorGUI.indentLevel--;

            }
            else
            {
                targetMat.SetFloat("_MainTexCustomDataON", 0);
            }
            // 内部自己调
            materialEditor.ShaderProperty(_CustomBloomIntensity, "_CustomBloomIntensity");
            materialEditor.ShaderProperty(_CustomBloomAlphaOffset, "_CustomBloomAlphaOffset");
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        #endregion

        #region 高级选项
        EditorGUILayout.Space(20); // 与上文隔开一段距离
        m_Advanced = EditorGUILayout.BeginFoldoutHeaderGroup(m_Advanced, "高级选项");
        if (m_Advanced)
        {
            #region 顶点动画
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = _VertexAnim.hasMixedValue;
            var VertexAnimON = EditorGUILayout.Toggle("顶点动画", _VertexAnim.floatValue == 1);
            if (EditorGUI.EndChangeCheck())
                _VertexAnim.floatValue = VertexAnimON ? 1 : 0;
            if (_VertexAnim.floatValue == 1)
            {
                targetMat.EnableKeyword("_VERTEXANIM");
                // 具体功能
                EditorGUI.indentLevel++;
                materialEditor.TexturePropertySingleLine(new GUIContent("动画贴图 R:动画 G:Mask B:染色"), _VertexAnimTex);
                #region 顶点动画贴图Y通道
                EditorGUI.BeginChangeCheck(); EditorGUI.showMixedValue = _VATexG.hasMixedValue;
                var _VATexGON = EditorGUILayout.Toggle("G通道遮罩开关", _VATexG.floatValue == 1);
                if (EditorGUI.EndChangeCheck()) _VATexG.floatValue = _VATexGON ? 1 : 0;
                #endregion
                #region 顶点动画贴图Z通道
                EditorGUI.BeginChangeCheck();
                EditorGUI.showMixedValue = _VertexAnimTint.hasMixedValue;
                var VertexAnimTint_ON = EditorGUILayout.Toggle("B通道染色开关", _VertexAnimTint.floatValue == 1);
                if (EditorGUI.EndChangeCheck())
                    _VertexAnimTint.floatValue = VertexAnimTint_ON ? 1 : 0;
                if (_VertexAnimTint.floatValue == 1)
                {
                    targetMat.SetFloat("_VertexAnimTint", 1);
                    EditorGUI.indentLevel++;
                    // CustomData2.x
                    EditorGUI.BeginChangeCheck();
                    EditorGUI.showMixedValue = _VertexAnimCustomData.hasMixedValue;
                    var VertexAnimCustomData_ON = EditorGUILayout.Toggle("启用customData2.x 控制染色范围", _VertexAnimCustomData.floatValue == 1);
                    if (EditorGUI.EndChangeCheck())
                        _VertexAnimCustomData.floatValue = VertexAnimCustomData_ON ? 1 : 0;
                    if (_VertexAnimCustomData.floatValue == 1)
                    {
                        // materialEditor.ShaderProperty(_VertexAnimScale, "染色范围");
                        materialEditor.ShaderProperty(_VertexAnimWidth, "染色宽度");
                        targetMat.SetFloat("_VertexAnimCustomData", 1);
                    }
                    else
                    {
                        materialEditor.ShaderProperty(_VertexAnimScale, "染色范围");
                        materialEditor.ShaderProperty(_VertexAnimWidth, "染色宽度");
                        targetMat.SetFloat("_VertexAnimCustomData", 0);
                    }
                    EditorGUI.EndChangeCheck();
                    materialEditor.ShaderProperty(_VATint1, "最底层颜色");
                    materialEditor.ShaderProperty(_VATint2, "中间层颜色");
                    materialEditor.ShaderProperty(_VATint3, "最上层颜色");
                    EditorGUI.indentLevel--;
                }
                else
                {
                    targetMat.SetFloat("_VertexAnimTint", 0);
                }
                #endregion
                // CustomData2.xy控制offset
                EditorGUI.BeginChangeCheck();
                EditorGUI.showMixedValue = _CustomVertexAnimOffset.hasMixedValue;
                var CustomVAOffset = EditorGUILayout.Toggle("启用CustomData2.x 控制贴图offset", _CustomVertexAnimOffset.floatValue == 1);
                if (EditorGUI.EndChangeCheck())
                    _CustomVertexAnimOffset.floatValue = CustomVAOffset ? 1 : 0;
                if (_CustomVertexAnimOffset.floatValue == 1)
                {
                    targetMat.SetFloat("_CustomVertexAnimOffset", 1);
                }
                else
                {
                    targetMat.SetFloat("_CustomVertexAnimOffset", 0);
                }
                materialEditor.ShaderProperty(_VertexAnimTiling, "xy:R通道Tiling zw:B通道Tiling");
                BaseShaderGUI.TwoFloatSingleLine(new GUIContent("动画速度"), _VertexAnimSpeedX, new GUIContent("X速度"), _VertexAnimSpeedY, new GUIContent("Y速度"), materialEditor);
                materialEditor.ShaderProperty(_VertexAnimIntensity, "动画强度");

                EditorGUI.EndChangeCheck();
                EditorGUI.indentLevel--;
            }
            else
            {
                targetMat.DisableKeyword("_VERTEXANIM");
                targetMat.SetFloat("_VertexAnimTint", 0);   // 关上顶点动画选项后，这俩也要关闭
                targetMat.SetFloat("_VertexAnimCustomData", 0);
            }
            EditorGUI.EndChangeCheck();
            EditorGUILayout.Space(3);  // 效果之间有一些间距
            #endregion

            #region 扭曲
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = _Distortion.hasMixedValue;
            var _DISTORTION_ON = EditorGUILayout.Toggle("扭曲", _Distortion.floatValue == 1);
            if (EditorGUI.EndChangeCheck())
                _Distortion.floatValue = _DISTORTION_ON ? 1 : 0;
            EditorGUI.showMixedValue = false;
            EditorGUI.indentLevel++;
            // 扭曲开启后效果
            if (_Distortion.floatValue == 1)
            {
                targetMat.EnableKeyword("_DISTORTION_ON");
                // 扭曲影响背景
                EditorGUI.BeginChangeCheck();
                EditorGUI.showMixedValue = _DistortionOpaque.hasMixedValue;
                var DistortionOpaque_ON = EditorGUILayout.Toggle("扭曲影响背景", _DistortionOpaque.floatValue == 1);
                if (EditorGUI.EndChangeCheck())
                {
                    _DistortionOpaque.floatValue = DistortionOpaque_ON ? 1 : 0;
                    if (_DistortionOpaque.floatValue == 1)
                    {
                        targetMat.SetFloat("_DistortionOpaque", 1);
                        targetMat.SetFloat("_DistortionTransparents", 0); // 开启不透明，透明就关上
                    }
                    else
                    {
                        targetMat.SetFloat("_DistortionOpaque", 0);
                    }
                }

                #region 扭曲影响透明背景
                EditorGUI.BeginChangeCheck();
                EditorGUI.showMixedValue = _DistortionTransparents.hasMixedValue;
                var DistortionTransparents_ON = EditorGUILayout.Toggle("扭曲影响透明背景", _DistortionTransparents.floatValue == 1);
                if (EditorGUI.EndChangeCheck())
                {
                    _DistortionTransparents.floatValue = DistortionTransparents_ON ? 1 : 0;
                    if (_DistortionTransparents.floatValue == 1)
                    {
                        targetMat.SetFloat("_DistortionTransparents", 1);
                        targetMat.SetFloat("_DistortionOpaque", 0);     // 开启透明，不透明就关上
                    }
                    else
                    {
                        targetMat.SetFloat("_DistortionTransparents", 0);
                    }
                }
                EditorGUI.indentLevel++;
                #endregion

                #region 扭曲极坐标
                EditorGUI.indentLevel++;
                EditorGUI.BeginChangeCheck();
                EditorGUI.showMixedValue = _PolarCoordinates.hasMixedValue;
                var PolarCoordinatesON = EditorGUILayout.Toggle("极坐标", _PolarCoordinates.floatValue == 1);
                if (EditorGUI.EndChangeCheck())
                    _PolarCoordinates.floatValue = PolarCoordinatesON ? 1 : 0;
                if (_PolarCoordinates.floatValue == 1)
                {
                    targetMat.EnableKeyword("_POLARUV");
                }
                else
                {
                    targetMat.DisableKeyword("_POLARUV");
                }
                EditorGUI.EndChangeCheck();
                EditorGUI.indentLevel--;
                #endregion

                #region 屏幕采样扭曲贴图
                EditorGUI.indentLevel++;
                EditorGUI.BeginChangeCheck();
                EditorGUI.showMixedValue = _DistortionScreenUV.hasMixedValue;
                var DistortionScreenUV = EditorGUILayout.Toggle("开启屏幕空间采样扭曲贴图", _DistortionScreenUV.floatValue == 1);
                if (EditorGUI.EndChangeCheck())
                    _DistortionScreenUV.floatValue = DistortionScreenUV ? 1 : 0;
                if (_DistortionScreenUV.floatValue == 1)
                {
                    targetMat.SetFloat("_DistortionScreenUV", 1);
                }
                else
                {
                    targetMat.SetFloat("_DistortionScreenUV", 0);
                }
                EditorGUI.EndChangeCheck();
                EditorGUI.indentLevel--;
                #endregion

                #region 透明copycolor混合 
                EditorGUI.indentLevel--;
                    DoPopup(new GUIContent("混合方式"), _UseCopyColorBlend, Enum.GetNames(typeof(CopyTransColorBlend)));
                    if ((CopyTransColorBlend)targetMat.GetFloat("_UseCopyColorBlend") == CopyTransColorBlend.NoBlend)
                    {
                        targetMat.SetVector("_CopyColorBlend", new Vector4(0, 0, 0, 0));
                    }
                    else if ((CopyTransColorBlend)targetMat.GetFloat("_UseCopyColorBlend") == CopyTransColorBlend.mul)
                    {
                        targetMat.SetVector("_CopyColorBlend", new Vector4(1, 0, 0, 0));
                    }
                    else if ((CopyTransColorBlend)targetMat.GetFloat("_UseCopyColorBlend") == CopyTransColorBlend.add)
                    {
                        targetMat.SetVector("_CopyColorBlend", new Vector4(0, 1, 0, 0));
                    }
                    // 这里留空位了
                    EditorGUI.indentLevel++;
                    #endregion

                materialEditor.ShaderProperty(_DistortionTex, "扭曲贴图");
                BaseShaderGUI.TwoFloatSingleLine(new GUIContent("扭曲图偏移"), _DissolveX, new GUIContent("X速度"), _DissolveY, new GUIContent("Y速度"), materialEditor);
                materialEditor.ShaderProperty(_DistortionIntensity, "扭曲强度");
            }
            else
            {
                targetMat.DisableKeyword("_DISTORTION_ON");
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(3);  // 效果之间有一些间距
            #endregion

            #region 菲尼尔
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = _Fresnel.hasMixedValue;
            var _FRESNEL_ON = EditorGUILayout.Toggle("菲尼尔", _Fresnel.floatValue == 1);
            if (EditorGUI.EndChangeCheck())
                _Fresnel.floatValue = _FRESNEL_ON ? 1 : 0;
            EditorGUI.showMixedValue = false;
            // 硬溶解开启后效果
            EditorGUI.indentLevel++;
            if (_Fresnel.floatValue == 1)
            {
                targetMat.EnableKeyword("_FRESNEL_ON");
                EditorGUI.BeginChangeCheck(); EditorGUI.showMixedValue = _InvertFresnel.hasMixedValue;
                var _INVERTFrensenl = EditorGUILayout.Toggle("反转菲尼尔范围", _InvertFresnel.floatValue == 1);
                if (EditorGUI.EndChangeCheck()) _InvertFresnel.floatValue = _INVERTFrensenl ? 1 : 0;
                EditorGUI.BeginChangeCheck(); EditorGUI.showMixedValue = _FresnelA.hasMixedValue;
                var _FRENSELA = EditorGUILayout.Toggle("菲尼尔颜色的A影响整体透明度", _FresnelA.floatValue == 1);
                if (EditorGUI.EndChangeCheck()) _FresnelA.floatValue = _FRENSELA ? 1 : 0;
                // materialEditor.ShaderProperty(_InvertFresnel, "反转菲尼尔范围");
                materialEditor.ShaderProperty(_FresnelTex, "菲尼尔贴图");
                BaseShaderGUI.TwoFloatSingleLine(new GUIContent("菲尼尔贴图偏移速度"), _FresnelOffsetX, new GUIContent("X速度"), _FresnelOffsetY, new GUIContent("Y速度"), materialEditor);
                materialEditor.ShaderProperty(_FresnelColor, "菲尼尔颜色");
                materialEditor.ShaderProperty(_FresnelSideScale, "菲尼尔范围");
                materialEditor.ShaderProperty(_FresnelWidth, "菲尼尔边缘软硬");
                materialEditor.ShaderProperty(_FresnelIntensity, "菲尼尔强度");
            }
            else
            {
                targetMat.DisableKeyword("_FRESNEL_ON");
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(3);  // 效果之间有一些间距
            #endregion

            #region 硬边光
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = _HardRim.hasMixedValue;
            var _HARDRIM_ON = EditorGUILayout.Toggle("硬边光", _HardRim.floatValue == 1);
            if (EditorGUI.EndChangeCheck())
                _HardRim.floatValue = _HARDRIM_ON ? 1 : 0;
            EditorGUI.showMixedValue = false;
            // 硬溶解开启后效果
            EditorGUI.indentLevel++;
            if (_HardRim.floatValue == 1)
            {
                targetMat.EnableKeyword("_HARDRIM");
                if (_Fresnel.floatValue != 1)
                    materialEditor.ShaderProperty(_HardRimIntensity, "硬边光强度");
                materialEditor.ShaderProperty(_HardRimWidth, "硬边光宽度");

            }
            else
            {
                targetMat.DisableKeyword("_HARDRIM");
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(3);  // 效果之间有一些间距
            #endregion

            #region 视差
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = _Parallax.hasMixedValue;
            var _PARALLAX_ON = EditorGUILayout.Toggle("视差", _Parallax.floatValue == 1);
            if (EditorGUI.EndChangeCheck())
                _Parallax.floatValue = _PARALLAX_ON ? 1 : 0;
            EditorGUI.showMixedValue = false;
            // 硬溶解开启后效果
            EditorGUI.indentLevel++;
            if (_Parallax.floatValue == 1)
            {
                targetMat.EnableKeyword("_PARALLAX");
                materialEditor.ShaderProperty(_ParallaxTex, "视差贴图.g");
                materialEditor.ShaderProperty(_ParallaxIntensity, "视差强度");
                materialEditor.ShaderProperty(_ParallaxMaxStep, "视差最大步数");
                materialEditor.ShaderProperty(_ParallaxMinStep, "视差最小步数");
            }
            else
            {
                targetMat.DisableKeyword("_PARALLAX");
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(3);  // 效果之间有一些间距
            #endregion

            #region 深度雾
            EditorGUI.BeginChangeCheck();
            var depthAddFogEnable = EditorGUILayout.Toggle(depthFog, m_depthAddFogMaterialProperty.floatValue == 1);
            if (EditorGUI.EndChangeCheck())
                m_depthAddFogMaterialProperty.floatValue = depthAddFogEnable ? 1 : 0;
            if (depthAddFogEnable)
            {
                targetMat.EnableKeyword("_DEPTH_FOG_ADD_ON");
            }
            else
            {
                targetMat.DisableKeyword("_DEPTH_FOG_ADD_ON");
            }

            #endregion

            if ((SurfaceType)targetMat.GetFloat("_Surface") == SurfaceType.Transparent)
            {
                #region 软粒子
                // 软粒子开关
                EditorGUI.BeginChangeCheck();
                EditorGUI.showMixedValue = _SoftParticle.hasMixedValue;
                var _SOFTPARTICLES_ON = EditorGUILayout.Toggle("软粒子", _SoftParticle.floatValue == 1);
                if (EditorGUI.EndChangeCheck())
                    _SoftParticle.floatValue = _SOFTPARTICLES_ON ? 1 : 0;
                EditorGUI.showMixedValue = false;
                // 软粒子开启后效果
                EditorGUI.indentLevel++;
                if (_SoftParticle.floatValue == 1)
                {
                    targetMat.EnableKeyword("_SOFTPARTICLES_ON");
                    materialEditor.ShaderProperty(_SoftParticleFadeParamsNear, "Near");
                    materialEditor.ShaderProperty(_SoftParticleFadeParamsFar, "Far");
                    materialEditor.ShaderProperty(_SoftParticleFadeHeightMapIntensity, "细化强度");
                    materialEditor.ShaderProperty(_SoftParticleFadeHeightMapScale, "细化范围");
                }
                else
                {
                    targetMat.DisableKeyword("_SOFTPARTICLES_ON");
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.Space(3);  // 效果之间有一些间距
                #endregion
            }

            // 互斥的功能
            // 用eunm写的，这样可以强调每个互斥功能的独立性。shader方面可以把互斥的feature写在同一行了，
            // 通过判断不同渲染模式，决定显示不同的feature，好像只能写两遍

            if ((SurfaceType)targetMat.GetFloat("_Surface") == SurfaceType.Opaque)
            {
                #region 不透明互斥的功能
                DoPopup(feature, _Feature_Qpaque, Enum.GetNames(typeof(FeatureO)));
                if ((FeatureO)targetMat.GetFloat("_Feature_Qpaque") == FeatureO.Off)
                {
                    // off 的时候关闭变体
                    // targetMat.DisableKeyword("_HARDDISSOLVE_ON");
                    // targetMat.DisableKeyword("_SOFTDISSOLVE_ON");
                    targetMat.DisableKeyword("_DISSOLVE");
                }
                else if ((FeatureO)targetMat.GetFloat("_Feature_Qpaque") == FeatureO.HardDissolve)
                {
                    EditorGUI.indentLevel++;
                    /// 溶解custom.x 开关
                    EditorGUI.BeginChangeCheck();
                    EditorGUI.showMixedValue = _DissolveCustomData.hasMixedValue;
                    var DissolveCustomDataOn = EditorGUILayout.Toggle("启用customData1.x 控制溶解", _DissolveCustomData.floatValue == 1);
                    if (EditorGUI.EndChangeCheck())
                        _DissolveCustomData.floatValue = DissolveCustomDataOn ? 1 : 0;
                    if (_DissolveCustomData.floatValue == 1)
                    {
                        targetMat.SetFloat("_DissolveCustomData", 1);
                        targetMat.SetFloat("_DissolveIntensity", 0);
                    }
                    else
                    {
                        targetMat.SetFloat("_DissolveCustomData", 0);
                    }
                    EditorGUI.EndChangeCheck();

                    // 先关闭其他变体，再开启功能需要的变体，再是面板和逻辑
                    // targetMat.DisableKeyword("_SOFTDISSOLVE_ON");
                    // targetMat.EnableKeyword("_HARDDISSOLVE_ON");
                    targetMat.EnableKeyword("_DISSOLVE");
                    targetMat.SetFloat("_DissolveType", 0);
                    materialEditor.ShaderProperty(_DissolveTex, "溶解贴图");
                    materialEditor.ShaderProperty(_DissolveEdgeColor, "边缘颜色");
                    materialEditor.ShaderProperty(_DissolveEdgeWidth, "边缘宽度");
                    EditorGUI.BeginChangeCheck();
                    if (EditorGUI.EndChangeCheck())
                        _DissolveCustomData.floatValue = DissolveCustomDataOn ? 1 : 0;
                    if (_DissolveCustomData.floatValue == 0)
                    {
                        materialEditor.ShaderProperty(_DissolveIntensity, "溶解强度");
                    }
                    BaseShaderGUI.TwoFloatSingleLine(new GUIContent("溶解图偏移"), _DissolveSpeedX, new GUIContent("X速度"), _DissolveSpeedY, new GUIContent("Y速度"), materialEditor);
                    EditorGUI.indentLevel--;
                }
                #endregion
            }
            else if ((SurfaceType)targetMat.GetFloat("_Surface") == SurfaceType.Transparent)
            {
                #region 透明互斥的功能
                DoPopup(feature, _Feature_Transparent, Enum.GetNames(typeof(FeatureT)));
                if ((FeatureT)targetMat.GetFloat("_Feature_Transparent") == FeatureT.Off)
                {
                    // off 的时候关闭变体
                    // targetMat.DisableKeyword("_HARDDISSOLVE_ON");
                    // targetMat.DisableKeyword("_SOFTDISSOLVE_ON");
                    targetMat.DisableKeyword("_DISSOLVE");
                }
                else if ((FeatureT)targetMat.GetFloat("_Feature_Transparent") == FeatureT.HardDissolve)
                {
                    EditorGUI.indentLevel++;
                    // 溶解custom.x 开关
                    EditorGUI.BeginChangeCheck();
                    EditorGUI.showMixedValue = _DissolveCustomData.hasMixedValue;
                    var DissolveCustomDataOn = EditorGUILayout.Toggle("启用customData1.x 控制溶解", _DissolveCustomData.floatValue == 1);
                    if (EditorGUI.EndChangeCheck())
                        _DissolveCustomData.floatValue = DissolveCustomDataOn ? 1 : 0;
                    if (_DissolveCustomData.floatValue == 1)
                    {
                        targetMat.SetFloat("_DissolveCustomData", 1);
                    }
                    else
                    {
                        targetMat.SetFloat("_DissolveCustomData", 0);
                    }
                    EditorGUI.EndChangeCheck();

                    // 先关闭其他变体，再开启功能需要的变体，再是面板和逻辑
                    // targetMat.DisableKeyword("_SOFTDISSOLVE_ON");
                    // targetMat.EnableKeyword("_HARDDISSOLVE_ON");
                    targetMat.EnableKeyword("_DISSOLVE");
                    targetMat.SetFloat("_DissolveType", 0);
                    materialEditor.ShaderProperty(_DissolveTex, "溶解贴图");
                    materialEditor.ShaderProperty(_DissolveEdgeColor, "边缘颜色");
                    materialEditor.ShaderProperty(_DissolveEdgeWidth, "边缘宽度");
                    materialEditor.ShaderProperty(_DissolveIntensity, "溶解强度");
                    BaseShaderGUI.TwoFloatSingleLine(new GUIContent("溶解图偏移"), _DissolveSpeedX, new GUIContent("X速度"), _DissolveSpeedY, new GUIContent("Y速度"), materialEditor);
                    EditorGUI.indentLevel--;
                }
                else if ((FeatureT)targetMat.GetFloat("_Feature_Transparent") == FeatureT.SoftDissolve)
                {
                    EditorGUI.indentLevel++;
                    // 溶解custom.x 开关
                    EditorGUI.BeginChangeCheck();
                    EditorGUI.showMixedValue = _DissolveCustomData.hasMixedValue;
                    var DissolveCustomDataOn = EditorGUILayout.Toggle("启用customData1.x 控制溶解", _DissolveCustomData.floatValue == 1);
                    if (EditorGUI.EndChangeCheck())
                        _DissolveCustomData.floatValue = DissolveCustomDataOn ? 1 : 0;
                    if (_DissolveCustomData.floatValue == 1)
                    {
                        targetMat.SetFloat("_DissolveCustomData", 1);
                    }
                    else
                    {
                        targetMat.SetFloat("_DissolveCustomData", 0);
                    }
                    EditorGUI.EndChangeCheck();

                    // targetMat.DisableKeyword("_HARDDISSOLVE_ON");
                    // targetMat.EnableKeyword("_SOFTDISSOLVE_ON");
                    targetMat.EnableKeyword("_DISSOLVE");
                    targetMat.SetFloat("_DissolveType", 1);
                    materialEditor.ShaderProperty(_DissolveTex, "溶解贴图");
                    materialEditor.ShaderProperty(_DissolveIntensity, "溶解强度");
                    materialEditor.ShaderProperty(_SoftValue, "边缘软度");
                    materialEditor.ShaderProperty(_DissolveEdgeColor, "边缘颜色");
                    materialEditor.ShaderProperty(_DissolveEdgeWidthSoft, "边缘宽度");
                    BaseShaderGUI.TwoFloatSingleLine(new GUIContent("溶解图偏移"), _DissolveSpeedX, new GUIContent("X速度"), _DissolveSpeedY, new GUIContent("Y速度"), materialEditor);
                    EditorGUI.indentLevel--;
                }
                #endregion
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        #endregion

        EditorGUILayout.Space(20);
        EditorGUI.BeginChangeCheck();
        EditorGUI.showMixedValue = queueOffsetProp.hasMixedValue;
        var queue = EditorGUILayout.IntSlider(queueSlider, (int)queueOffsetProp.floatValue, -100, 100);
        if (EditorGUI.EndChangeCheck())
            queueOffsetProp.floatValue = queue;
        EditorGUI.showMixedValue = false;
        materialEditor.RenderQueueField(); // Render Queue
    }
    #region 混合模式
    public void DoPopup(GUIContent label, MaterialProperty property, string[] options)
    {
        DoPopup(label, property, options, materialEditor);
    }

    public static void DoPopup(GUIContent label, MaterialProperty property, string[] options, MaterialEditor materialEditor)
    {
        // 抛异常用的
        if (property == null)
            throw new ArgumentNullException("property");

        EditorGUI.showMixedValue = property.hasMixedValue;
        var mode = property.floatValue;
        EditorGUI.BeginChangeCheck();
        mode = EditorGUILayout.Popup(label, (int)mode, options);
        if (EditorGUI.EndChangeCheck())
        {
            materialEditor.RegisterPropertyChangeUndo(label.text);
            property.floatValue = mode;
        }
        EditorGUI.showMixedValue = false;
    }
    public static void SetupMaterialBlendMode(Material material)
    {
        if (material == null)
            throw new ArgumentNullException("material"); // ??

        bool alphaClip = material.GetFloat("_AlphaClip") == 1;
        bool opaqueDissolve = (SurfaceType)material.GetFloat("_Surface") == SurfaceType.Opaque && (FeatureO)material.GetFloat("_Feature_Qpaque") == FeatureO.HardDissolve;  // 因为只有硬溶解，后面如果加其他功能这里还要附加判断
        if (alphaClip || opaqueDissolve)
        {
            material.EnableKeyword("_ALPHATEST_ON");
        }
        else
        {
            material.DisableKeyword("_ALPHATEST_ON");
        }

        SurfaceType surfaceType = (SurfaceType)material.GetFloat("_Surface");
        if (surfaceType == SurfaceType.Opaque)
        {
            if (alphaClip || opaqueDissolve)
            {
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                material.SetOverrideTag("RenderType", "TransparentCutout");
            }
            else
            {
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
                material.SetOverrideTag("RenderType", "Opaque");
            }
            // 所以这边只有材质里有这个 "_QueueOffset" 才可以offset
            material.renderQueue += material.HasProperty("_QueueOffset") ? (int)material.GetFloat("_QueueOffset") : 0;
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            if (material.GetFloat("_CustomZwrite") < 0.5)
            {
                material.SetInt("_ZWrite", 1);
            }
        }
        else
        {
            BlendMode blendMode = (BlendMode)material.GetFloat("_Blend");
            // 颜色混合模式
            switch (blendMode)
            {
                case BlendMode.Alpha:
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    // material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.SetFloat("_PreAlphaMul", 0); // 这边不用变体了改用 lerp ，1 就是开启，0 就是关闭
                    break;
                case BlendMode.Premultiply:
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetFloat("_PreAlphaMul", 1);
                    break;
                case BlendMode.Additive:
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetFloat("_PreAlphaMul", 0);
                    break;
                case BlendMode.Multiply:
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.DstColor);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetFloat("_PreAlphaMul", 0);
                    break;
            }
            material.SetOverrideTag("RenderType", "Transparent");   // 重新设置shader中的标签
            if (material.GetFloat("_CustomZwrite") < 0.5)
            {
                material.SetInt("_ZWrite", 0);
            }
            material.renderQueue = (int)RenderQueue.Transparent;
            material.renderQueue += material.HasProperty("_QueueOffset") ? (int)material.GetFloat("_QueueOffset") : 0;

        }
    }
    #endregion

    //#region VertexStreams
    //public static void DoVertexStreamsArea(Material material, List<ParticleSystemRenderer> renderers, bool useLighting = false)
    //{
    //    EditorGUILayout.Space();
    //    // Display list of streams required to make this shader work
    //    bool useCustom1x = (material.GetFloat("_DissolveCustomData") > 0.5f);   // 溶解customData1.x




    //    bool useNormalMap = false;
    //    bool useFlipbookBlending = (material.GetFloat("_FlipbookBlending") > 0.0f);
    //    if (material.HasProperty("_BumpMap"))
    //        useNormalMap = material.GetTexture("_BumpMap");

    //    bool useGPUInstancing = ShaderUtil.HasProceduralInstancing(material.shader);
    //    if (useGPUInstancing && renderers.Count > 0)
    //    {
    //        if (!renderers[0].enableGPUInstancing || renderers[0].renderMode != ParticleSystemRenderMode.Mesh)
    //            useGPUInstancing = false;
    //    }

    //    // Build the list of expected vertex streams
    //    List<ParticleSystemVertexStream> streams = new List<ParticleSystemVertexStream>();
    //    List<string> streamList = new List<string>();

    //    streams.Add(ParticleSystemVertexStream.Position);
    //    streamList.Add("Position (POSITION.xyz)");

    //    if (useLighting || useNormalMap)
    //    {
    //        streams.Add(ParticleSystemVertexStream.Normal);
    //        streamList.Add("Normal (NORMAL.xyz)");
    //        if (useNormalMap)
    //        {
    //            streams.Add(ParticleSystemVertexStream.Tangent);
    //            streamList.Add("Tangent (TANGENT.xyzw)");
    //        }
    //    }

    //    streams.Add(ParticleSystemVertexStream.Color);
    //    streamList.Add(useGPUInstancing ? "Color (INSTANCED0.xyzw)" : "Color (COLOR.xyzw)");
    //    streams.Add(ParticleSystemVertexStream.UV);
    //    streamList.Add("UV (TEXCOORD0.xy)");

    //    List<ParticleSystemVertexStream> instancedStreams = new List<ParticleSystemVertexStream>(streams);

    //    if (useGPUInstancing)
    //    {
    //        instancedStreams.Add(ParticleSystemVertexStream.AnimFrame);
    //        streamList.Add("AnimFrame (INSTANCED1.x)");
    //    }
    //    else if (useFlipbookBlending && !useGPUInstancing)
    //    {
    //        streams.Add(ParticleSystemVertexStream.UV2);
    //        streamList.Add("UV2 (TEXCOORD0.zw)");
    //        streams.Add(ParticleSystemVertexStream.AnimBlend);
    //        streamList.Add("AnimBlend (TEXCOORD1.x)");
    //    }

    //    vertexStreamList = new ReorderableList(streamList, typeof(string), false, true, false, false);

    //    vertexStreamList.drawHeaderCallback = (Rect rect) => {
    //        EditorGUI.LabelField(rect, "Vertex Streams");
    //    };

    //    vertexStreamList.DoLayoutList();

    //    // Display a warning if any renderers have incorrect vertex streams
    //    string Warnings = "";
    //    List<ParticleSystemVertexStream> rendererStreams = new List<ParticleSystemVertexStream>();
    //    foreach (ParticleSystemRenderer renderer in renderers)
    //    {
    //        renderer.GetActiveVertexStreams(rendererStreams);

    //        bool streamsValid;
    //        if (useGPUInstancing && renderer.renderMode == ParticleSystemRenderMode.Mesh && renderer.supportsMeshInstancing)
    //            streamsValid = CompareVertexStreams(rendererStreams, instancedStreams);
    //        else
    //            streamsValid = CompareVertexStreams(rendererStreams, instancedStreams);

    //        if (!streamsValid)
    //            Warnings += "-" + renderer.name + "\n";
    //    }

    //    if (!string.IsNullOrEmpty(Warnings))
    //    {
    //        EditorGUILayout.HelpBox(
    //            "The following Particle System Renderers are using this material with incorrect Vertex Streams:\n" +
    //            Warnings, MessageType.Error, true);
    //        // Set the streams on all systems using this material
    //        if (GUILayout.Button(new GUIContent("Fix Now",
    //            "Apply the vertex stream layout to all Particle Systems using this material"), EditorStyles.miniButton, GUILayout.ExpandWidth(true)))
    //        {
    //            Undo.RecordObjects(renderers.Where(r => r != null).ToArray(), "Apply custom vertex streams from material");

    //            foreach (ParticleSystemRenderer renderer in renderers)
    //            {
    //                if (useGPUInstancing && renderer.renderMode == ParticleSystemRenderMode.Mesh && renderer.supportsMeshInstancing)
    //                    renderer.SetActiveVertexStreams(instancedStreams);
    //                else
    //                    renderer.SetActiveVertexStreams(streams);
    //            }
    //        }
    //    }
    //}
    //private static bool CompareVertexStreams(IEnumerable<ParticleSystemVertexStream> a, IEnumerable<ParticleSystemVertexStream> b)
    //{
    //    var differenceA = a.Except(b);
    //    var differenceB = b.Except(a);
    //    var difference = differenceA.Union(differenceB).Distinct();
    //    if (!difference.Any())
    //        return true;
    //    // If normals are the only difference, ignore them, because the default particle streams include normals, to make it easy for users to switch between lit and unlit
    //    if (difference.Count() == 1)
    //    {
    //        if (difference.First() == ParticleSystemVertexStream.Normal)
    //            return true;
    //    }
    //    return false;
    //}
    //#endregion

}
