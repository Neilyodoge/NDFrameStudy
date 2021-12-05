using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;  //

public class GameObjectPoolData
{
    // 对象池中 父节点。这边的案例是子弹
    public GameObject fatherObj;
    // 对象容器会比列表 (List) 更优化,List 加入新的GameObj的时候会比 Queue更耗
    public Queue<GameObject> poolQueue;

    public GameObjectPoolData(GameObject obj,GameObject poolRootObj)  // 构造函数
    {
        // 创建父节点 并设置到对象池根节点下方
        fatherObj = new GameObject(obj.name);
        fatherObj.transform.SetParent(poolRootObj.transform);   // 设置父级的transform
        poolQueue = new Queue<GameObject>();
        // 把首次创建时，需要放入的对象，放进容器
        PushObj(obj);
    }

    public void PushObj(GameObject obj)
    {
        // 对象进容器
        poolQueue.Enqueue(obj);
        // 设置父物体
        obj.transform.SetParent(fatherObj.transform);
        // 设置隐藏
        obj.SetActive(false);
    }
    /// <summary>
    /// 获取数据
    /// </summary>
    /// <returns></returns>
    public GameObject GetObj()
    {
        GameObject obj = poolQueue.Dequeue();
        // 显示对象
        obj.SetActive(true);
        // 父物体设置为空
        obj.transform.parent = null;
        // 把显示后的对象移动回默认场景
        SceneManager.MoveGameObjectToScene(obj, SceneManager.GetActiveScene());
        return obj;
    }
}
