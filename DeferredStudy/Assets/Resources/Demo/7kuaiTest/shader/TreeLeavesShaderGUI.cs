using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
//using static NPOI.HSSF.Util.HSSFColor;

public class TreeLeavesShaderGUI : ShaderGUI
{
    // OnGuI 接收的两个参数 ：
    MaterialEditor materialEditor;//当前材质面板
    MaterialProperty[] materialProperty;//当前shader的properties
    Material targetMat;//绘制对象材质球

    private bool m_BaseProp = true;
    private bool m_VerticalTint = true;
    private bool m_VolumeSense = true;
    private bool m_Others = true;

    private int debug_1x = 0;
    private int debug_1y = 0;
    private int debug_1z = 0;
    private int debug_1w = 0;
    private int debug_2x = 0;
    private int debug_2y = 0;
    private int debug_2z = 0;
    private int debug_2w = 0;
    private int noDarkGray = 0;
    private int noRefPart = 0;
    private int isPlant = 0;

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        this.materialEditor = materialEditor;
        this.materialProperty = properties;
        this.targetMat = materialEditor.target as Material; // 当前材质球

        show();
    }
    void show()
    {
        #region Shader 属性
        // Shader属性（声明之后直接赋值了）
        MaterialProperty _DebugProp1 = FindProperty("_DebugProp1", materialProperty);
        MaterialProperty _BaseMap = FindProperty("_BaseMap", materialProperty);
        MaterialProperty _saturate = FindProperty("_saturate", materialProperty);
        MaterialProperty _LightIntensity = FindProperty("_LightIntensity", materialProperty);
        MaterialProperty _DarkColor = FindProperty("_DarkColor", materialProperty);
        MaterialProperty _ToonCutPos = FindProperty("_ToonCutPos", materialProperty);
        MaterialProperty _BaseColor = FindProperty("_BaseColor", materialProperty);
        MaterialProperty _LerpColor = FindProperty("_LerpColor", materialProperty);
        MaterialProperty _TreeLerpTop = FindProperty("_TreeLerpTop", materialProperty);
        MaterialProperty _TreeLerpRoot = FindProperty("_TreeLerpRoot", materialProperty);
        MaterialProperty _TreeLerpIntensity = FindProperty("_TreeLerpIntensity", materialProperty);
        MaterialProperty _AOTint = FindProperty("_AOTint", materialProperty);
        MaterialProperty _AORange = FindProperty("_AORange", materialProperty);
        MaterialProperty _FaceLightGrayScale = FindProperty("_FaceLightGrayScale", materialProperty);
        MaterialProperty _FaceLightGrayIntensity = FindProperty("_FaceLightGrayIntensity", materialProperty);
        MaterialProperty _Magnitude = FindProperty("_Magnitude", materialProperty);
        //MaterialProperty _Frequency = FindProperty("_Frequency", materialProperty);
        //MaterialProperty _WindSineIntensity = FindProperty("_WindSineIntensity", materialProperty);
        MaterialProperty _WindDirection = FindProperty("_WindDirection", materialProperty);
        MaterialProperty _SubSurfaceGain = FindProperty("_SubSurfaceGain", materialProperty);
        MaterialProperty _SubSurfaceScale = FindProperty("_SubSurfaceScale", materialProperty);
        MaterialProperty _refIntensity = FindProperty("_refIntensity", materialProperty);
        MaterialProperty _refDis = FindProperty("_refDis", materialProperty);
        MaterialProperty _refScale = FindProperty("_refScale", materialProperty);
        //MaterialProperty _DitherAmountMax = FindProperty("_DitherAmountMax", materialProperty);
        //MaterialProperty _DitherAmountMin = FindProperty("_DitherAmountMin", materialProperty);
        MaterialProperty _CustomBloomIntensity = FindProperty("_CustomBloomIntensity", materialProperty);
        MaterialProperty _CustomBloomAlphaOffset = FindProperty("_CustomBloomAlphaOffset", materialProperty);
        MaterialProperty _IsPlant = FindProperty("_IsPlant", materialProperty);
        MaterialProperty _FlatClip = FindProperty("_FlatClip", materialProperty);
        MaterialProperty _CutIntensity = FindProperty("_CutIntensity", materialProperty);
        #endregion

        #region GUI绘制面板
        //materialEditor.ShaderProperty(_IsPlant, "盆栽用");
        //isPlant = targetMat.GetInt("_IsPlant");
        // 基础参数
        m_BaseProp = EditorGUILayout.BeginFoldoutHeaderGroup(m_BaseProp, "基础参数", EditorStyles.foldoutPreDrop);
        if (m_BaseProp)
        {
            // 临时加的平面剔除部分
            materialEditor.ShaderProperty(_FlatClip, "平面剔除开关");
            materialEditor.ShaderProperty(_CutIntensity, "平面剔除开关");



            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            // 显示图片用
            materialEditor.TexturePropertySingleLine(new GUIContent("贴图"), _BaseMap, null);
            materialEditor.ShaderProperty(_saturate, "饱和度");
#if UNITY_EDITOR
            EditorGUI.BeginChangeCheck();
            var Debug_1w = EditorGUILayout.Toggle("debug阴影范围", debug_1w == 1);
            if (EditorGUI.EndChangeCheck())
                debug_1w = Debug_1w ? 1 : 0;  // 这里是为了不让这个参数有除01外其他值

            if (debug_1w > 0.5)
                targetMat.SetVector("_DebugProp2", new Vector4(0, 0, 0, 1));
#endif
            materialEditor.ShaderProperty(_ToonCutPos, "明暗交界线位置.默认0,特殊需求可以调");
            materialEditor.ShaderProperty(_LightIntensity, "亮部强度");
            materialEditor.ShaderProperty(_DarkColor, "暗部颜色");
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space(5);   // 中间空一些距离
        // 垂直染色
        m_VerticalTint = EditorGUILayout.BeginFoldoutHeaderGroup(m_VerticalTint, "垂直染色", EditorStyles.foldoutPreDrop);
        if (m_VerticalTint)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
#if UNITY_EDITOR
            EditorGUI.BeginChangeCheck();
            var Debug_1x = EditorGUILayout.Toggle("debug染色效果(叠加前)", debug_1x == 1);
            if (EditorGUI.EndChangeCheck())
                debug_1x = Debug_1x ? 1 : 0;  // 这里是为了不让这个参数有除01外其他值
            EditorGUI.BeginChangeCheck();
            var Debug_1y = EditorGUILayout.Toggle("debug无垂直染色对比", debug_1y == 1);
            if (EditorGUI.EndChangeCheck())
                debug_1y = Debug_1y ? 1 : 0;

            if (debug_1y > 0.5)
                targetMat.SetVector("_DebugProp1", new Vector4(0, 1, 0, 0));
            else if (debug_1x > 0.5)
                targetMat.SetVector("_DebugProp1", new Vector4(1, 0, 0, 0));
#endif
            EditorGUILayout.HelpBox("↓↓↓这里顶端要比底端 数值大↓↓↓", MessageType.Warning);
            materialEditor.ShaderProperty(_TreeLerpTop, "树冠染色顶端高度");
            materialEditor.ShaderProperty(_TreeLerpRoot, "树冠染色底端高度");
            materialEditor.ShaderProperty(_TreeLerpIntensity, "树冠染色强度");
            materialEditor.ShaderProperty(_BaseColor, "渐变颜色上");
            materialEditor.ShaderProperty(_LerpColor, "渐变颜色下");
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space(5);   // 中间空一些距离
        // 体积感
        m_VolumeSense = EditorGUILayout.BeginFoldoutHeaderGroup(m_VolumeSense, "体积感", EditorStyles.foldoutPreDrop);
        if (m_VolumeSense)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUI.indentLevel++;
#if UNITY_EDITOR
            EditorGUI.BeginChangeCheck();
            var Debug_1z = EditorGUILayout.Toggle("debug AO范围", debug_1z == 1);
            if (EditorGUI.EndChangeCheck())
                debug_1z = Debug_1z ? 1 : 0;  // 这里是为了不让这个参数有除01外其他值/

            if (debug_1z > 0.5)
                targetMat.SetVector("_DebugProp1", new Vector4(0, 0, 1, 0));
#endif
            materialEditor.ShaderProperty(_AOTint, "AO中心体积感颜色");
            materialEditor.ShaderProperty(_AORange, "AO中心体积感范围");
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(3);
            if (isPlant == 0)
            {
                GUILayout.Label("阴影灰阶-调之前请看文档", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUI.BeginChangeCheck();
                var NoDarkGray = EditorGUILayout.Toggle("！我就不要暗部灰阶！", noDarkGray == 1);
                if (EditorGUI.EndChangeCheck())
                    noDarkGray = NoDarkGray ? 1 : 0;  // 这里是为了不让这个参数有除01外其他值
                if (noDarkGray > 0.5)
                {
                    targetMat.SetFloat("_FaceLightGrayScale", 7f);
                    targetMat.SetFloat("_FaceLightGrayIntensity", 0f);
                }
                else
                {
#if UNITY_EDITOR
                    EditorGUI.BeginChangeCheck();
                    var Debug_1w = EditorGUILayout.Toggle("debug阴影范围", debug_1w == 1);
                    if (EditorGUI.EndChangeCheck())
                        debug_1w = Debug_1w ? 1 : 0;  // 这里是为了不让这个参数有除01外其他值

                    if (debug_1w > 0.5)
                        targetMat.SetVector("_DebugProp1", new Vector4(0, 0, 0, 1));
#endif
                    // materialEditor.ShaderProperty(_FaceLightGrayScale, "阴影灰阶范围");
                    materialEditor.ShaderProperty(_FaceLightGrayIntensity, "阴影灰阶强度");
                }
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space(5);   // 中间空一些距离

        // 其他
        m_Others = EditorGUILayout.BeginFoldoutHeaderGroup(m_Others, "其他", EditorStyles.foldoutPreDrop);
        if (m_Others)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            if (isPlant == 0)
            {
                GUILayout.Label("透光", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
#if UNITY_EDITOR
                EditorGUI.BeginChangeCheck();
                var Debug_2y = EditorGUILayout.Toggle("debug透光范围", debug_2y == 1);
                if (EditorGUI.EndChangeCheck())
                    debug_2y = Debug_2y ? 1 : 0;  // 这里是为了不让这个参数有除01外其他值

                if (debug_2y > 0.5)
                    targetMat.SetVector("_DebugProp2", new Vector4(0, 1, 0, 0));
#endif
                materialEditor.ShaderProperty(_SubSurfaceGain, "透光强度");
                materialEditor.ShaderProperty(_SubSurfaceScale, "透光范围");
                EditorGUI.indentLevel--; EditorGUILayout.Space(3);
                GUILayout.Label("边缘过度-调之前请看文档", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;

                var NoRefPart = EditorGUILayout.Toggle("！我就不要边缘过度！", noRefPart == 1);
                if (EditorGUI.EndChangeCheck())
                    noRefPart = NoRefPart ? 1 : 0;  // 这里是为了不让这个参数有除01外其他值
                if (noRefPart < 0.5)
                {
#if UNITY_EDITOR
                    EditorGUI.BeginChangeCheck();
                    var Debug_2z = EditorGUILayout.Toggle("debug边缘过度", debug_2z == 1);
                    if (EditorGUI.EndChangeCheck())
                        debug_2z = Debug_2z ? 1 : 0;  // 这里是为了不让这个参数有除01外其他值

                    if (debug_2z > 0.5)
                        targetMat.SetVector("_DebugProp2", new Vector4(0, 0, 1, 0));
#endif
                    materialEditor.ShaderProperty(_refIntensity, "边缘过度强度");
                    materialEditor.ShaderProperty(_refDis, "边缘过度有效距离");
                    materialEditor.ShaderProperty(_refScale, "边缘过度范围");
                }
                else
                    targetMat.SetFloat("_refIntensity", 0f);
                EditorGUI.indentLevel--; EditorGUILayout.Space(3);
            }
            GUILayout.Label("风", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.HelpBox("---这里参数需要同步到PreZ上---", MessageType.Warning);
#if UNITY_EDITOR
            EditorGUI.BeginChangeCheck();
            var Debug_2x = EditorGUILayout.Toggle("debug风强", debug_2x == 1);
            if (EditorGUI.EndChangeCheck())
                debug_2x = Debug_2x ? 1 : 0;  // 这里是为了不让这个参数有除01外其他值

            if (debug_2x > 0.5)
                targetMat.SetVector("_DebugProp2", new Vector4(1, 0, 0, 0));
#endif
            materialEditor.ShaderProperty(_Magnitude, "随风漂移强度");
            //materialEditor.ShaderProperty(_Frequency, "随风飘动频率");
            //materialEditor.ShaderProperty(_WindSineIntensity, "风的规律波动强度");
            materialEditor.ShaderProperty(_WindDirection, "风向 (x,y,z)");
            EditorGUI.indentLevel--; EditorGUILayout.Space(3);
            materialEditor.ShaderProperty(_CustomBloomIntensity, "_CustomBloomIntensity");
            materialEditor.ShaderProperty(_CustomBloomAlphaOffset, "_CustomBloomAlphaOffset");
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space(5);   // 中间空一些距离
#if UNITY_EDITOR
        if (debug_1x == 0 && debug_1y == 0 && debug_1z == 0 && debug_1w == 0)
        {
            targetMat.SetVector("_DebugProp1", new Vector4(0, 0, 0, 0));
        }
        if (debug_2x == 0 && debug_2y == 0 && debug_2z == 0) // && debug_2w == 0
        {
            targetMat.SetVector("_DebugProp2", new Vector4(0, 0, 0, 0));
        }
#endif
        #endregion

        EditorGUILayout.Space(20);
        // Render Queue
        materialEditor.RenderQueueField();
    }

    #region 函数库
    /// <summary>
    /// 开关
    /// </summary>
    /// <param name="prop">shader中的参数</param>
    /// <param name="label">GUI面板显示的名称</param>
    private void DrawPropertyToggle(MaterialProperty prop, string label)
    {
        if (prop.type == MaterialProperty.PropType.Float || prop.type == MaterialProperty.PropType.Range)
        {
            bool enabled = prop.floatValue != 0.0f;
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = prop.hasMixedValue;
            enabled = EditorGUILayout.Toggle(label, enabled);
            EditorGUI.showMixedValue = false;

            if (EditorGUI.EndChangeCheck())
            {
                prop.floatValue = enabled ? 1.0f : 0.0f;
            }
        }
    }
    #endregion

}
