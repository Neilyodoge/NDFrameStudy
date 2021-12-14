using System;   //
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResManager : ManagerBase<ResManager>
{
    // 需要缓存的类型。字典比List节省了遍历List的过程
    private Dictionary<Type, bool> wantCacheDic;

    public override void Init()
    {
        base.Init();
        // TODO: 替换成真实的配置
        wantCacheDic = new Dictionary<Type, bool>();
    }

    // 检查一个类型是否需要缓存
    private bool CheckCacheDic(Type type)
    {
        return wantCacheDic.ContainsKey(type);
    }

    // 获取实例-普通class。如果类型需要缓存，会从对象池中获取
    public T Load<T>() where T : class, new()
    {
        // 需要缓存
        if (CheckCacheDic(typeof(T)))
        {
            return PoolManager.Instance.GetObject<T>();
        }
        else
        {
            return new T();
        }
    }

    // 获取实例-组件类型
    public T Load<T>(string path, Transform parent = null) where T : Component
    {
        if (CheckCacheDic(typeof(T)))
        {
            return PoolManager.Instance.GetGameObject<T>(GetPrefab(path), parent);
        }
        else
        {
            return InstantiateForPrefab(path).GetComponent<T>();
        }
    }

    // 获取预制体
    public GameObject GetPrefab(string path)
    {
        GameObject prefab = Resources.Load<GameObject>(path);
        if (prefab != null)
        {
            return prefab;
        }
        else
        {
            throw new Exception("ND:预制体路径有误，没有找到预制体");
        }
    }
    public GameObject InstantiateForPrefab(string path, Transform parent = null)
    {
        return InstantiateForPrefab(GetPrefab(path),parent);
    }

    // 基于prefab实例化
    public GameObject InstantiateForPrefab(GameObject prefab, Transform parent = null)
    {
        GameObject go = GameObject.Instantiate<GameObject>(prefab, parent);
        go.name = prefab.name;
        return go;
    }
}
