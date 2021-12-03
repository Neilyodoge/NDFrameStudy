using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ManagerBase : MonoBehaviour   // 来应对没有泛型的引用
{
    public virtual void Init() { }
}
public abstract class ManagerBase<T>/*泛型的*/ : ManagerBase where T : ManagerBase<T>  // abstract ;通过继承 ManagerBase 来间接继承 MonoBehaviour
{
    public static T Instance;
    /// <summary>
    /// 管理器初始化
    /// </summary>
    public override void Init()  // Iniv 初始化;重写的基类中的 Init
    {
        Instance = this as T; // 强转
    }
}
