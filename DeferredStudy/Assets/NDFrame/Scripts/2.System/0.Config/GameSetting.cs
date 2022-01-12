using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;              //
using Sirenix.OdinInspector;    //
using System;                   //
using System.Reflection;        //

/// <summary>
/// 框架层面的游戏设置
/// 对象池缓存设置、UI元素的设置
/// </summary>
[CreateAssetMenu(fileName = "GameSetting", menuName = "NDFrame/Config/GameSetting")]
public class GameSetting : ConfigBase
{
    [LabelText("对象池设置")]
    [DictionaryDrawerSettings(KeyLabel = "类型",ValueLabel = "皆可缓存")]
    public Dictionary<Type, bool> cacheDic = new Dictionary<Type, bool>();

#if UNITY_EDITOR // 只在unity editor中有效
    [Button(Name = "初始化游戏配置",ButtonHeight = 50)]
    [GUIColor(0,1,0)]
    private void Init()
    {
        PoolAttributeOnEditor();
        // Debug.Log("GameSetting 初始化");
    }
    /// <summary>
    /// 编译前执行函数
    /// </summary>
    [InitializeOnLoadMethod]    // 给方法的初始化
    private static void LoadForEditor()
    {
        if (GameObject.Find("GameRoot") != null)    // 不这样会报错
        {
            GameObject.Find("GameRoot").GetComponent<GameRoot>().GameSetting.Init();
        }
        
    }

    /// <summary>
    /// 将带有Pool特性的类加入缓存池字典
    /// </summary>
    private void PoolAttributeOnEditor()
    {
        cacheDic.Clear();
        // 获取所有程序集
        System.Reflection.Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();    // 当前域里的所有
        // 遍历程序集
        foreach (System.Reflection.Assembly assembly in asms)
        {
            // 遍历程序集下的每个类型
            Type[] types = assembly.GetTypes();
            foreach (Type type in types)
            {
                // 获取pool特性
                PoolAttribute pool = type.GetCustomAttribute<PoolAttribute>();
                if (pool != null)
                {
                    cacheDic.Add(type, true);
                }
            }
        }
    }

#endif
}
