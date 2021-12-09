using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolData : MonoBehaviour
{
    public Queue<object> poolQueue = new Queue<object>();     // 对象容器

    public ObjectPoolData(object obj)   // 构造函数
    {
        PushObj(obj);
    }

    /// <summary>
    /// 将对象放进对象池
    /// </summary>
    /// <param name="obj"></param>
    public void PushObj(object obj)
    {
        poolQueue.Enqueue(obj);         // 添加队列
    }
    /// <summary>
    /// 从对象池中获取对象
    /// </summary>
    /// <returns></returns>
    public object GetObj()
    {
        return poolQueue.Dequeue();     // 移除队列
    }
}
