using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;


[Serializable]
public class TestSave
{
    public string Name;
}
public class Test : MonoBehaviour
{
    void Start()
    {
        TestSave ts = new TestSave() { Name = "李四" };
        // SaveManager.SaveObject(ts);
        Debug.Log(SaveManager.LoadObject<TestSave>().Name); // 从磁盘走的
        Debug.Log(SaveManager.LoadObject<TestSave>().Name); // 缓存出来的
    }
    private void Update()
    {
    }
}
