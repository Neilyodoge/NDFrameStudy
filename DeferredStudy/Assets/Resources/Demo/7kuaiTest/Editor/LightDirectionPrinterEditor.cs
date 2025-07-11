using UnityEditor;
using UnityEngine;

// 自定义 Inspector 脚本，为 LightDirectionPrinter 组件添加界面
[CustomEditor(typeof(LightDirectionPrinter))]
public class LightDirectionPrinterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 获取目标组件
        LightDirectionPrinter printer = (LightDirectionPrinter)target;

        // 获取挂载对象的 Light 组件
        Light directionalLight = printer.GetComponent<Light>();

        // 绘制默认 Inspector 属性（包括 printDirection 勾选框）
        DrawDefaultInspector();

        // 检查是否有 Light 组件
        if (directionalLight == null || directionalLight.type != LightType.Directional)
        {
            EditorGUILayout.HelpBox("请将此脚本挂载到平行光（Directional Light）上！", MessageType.Warning);
            return;
        }

        // 如果勾选了 printDirection，则打印方向信息
        if (printer.printDirection)
        {
            // 获取平行光的旋转（欧拉角）
            Vector3 eulerAngles = directionalLight.transform.eulerAngles;

            // 获取光源的 forward 方向（世界空间）
            Vector3 lightDirection = directionalLight.transform.forward;

            // 打印旋转角度和方向向量
            Debug.Log($"平行光旋转角度 (Euler Angles): X={eulerAngles.x:F2}, Y={eulerAngles.y:F2}, Z={eulerAngles.z:F2}");
            Debug.Log($"光源方向向量 (Light Direction): X={lightDirection.x:F4}, Y={lightDirection.y:F4}, Z={lightDirection.z:F4}");

            // 可选：提供一个按钮手动触发打印
            if (GUILayout.Button("打印方向向量"))
            {
                Debug.Log($"[手动触发] 平行光旋转角度: X={eulerAngles.x:F2}, Y={eulerAngles.y:F2}, Z={eulerAngles.z:F2}");
                Debug.Log($"[手动触发] 光源方向向量: X={lightDirection.x:F4}, Y={lightDirection.y:F4}, Z={lightDirection.z:F4}");
            }
        }

        // 标记对象为已修改，确保更改保存
        if (GUI.changed)
        {
            EditorUtility.SetDirty(printer);
        }
    }
}