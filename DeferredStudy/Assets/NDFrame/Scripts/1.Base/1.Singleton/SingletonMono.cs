using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonMono<T> : MonoBehaviour where T:SingletonMono<T> // <T> 是泛型
{
    public static T Instance;
    protected virtual void Awake()  // virtual 虚方法，子类可以重写
    {
        Instance = this as T; // 强转
    }
}
