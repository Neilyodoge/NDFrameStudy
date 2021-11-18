using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 单例模式的基类，全局只有一个
/// </summary>
public class Singleton<T> where T : Singleton<T>, new() // 限定了一下类型
{
    private static T instance;
    public static T Instance
    {
        get 
        {
            if (instance == null)   // 没有被实例化过
            {
                instance = new T();
            }
            return instance;
        }
    }
}
