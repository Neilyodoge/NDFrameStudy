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
    public override void Init()
    {
        base.Init();
    }
    /// <summary>
    /// 获取GameObject
    /// </summary>
    /// <typeparam name="T">最终需要的组件</typeparam>
    /// <param name="prefab"></param>
    /// <returns></returns>
    public T GetGameObject<T>(GameObject prefab) where T : UnityEngine.Object   // 泛型
    {
        GameObject obj = GetGameObject(prefab);
        if (obj != null)
        {
            return obj.GetComponent<T>();
        }
        return null;
    }

    public GameObject GetGameObject(GameObject prefab)  // 非泛型，与上面做区分
    {
        GameObject obj = null;
        string name = prefab.name;  // 通过prefab的名称去找 字典Dictionary 的 string
        // 检查有没有具体的子级
        if (CheckGameObjectCache(prefab))
        {
            obj = gameObjectPoolDic[name].GetObj(); // 有就拿出去
        }
        else    // 没有的话实例化一个
        {
            obj = GameObject.Instantiate(prefab);   // 实例化
            obj.name = name;                        // 正常实例化之后是有个 name(clone) 的，这里是为了名字一样
        }
        return null;
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
    public void Clear() // 跨场景的时候清空
    {
        gameObjectPoolDic.Clear();
    }
}
