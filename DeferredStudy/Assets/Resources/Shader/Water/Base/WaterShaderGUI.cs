using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WaterShaderGUI : ShaderGUI
{

    enum DebugOption
    {
        waterSide = 0,
        DepthWater = 1,
        SH = 2,
        fresnelPart = 3,
        shadowPart = 4,
        VertexAnim = 5,
        heightLight = 6,
        bb = 7
    }

    private DebugOption debugOption;

    public bool isFirstTimeApply = true;

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        Material targetMat = materialEditor.target as Material;

        // 首次应用
        if (isFirstTimeApply)
        {
            targetMat.SetShaderPassEnabled("ShadowCaster", true);

            isFirstTimeApply = false;
        }

        EditorGUILayout.LabelField("Debug");

        bool useDebugMode = targetMat.IsKeywordEnabled("_DEBUGMODE");

        EditorGUI.BeginChangeCheck();

        useDebugMode = EditorGUILayout.Toggle("开启测试模式", useDebugMode);

        if (EditorGUI.EndChangeCheck())
        {
            if (useDebugMode)
            {
                targetMat.EnableKeyword("_DEBUGMODE");
            }
            else
            {
                targetMat.DisableKeyword("_DEBUGMODE");
            }
        }

        debugOption = (DebugOption)targetMat.GetInt("_Debug");

        EditorGUI.BeginChangeCheck();

        debugOption = (DebugOption)EditorGUILayout.EnumPopup("Debug选项：", debugOption);

        if (EditorGUI.EndChangeCheck())
        {
            targetMat.SetInt("_Debug", (int)debugOption);
        }

        base.OnGUI(materialEditor, properties);
    }
}
