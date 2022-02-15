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
        TestSave ts = new TestSave() { Name = "123" };
        TestSave ts3 = new TestSave() { Name = "345" };
        // SaveManager.SaveFile(new TestSave() { Name = "123" }, Application.persistentDataPath + "/" + 1);
        SaveManager.SaveObject(ts, "UserInfoX");
        SaveManager.SaveObject(ts3, "UserInfo",3);

        TestSave ts2 = SaveManager.LoadObject<TestSave>("UserInfo", 3);
        Debug.Log(ts2.Name);
    }
    private void Update()
    {
    }
}
