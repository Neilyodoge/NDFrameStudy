using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;

/// <summary>
///  NeilyodogFrame 框架主要的拓展方法
/// </summary>
public static class NDExtension
{
    #region 获取特性
    /// <summary>
    /// 获取特性
    /// </summary>
  // 这里是获取自己的特性
    public static T GetAttribute<T>(this object obj) where T: Attribute    // 这里的obj是个实例,后面的Attribute是个类型约束
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
    #endregion

}
