using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;              //
using Sirenix.OdinInspector;    //

/// <summary>
/// 框架层面的游戏设置
/// 对象池缓存设置、UI元素的设置
/// </summary>
[CreateAssetMenu(fileName = "GameSetting", menuName = "NDFrame/Config/GameSetting")]
public class GameSetting : ConfigBase
{

#if UNITY_EDITOR // 只在unity editor中有效
    [Button(Name = "初始化游戏配置",ButtonHeight = 50)]
    [GUIColor(0,1,0)]
    private void Init()
    {
        // Debug.Log("GameSetting 初始化");
    }
    /// <summary>
    /// 编译前执行函数
    /// </summary>
    [InitializeOnLoadMethod]    // 给方法的初始化
    private static void LoadForEditor()
    {
        GameObject.Find("GameRoot").GetComponent<GameRoot>().GameSetting.Init();
    }
#endif
}
