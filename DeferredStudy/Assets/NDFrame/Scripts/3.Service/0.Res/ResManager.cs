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
        wantCacheDic.Add(typeof(CubeController),true);
    }
    /// <summary>
    /// 加载unity资源 如AudionClip Sprite。这种的特点就是不需要实例化
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public T LoadAsset<T>(string path) where T : UnityEngine.Object
    {
        return Resources.Load<T>(path);
    }

    /// <summary>
    /// 检查一个类型是否需要缓存
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private bool CheckCacheDic(Type type)
    {
        return wantCacheDic.ContainsKey(type);
    }

    /// <summary>
    /// 获取实例-普通class。如果类型需要缓存，会从对象池中获取
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
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

    /// <summary>
    /// 获取实例-组件类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 异步加载游戏物体
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <param name="callBack"></param>
    /// <param name="parent"></param>
    public void LoadGameObjectAsync<T>(string path, Action<T> callBack = null,Transform parent = null) where T : UnityEngine.Object
    {
        // 对象池里面有
        if (CheckCacheDic(typeof(T)))
        {
            GameObject go = PoolManager.Instance.CheckCacheAndLoadGameObject(path, parent);
            if (go != null) // 对象有
            {
                callBack?.Invoke(go.GetComponent<T>());
            }
            else // 对象池没有
            {
                StartCoroutine(DoLoadGameObjectAsync<T>(path, callBack, parent));
            }
        }
        else // 对象池没有
        {
            StartCoroutine(DoLoadGameObjectAsync<T>(path, callBack, parent));
        }
    }
    IEnumerator DoLoadGameObjectAsync<T>(string path, Action<T> callBack = null, Transform parent = null) where T : UnityEngine.Object
    {
        ResourceRequest request = Resources.LoadAsync<GameObject>(path);
        yield return request;
        GameObject go = InstantiateForPrefab(request.asset as GameObject, parent);
        callBack?.Invoke(go.GetComponent<T>()); // 判断callBack是不是null，可以不写
    }

    /// <summary>
    /// 异步加载Unity资源 AudioClip Sprite GameObject(prefab)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <param name="callBack"></param>
    public void LoadAssetAsync<T>(string path, Action<T> callBack) where T : UnityEngine.Object
    {
        StartCoroutine(DoLoadAssetAsync<T>(path, callBack));
    }

    IEnumerator DoLoadAssetAsync<T>(string path, Action<T> callBack) where T : UnityEngine.Object   // 携程
    {
        ResourceRequest request = Resources.LoadAsync<T>(path);
        yield return request;
        callBack?.Invoke(request.asset as T); // 判断callBack是不是null，可以不写
    }

    /// <summary>
    /// 获取预制体
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 基于prefab实例化
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public GameObject InstantiateForPrefab(GameObject prefab, Transform parent = null)
    {
        GameObject go = GameObject.Instantiate<GameObject>(prefab, parent);
        go.name = prefab.name; // 游戏物体实例化和物体名称一致
        return go;
    }
}
