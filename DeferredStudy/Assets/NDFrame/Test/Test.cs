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
        SaveItem saveItem = SaveManager.CreateSaveItem();
        SaveManager.SaveObject(new TestSave() { Name = "Neilyodog"}, saveItem);

        Debug.Log(SaveManager.LoadObject<TestSave>().Name);
        SaveManager.DeleteSaveItem(saveItem);
        Debug.Log(SaveManager.LoadObject<TestSave>().Name); // 这里为了验证是否找的到，因为找不到所以会报错
    }
    private void Update()
    {
    }
}
