using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : ManagerBase<PoolManager>
{
    // 根节点
    [SerializeField]
    private GameObject poolRootObj;
    /// <summary>
    /// GameObject对象容器。通过string来找到GameRoot下面的某个物体
    /// </summary>
    public Dictionary<string, GameObjectPoolData> gameObjectPoolDic = new Dictionary<string, GameObjectPoolData>();
    /// <summary>
    /// 普通类 对象容器
    /// </summary>
    public Dictionary<string, ObjectPoolData> objectPoolDic = new Dictionary<string, ObjectPoolData>();
    public override void Init()
    {
        base.Init();
    }

    #region GameObject 对象相关操作
    /// <summary>
    /// 获取GameObject
    /// </summary>
    /// <typeparam name="T">最终需要的组件</typeparam>
    /// <param name="prefab"></param>
    /// <returns></returns>
    public T GetGameObject<T>(GameObject prefab, Transform parent = null) where T : UnityEngine.Object   // 泛型
    {
        GameObject obj = GetGameObject(prefab,parent);
        if (obj != null)
        {
            return obj.GetComponent<T>();
        }
        return null;
    }

    public GameObject GetGameObject(GameObject prefab, Transform parent = null)  // 非泛型，与上面做区分，
    {
        GameObject obj = null;
        string name = prefab.name;  // 通过prefab的名称去找 字典Dictionary 的 string
        // 检查有没有具体的子级
        if (CheckGameObjectCache(prefab))
        {
            obj = gameObjectPoolDic[name].GetObj(parent); // 有就拿出去,顺便把父级拿出去
        }
        else    // 没有的话实例化一个
        {
            obj = GameObject.Instantiate(prefab,parent);   // 根据父级实例化
            obj.name = name;                               // 正常实例化之后是有个 name(clone) 的，这里是为了名字一样
        }
        return obj;
    }

    /// <summary>
    /// GameObject 放进对象池里
    /// </summary>
    /// <param name="obj"></param>
    public void PushGameObject(GameObject obj)
    {
        string name = obj.name;
        // 现在有没有这一层
        if (gameObjectPoolDic.ContainsKey(name))
        {
            gameObjectPoolDic[name].PushObj(obj);
        }
        else
        {
            gameObjectPoolDic.Add(name, new GameObjectPoolData(obj, poolRootObj));
        }
    }

    /// <summary>
    /// 检查有没有某一层对象池数据
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns></returns>
    private bool CheckGameObjectCache(GameObject prefab)
    {
        string name = prefab.name;
        // 前半句是有没有 GameObjectPoolData 也就是子集 ；  后半句是里面有没有数据
        return gameObjectPoolDic.ContainsKey(name) && gameObjectPoolDic[name].poolQueue.Count > 0;
    }

    /// <summary>
    /// 检查缓存，如果成功，则加载游戏物体;不成功返回null
    /// </summary>
    /// <returns></returns>
    public GameObject CheckCacheAndLoadGameObject(string path, Transform parent)
    {
        // 通过路径获取最终预制体的名称 "UI/LoginWindow"
        string[] pathSplit = path.Split('/');
        string prefabName = pathSplit[pathSplit.Length - 1];
        // 对象池有数据
        if (gameObjectPoolDic.ContainsKey(prefabName) && gameObjectPoolDic[prefabName].poolQueue.Count > 0) // 判断字典是不是包含这个name,然后是有没有数据
        {
            return gameObjectPoolDic[prefabName].GetObj(parent);
        }
        else
        {
            return null;
        }
    }
    #endregion
    #region 普通对象相关操作
    // 获取普通对象
    public T GetObject<T>() where T : class,new()   // 限定为 class 可以 new。T是类型
    {
        T obj;
        if (CheckObjectCache<T>())
        {
            string name = typeof(T).FullName;
            obj = (T)objectPoolDic[name].GetObj();
            return obj;
        }
        else
        {
            return new T();
        }
    }

    public void PushObject(object obj)
    {
        string name = obj.GetType().FullName;
        // 现在有没有这一层
        if (objectPoolDic.ContainsKey(name))
        {
            objectPoolDic[name].PushObj(obj);
        }
        else
        {
            objectPoolDic.Add(name, new ObjectPoolData(obj));   // 首次放入对象池时候，使用构造函数
        }
    }

    // 检查有没有某一层对象池数据
    private bool CheckObjectCache<T>()
    {
        string name = typeof(T).FullName;       // FullName 会带命名空间
        // 前半句是有没有 GameObjectPoolData 也就是子集 ；  后半句是里面有没有数据
        //return ObjectPoolDic.ContainsKey(name) && gameObjectPoolDic[name].poolQueue.Count > 0;
        return objectPoolDic.ContainsKey(name) && objectPoolDic[name].poolQueue.Count > 0;
    }
    #endregion

    #region 删除逻辑
    /// <summary>
    /// 删除全部
    /// </summary>
    /// <param name="clearGameObject">是否删除游戏物体</param>
    /// <param name="ClearCObject">是否删除普通C#对象</param>
    public void Clear(bool clearGameObject = true, bool ClearCObject = true) // 跨场景的时候清空
    {
        // 清理GameObject
        if (clearGameObject)
        {   // 清理GameRoot的子集PoolRoot底下的全部对象
            for (int i = 0; i < poolRootObj.transform.childCount; i++)
            {
                Destroy(poolRootObj.transform.GetChild(0).gameObject); // 挨个删除每一个。这里 GetChild() 0 即可，因为删除的时候，子物体的数量编号都发生了变化，所以删除第一个就行 
            }
            gameObjectPoolDic.Clear();
        }
        // 清理字典物体
        if (ClearCObject)         // 判断C#数据要不要清理
        {
            objectPoolDic.Clear();
        }
    }

    public void ClearAllGameObject()                   // 清楚全部 Object
    {
        Clear(true, false);
    }

    public void ClearGameObject(string prefabName)  // 逐名字删除
    {
        Transform go = poolRootObj.transform.Find(prefabName);  // 这里如果直接拿GameObject的话，Find有可能是Null，导致没法.gameObject的
        if (go != null)
        {
            Destroy(go.gameObject);                             // 所以在这里先拿到Transform再.gameObject
            gameObjectPoolDic.Remove(prefabName);
        }
    }
    public void ClearGameObject(GameObject prefab)  // 逐prefab删除
    {
        ClearGameObject(prefab.name);
    }
    public void ClearAllObject()
    {
        Clear(false, true);
    }
    public void ClearObject<T>()
    {
        objectPoolDic.Remove(typeof(T).FullName);
    }
    public void ClearObject(Type type)
    {
        objectPoolDic.Remove(type.FullName);
    }
    #endregion

}
