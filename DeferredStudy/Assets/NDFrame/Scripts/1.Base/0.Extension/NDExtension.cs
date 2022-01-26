using System;
using System.Reflection;
using UnityEngine;

/// <summary>
///  NeilyodogFrame 框架主要的拓展方法
/// </summary>
public static class NDExtension
{
    #region 通用
    /// <summary>
    /// 获取特性
    /// </summary>
  // 这里是获取自己的特性
    public static T GetAttribute<T>(this object obj) where T : Attribute    // 这里的obj是个实例,后面的Attribute是个类型约束
    {
        return obj.GetType().GetCustomAttribute<T>();
    }
    // 这里是重载函数，获取其他特性
    // 参数: 
    //    type:特性所在的类型
    public static T GetAttribute<T>(this object obj, Type type) where T : Attribute
    {
        return type.GetCustomAttribute<T>();
    }

    //使用的时候
    // 特性名字 变量名字 = this.GetAttribute<特性名字>(typeof(想从哪个对象身上获取));
    // TestAttribute test = this.GetAttribute<TestAttribute>(typeof(GameObject));
    // 然后可以 if (变量名字.特性名字)

    /// <summary>
    /// 数组相等对比
    /// </summary>
    public static bool ArrayEquals(this object[] objs, object[] other)
    {
        if (other == null || objs.GetType() != other.GetType())
        {
            return false;
        }
        if (objs.Length == other.Length)
        {
            for (int i = 0; i < objs.Length; i++)
            {
                if (!objs[i].Equals(other[i]))
                {
                    return false;
                }
            }
        }
        else
        {
            return false;
        }
        return true;
    }
    #endregion

    #region 资源管理
    /// <summary>
    /// GameObject放入对象池
    /// </summary>
    /// <param name="go"></param>
    public static void NDGameObjectPushPool(this GameObject go)
    { 
        PoolManager.Instance.PushGameObject(go);
    }
    /// <summary>
    /// GameObject放入对象池
    /// </summary>
    /// <param name="go"></param>
    public static void NDGameObjectPushPool(this Component com)
    {
        NDGameObjectPushPool(com.gameObject);
    }

    /// <summary>
    /// 普通类放进池子
    /// </summary>
    /// <param name="obj"></param>
    public static void NDObjectPushPool(this object obj)
    {
        PoolManager.Instance.PushObject(obj);
    }
    #endregion
}
