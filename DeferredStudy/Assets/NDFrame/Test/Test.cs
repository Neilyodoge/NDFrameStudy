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
       // SaveManager.SaveFile(new TestSave() { Name = "123" }, Application.persistentDataPath + "/" + 1);
    }
    private void Update()
    {
    }
}
