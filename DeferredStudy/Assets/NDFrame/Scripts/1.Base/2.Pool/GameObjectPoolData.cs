using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;  //

/// <summary>
/// 普通类 对象 对象池数据
/// </summary>
public class GameObjectPoolData
{
    public GameObject fatherObj;        // 对象池中 父节点。这边的案例是子弹
    public Queue<GameObject> poolQueue; // 对象容器会比列表 (List) 更优化,List 加入新的GameObj的时候会比 Queue更耗

    public GameObjectPoolData(GameObject obj,GameObject poolRootObj)  // 构造函数(为了初始化用的)
    {
        // 创建父节点 并设置到对象池根节点下方
        fatherObj = new GameObject(obj.name);
        fatherObj.transform.SetParent(poolRootObj.transform);   // 设置父级的transform
        poolQueue = new Queue<GameObject>();
        // 把首次创建时，需要放入的对象，放进容器
        PushObj(obj);
    }
    /// <summary>
    /// 将对象放进对象池
    /// </summary>
    /// <param name="obj"></param>
    public void PushObj(GameObject obj)
    {
        poolQueue.Enqueue(obj);                             // 对象进容器
        obj.transform.SetParent(fatherObj.transform);       // 设置父物体
        obj.SetActive(false);                               // 设置隐藏
    }
    /// <summary>
    /// 从对象池中获取对象
    /// </summary>
    /// <returns></returns>
    public GameObject GetObj(Transform parent = null /*这里可以根据父级别走*/)
    {
        GameObject obj = poolQueue.Dequeue();
        obj.SetActive(true);                            // 显示对象
        obj.transform.SetParent(parent);                // 设置父物体
        if (parent == null)                             // 如果父物体为空
        {
            SceneManager.MoveGameObjectToScene(obj, SceneManager.GetActiveScene());     // 把显示后的对象移动回默认场景
        }

        
        return obj;
    }
}
