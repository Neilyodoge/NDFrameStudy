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

        Debug.Log(SaveManager.LoadObject<TestSave>(saveItem.saveID).Name);
        SaveManager.DeleteSaveItem(saveItem);
    }
    private void Update()
    {
    }
}
