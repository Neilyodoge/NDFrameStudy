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
        TestSave testSave = new TestSave { Name = "张三" };
        SaveManager.SaveFile(testSave, Application.persistentDataPath + "/张三");
        TestSave testSave1 = SaveManager.LoadFile<TestSave>(Application.persistentDataPath + "/张三");
        Debug.Log(testSave1.Name);
    }
    private void Update()
    {
    }
}
